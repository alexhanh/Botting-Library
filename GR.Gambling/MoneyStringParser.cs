using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling
{
    /// <summary>
    /// Currently supported:
    /// </summary>
    public class MoneyStringParser
    {
        // $3
        // €3
        // 1,000
        // 1 000
        // 1 000.30
        public static int ToCents(string s)
        {
            s = s.Replace("USD", "");
            s = s.Replace("usd", "");
            s = s.Replace("EUR", "");
            s = s.Replace("eur", "");
			s = s.Replace("£", "");
            s = s.Replace("$", "");
            s = s.Replace("€", "");
            s = s.Replace(" ", "");
            s = s.Replace(",", "");

            bool decimal_point = (s.IndexOf(".") > -1);

            if (decimal_point)
            {
                //s = s.Replace(".", "");
                int cents = 0;
                string[] parts = s.Split(new char[] { '.' });
                if (parts[0].Length > 0 && parts[0][0] != '0')
                    cents = int.Parse(parts[0]) * 100;

                return cents + int.Parse(parts[1]);
            }
            
            return int.Parse(s) * 100;
        }

		public static int ToCents(string s, string thousands_seperator, string decimal_seperator)
		{
            s = s.Replace("USD", "");
            s = s.Replace("usd", "");
            s = s.Replace("EUR", "");
            s = s.Replace("eur", "");
			s = s.Replace("£", "");
			s = s.Replace("$", "");
			s = s.Replace("€", "");
			s = s.Replace(" ", "");
			s = s.Replace(thousands_seperator, "");

			bool decimal_point = (s.IndexOf(decimal_seperator) > -1);

			if (decimal_point)
			{
				//s = s.Replace(".", "");
				int cents = 0;
				string[] parts = s.Split(new string[] { decimal_seperator }, StringSplitOptions.None);
				if (parts[0].Length > 0 && parts[0][0] != '0')
					cents = int.Parse(parts[0]) * 100;

				return cents + int.Parse(parts[1]);
			}

			return int.Parse(s) * 100;
		}
    }
}
