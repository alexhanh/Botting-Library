using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GR.Cryptography
{
    public class MD5
    {
        public static string Checksum(Stream inputStream)
        {
                byte[] hash = System.Security.Cryptography.MD5.Create().ComputeHash(inputStream);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
        }
    }
}
