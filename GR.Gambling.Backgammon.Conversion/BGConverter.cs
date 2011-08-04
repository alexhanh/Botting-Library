using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using GR.Gambling.Backgammon;

namespace GR.Gambling.Backgammon.Conversion
{
    public class BGConverter
    {
        // See drawboard.c in Gnubg source files.
        public static string ToGnuBgASCII(GameState gs, string[] player_names)
        {
            string[] a = new string[7];

            string achX = "     X6789ABCDEF";
            string achO = "     O6789ABCDEF";

            a[0] = string.Format("O: {0}", player_names[0]);
            a[6] = string.Format("X: {0}", player_names[1]);

            if (gs.Score(0) == 1)
                a[1] = string.Format("{0} point", gs.Score(0));
            else
                a[1] = string.Format("{0} points", gs.Score(0));

            if (gs.Score(1) == 1)
                a[5] = string.Format("{0} point", gs.Score(1));
            else
                a[5] = string.Format("{0} points", gs.Score(1));

            if (gs.OfferType == OfferType.Double)
            {
                a[(gs.PlayerOnTurn == 1) ? 4 : 2] = string.Format("Cube offered at {0}", gs.Cube.Value);
            }
            else
            {
                int index = (gs.PlayerOnRoll == 1) ? 4 : 2;

                if (gs.DiceRolled)
                    a[index] += string.Format("Rolled {0}{1}", gs.Dice[0], gs.Dice[1]);
                else if (!gs.HasOffer)
                    a[index] = "On roll";
                else
                {
                }

                if (gs.Cube.Centered)
                {
                    if (gs.GameType == GameType.Match)
                        a[3] += string.Format("{0} point match (Cube: {1})", gs.MatchTo, gs.Cube.Value);
                    else
                        a[3] += string.Format("(Cube: {0})", gs.Cube.Value);
                }
                else
                {
                    a[(gs.Cube.Owner == 1) ? 6 : 0] = string.Format("{0}: {1} (Cube: {2})", (gs.Cube.Owner == 1) ? "X" : "O", "CubeOwnersName", gs.Cube.Value);

                    if (gs.GameType == GameType.Match)
                        a[3] += string.Format("{0} point match", gs.MatchTo);
                }
            }

            if (gs.OfferType == OfferType.Resign)
            {
                string[] resign_strigns = new string[] { "single game", "gammon", "backgammon" };
                a[(gs.PlayerOnRoll == 1) ? 4 : 2] += string.Format(", resigns {0}", resign_strigns[(int)gs.ResignOfferValue - 1]);
            }

            Board b = gs.Board;
            Console.WriteLine(b.CapturedCount(0) + " " + b.CapturedCount(1));
            if (gs.PlayerOnRoll == 0)
                b = SwapSides(b);

            string s = "";

            s += string.Format(" {0,-15} {1}: ", "GNU Backgammon", "Position ID");

            s += "4HPwATDg6+ABMA";//GnuBg.ToPositionID(gs.Board, gs.PlayerOnRoll);

            s += Environment.NewLine;

            s += string.Format("                 {0}   : {1}" + Environment.NewLine, "Match ID", "MQHgAAAACAAA"); //GnuBg.ToMatchID(gs);

            s += (gs.PlayerOnRoll == 1) ? " +13-14-15-16-17-18------19-20-21-22-23-24-+     " : " +12-11-10--9--8--7-------6--5--4--3--2--1-+     ";

            s += a[0] + Environment.NewLine;
            Console.WriteLine(b.CapturedCount(0) + " " + b.CapturedCount(1));
            int x = 0, y = 0;
            for (y = 0; y < 4; y++)
            {
                s += " ";
                s += "|";

                for (x = 12; x < 18; x++)
                {
                    s += " ";
                    s += (b.PointCount(1, x) > y) ? "X" : (b.PointCount(0, 23 - x) > y) ? "O" : " "; // X or O or ' ' TODO!
                    s += " ";
                }

                s += "|";
                s += " ";
                s += b.CapturedCount(0) > y ? "O" : " "; // O or ' ' TODO!
                s += " ";
                s += "|";

                for (; x < 24; x++)
                {
                    s += " ";
                    s += (b.PointCount(1, x) > y) ? "X" : (b.PointCount(0, 23 - x) > y) ? "O" : " "; // X or O or ' ' TODO!
                    s += " ";
                }

                s += "|";
                s += " ";

                for (x = 0; x < 3; x++)
                    s += b.FinishedCount(0) > (5 * x + y) ? "O" : " "; // O or ' ' TODO!

                s += " ";

                if (y < 2 && a[y + 1] != "")
                {
                    s += a[y + 1];
                }

                s += Environment.NewLine;
            }

            s += " ";
            s += "|";

            for (x = 12; x < 18; x++)
            {
                s += " ";
                s += b.PointCount(1, x) > 0 ? achX[b.PointCount(1, x)] : achO[b.PointCount(0, 23 - x)];// TODO, WTF?
                s += " ";
            }

            s += "|";
            s += " ";
            s += achO[b.CapturedCount(0)]; // TODO!
            s += " ";
            s += "|";

            for (; x < 24; x++)
            {
                s += " ";
                s += b.PointCount(1, x) > 0 ? achX[b.PointCount(1, x)] : achO[b.PointCount(0, 23 - x)]; // TODO!
                s += " ";
            }

            s += "|";
            s += " ";

            for (x = 0; x < 3; x++)
                s += b.FinishedCount(0) > (5 * x + 4) ? "O" : " "; ; // TODO!

            s += Environment.NewLine;

            s += (gs.PlayerOnRoll == 1) ? "v" : "^";

            s += "|                  |BAR|                  |     ";

            s += a[3];

            s += Environment.NewLine;

            s += " ";
            s += "|";

            for (x = 11; x > 5; x--)
            {
                s += " ";
                s += b.PointCount(1, x) > 0 ? achX[b.PointCount(1, x)] : achO[b.PointCount(0, 23 - x)]; // TODO!
                s += " ";
            }

            s += "|";
            s += " ";
            s += achX[b.CapturedCount(1)]; // TODO!
            s += " ";
            s += "|";

            for (; x >= 0; x--)
            {
                s += " ";
                s += b.PointCount(1, x) > 0 ? achX[b.PointCount(1, x)] : achO[b.PointCount(0, 23 - x)]; // TODO!
                s += " ";
            }

            s += "|";
            s += " ";

            for (x = 0; x < 3; x++)
                s += b.FinishedCount(1) > (5 * x + 4) ? "X" : " "; // TODO!

            s += Environment.NewLine;

            for (y = 3; y >= 0; y--)
            {
                s += " ";
                s += "|";

                for (x = 11; x > 5; x--)
                {
                    s += " ";
                    s += b.PointCount(1, x) > y ? "X" : b.PointCount(0, 23 - x) > y ? "O" : " "; // TODO!
                    s += " ";
                }

                s += "|";
                s += " ";
                s += b.CapturedCount(1) > y ? "X" : " "; // TODO!
                s += " ";
                s += "|";

                for (; x >= 0; x--)
                {
                    s += " ";
                    s += b.PointCount(1, x) > y ? "X" : b.PointCount(0, 23 - x) > y ? "O" : " "; // TODO!
                    s += " ";
                }

                s += "|";
                s += " ";

                for (x = 0; x < 3; x++)
                    s += b.FinishedCount(1) > (5 * x + y) ? "X" : " "; // TODO!

                s += " ";

                if (y < 2)
                    s += a[5 - y];

                s += Environment.NewLine;
            }

            s += (gs.PlayerOnRoll == 1) ? " +12-11-10--9--8--7-------6--5--4--3--2--1-+     " : " +13-14-15-16-17-18------19-20-21-22-23-24-+     ";

            s += a[6];

            s += Environment.NewLine;

            Console.WriteLine(s);

            return s;
        }

