using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using GR.Win32;

namespace GR.Input
{
    public enum MouseButton : int
    {
        Left = 0x1,
        Right = 0x2,
        Middle = 0x4
    }

    public class Mouse
    {
        private static Rectangle GetWindowRect(IntPtr hwnd)
        {
            Interop.RECT tmp_rect = new Interop.RECT();
            Interop.GetWindowRect(hwnd, ref tmp_rect);
            Rectangle rect = new Rectangle(tmp_rect.left, tmp_rect.top,
                tmp_rect.right - tmp_rect.left, tmp_rect.bottom - tmp_rect.top);

            return rect;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }


        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
        }

        private enum Flags
        {
            MOUSEEVENTF_MOVE = 0x0001, /* mouse move */
            MOUSEEVENTF_LEFTDOWN = 0x0002, /* left button down */
            MOUSEEVENTF_LEFTUP = 0x0004, /* left button up */
            MOUSEEVENTF_RIGHTDOWN = 0x0008, /* right button down */
            MOUSEEVENTF_RIGHTUP = 0x0010, /* right button up */
            MOUSEEVENTF_MIDDLEDOWN = 0x0020, /* middle button down */
            MOUSEEVENTF_MIDDLEUP = 0x0040, /* middle button up */
            MOUSEEVENTF_XDOWN = 0x0080, /* x button down */
            MOUSEEVENTF_XUP = 0x0100, /* x button down */
            MOUSEEVENTF_WHEEL = 0x0800, /* wheel button rolled */
            MOUSEEVENTF_VIRTUALDESK = 0x4000, /* map to entire virtual desktop */
            MOUSEEVENTF_ABSOLUTE = 0x8000 /* absolute move */
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private static Window window;

        private static ClickDistribution rect_distribution = new NormalDistribution(new Random(), 0.7); 
		private static ClickDistribution ellipse_distribution = new EllipticalNormalDistribution(new Random(), 0.65);

        public static void SetClickDistributions(ClickDistribution rect, ClickDistribution ellipse)
        {
            rect_distribution = rect;
            ellipse_distribution = ellipse;
        }

        public static void SetWindow(Window window)
        {
            Mouse.window = window;
        }

        private static Point ToScreen(Point point)
        {
            Rectangle window_rect = window.Rect;

            int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            int x = window_rect.Left + point.X;
            int y = window_rect.Top + point.Y;

            return new Point(x * 65535 / width, y * 65535 / height);
        }

		public static Point ToScreenAbs(Point point)
		{
			int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
			int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

			int x = point.X;
			int y = point.Y;

			return new Point(x * 65535 / width, y * 65535 / height);
		}

        public static Point GetPixel(double x, double y)
        {
            Rectangle window_rect = window.Rect;

            return new Point((int)(window_rect.Width * x), (int)(window_rect.Height * y));
        }

        public static void SetPosition(Point screen_point)
        {
            INPUT[] InputData = new INPUT[1];

            InputData[0].mi.dx = screen_point.X;
            InputData[0].mi.dy = screen_point.Y;

            InputData[0].mi.dwFlags = (int)(Flags.MOUSEEVENTF_ABSOLUTE | Flags.MOUSEEVENTF_MOVE);

            if (SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                Console.WriteLine("SendInput mouse error");
            }
        }

        public static Point CursorPos()
        {
            Rectangle window_rect;
            if (window != null) window_rect = window.Rect;
            else window_rect = new Rectangle(0, 0, 5, 5);

            return new Point(Cursor.Position.X - window_rect.Left, Cursor.Position.Y - window_rect.Top);
        }

		private static Random random = new Random();

        public static void Move(Point point)
        {
			Point screen_point;
			if (window == null)
				screen_point = ToScreenAbs(point);
			else
				screen_point = ToScreen(point);

            int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            double current_x = 65536 * Cursor.Position.X / (double)width;
            double current_y = 65536 * Cursor.Position.Y / (double)height;

            /*int iterations=10+random.Next(5);

            double dist = Math.Max(Math.Abs(screen_point.X - current_x), Math.Abs(screen_point.Y - current_y));

            double dx = (screen_point.X - current_x) / iterations,
                dy = (screen_point.Y - current_y) / iterations;

            Point[] points = new Point[iterations];

            int random_x = Math.Abs((int)(dx / 2)), random_y = Math.Abs((int)(dy / 2));

            for (int i = 0; i < iterations; i++)
            {
                current_x += dx;
                current_y += dy;

                points[i] = new Point((int)(current_x+random.Next(random_x)), (int)(current_y+random.Next(random_y)));
            }

            for (int i = 0; i < iterations; i++) 
            {
                SetPosition(points[i]);

                Thread.Sleep((int)(20*(dist/160000.0)));
            }*/

            double dist = Math.Max(Math.Abs(screen_point.X - current_x), Math.Abs(screen_point.Y - current_y));

            double dx = (screen_point.X - current_x),
                dy = (screen_point.Y - current_y);

            double max_dev = Math.Min(10, dist / 20);

            int totalTime = (int)((10 + random.Next(5)) * (dist / 1000.0));

            int startTime = Environment.TickCount;
            int time;

            Thread.Sleep(1);

            while ((time = Environment.TickCount) < startTime + totalTime)
            {
                double elapsed = time - startTime;

                SetPosition(new Point(
                    (int)(current_x + dx * elapsed / totalTime + random.Next((int)max_dev)),
                    (int)(current_y + dy * elapsed / totalTime + random.Next((int)max_dev))));

                Thread.Sleep(10);
            }
            
            SetPosition(screen_point);
        }

