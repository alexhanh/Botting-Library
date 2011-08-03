using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using GR.Win32;

namespace GR.Input
{
    // http://msdn.microsoft.com/en-us/library/ms927178.aspx
    // http://www.pinvoke.net/default.aspx/user32.SendInput
    // http://terpconnect.umd.edu/~nsw/ench250/scancode.htm
    public enum KeyboardKey : ushort
    {
        A = 0x001E,
        B = 0x0030,
        C = 0x002E,
        D = 0x0020,
        E = 0x0012,
        F = 0x0021,
        G = 0x0022,
        H = 0x0023,
        I = 0x0017,
        J = 0x0024,
        K = 0x0025,
        L = 0x0026,
        M = 0x0032,
        N = 0x0031,
        O = 0x0018,
        P = 0x0019,
        Q = 0x0010,
        R = 0x0013,
        S = 0x001F,
        T = 0x0014,
        U = 0x0016,
        V = 0x002F,
        W = 0x0011,
        X = 0x002D,
        Y = 0x0015,
        Z = 0x002C,
        N1 = 0x0002,
        N2 = 0x0003,
        N3 = 0x0004,
        N4 = 0x0005,
        N5 = 0x0006,
        N6 = 0x0007,
        N7 = 0x0008,
        N8 = 0x0009,
        N9 = 0x000A,
        N0 = 0x000B,
        // Are these all 'extended'? prefixed with 0x8000 for convinience
        Period = 0x80BE,
        Decimal = 0x806E,
        Shift = 0x8010,
        Control = 0x8011,
        Escape = 0x801B,
        Space = 0x8020,
        End = 0x8023,
        Home = 0x8024,
        Left = 0x8025,
        Up = 0x8026,
        Right = 0x8027,
        Down = 0x8028,
        Insert = 0x802D,
        Delete = 0x802E,
        Enter = 0x800D,
        Tab = 0x8009,
        Semicolon = 0x8027,
        LeftWindows = 0x805B
    }

    public class Keyboard
    {
        private static bool[] is_down = new bool[0x9000];

        private static Dictionary<int, KeyboardKey> number2key = new Dictionary<int, KeyboardKey>
        {
            {0, KeyboardKey.N0},
            {1, KeyboardKey.N1},
            {2, KeyboardKey.N2},
            {3, KeyboardKey.N3},
            {4, KeyboardKey.N4},
            {5, KeyboardKey.N5},
            {6, KeyboardKey.N6},
            {7, KeyboardKey.N7},
            {8, KeyboardKey.N8},
            {9, KeyboardKey.N9}
        };

        private static Dictionary<char, KeyboardKey> char2key = new Dictionary<char, KeyboardKey>
        {
            { 'a', KeyboardKey.A },
            { 'b', KeyboardKey.B },
            { 'c', KeyboardKey.C },
            { 'd', KeyboardKey.D },
            { 'e', KeyboardKey.E },
            { 'f', KeyboardKey.F },
            { 'g', KeyboardKey.G },
            { 'h', KeyboardKey.H },
            { 'i', KeyboardKey.I },
            { 'j', KeyboardKey.J },
            { 'k', KeyboardKey.K },
            { 'l', KeyboardKey.L },
            { 'm', KeyboardKey.M },
            { 'n', KeyboardKey.N },
            { 'o', KeyboardKey.O },
            { 'p', KeyboardKey.P },
            { 'q', KeyboardKey.Q },
            { 'r', KeyboardKey.R },
            { 's', KeyboardKey.S },
            { 't', KeyboardKey.T },
            { 'u', KeyboardKey.U },
            { 'v', KeyboardKey.V },
            { 'w', KeyboardKey.W },
            { 'x', KeyboardKey.X },
            { 'y', KeyboardKey.Y },
            { 'z', KeyboardKey.Z },
            { '0', KeyboardKey.N0 },
            { '1', KeyboardKey.N1 },
            { '2', KeyboardKey.N2 },
            { '3', KeyboardKey.N3 },
            { '4', KeyboardKey.N4 },
            { '5', KeyboardKey.N5 },
            { '6', KeyboardKey.N6 },
            { '7', KeyboardKey.N7 },
            { '8', KeyboardKey.N8 },
            { '9', KeyboardKey.N9 },
            { ' ', KeyboardKey.Space },
            { '.', KeyboardKey.Period },
            { ',', KeyboardKey.Decimal }
        };

