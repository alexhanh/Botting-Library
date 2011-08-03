using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace GR.Input
{
	public class ScarMath
	{
		public static double Hypotenuse(double a, double b)
		{
			return Math.Sqrt(a * a + b * b);
		}

		public static int LRound(double i)
		{
			return (int)(i + 0.5);
		}
	}

	public class ScarMouse
	{
		private static Random random = new Random();
		private static int mouseSpeed = 15; // Default value in SCAR is 15.

		public static Point GetMousePos()
		{
			// TODO: Might concider using interopping.. research what is better.
			return Cursor.Position;
		}

		public static void MMouse(int x, int y, int rx, int ry)
		{
			int cx, cy;
			Point currentPos = GetMousePos();
			cx = currentPos.X;
			cy = currentPos.Y;

			double randSpeed = (random.Next(mouseSpeed) / 2.0 + mouseSpeed) / 10.0;

			if (randSpeed <= 0.001)
				randSpeed = 0.1;

			WindMouse(cx, cy, x, y, 9.0, 3.0, 10.0 / randSpeed, 15.0 / randSpeed, 10.0 * randSpeed, 10.0 * randSpeed);
		}

		public static void MoveMouse(int x, int y)
		{
			mouseCapture.SetPixel(x, y, Color.Red);
			Cursor.Position = new Point(x, y);
		}

		public static void WindMouse(double xs, double ys, double xe, double ye,
									 double gravity, double wind, double minWait, double maxWait, double maxStep, double targetArea)
		{
			double veloX = 0.0, veloY = 0.0, windX = 0.0, windY = 0.0, veloMag = 0.0, dist = 0.0, randomDist = 0.0, lastDist = 0.0, step = 0.0;
			int lastX = 0, lastY = 0;
			double sqrt2, sqrt3, sqrt5;

			sqrt2 = Math.Sqrt(2);
			sqrt3 = Math.Sqrt(3);
			sqrt5 = Math.Sqrt(5);

			while ((dist = ScarMath.Hypotenuse(xs - xe, ys - ye)) > 1.0)
			{
				wind = Math.Min(wind, dist);

				if (dist >= targetArea)
				{
					windX = windX / sqrt3 + (random.Next((int)Math.Round(wind) * 2 + 1) - wind) / sqrt5;
					windY = windY / sqrt3 + (random.Next((int)Math.Round(wind) * 2 + 1) - wind) / sqrt5;
				}
				else
				{
					windX = windX / sqrt2;
					windY = windY / sqrt2;

					if (maxStep < 3.0)
						maxStep = random.Next(3) + 3.0;
					else
						maxStep = maxStep / sqrt5;
				}

				veloX = veloX + windX;
				veloY = veloY + windY;
				veloX = veloX + gravity * (xe - xs) / dist;
				veloY = veloY + gravity * (ye - ys) / dist;

				if (ScarMath.Hypotenuse(veloX, veloY) > maxStep)
				{
					randomDist = maxStep / 2.0 + random.Next((int)Math.Round(maxStep) / 2);
					veloMag = Math.Sqrt(veloX * veloX + veloY * veloY);
					veloX = (veloX / veloMag) * randomDist;
					veloY = (veloY / veloMag) * randomDist;
				}

				lastX = (int)Math.Round(xs);
				lastY = (int)Math.Round(ys);
				xs = xs + veloX;
				ys = ys + veloY;

				int roundxs = (int)Math.Round(xs);
				int roundys = (int)Math.Round(ys);

				if (lastX != roundxs || lastY != roundys)
				{
					MoveMouse(roundxs, roundys);
				}

				step = ScarMath.Hypotenuse(xs - lastX, ys - lastY);
				Thread.Sleep((int)Math.Round((maxWait - minWait) * (step / maxStep) + minWait));
				lastDist = dist;
			}

			if ((int)Math.Round(xe) != (int)Math.Round(xs) || (int)Math.Round(ye) != (int)Math.Round(ys))
				MoveMouse((int)Math.Round(xe), (int)Math.Round(ye));
		}

		public static void MoveMouseSmoothEx(int x, int y, int minsleeptime, int maxsleeptime,
											 int maxdistance, int gravity, int forces)
		{
			int currX = 0, currY = 0;
			int newX = 0, newY = 0;
			double totalDist = 0, force = 0, dist = 0, velocity = 0, angle;
			double windAngle = 0;
			double wind = 0;
			int loops = 0;

			Point currentPos = GetMousePos();
			currX = currentPos.X;
			currY = currentPos.Y;

			velocity = 6; // Start moving the mouse, mostly to keep wind from being to strong
			while ((currX != x) || (currY != y))
			{
				++loops;
				// pow was overkill for squaring integers
				totalDist = Math.Sqrt((double)((x - currX) * (x - currX)) + ((y - currY) * (y - currY)));
				angle = Math.Atan2((double)currY - y, (double)currX - x);

				if (maxdistance > totalDist)  // Make sure we don't go too far.
					maxdistance = ScarMath.LRound(totalDist);

				if (totalDist < (forces / 1.5)) // Can we overcome the wind?
					MoveMouse(x, y);
				else
				{
					force = (gravity * 100) / (totalDist * totalDist); // Make gravity a bit stronger
					dist = velocity;
					velocity += force * loops;
					if (dist > maxdistance)
					{
						dist = maxdistance - (random.Next(maxdistance) / 2);
						gravity = 0; // don't increase velocity when we reach max speed.
					}
					windAngle = (random.Next(360)) / 180 * 3.142;  // Wheres the wind blowing?
					wind = random.Next(forces) * 0.5; // How hard will it blow?

					if (forces > totalDist)
						forces = ScarMath.LRound(totalDist);

					newX = ScarMath.LRound(currX - (Math.Cos(angle) * dist) + (Math.Cos(windAngle) * wind));
					newY = ScarMath.LRound(currY - (Math.Sin(angle) * dist) + (Math.Cos(windAngle) * wind));
					MoveMouse(newX, newY);  // Looks like we made it.
				}
				// GetMousePos is after the sleep to ensure fluid motion in case the user 
				// moves their mouse.
				Thread.Sleep(10 + minsleeptime + (random.Next(maxsleeptime - minsleeptime)));
				Point newPoint = GetMousePos(); // Let me know if I should exit the loop
				currX = newPoint.X;
				currY = newPoint.Y;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public long x;
			public long y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public long Left;
			public long Top;
			public long Right;
			public long Bottom;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetCursorPos(out POINT lpPoint);

		private static long Rand(long low, long high)
		{
			return (int)((high - low + 1) * random.NextDouble()) + low;
		}

		public static void MoveMouseCC(IntPtr hwnd, int x, int y, int range, int segments, int pcount, int delay)
		{
			double u = 0.0, nc1 = 0.0, nc2 = 0.0, nc3 = 0.0, nc4 = 0.0;
			RECT r = new RECT();
			int i = 0;
			POINT dummyp;

			POINT[] p2 = new POINT[0];
			POINT[] p1 = new POINT[pcount + 2];
			
			if (segments < 15)
				segments = 50;

			segments = segments + random.Next(-12, 13);

			GetCursorPos(out dummyp);

			GetWindowRect(hwnd, ref r);

			x = x + random.Next(-range, range + 1) + (int)r.Left;
			y = y + random.Next(-range, range + 1) + (int)r.Top;

			if (pcount < 3)
				pcount = 3;

			if ((Math.Abs(dummyp.x - x) + Math.Abs(dummyp.y - y)) < 150)
			{
				pcount = 3;
				segments = 15;
			}

			for (i = 0; i < p1.Length; i++)
			{
				if (i < 2)
					p1[i] = dummyp;
				else if (i >= pcount)
				{
					p1[i].x = x;
					p1[i].y = y;
				}
				else
				{
					p1[i].x = Math.Min(dummyp.x, x) + (int)(random.NextDouble() * Math.Abs(dummyp.x - x));
					p1[i].y = Math.Min(dummyp.y, y) + (int)(random.NextDouble() * Math.Abs(dummyp.y - y));
				}
			}

			for (i = 1; i < pcount; i++)
			{
				double inc = 1.0 / (double)segments;
				for (u = 0.0; u <= 1; u += inc)
				{
					double u3 = u * u * u;
					double u2 = u * u;
					nc1 = -u3 / 6.0 + u2 / 2.0 - u / 2.0 + 1.0 / 6.0;
					nc2 = u3 / 2.0 - u2 + 2.0 / 3.0;
					nc3 = (-u3 + u2 + u) / 2.0 + 1.0 / 6.0;
					nc4 = (u3) / 6.0;
					dummyp.x = (int)(nc1 * p1[i - 1].x + nc2 * p1[i].x + nc3 * p1[i + 1].x + nc4 * p1[i + 2].x);
					dummyp.y = (int)(nc1 * p1[i - 1].y + nc2 * p1[i].y + nc3 * p1[i + 1].y + nc4 * p1[i + 2].y);

					Array.Resize(ref p2, p2.Length + 1);
					p2[p2.Length - 1] = dummyp;
				}
			}

			for (i = 0; i < p2.Length; i++)
			{
				MoveMouse((int)p2[i].x, (int)p2[i].y);

				Thread.Sleep(delay);
			}
		}

		private static Bitmap mouseCapture = new Bitmap(1200, 1200);

		public static void StartTest()
		{
			Graphics g = Graphics.FromImage(mouseCapture);
			g.Clear(Color.White);
		}

		public static void EndTest()
		{
			mouseCapture.Save("mousepath.bmp");
		}
	}
}