        /// <summary>
        /// Clicks target point relative to window set.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mouse_button">Mouse button (MouseButtons.Left or MouseButtons.Right)</param>
        public static void Click(Point point, MouseButton mouse_button)
        {
            SetPosition(point);

            INPUT[] InputData = new INPUT[3];

            Point screen_point = ToScreen(point);
            InputData[0].mi.dx = screen_point.X;
            InputData[0].mi.dy = screen_point.Y;

            InputData[0].mi.dwFlags = (int)(Flags.MOUSEEVENTF_ABSOLUTE | Flags.MOUSEEVENTF_MOVE);

            if ((mouse_button & MouseButton.Left) == MouseButton.Left)
            {
                InputData[1].mi.dwFlags = (int)Flags.MOUSEEVENTF_LEFTDOWN;
                InputData[2].mi.dwFlags = (int)Flags.MOUSEEVENTF_LEFTUP;
            }
            else if ((mouse_button & MouseButton.Right) == MouseButton.Right)
            {
                InputData[1].mi.dwFlags = (int)Flags.MOUSEEVENTF_RIGHTDOWN;
                InputData[2].mi.dwFlags = (int)Flags.MOUSEEVENTF_RIGHTUP;
            }
            else
            {
                Console.WriteLine("unknown mouse button");
                return;
            }

            if (SendInput(3, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                Console.WriteLine("SendInput mouse error");
            }
        }

        private static void SetButton(MouseButton mouse_button, bool press)
        {
            INPUT[] InputData = new INPUT[1];

            if ((mouse_button & MouseButton.Left) == MouseButton.Left)
            {
                if (press)
                    InputData[0].mi.dwFlags = (int)Flags.MOUSEEVENTF_LEFTDOWN;
                else
                    InputData[0].mi.dwFlags = (int)Flags.MOUSEEVENTF_LEFTUP;
            }
            else if ((mouse_button & MouseButton.Right) == MouseButton.Right)
            {
                if (press)
                    InputData[0].mi.dwFlags = (int)Flags.MOUSEEVENTF_RIGHTDOWN;
                else
                    InputData[0].mi.dwFlags = (int)Flags.MOUSEEVENTF_RIGHTUP;
            }
            else
            {
                Console.WriteLine("unknown mouse button");
                return;
            }

            if (SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                Console.WriteLine("SendInput mouse error");
            }
        }

        /// <summary>
        /// Clicks inside a circle defined by the center of the circle as point and maximum distance as the radius.
        /// Uses the ellipse distribution.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="max_dist"></param>
        /// <param name="mouse_button"></param>
        public static void Click(Point point, int max_dist, MouseButton mouse_button)
        {
            Point click_point = ellipse_distribution.Click(new Button(max_dist * 2, max_dist * 2));

            point.Offset(click_point.X - max_dist, click_point.Y - max_dist);

            Click(click_point, mouse_button);
        }

        public static void MoveClick(Point point, int max_dist, MouseButton mouse_button)
        {
            Console.WriteLine("MoveClicking() " + point);

            Point click_point = ellipse_distribution.Click(new Button(max_dist * 2, max_dist * 2));

            point.Offset(click_point.X - max_dist, click_point.Y - max_dist);

            Move(point);

            Click(point, mouse_button);
        }

		public static void MovePauseClick(Point point, int max_dist, MouseButton mouse_button, int pause)
		{
			Console.WriteLine("MovePauseClicking() " + point);

			Point click_point = ellipse_distribution.Click(new Button(max_dist * 2, max_dist * 2));

			point.Offset(click_point.X - max_dist, click_point.Y - max_dist);

			Point relative_point = point;
			Console.WriteLine(relative_point);

			point.Offset(window.Rect.Location);
			
			//Move(point);
			Point cur_pos = Cursor.Position;
			int distance = (int)Math.Sqrt((cur_pos.X - point.X) * (cur_pos.X - point.X) + (cur_pos.Y - point.Y) * (cur_pos.Y - point.Y));

			Console.WriteLine(distance + "px");

			Cursor.Position = point;

			Thread.Sleep(2*distance);
			//Thread.Sleep(pause);
			
			Click(relative_point, mouse_button);
		}

        public static void MoveClick(Rectangle rectangle, MouseButton mouse_button)
        {
            Console.WriteLine("MoveClicking() " + rectangle.ToString());

            Point click_point = rect_distribution.Click(new Button(rectangle.Width, rectangle.Height));
            click_point.Offset(rectangle.Location);

            Move(click_point);

			//Thread.Sleep(1000);

            Click(click_point, mouse_button);
        }

		public static void MovePauseClick(Rectangle rectangle, MouseButton mouse_button, int pause)
		{
			Console.WriteLine("MovePauseClicking() " + rectangle.ToString());

			Point click_point = rect_distribution.Click(new Button(rectangle.Width, rectangle.Height));
			click_point.Offset(rectangle.Location);

			Point relative_point = click_point;

			click_point.Offset(window.Rect.Location);
			//Move(click_point);
			Point cur_pos = Cursor.Position;
			//Point screen_point = ToScreenAbs(click_point);
			//Console.WriteLine(cur_pos + " " + click_point);
			int distance = (int)Math.Sqrt((cur_pos.X - click_point.X) * (cur_pos.X - click_point.X) + (cur_pos.Y - click_point.Y) * (cur_pos.Y - click_point.Y));

			Console.WriteLine(distance + "px");

			Cursor.Position = click_point;

			Thread.Sleep(distance);
			//Thread.Sleep(pause);

			Click(relative_point, mouse_button);

			//Thread.Sleep(5000);
		}

        public static void Drag(Point start, Point end, MouseButton mouse_button)
        {
            Console.WriteLine("Dragging() " + start + " " + end);

            Move(start);
            SetButton(mouse_button, true);
            Thread.Sleep(100);
            Move(end);
            Thread.Sleep(100);
            SetButton(mouse_button, false);
        }

		public static void SlowDrag(Point start, Point end, MouseButton mouse_button)
		{
			Console.WriteLine("Dragging() " + start + " " + end);

			Move(start);
			SetButton(mouse_button, true);
			Thread.Sleep(400);
			Move(end);
			Thread.Sleep(400);
			SetButton(mouse_button, false);
		}

        /// <summary>
        /// Clicks inside a rectangle relative to the parent window and uses the rectangular distribution attached.
        /// </summary>
        /// <param name="parent_window"></param>
        /// <param name="rectangle"></param>
        /// <param name="mouse_button"></param>
        public static void MoveClick(Rectangle rectangle, Window parent_window, MouseButton mouse_button)
        {
            Mouse.SetWindow(parent_window);

            MoveClick(rectangle, mouse_button);
        }
    }
}


/*
procedure WindMouse(xs, ys, xe, ye, gravity, wind, minWait, maxWait, maxStep, targetArea: extended); 
var veloX, veloY, windX, windY, veloMag, dist, randomDist, lastDist, step: extended; lastX, lastY: integer; sqrt2, sqrt3, sqrt5: extended; begin sqrt2:= sqrt(2); sqrt3:= sqrt(3); sqrt5:= sqrt(5); while hypot(xs - xe, ys - ye) > 1 do begin dist:= hypot(xs - xe, ys - ye); wind:= minE(wind, dist); if dist >= targetArea then begin windX:= windX / sqrt3 + (random(round(wind) * 2 + 1) - wind) / sqrt5; windY:= windY / sqrt3 + (random(round(wind) * 2 + 1) - wind) / sqrt5; end else begin windX:= windX / sqrt2; windY:= windY / sqrt2; if (maxStep < 3) then begin maxStep:= random(3) + 3.0; end else begin maxStep:= maxStep / sqrt5; end; end; veloX:= veloX + windX; veloY:= veloY + windY; veloX:= veloX + gravity * (xe - xs) / dist; veloY:= veloY + gravity * (ye - ys) / dist; if hypot(veloX, veloY) > maxStep then begin randomDist:= maxStep / 2.0 + random(round(maxStep) / 2); veloMag:= sqrt(veloX * veloX + veloY * veloY); veloX:= (veloX / veloMag) * randomDist; veloY:= (veloY / veloMag) * randomDist; end; lastX:= Round(xs); lastY:= Round(ys); xs:= xs + veloX; ys:= ys + veloY; if (lastX <> Round(xs)) or (lastY <> Round(ys)) then MoveMouse(Round(xs), Round(ys)); step:= hypot(xs - lastX, ys - lastY); wait(round((maxWait - minWait) * (step / maxStep) + minWait)); lastdist:= dist; end; if (Round(xe) <> Round(xs)) or (Round(ye) <> Round(ys)) then MoveMouse(Round(xe), Round(ye)); end;

{*************************************************************************** procedure MMouse(x, y, rx, ry: integer); By: Benland100 Description: Moves the mouse. **************************************************************************} //Randomness is just added to the x,y. Might want to change that. procedure MMouse(x, y, rx, ry: integer); var cx, cy: integer; randSpeed: extended; begin randSpeed:= (random(MouseSpeed) / 2.0 + MouseSpeed) / 10.0; if randSpeed = 0.0 then randSpeed := 0.1; getMousePos(cx,cy); X := x + random(rx); Y := y + random(ry); WindMouse(cx,cy,x,y,9.0,3.0,10.0/randSpeed,15.0/randSpeed,10.0randSpeed,10.0*randSpeed); end;

Here are some methods written in SCAR. Converting them C# shouldn't be too hard, these are quite realistic.*/