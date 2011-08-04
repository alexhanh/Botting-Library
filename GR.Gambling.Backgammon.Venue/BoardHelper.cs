using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.Venue
{
    public class BoardHelper
    {
        /// <summary>
        /// Add missing chequers so that all we need is to know if a slot has 5 chequers and the total pip count.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="pips">pips[0] should contain total pip count for board[0].</param>
        public static void AddMissingChequers(ref Board board, int[] pips)
        {
            List<int> bulk_points = new List<int>();
            int total_count, missing, missing_pips, count;

			Console.WriteLine(board.ToString());

            for (int p = 0; p < 2; p++)
            {
                bulk_points.Clear();
                total_count = 0;

                // Assumes we always get the right finished counts.
                total_count += board.FinishedCount(p);
				
                // But not captured.
                total_count += board.CapturedCount(p);
                if (board.CapturedCount(p) == 5)
                    bulk_points.Add(24);

                for (int point = 0; point < 24; point++)
                {
                    count = board.PointCount(p, point);
					//Console.WriteLine(count);
                    if (count > 0)
                        total_count += count;

                    if (count == 5)
                        bulk_points.Add(point);
                }

                missing = 15 - total_count;

                if (missing > 0)
                {
                    if (bulk_points.Count == 1)
                    {
						Console.WriteLine("bulk points = 1");
                        if (bulk_points[0] == 24)
                            board.IncreaseCaptured(p, missing);
                        else
                            board.AddToPoint(p, bulk_points[0], missing);

                        continue;
                    }

                    if (bulk_points.Count == 2)
                    {
						Console.WriteLine("bulk points = 2");
                        missing_pips = pips[p] - board.PipCount(p);

                        int slot1 = bulk_points[0] + 1;
                        int slot2 = bulk_points[1] + 1;

                        int slot1_missing = (missing_pips - slot2 * missing) / (slot1 - slot2);
                        int slot2_missing = (missing_pips - slot1 * slot1_missing) / slot2;

                        if ((missing_pips - slot2 * missing) % (slot1 - slot2) != 0 ||
                            (missing_pips - slot1 * slot1_missing) % slot2 != 0)
                        {
							throw new Exception("Missmatch in AddMissingChequers(). Players {" + p + "}, Missing pips {" + missing_pips + "}, missing {" + missing + "} slot1 {" + slot1 + "} slot2 {" + slot2 + "}");
                        }

                        if (slot1 == 25)
                            board.IncreaseCaptured(p, slot1_missing);
                        else
                            board.AddToPoint(p, slot1 - 1, slot1_missing);

                        if (slot2 == 25)
                            board.IncreaseCaptured(p, slot2_missing);
                        else
                            board.AddToPoint(p, slot2 - 1, slot2_missing);
                    }
                }
            }
        }
    }
}
