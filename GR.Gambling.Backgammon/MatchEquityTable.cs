using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace GR.Gambling.Backgammon.Tools
{
	// http://lists.gnu.org/archive/html/bug-gnubg/2006-07/msg00108.html
	// http://page.mi.fu-berlin.de/tkoch/Backgammon/

    //			MatchEquityTable.CreateMoneyGameMET("test.xml", 1.0, 1.0, true);
	public class MatchEquityTable
	{
		private static XmlElement CreateMETRow(XmlDocument xmldoc, double[] values)
		{
			NumberFormatInfo ni = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone();
			ni.NumberDecimalSeparator = ".";

			XmlElement row = xmldoc.CreateElement("row");

			for (int i = 0; i < values.Length; i++)
			{
				XmlElement me = xmldoc.CreateElement("me");
				me.AppendChild(xmldoc.CreateTextNode(values[i].ToString("0.0#######", ni)));
				row.AppendChild(me);
			}

			return row;
		}

		private static double METValue(double stake, double limit, bool rake, int points)
		{
			double pot = stake * points;
			if (pot > limit) pot = limit;
			if (rake)
				return 0.5 + 0.5 * (pot - 0.05 * pot) / limit;
			else
				return 0.5 - 0.5 * pot / limit; 
		}

        private static double MetValue(int points, int max_points)//(int stake, int limit, int points, int max_points)
        {
            return 0.5 + (double)points / (double)max_points / 2.0;
            //return 0.5 + (double)Math.Min(stake * points, limit) / (double)limit / 2.0;
        }

		private static double MetValue(int stake, int limit, int points, int max_points)
		{
			//return 0.5 + (double)points / (double)max_points / 2.0;
			return 0.5 + (double)Math.Min(stake * points, limit) / (double)limit / 2.0;
		}

        public static void Test(string filepath, int stake, int limit)
        {
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.AppendChild(xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));

            xmldoc.AppendChild(xmldoc.CreateDocumentType("met", "-//GNU Backgammon//DTD Match Equity Tables//EN", "met.dtd", null));

            XmlElement root = xmldoc.CreateElement("met");

            xmldoc.AppendChild(root);

            XmlElement info = xmldoc.CreateElement("info");

            info.AppendChild(xmldoc.CreateElement("name"));
            info.LastChild.AppendChild(xmldoc.CreateTextNode("GRLib"));
            info.AppendChild(xmldoc.CreateElement("description"));
            info.LastChild.AppendChild(xmldoc.CreateTextNode("Stake: " + stake + " Limit: " + limit + " (in cents) rakeless table stakes MET."));
            info.AppendChild(xmldoc.CreateElement("length"));
            info.LastChild.AppendChild(xmldoc.CreateTextNode(""));//max_points.ToString()));

            root.AppendChild(info);

            // pre-crawford
            XmlElement pre = xmldoc.CreateElement("pre-crawford-table");
            pre.SetAttribute("type", "explicit");

            root.AppendChild(pre);

            // post-crawford
            XmlElement post = xmldoc.CreateElement("post-crawford-table");
            post.SetAttribute("type", "explicit");
            post.SetAttribute("player", "both");

            //double[] values = new double[max_points];
            //post.AppendChild(CreateMETRow(xml, values));

            root.AppendChild(post);

            xmldoc.Save(filepath);
        }

        public static void CreateRakelessMet(string filename, int stake, int limit)
        {
            int max_points = (int)(limit / stake);
            if (max_points * stake < limit) 
                max_points++;
            if (max_points < 1)
                max_points = 1;

            XmlDocument xml = new XmlDocument();

            xml.AppendChild(xml.CreateNode(XmlNodeType.XmlDeclaration, "", ""));

            xml.AppendChild(xml.CreateDocumentType("met", "-//GNU Backgammon//DTD Match Equity Tables//EN", "met.dtd", null));

            XmlElement root = xml.CreateElement("met");
            xml.AppendChild(root);

            XmlElement info_element = xml.CreateElement("info");
            info_element.AppendChild(xml.CreateElement("name"));
            info_element.LastChild.AppendChild(xml.CreateTextNode("GRLib"));
            info_element.AppendChild(xml.CreateElement("description"));
            info_element.LastChild.AppendChild(xml.CreateTextNode("Stake: " + stake + " Limit: " + limit + " (in cents) rakeless table stakes MET."));
            info_element.AppendChild(xml.CreateElement("length"));
            info_element.LastChild.AppendChild(xml.CreateTextNode(max_points.ToString()));

            root.AppendChild(info_element);

            // pre-crawford
            XmlElement pre = xml.CreateElement("pre-crawford-table");
            pre.SetAttribute("type", "explicit");

			// post-crawford
			XmlElement post = xml.CreateElement("post-crawford-table");
			post.SetAttribute("type", "explicit");
			post.SetAttribute("player", "both");

			double[] hero_values = new double[max_points];
			int start_points = 0;
			for (int max = max_points; max > 0; max--)
			{
				for (int points = start_points, counter = 0; counter < max_points; counter++, points++)
				{
					hero_values[counter] = MetValue(stake, limit, points, max_points);//MetValue(points, max_points);
				}

				if (max == max_points)
					post.AppendChild(CreateMETRow(xml, hero_values));

				pre.AppendChild(CreateMETRow(xml, hero_values));

				start_points--;
			}

            root.AppendChild(pre);

            root.AppendChild(post);

            xml.Save(filename);
        }

		public static int CreateMoneyGameMET(string filename, double stake, double limit, bool rake)
		{
			int max_points = (int)(limit / stake);//Game.MaxPoints(stake, limit);
			if (max_points * stake < limit) max_points++;
			if (max_points < 1) max_points = 1;

			try
			{

				XmlDocument xmldoc = new XmlDocument();
				xmldoc.AppendChild(xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));

				xmldoc.AppendChild(xmldoc.CreateDocumentType("met", "-//GNU Backgammon//DTD Match Equity Tables//EN",
					 "met\\" + "met.dtd", null));

				//root element
				XmlElement root = xmldoc.CreateElement("met");
				xmldoc.AppendChild(root);

				//info element	
				XmlElement info = xmldoc.CreateElement("info");

				info.AppendChild(xmldoc.CreateElement("name"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode("Munkittajat ry"));
				info.AppendChild(xmldoc.CreateElement("description"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode("How to own the PartyGammon cash games"));
				info.AppendChild(xmldoc.CreateElement("length"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode(max_points.ToString()));

				root.AppendChild(info);



				// pre-crawford

				XmlElement pre = xmldoc.CreateElement("pre-crawford-table");
				pre.SetAttribute("type", "explicit");


				// hero win
				double[] hero_values = new double[max_points];
				//	for (int i = 0; i < hero_values.Length; i++) hero_values[i] = -1000;

				for (int i = max_points - 1; i >= 1; i--)
				{
					hero_values[hero_values.Length - 1] = METValue(stake, limit, true, i);

					pre.AppendChild(CreateMETRow(xmldoc, hero_values));
				}

				// opponent win
				double[] opponent_values = new double[max_points];
				//	for (int i = 0; i < opponent_values.Length; i++) opponent_values[i] = -1000;

				for (int i = 0; i < max_points; i++)
				{
					opponent_values[i] = METValue(stake, limit, false, max_points - 1 - i);
				}

				pre.AppendChild(CreateMETRow(xmldoc, opponent_values));


				root.AppendChild(pre);



				// post-crawford

				XmlElement post = xmldoc.CreateElement("post-crawford-table");
				post.SetAttribute("type", "explicit");
				post.SetAttribute("player", "both");

				double[] values = new double[max_points];
				//	for (int i=0; i<values.Length; i++) values[i]=-1000;
				post.AppendChild(CreateMETRow(xmldoc, values));

				root.AppendChild(post);




				xmldoc.Save("met//" + filename);

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			return max_points;
		}
	}

	/*class MatchEquityTable
	{
		private XmlElement CreateMETRow(XmlDocument xmldoc, double[] values)
		{
			NumberFormatInfo ni = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone();
			ni.NumberDecimalSeparator = ".";

			XmlElement row = xmldoc.CreateElement("row");

			for (int i = 0; i < values.Length; i++)
			{
				XmlElement me = xmldoc.CreateElement("me");
				me.AppendChild(xmldoc.CreateTextNode(values[i].ToString("0.00#######", ni)));
				row.AppendChild(me);
			}

			return row;
		}

		private double METValue(double stake, double limit, bool rake, Player winner, int points)
		{
			return 0.5 + 0.5 * game.NetWin(stake, limit, rake, winner, points) / limit;
		}

		public static int CreateMoneyGameMET(string filename, double stake, double limit, bool rake)
		{
			int max_points = Game.MaxPoints(stake, limit);

			try
			{

				XmlDocument xmldoc = new XmlDocument();
				xmldoc.AppendChild(xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));

				xmldoc.AppendChild(xmldoc.CreateDocumentType("met", "-//GNU Backgammon//DTD Match Equity Tables//EN",
					gnubg_dir + "met\\" + "met.dtd", null));

				//root element
				XmlElement root = xmldoc.CreateElement("met");
				xmldoc.AppendChild(root);

				//info element	
				XmlElement info = xmldoc.CreateElement("info");

				info.AppendChild(xmldoc.CreateElement("name"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode("Munkittajat ry"));
				info.AppendChild(xmldoc.CreateElement("description"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode("How to own the PartyGammon cash games"));
				info.AppendChild(xmldoc.CreateElement("length"));
				info.LastChild.AppendChild(xmldoc.CreateTextNode(max_points.ToString()));

				root.AppendChild(info);



				// pre-crawford

				XmlElement pre = xmldoc.CreateElement("pre-crawford-table");
				pre.SetAttribute("type", "explicit");


				// hero win
				double[] hero_values = new double[max_points];
				//	for (int i = 0; i < hero_values.Length; i++) hero_values[i] = -1000;

				for (int i = max_points - 1; i >= 1; i--)
				{
					hero_values[hero_values.Length - 1] = METValue(stake, limit, rake, Player.Hero, i);

					pre.AppendChild(CreateMETRow(xmldoc, hero_values));
				}

				// opponent win
				double[] opponent_values = new double[max_points];
				//	for (int i = 0; i < opponent_values.Length; i++) opponent_values[i] = -1000;

				for (int i = 0; i < max_points; i++)
				{
					opponent_values[i] = METValue(stake, limit, rake, Player.Opponent, max_points - 1 - i);
				}

				pre.AppendChild(CreateMETRow(xmldoc, opponent_values));


				root.AppendChild(pre);



				// post-crawford

				XmlElement post = xmldoc.CreateElement("post-crawford-table");
				post.SetAttribute("type", "explicit");
				post.SetAttribute("player", "both");

				double[] values = new double[max_points];
				//	for (int i=0; i<values.Length; i++) values[i]=-1000;
				post.AppendChild(CreateMETRow(xmldoc, values));

				root.AppendChild(post);




				xmldoc.Save(gnubg_dir + "met\\" + filename);

			}
			catch (Exception)
			{
			}

			return max_points;
		}
	}*/
}