        // This is *ASSUMING* english locale key mapping.
        private static Dictionary<char, List<KeyboardKey>> chars2key = new Dictionary<char, List<KeyboardKey>>
        {
            // Upper-case
            { 'A', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.Shift, KeyboardKey.A } },
            { 'B', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.B } },
            { 'C', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.C } },
            { 'D', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.D } },
            { 'E', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.E } },
            { 'F', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.F } },
            { 'G', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.G } },
            { 'H', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.H } },
            { 'I', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.I } },
            { 'J', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.J } },
            { 'K', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.K } },
            { 'L', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.L } },
            { 'M', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.M } },
            { 'N', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N } },
            { 'O', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.O } },
            { 'P', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.P } },
            { 'Q', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.Q } },
            { 'R', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.R } },
            { 'S', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.S } },
            { 'T', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.T } },
            { 'U', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.U } },
            { 'V', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.V } },
            { 'W', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.W } },
            { 'X', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.X } },
            { 'Y', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.Y } },
            { 'Z', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.Z } },

            // Lower-case
            { 'a', new List<KeyboardKey> { KeyboardKey.A } },
            { 'b', new List<KeyboardKey> { KeyboardKey.B } },
            { 'c', new List<KeyboardKey> { KeyboardKey.C } },
            { 'd', new List<KeyboardKey> { KeyboardKey.D } },
            { 'e', new List<KeyboardKey> { KeyboardKey.E } },
            { 'f', new List<KeyboardKey> { KeyboardKey.F } },
            { 'g', new List<KeyboardKey> { KeyboardKey.G } },
            { 'h', new List<KeyboardKey> { KeyboardKey.H } },
            { 'i', new List<KeyboardKey> { KeyboardKey.I } },
            { 'j', new List<KeyboardKey> { KeyboardKey.J } },
            { 'k', new List<KeyboardKey> { KeyboardKey.K } },
            { 'l', new List<KeyboardKey> { KeyboardKey.L } },
            { 'm', new List<KeyboardKey> { KeyboardKey.M } },
            { 'n', new List<KeyboardKey> { KeyboardKey.N } },
            { 'o', new List<KeyboardKey> { KeyboardKey.O } },
            { 'p', new List<KeyboardKey> { KeyboardKey.P } },
            { 'q', new List<KeyboardKey> { KeyboardKey.Q } },
            { 'r', new List<KeyboardKey> { KeyboardKey.R } },
            { 's', new List<KeyboardKey> { KeyboardKey.S } },
            { 't', new List<KeyboardKey> { KeyboardKey.T } },
            { 'u', new List<KeyboardKey> { KeyboardKey.U } },
            { 'v', new List<KeyboardKey> { KeyboardKey.V } },
            { 'w', new List<KeyboardKey> { KeyboardKey.W } },
            { 'x', new List<KeyboardKey> { KeyboardKey.X } },
            { 'y', new List<KeyboardKey> { KeyboardKey.Y } },
            { 'z', new List<KeyboardKey> { KeyboardKey.Z } },

            // Numbers
            { '0', new List<KeyboardKey> { KeyboardKey.N0 } },
            { '1', new List<KeyboardKey> { KeyboardKey.N1 } },
            { '2', new List<KeyboardKey> { KeyboardKey.N2 } },
            { '3', new List<KeyboardKey> { KeyboardKey.N3 } },
            { '4', new List<KeyboardKey> { KeyboardKey.N4 } },
            { '5', new List<KeyboardKey> { KeyboardKey.N5 } },
            { '6', new List<KeyboardKey> { KeyboardKey.N6 } },
            { '7', new List<KeyboardKey> { KeyboardKey.N7 } },
            { '8', new List<KeyboardKey> { KeyboardKey.N8 } },
            { '9', new List<KeyboardKey> { KeyboardKey.N9 } },

            // Special characters
            { '!', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N1 } },
            { '@', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N2 } },
            { '#', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N3 } },
            { '$', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N4 } },
            { '%', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N5 } },
            { '^', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N6 } },
            { '&', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N7 } },
            { '*', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N8 } },
            { '(', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N9 } },
            { ')', new List<KeyboardKey> { KeyboardKey.Shift, KeyboardKey.N0 } },
            { ' ', new List<KeyboardKey> { KeyboardKey.Space } },
            { '.', new List<KeyboardKey> { KeyboardKey.Period } },
            { ',', new List<KeyboardKey> { KeyboardKey.Decimal } },
            { ';', new List<KeyboardKey> { KeyboardKey.Semicolon } }
        };

        static public bool IsDown(KeyboardKey key)
        {
            return is_down[(ushort)key];
        }

        static public void KeyDown(KeyboardKey key)
        {
            SetKey(key, true);
        }

        static public void KeyUp(KeyboardKey key)
        {
            SetKey(key, false);
        }

        static public void PressKey(KeyboardKey key)
        {
            SetKey(key, true);
            Thread.Sleep(50);
            SetKey(key, false);
        }

        static public void PressNumberKey(int number)
        {
            if (number < 10 && number >= 0)
            {
                PressKey(number2key[number]);
            }
        }

        static private void SetKey(KeyboardKey key, bool down)
        {
            if (is_down[(ushort)key] == down)
                return;

            is_down[(ushort)key] = down;

            User32.INPUT[] input_data = new User32.INPUT[1];
            input_data[0] = new User32.INPUT();

            input_data[0].type = (int)User32.InputType.INPUT_KEYBOARD;


            input_data[0].ki.dwFlags = 0;
            if ((int)key > 0x8000)
            {
                input_data[0].ki.wVk = (ushort)(key - 0x8000);
                // input_data[0].ki.dwFlags |= (uint)User32.SendInputFlags.KEYEVENTF_EXTENDEDKEY;
            }
            else
            {
                input_data[0].ki.wScan = (ushort)key;
                input_data[0].ki.dwFlags |= (uint)User32.SendInputFlags.KEYEVENTF_SCANCODE;
            }

            if (!down)
                input_data[0].ki.dwFlags |= (uint)User32.SendInputFlags.KEYEVENTF_KEYUP;

            uint result = User32.SendInput(1, input_data, Marshal.SizeOf(input_data[0]));

            if (result != 1)
                Console.WriteLine("SendInput result != 1 {=" + result + "}");
        }

        private static Stack<KeyboardKey> pressed_stack = new Stack<KeyboardKey>();
        static public void Write(string text)
        {
            Random random = new Random();

            foreach (char c in text)
            {
                foreach (KeyboardKey kb_key in chars2key[c])
                {
                    Keyboard.KeyDown(kb_key);

                    pressed_stack.Push(kb_key);

                    Thread.Sleep(random.Next(25, 75));
                }

                while (pressed_stack.Count > 0)
                {
                    KeyboardKey kb_key = pressed_stack.Pop();

                    Keyboard.KeyUp(kb_key);

                    Thread.Sleep(random.Next(20, 50));
                }
            }
        }
    }
}
