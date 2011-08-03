using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GR.Win32
{
    public class Interop
    {
        #region Windowing

        [DllImport("user32.dll")]
        public static extern bool CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		public enum Messages
		{
			WM_GETTEXT = 0x000D,
			WM_GETTEXTLENGTH = 0x000E,
			GW_HWNDFIRST = 0,        
			GW_HWNDLAST = 1,   
			GW_HWNDNEXT = 2,    
			GW_HWNDPREV = 3,    
			GW_OWNER = 4,    
			GW_CHILD = 5,       
			GW_MAX = 5       
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, [Out] StringBuilder lpString);

		[DllImport("user32.dll")]
		public static extern int GetWindowRect(IntPtr hwnd, ref RECT rc);

		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);
		
		public delegate bool EnumWindowProc(IntPtr hwnd, int lParam);
		[DllImport("user32.dll")]
		public static extern int EnumWindows(EnumWindowProc callPtr, int lPar);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool OpenIcon(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
		   int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll", EntryPoint="GetWindow")]
		public static extern IntPtr GetNextWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int wFlag);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();
		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);
		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);
		[DllImport("user32.dll")]
		public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

		public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
			int nWidth, int nHeight, IntPtr hObjectSource,
			int nXSrc, int nYSrc, int dwRop);
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
			int nHeight);
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		public static extern bool DeleteDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		public enum ClipBoxMessage
		{
			ERROR = 0,
			NULLREGION = 1,
			SIMPLEREGION = 2,
			COMPLEXREGION = 3
		}
		[DllImport("gdi32.dll")]
		public static extern int GetClipBox(IntPtr hdc, out RECT lprc);
    }
}
