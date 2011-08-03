using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GR.Imaging
{
	public class Screenshot
	{
		public static void Take(string filename)
		{
			Rectangle rect = Screen.GetBounds(Point.Empty);
			using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
			{
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					g.CopyFromScreen(Point.Empty, Point.Empty, rect.Size);
				}
				bitmap.Save(filename);
			}
		}

		#region Cursor Stuff
		private const int CURSOR_SHOWING = 1;

		[StructLayout(LayoutKind.Sequential)]
		private struct CURSORINFO
		{
			// Fields
			public int cbSize;
			public int flags;
			public IntPtr hCursor;
			public Point ptScreenPos;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct ICONINFO
		{
			// Fields
			public bool fIcon;
			public int xHotspot;
			public int yHotspot;
			public IntPtr hbmMask;
			//Handle of the icon’s bitmask bitmap.
			public IntPtr hbmColor;
			//Handle of the icon’s color bitmap. Optional for monochrome icons.
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			// Fields
			public int x;
			public int y;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr CopyIcon(IntPtr hIcon);

		[DllImport("gdi32.dll")]
		private static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("user32.dll")]
		private static extern bool DestroyIcon(IntPtr hIcon);

		[DllImport("user32.dll")]
		private static extern bool GetCursorInfo(ref CURSORINFO pci);

		[DllImport("user32.dll")]
		private static extern bool GetIconInfo(IntPtr hIcon, ref ICONINFO piconinfo);

		[DllImport("user32.dll")]
		private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);

		private static int GetGDIHandleCount()
		{
			return GetGuiResources(Process.GetCurrentProcess().Handle, 0);
		}

		private static int GetUserHandleCount()
		{
			return GetGuiResources(Process.GetCurrentProcess().Handle, 1);
		}

		private static void HandleMessage(string message)
		{
			Debug.WriteLine("HC: " + message + ": GDI: " + GetGDIHandleCount().ToString() + ": User: " + GetUserHandleCount().ToString());
		}
		#endregion

		public static Bitmap CaptureCursor(ref int x, ref int y)
		{
			//Return value initially nothing
			Bitmap bmp = null;

			CURSORINFO curInfo = new CURSORINFO();
			curInfo.cbSize = Marshal.SizeOf(curInfo);

			//HandleMessage("Start")

			if (GetCursorInfo(ref curInfo))
			{
				if (curInfo.flags == CURSOR_SHOWING)
				{
					IntPtr hicon = CopyIcon(curInfo.hCursor);

					if (hicon != IntPtr.Zero)
					{
						ICONINFO icoInfo = default(ICONINFO);
						if (GetIconInfo(hicon, ref icoInfo))
						{
							//Delete the mask, if present.
							if (icoInfo.hbmMask != IntPtr.Zero)
							{
								DeleteObject(icoInfo.hbmMask);
							}

							//Delete the color bitmap, if present.
							if (icoInfo.hbmColor != IntPtr.Zero)
							{
								DeleteObject(icoInfo.hbmColor);
							}

							x = curInfo.ptScreenPos.X - icoInfo.xHotspot;

							y = curInfo.ptScreenPos.Y - icoInfo.yHotspot;
						}

						Icon ic = Icon.FromHandle(hicon);
						
						bmp = ic.ToBitmap();

						//Must destroy the icon object we got from CopyIcon

						DestroyIcon(hicon);
					}
				}
			}

			//HandleMessage("End")


			return bmp;
		}
	}
}
