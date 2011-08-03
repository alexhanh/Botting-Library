using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GR.Common.Logging
{
    // TODO: Implement missing append, write and writeline methods.
    public class ConsoleCapturer : TextWriter
    {
        // Maintain an internal buffer in a StringBuilder object.
        private StringBuilder sb;

        public ConsoleCapturer(int max_capacity)
        {
            sb = new StringBuilder(max_capacity, max_capacity);
        }

        public void StartCapturing()
        {
            Console.SetError(this);
            Console.SetOut(this);
        }

        public override string ToString()
        {
            return sb.ToString();
        }

		public string Log { get { return sb.ToString(); } }

		public delegate void OnDataReceived(string s);
		public event OnDataReceived DataReceived;

        // TODO: The append crashes when s.Length > sb.MaxCapacity.
        private void Append(string s)
        {
			if (DataReceived != null)
				DataReceived(s);

            int space_needed = s.Length - (sb.MaxCapacity - sb.Length);

            // Make room for the string.
            if (space_needed > 0)
                sb.Remove(0, space_needed);

            sb.Append(s);
        }

        private void Append(char[] chars)
        {
			if (DataReceived != null)
				DataReceived(new string(chars));

            int space_needed = chars.Length - (sb.MaxCapacity - sb.Length);

            // Make room for the string.
            if (space_needed > 0)
                sb.Remove(0, space_needed);

            sb.Append(chars);
        }

        private void Append(char c)
        {
			if (DataReceived != null)
				DataReceived(new string(c, 1));

            if (sb.Length == sb.MaxCapacity)
                sb.Remove(0, 1);

            sb.Append(c);
        }

        /*private void Append(char[] buffer, int index, int count)
        {
              
        }*/

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Write(string line)
        {
            Append(line);
        }

        public override void Write(char value)
        {
            Append(value);
        }

        public override void Write(char[] buffer)
        {
            Append(buffer);
        }

        /*public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
        }*/

        public override void Write(string format, object arg0)
        {
            Append(string.Format(format, arg0));
        }

        public override void Write(string format, object arg0, object arg1)
        {
            Append(string.Format(format, arg0, arg1));
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            Append(string.Format(format, arg0, arg1, arg2));
        }

        public override void Write(string format, params object[] arg)
        {
            Append(string.Format(format, arg));
        }

        public override void WriteLine()
        {
            Append(Environment.NewLine);
        }

        public override void WriteLine(string line)
        {
            Append(line);
            WriteLine();
        }

        public override void WriteLine(char value)
        {
            Append(value);
            WriteLine();
        }

        public override void WriteLine(char[] buffer)
        {
            Append(buffer);
            WriteLine();
        }

        /*public override void WriteLine(char[] buffer, int index, int count)
        {
            base.WriteLine(buffer, index, count);
        }*/

        public override void WriteLine(string format, object arg0)
        {
            Write(format, arg0);
            WriteLine();
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            Write(format, arg0, arg1);
            WriteLine();
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Write(format, arg0, arg1, arg2);
            WriteLine();
        }

        public override void WriteLine(string format, params object[] arg)
        {
            Write(format, arg);
            WriteLine();
        }
    }
}