        private static Board SwapSides(Board b)
        {
            Board sb = b.Clone();

            int n;
            for (int s = 0; s < 24; s++)
            {
                n = b.PointCount(0, s);
                sb.SetPoint(0, s, b.PointCount(1, s));
                sb.SetPoint(1, s, n);
            }

            sb.SetFinished(1, b.FinishedCount(0));
            sb.SetFinished(0, b.FinishedCount(1));

            sb.SetCaptured(1, b.CapturedCount(0));
            sb.SetCaptured(0, b.CapturedCount(1));

            return sb;
        }

        // TODO, unfinished.
        // See drawboard.c to see how FIBS ID handling is done in gnubg.
        // http://www.fibs.com/fibs_interface.html#board_state
        public static GameState FromFIBS(string id, ref string error)
        {
            GameState gs = new GameState(GameType.Match);

            error = "";

            string[] s = id.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (s.Length != 53)
                return null;

            if (s[0] != "board")
                return null;

            //string[] player_names = new string[] { s[1], s[2] };

            gs.SetName(0, s[1]);
            gs.SetName(1, s[2]);

            int match_length = 0;
            if (!int.TryParse(s[3], out match_length))
            {
                error = "FIBS ID parsing error: Incorrect match length.";
                return null;
            }

            gs.MatchTo = match_length;

            int player0score, player1score;
            if (!int.TryParse(s[4], out player0score) || !int.TryParse(s[5], out player1score))
            {
                error = "FIBS ID parsing error: Invalid match score.";
                return null;
            }

            gs.SetScore(player0score, player1score);

            int count;
            int[] total_counts = new int[2];
            for (int p = 0; p < 26; p++)
            {
                if (!int.TryParse(s[6 + p], out count))
                {
                    error = "FIBS ID parsing error: Invalid board.";
                    return null;
                }

                if (p == 0)
                {
                    if (count > 0)
                        return null;

                    gs.Board.SetCaptured(1, -count);
                    total_counts[1] += -count;
                }

                if (p == 25)
                {
                    if (count < 0)
                        return null;

                    gs.Board.SetCaptured(0, count);
                    total_counts[0] += count;
                }

                if (p >= 1 && p <= 24 && count != 0)
                {
                    gs.Board.SetPoint(0, 6 + p - 1, count);
                    total_counts[count > 0 ? 0 : 1] += Math.Abs(count);
                }
            }

            int turn;
            if (!int.TryParse(s[32], out turn))
                return null;

            gs.PlayerOnTurn = (turn == -1) ? 0 : 1;

            int[] dice = new int[2];
            if (!int.TryParse(s[33], out dice[0]) && !int.TryParse(s[34], out dice[1]))
                return null;

            if (dice[0] > 0 && dice[1] > 0)
                gs.SetDice(dice[0], dice[1]);

            int cube_value;
            if (!int.TryParse(s[35], out cube_value))
                return null;



            return gs;
        }

        private static bool IsNumber(string s, bool negative_sign)
        {
            for (int i = 0; i < s.Length; i++)
            {

            }

            foreach (char c in s)
                if (!char.IsDigit(c))
                    return false;

            return true;
        }
    }
}
