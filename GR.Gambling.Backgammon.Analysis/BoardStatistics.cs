using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon;
using GR.Gambling.Backgammon.Tools;

namespace GR.Gambling.Backgammon.Analysis
{
    public class BoardStatistics
    {
        private static Dictionary<string, int> board_hash2rolls;//HashSet<string> board_hash;

        public static int RollsNeeded(Board board, int[] dice, int player)
        {
            if (!board.IsPureRace())
                return -1;

            //board_hash = new HashSet<string>();
            board_hash2rolls = new Dictionary<string, int>();

            int min_rolls = int.MaxValue;
            Rollout(board, dice, player, 0, ref min_rolls);

            return min_rolls;
        }

        private static void Rollout(Board board, int[] dice, int player, int rolls, ref int min_rolls)
        {
            if (board.FinishedCount(player) == 15)
            {
                if (rolls < min_rolls)
                    min_rolls = rolls;

                return;
            }

            List<Play> legal_plays = board.LegalPlays(player, dice);

            foreach (Play legal_play in legal_plays)
            {
                foreach (Move move in legal_play)
                    board.MakeMove(player, move);

                string hash = board.HashString();

                if (!board_hash2rolls.ContainsKey(hash))
                {
                    board_hash2rolls[hash] = rolls;

                    Rollout(board, dice, player, rolls + 1, ref min_rolls);
                }
                else
                {
                    if (rolls < board_hash2rolls[hash])
                    {
                        board_hash2rolls[hash] = rolls;

                        Rollout(board, dice, player, rolls + 1, ref min_rolls);
                    }
                }

                for (int i = legal_play.Count - 1; i >= 0; i--)
                    board.UndoMove(player, legal_play[i]);
            }
        }
    }
}
