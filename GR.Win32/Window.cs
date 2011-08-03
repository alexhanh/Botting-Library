using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GR.Win32
{
    public enum ScreenCorner
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

	public class Window
	{
		private IntPtr window_handle;
		public IntPtr Handle
		{
			get { return window_handle; }
			set { window_handle = value; }
		}

		public Window(IntPtr window_handle)
		{
			Handle = window_handle;
		}

		public override int GetHashCode()
		{
			return window_handle.ToInt32();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType()) return false;

			if (Handle.Equals(((Window)obj).Handle)) return true;

			return false;
		}

		public string Title { get { return GetWindowText(window_handle); } }
		public Rectangle Rect { get { return GetWindowRect(window_handle); } }
        public string ClassName
        {
            get
            {
                int length = 64;
                while (true)
                {
                    StringBuilder sb = new StringBuilder(length);
                    
                    int result = User32.GetClassName(window_handle, sb, sb.Capacity);
                    
                    if (result == 0)
                        return "";

                    if (sb.Length != length - 1)
                    {
                        return sb.ToString();
                    }
                    length *= 2;
                }
            }
        }

		// methods

		public void SetForeground()
		{
			Interop.SetForegroundWindow(window_handle);
		}

		public static Window ForegroundWindow()
		{
			return new Window(Interop.GetForegroundWindow());
		}

		public void Minimize()
		{
			Interop.CloseWindow(Handle);
		}

		public bool IsMinimized()
		{
			return Interop.IsIconic(Handle);
		}

        public void Hide()
        {
            User32.ShowWindow(Handle.ToInt32(), 0);
        }

        public void MakeVisible()
        {
            User32.ShowWindow(Handle.ToInt32(), 5);
        }

		public bool Visible
		{
			get
			{
				IntPtr hdc = Interop.GetDC(this.window_handle);
				//Graphics g = Graphics.FromHwnd(this.window_handle);
				//IntPtr hdc = g.GetHdc();

				Interop.RECT rect = new Interop.RECT();

				bool visible = false;

				int ret_value = Interop.GetClipBox(hdc, out rect);
				if (ret_value == (int)Interop.ClipBoxMessage.SIMPLEREGION)
				{
					Interop.RECT client_rect = new Interop.RECT();
					Interop.GetClientRect(this.window_handle, out client_rect);
					if (client_rect.bottom == rect.bottom &&
						client_rect.left == rect.left &&
						client_rect.right == rect.right &&
						client_rect.top == rect.top)
						visible = true;
				}

				//g.ReleaseHdc(hdc);
				//g.Dispose();

				Interop.ReleaseDC(this.window_handle, hdc);

				return visible;
			}
		}

		public void Restore()
		{
			Interop.OpenIcon(Handle);
		}

		public void Focus()
		{
			Interop.SetFocus(Handle);
		}

		public Bitmap Capture()
		{
            return CaptureScreenNew(this.Handle);//Capture(new Rectangle(0, 0, Rect.Width, Rect.Height));
		}

		public Bitmap Capture(Rectangle rect)
		{
			return CaptureScreen(Handle, rect);
		}

		public void SetPosition(Point position)
		{
			Interop.SetWindowPos(Handle, IntPtr.Zero, position.X, position.Y, Rect.Width, Rect.Height, 0);
		}

        /// <summary>
        /// This method tries to set the position of the window so that it's fully visible but one of it's corners is attached to given parameter.
        /// </summary>
        public void AttachToCorner(ScreenCorner corner)
        {
            if (corner == ScreenCorner.UpperLeft)
                SetPosition(new Point(0, 0));
            if (corner == ScreenCorner.UpperRight)
                SetPosition(new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Rect.Width, 0));
            if (corner == ScreenCorner.LowerRight)
                SetPosition(new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Rect.Width, Screen.PrimaryScreen.WorkingArea.Height - this.Rect.Height));
            if (corner == ScreenCorner.LowerLeft)
                SetPosition(new Point(0, Screen.PrimaryScreen.WorkingArea.Height - this.Rect.Height));
        }

		public void Resize(Size size)
		{
			Interop.SetWindowPos(Handle, IntPtr.Zero, Rect.Left, Rect.Top, size.Width, size.Height, 0);
		}

		public string GetText()
		{
			int length = Interop.SendMessage(Handle, (uint)Interop.Messages.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);

			StringBuilder sb = new StringBuilder(length + 1);

			Interop.SendMessage(Handle, (uint)Interop.Messages.WM_GETTEXT, sb.Capacity, sb);

			return sb.ToString();
		}

		// static methods

		private static string enum_find_string = null;
		private static System.Collections.ArrayList enum_found_windows = null;

		public static Window[] FindWindows(string title_begin)
		{
			enum_find_string = title_begin;
			enum_found_windows = new System.Collections.ArrayList();

			Interop.EnumWindowProc callBackPtr = new Interop.EnumWindowProc(EnumCallback);
			Interop.EnumWindows(callBackPtr, 0);

			if (enum_found_windows.Count == 0) return null;

			return (Window[])enum_found_windows.ToArray(typeof(Window));
		}

		public static Window FindWindow(string title_begin)
		{
			Window[] windows = FindWindows(title_begin);

			if (windows == null) return null;

			return windows[0];
		}

        public Window FindChildWindow(string text_begin)
        {
            IntPtr first = IntPtr.Zero;

            Window child = new Window(Interop.FindWindowEx(Handle, first, "", null));

            while (child.Handle != IntPtr.Zero && !child.Title.StartsWith(text_begin))
            {
                child = new Window(Interop.FindWindowEx(Handle, child.Handle, "", null));
            }

            if (child.Handle == IntPtr.Zero) return null;
            return child;
        }

		/// <summary>
		/// Pass empty string to class_name to match all.
		/// Pass empty string to text_begin to match all.
		/// Pass null to begin_with to skip.
		/// </summary>
		/// <param name="class_name"></param>
		/// <param name="text_begin"></param>
		/// <param name="begin_with"></param>
		/// <returns></returns>
		public Window FindChildWindow(string class_name, string text_begin, Window begin_with)
		{
			IntPtr first = IntPtr.Zero;
			if (begin_with != null) first = begin_with.Handle;

			Window child = new Window(Interop.FindWindowEx(Handle, first, class_name, null));

			while (child.Handle != IntPtr.Zero && !child.Title.StartsWith(text_begin))
			{
				child = new Window(Interop.FindWindowEx(Handle, child.Handle, class_name, null));
			}

			if (child.Handle == IntPtr.Zero) return null;
			return child;
		}

		public Window ChildWindowFromPoint(Point point, string class_name, Window begin_with)
		{
			Point screen_point = new Point(Rect.X + point.X, Rect.Y + point.Y);

			IntPtr first = IntPtr.Zero;
			if (begin_with != null) first = begin_with.Handle;

			Window child = new Window(Interop.FindWindowEx(Handle, first, class_name, null));

			while (child.Handle != IntPtr.Zero)
			{
				Rectangle child_rect = child.Rect;

				if (screen_point.X >= child_rect.X && screen_point.X < child_rect.X + child_rect.Width &&
					screen_point.Y >= child_rect.Y && screen_point.Y < child_rect.Y + child_rect.Height)
				{
					break;
				}

				child = new Window(Interop.FindWindowEx(Handle, child.Handle, class_name, null));
			}

			if (child.Handle == IntPtr.Zero) return null;
			return child;
		}


		private static bool EnumCallback(IntPtr hwnd, int lParam)
		{
			string title = GetWindowText(hwnd);

			if (title.StartsWith(enum_find_string))
			{
				enum_found_windows.Add(new Window(hwnd));
				//Console.WriteLine(title);
			}

			return true;
		}

		public static string GetWindowText(IntPtr hwnd)
		{
			int length = Interop.GetWindowTextLength(hwnd);
			StringBuilder sb = new StringBuilder(length + 1);
			Interop.GetWindowText(hwnd, sb, sb.Capacity);

			return sb.ToString();
		}

		public Size Size
		{
			get
			{
				return Rect.Size;
			}
		}

		public static Rectangle GetWindowRect(IntPtr hwnd)
		{
			Interop.RECT tmp_rect = new Interop.RECT();
			Interop.GetWindowRect(hwnd, ref tmp_rect);
			Rectangle rect = new Rectangle(tmp_rect.left, tmp_rect.top,
				tmp_rect.right - tmp_rect.left, tmp_rect.bottom - tmp_rect.top);

			return rect;
		}

		public static Bitmap CaptureScreen(IntPtr hwnd, Rectangle rect)
		{
			Rectangle window_rect = Window.GetWindowRect(hwnd);

			Rectangle cap_rect = new Rectangle(
				window_rect.Left + rect.Left,
				window_rect.Top + rect.Top,
				rect.Width, rect.Height);

			Bitmap bmp = null;

			for (int i = 0; i < 50; i++)
			{
				try
				{
					bmp = new Bitmap(cap_rect.Width, cap_rect.Height);
					Graphics grb = Graphics.FromImage(bmp);
					grb.CopyFromScreen(cap_rect.Left, cap_rect.Top, 0, 0, bmp.Size);

					grb.Dispose();

					break;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);

                    Console.ReadLine();
				}

				Thread.Sleep(100);
			}

			return bmp;
		}

		public static Bitmap CaptureScreenNew(IntPtr hwnd)
		{
            Image img = null;
            try
            {
                // get te hDC of the target window
                IntPtr hdcSrc = Interop.GetWindowDC(hwnd);
                // get the size
                Interop.RECT windowRect = new Interop.RECT();
                Interop.GetWindowRect(hwnd, ref windowRect);
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;
                // create a device context we can copy to
                IntPtr hdcDest = Interop.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = Interop.CreateCompatibleBitmap(hdcSrc, width, height);
                // select the bitmap object
                IntPtr hOld = Interop.SelectObject(hdcDest, hBitmap);
                // bitblt over
                Interop.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, Interop.SRCCOPY);
                // restore selection
                Interop.SelectObject(hdcDest, hOld);
                // clean up
                Interop.DeleteDC(hdcDest);
                Interop.ReleaseDC(hwnd, hdcSrc);
                // get a .NET image object for it
                img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                Interop.DeleteObject(hBitmap);
            }
            catch (Exception e)
            {
                Console.WriteLine("LOL");
            }
			return (Bitmap)img;
		}

        public static IDictionary<IntPtr, string> GetOpenWindowsFromPID(int processID)
        {
            IntPtr hShellWindow = User32.GetShellWindow();
            Dictionary<IntPtr, string> dictWindows = new Dictionary<IntPtr, string>();

            User32.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == hShellWindow) return true;
                if (!User32.IsWindowVisible(hWnd)) return true;

                int length = User32.GetWindowTextLength(hWnd);
                if (length == 0) return true;

                uint windowPid;
                User32.GetWindowThreadProcessId(hWnd, out windowPid);
                if (windowPid != processID) return true;

                StringBuilder stringBuilder = new StringBuilder(length);
                User32.GetWindowText(hWnd, stringBuilder, length + 1);
                dictWindows.Add(hWnd, stringBuilder.ToString());
                return true;
            }, 0);

            return dictWindows;
        }

        public static Window[] GetOpenWindowsFromProcessID(int process_id, string title_begins)
        {
            IntPtr shell_window = User32.GetShellWindow();
            List<Window> windows = new List<Window>();

            User32.EnumWindows(delegate(IntPtr hwnd, int lparam)
            {
                if (hwnd == shell_window) return true;
                if (!User32.IsWindowVisible(hwnd)) return true;

                int length = User32.GetWindowTextLength(hwnd);
                if (length == 0) return true;

                uint window_pid;
                User32.GetWindowThreadProcessId(hwnd, out window_pid);
                if (window_pid != process_id) return true;

                StringBuilder sb = new StringBuilder(length);
                User32.GetWindowText(hwnd, sb, length + 1);
                if (!sb.ToString().StartsWith(title_begins)) return true;

                windows.Add(new Window(hwnd));

                return true;
            }, 0);

            return windows.ToArray();
        }

        public static Window GetOpenWindowFromProcessID(int process_id, string[] title_begins, out string matched_title)
        {
            IntPtr shell_window = User32.GetShellWindow();
            Window window = null;

            string title = "";

            User32.EnumWindows(delegate(IntPtr hwnd, int lparam)
            {
                if (hwnd == shell_window) return true;
                if (!User32.IsWindowVisible(hwnd)) return true;

                int length = User32.GetWindowTextLength(hwnd);
                if (length == 0) return true;

                uint window_pid;
                User32.GetWindowThreadProcessId(hwnd, out window_pid);
                if (window_pid != process_id) return true;

                StringBuilder sb = new StringBuilder(length);
                User32.GetWindowText(hwnd, sb, length + 1);

                string sb_s = sb.ToString();
                bool match = false;
                foreach (string title_begin in title_begins)
                    if (sb_s.StartsWith(title_begin))
                    {
                        match = true;
                        break;
                    }
                if (!match) return true;

                title = sb_s;
                window = new Window(hwnd);

                return false;
            }, 0);

            matched_title = title;

            return window;
        }

        public static List<Window> GetOpenWindowsFromProcessID(int process_id, string[] title_begins)
        {
            IntPtr shell_window = User32.GetShellWindow();
            List<Window> windows = new List<Window>();

            User32.EnumWindows(delegate(IntPtr hwnd, int lparam)
            {
                if (hwnd == shell_window) return true;
                if (!User32.IsWindowVisible(hwnd)) return true;

                int length = User32.GetWindowTextLength(hwnd);
                if (length == 0) return true;

                uint window_pid;
                User32.GetWindowThreadProcessId(hwnd, out window_pid);
                if (window_pid != process_id) return true;

                StringBuilder sb = new StringBuilder(length);
                User32.GetWindowText(hwnd, sb, length + 1);

                string sb_s = sb.ToString();
                bool match = false;
                foreach (string title_begin in title_begins)
                    if (sb_s.StartsWith(title_begin))
                    {
                        match = true;
                        break;
                    }
                if (!match) return true;

                windows.Add(new Window(hwnd));

                return true;
            }, 0);

            return windows;
        }
	}
}

