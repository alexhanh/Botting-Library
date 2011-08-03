using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;

namespace GR.Win32
{
    public class DOM
    {
        private static int EnumWindows(IntPtr hWnd, ref IntPtr lParam)
        {
            int retVal = 1;
            StringBuilder classname = new StringBuilder(128);
            Win32.GetClassName(hWnd, classname, classname.Capacity);
            /// check if the instance we have found is Internet Explorer_Server
            if ((bool)(string.Compare(classname.ToString(), "Internet Explorer_Server") == 0))
            {
                lParam = hWnd;
                retVal = 0;
            }
            return retVal;
        }

        public static IHTMLDocument2 DocumentFromDOM(IntPtr hWnd)
        {
            IHTMLDocument2 document = null;

            int lngMsg = 0;
            int lRes;

            Win32.EnumProc proc = new Win32.EnumProc(DOM.EnumWindows);

            Win32.EnumChildWindows(hWnd, proc, ref hWnd);
            if (!hWnd.Equals(IntPtr.Zero))
            {
                lngMsg = Win32.RegisterWindowMessage("WM_HTML_GETOBJECT");
                if (lngMsg != 0)
                {
                    Win32.SendMessageTimeout(hWnd, lngMsg, 0, 0, Win32.SMTO_ABORTIFHUNG, 1000, out lRes);
                    if (!(bool)(lRes == 0))
                    {
                        int hr = Win32.ObjectFromLresult(lRes, ref Win32.IID_IHTMLDocument2, 0, ref document);
                        if ((bool)(document == null))
                        {
                            //MessageBox.Show("No IHTMLDocument Found!", "Warning");
                            Console.WriteLine("No IHTMLDocument Found!");
                        }
                    }
                }
            }
            return document;
        }
    }

    class Win32
    {
        [DllImport("user32.dll", EntryPoint = "GetClassNameA")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

        /*delegate to handle EnumChildWindows*/
        public delegate int EnumProc(IntPtr hWnd, ref IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumProc lpEnumFunc, ref  IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA")]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutA")]
        public static extern int SendMessageTimeout(IntPtr hwnd, int msg, int wParam, int lParam, int fuFlags, int uTimeout, out int lpdwResult);



        public const int SMTO_ABORTIFHUNG = 0x2;

        public static Guid IID_IHTMLDocument2 = typeof(IHTMLDocument2).GUID;

        [DllImport("OLEACC.dll")]
        public static extern int ObjectFromLresult(int lResult, ref Guid riid, int wParam, ref IHTMLDocument2 ppvObject);
    }
}
