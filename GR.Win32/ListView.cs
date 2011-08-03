using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GR.Win32
{
    public class ListView : Window
    {
        ExternalProcess window_process;

        public ListView(IntPtr handle)
            : base(handle)
        {
            window_process = ExternalProcess.FromWindow(this);
        }

        public int ItemCount
        {
            get { return Interop.SendMessage(Handle, (uint)Messages.LVM_GETITEMCOUNT, 0, null); }
        }

        public string GetItem(int row, int column)
        {
            if (row >= ItemCount) return "";

            LVITEM item = new LVITEM();

            byte[] buffer = new byte[100];

            IntPtr external_buffer = window_process.AllocateMemory(buffer.Length);
            item.pszText = external_buffer;

            item.iItem = row;
            item.iSubItem = column;
            item.mask = LVIF_TEXT;
            item.cchTextMax = buffer.Length;

            unsafe
            {
                IntPtr item_pointer = new IntPtr((void*)&item);

                IntPtr external_item = window_process.AllocateMemory(Marshal.SizeOf(item));
                window_process.Write(item_pointer, external_item, Marshal.SizeOf(item));

                Interop.SendMessage(Handle, (uint)Messages.LVM_GETITEMA, IntPtr.Zero, external_item);

                window_process.Read(external_buffer, buffer, buffer.Length);

                window_process.FreeMemory(external_buffer);
                window_process.FreeMemory(external_item);
            }

            string text = new System.Text.ASCIIEncoding().GetString(buffer);

            return text.Substring(0, text.IndexOf((char)0));
        }

        public void SelectItem(int row)
        {
            if (row >= ItemCount) return;

            LVITEM item = new LVITEM();

            item.state = LVIS_SELECTED | LVIS_FOCUSED;
            item.stateMask = LVIS_SELECTED | LVIS_FOCUSED;
            item.mask = LVIF_STATE;

            Interop.SendMessage(Handle, (uint)Messages.LVM_ENSUREVISIBLE, new IntPtr(row), new IntPtr(0));

            unsafe
            {
                IntPtr item_pointer = new IntPtr((void*)&item);

                IntPtr external_item = window_process.AllocateMemory(Marshal.SizeOf(item));
                window_process.Write(item_pointer, external_item, Marshal.SizeOf(item));

                Interop.SendMessage(Handle, (uint)Messages.LVM_SETITEMSTATE, new IntPtr(row), external_item);

                window_process.FreeMemory(external_item);
            }
        }

        const int LVIF_TEXT = 0x0001;
        const int LVIS_FOCUSED = 0x0001;
        const int LVIS_SELECTED = 0x0002;
        const int LVIF_STATE = 0x0008;
        const int LVM_FIRST = 0x1000;

        enum Messages
        {
            LVM_GETITEMCOUNT = LVM_FIRST + 4,
            LVM_GETITEMA = LVM_FIRST + 5,
            LVM_GETITEMW = LVM_FIRST + 75,
            LVM_ENSUREVISIBLE = LVM_FIRST + 19,
            LVM_SETITEMSTATE = LVM_FIRST + 43
        }


        [StructLayout(LayoutKind.Sequential)]
        struct LVITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public uint lParam;
            public int iIndent;
        };
    }
}
