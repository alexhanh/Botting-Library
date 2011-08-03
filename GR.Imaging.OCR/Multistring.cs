using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Imaging.OCR
{
    public class Multistring
    {
        List<List<char>> chars = new List<List<char>>();

        public int Length { get { return chars.Count; } }

        public List<char> this[int index] { get { return chars[index]; } }

        /// <summary>
        /// Returns if this multistring contains multiple possibilities for the same character.
        /// </summary>
        /// <returns></returns>
        public bool IsMulti()
        {
            for (int i = 0; i < this.Length; i++)
                if (this[i].Count > 1)
                    return true;
            return false;
        }

        public bool IsMulti(int index)
        {
            if (this[index].Count > 1)
                return true;
            return false;
        }

        public void RemoveChar(int index, char c)
        {
            chars[index].Remove(c);
        }

        public List<char> GetPossibleChars(int index)
        {
            return chars[index];
        }

        public void AddPossibleChar(int index, char c)
        {
            chars[index].Add(c);
        }

        public void Append(char c)
        {
            List<char> list = new List<char>();
            list.Add(c);

            chars.Add(list);
        }

        public void Append(List<char> possibilities)
        {
            chars.Add(possibilities);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i].Count == 1)
                {
                    sb.Append(chars[i][0]);
                }
                else
                {
                    sb.Append('[');
                    foreach (char c in chars[i]) sb.Append(c);
                    sb.Append(']');
                }
            }

            return sb.ToString();
        }
    }
}
