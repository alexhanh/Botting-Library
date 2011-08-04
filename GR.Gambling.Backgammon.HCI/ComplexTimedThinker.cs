using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;
using GR.Math;
using GR.Gambling.Backgammon.Venue;

namespace GR.Gambling.Backgammon.HCI
{
    public class ComplexTimedThinker : TimedThinker
    {
        private Random random;
        double coefficient;
        double undo_probability;

        public ComplexTimedThinker(double coefficient, double undo_probability)
        {
            this.undo_probability = undo_probability;
            this.coefficient = coefficient;
            random = new Random();
        }

		public override int TimeOnTurnChanged(GameState gamestate, DoubleHint doubleHint, ResignHint resignHint)
        {
            // Player has captured pieces, think less time.
            if (gamestate.Board.CapturedCount(gamestate.PlayerOnRoll) > 0)
                return (int)(coefficient * Gaussian.Next(700, 250, 20000, 500, 2.0));

            if (!gamestate.CanDouble())
                return (int)(coefficient * Gaussian.Next(400, 500, 20000, 1500, 2.0));

            //  Parameter   Values
            //  Mean        1000 ms
            //  Minimum     500 ms
            //  Maximum     20000 ms
            //  Deviation   2.0 (95.4% are within 2000 ms from mean)
            return (int)(coefficient * Gaussian.Next(1000, 1000, 20000, 1700, 2.0));
        }

        public override int TimeOnDiceRolled(GameState gamestate)
        {
            if (gamestate.Board.IsPureRace())
                return (int)(coefficient * Gaussian.Next(1000, 350, 20000, 750, 2.0));
            else
                return (int)(coefficient * Gaussian.Next(1500, 750, 20000, 800, 2.0));
        }

        public override int TimeOnStartingDiceRolled(GameState gamestate)
        {
            //  Parameter   Values
            //  Mean        2000 ms
            //  Minimum     1000 ms
            //  Maximum     20000 ms
            //  Deviation   2.0 (95.4% are within 5000 ms from mean)
            return (int)(coefficient * Gaussian.Next(1000, 500, 20000, 1700, 2.0));
        }

        public override TimedPlay TimedPlayOnRoll(GameState gamestate, Play play)
        {
            throw new NotImplementedException();
        }

        private Play SortMoves(GameState gamestate, Play play, List<Play> legal_plays, List<Move> forced_moves)
        {
            if (gamestate.Dice[0] != gamestate.Dice[1])
            {
                // Both are on the bar, sort the enter which uses bigger die as first.
                if (play.Count == 2 && play[0].IsEnter && play[1].IsEnter && play[1].To < play[0].To)
                    play.Reverse();

                // Third condition avoids sequences where the second move is dependant on the first one
                if (play.Count == 2 && /*remove me when fixed*/ !play[0].IsEnter && !play[1].IsEnter && forced_moves.Count == 0 && gamestate.Board.PointCount(gamestate.PlayerOnRoll, play[1].From) > 0)
                {
                    // Case where there are two moves and they share the same 'from' point, sort so that the one with bigger distance is first.
                    if (play[0].From == play[1].From && play[0].Distance < play[1].Distance)
                        play.Reverse();
                    // Chain move, instead of 3/1 6/3 make 6/3/1
                    else if (play[0].From == play[1].To)
                        play.Reverse();
                    // Same as above already properly sorted. Don't do anything. This condition avoids it being passed below if the second play is a hit and being reversed.
                    else if (play[1].From == play[0].To)
                        ;
                    // If only one is a hit, greedily sort it as the first move
                    else if (play[1].HasHits && !play[0].HasHits)
                        play.Reverse();
                    else if (!play[0].HasHits && play[1].HasHits)
                        play.Reverse();
                    else
                    {
                    }
                }
            }
            else
            {
                /*if (play.Count == 4)
                {

                }*/
            }

            return play;
        }

        private bool HasBlockaded(GameState gamestate)
        {
            int consequent_blockades = 0;
            for (int point = 23; point >= 0; point--)
            {
                if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, point) > 2)
                    consequent_blockades++;
                else
                    consequent_blockades = 0;

                if (consequent_blockades >= 4)
                {
                    for (int point2 = point; point2 >= 0; point2--)
                    {
                        if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, point2) < 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override TimedPlay TimedPlayOnRoll(GameState gamestate, List<PlayHint> hints, VenueUndoMethod undo_method)
        {
            List<Play> legal_plays = gamestate.Board.LegalPlays(gamestate.PlayerOnRoll, gamestate.Dice);
            List<Move> forced_moves = Board.ForcedMoves(legal_plays);

            // Sort the moves.
            foreach (PlayHint hint in hints)
            {
                SortMoves(gamestate, hint.Play, legal_plays, forced_moves);
            }

            double optimal_move_equity = hints[0].Equity;

            double doubtful = 0.04;
            double bad = 0.08;
            double very_bad = 0.16;

            /*List<PlayHint> good_hints = new List<PlayHint>();
            List<PlayHint> doubtful_hints = new List<PlayHint>();
            List<PlayHint> bad_hints = new List<PlayHint>();
            List<PlayHint> very_bad_hints = new List<PlayHint>();

            // Skip the first one as it is the optimal one.
            for (int i = 1; i < hints.Count; i++)
            {
                double diff = optimal_move_equity - hints[i].Equity;
                if (diff >= very_bad)
                    very_bad_hints.Add(hints[i]);
                else if (diff >= bad)
                    very_bad_hints.Add(hints[i]);
                else if (diff >= doubtful)
                    doubtful_hints.Add(hints[i]);
                else
                    good_hints.Add(hints[i]);
            }*/

            /*if (legal_plays.Count != hints.Count)
            {
                Console.WriteLine("Legal plays and hint plays mis-match");
                Console.WriteLine("Legal plays: ");
                foreach (Play play in legal_plays)
                    Console.WriteLine(play);
                Console.WriteLine("Hint plays: ");
                foreach (PlayHint hint in hints)
                    Console.WriteLine(hint);

                throw new Exception();
            }*/

            // Is the play an obvious move? Check the equity difference to second best move.
            bool obvious_move = false;
            if (legal_plays.Count == 1 || (hints.Count > 1 && (hints[1].Equity - optimal_move_equity) > bad))
                obvious_move = true;

            List<Play> fake_plays = new List<Play>();

            if (!obvious_move && undo_method != VenueUndoMethod.None)
            {
                // Choose the amount of fake plays.
                int fake_plays_to_choose = 0;
                if (random.NextDouble() <= undo_probability)
                {
                    fake_plays_to_choose++;
                    while (random.NextDouble() < 0.1)
                        fake_plays_to_choose++;
                }

                // Choose the fake plays.
                for (int i = 0; i < fake_plays_to_choose; i++)
                {
                    int fake_play_index = 1;
                    while (random.Next(3) == 0)
                    {
                        fake_play_index++;
                        if (fake_play_index >= hints.Count)
                            fake_play_index = 0;
                    }

                    // Make it partial?
                    if (random.NextDouble() <= 0.3)
                    {
                        Play partial_play = new Play();
                        int moves_to_choose = 1;
                        while (random.Next(4) == 0)
                        {
                            moves_to_choose++;
                            if (moves_to_choose >= hints[fake_play_index].Play.Count)
                                moves_to_choose = 1;
                        }

                        for (int m = 0; m < moves_to_choose; m++)
                        {
                            partial_play.Add(hints[fake_play_index].Play[m]);
                        }

                        fake_plays.Add(partial_play);
                    }
                    else
                        fake_plays.Add(hints[fake_play_index].Play);
                }
            }

            TimedPlay timed_play = new TimedPlay();

            // Add undo sequences using fake plays.
            if (!obvious_move && hints.Count > 1 && fake_plays.Count > 0)
            {
                List<Move> made_moves = new List<Move>();
                //Queue<Move> made_moves = new Queue<Move>();
                //Stack<Move> made_moves = new Stack<Move>();

                foreach (Play fake_play in fake_plays)
                {
                    for (int i = 0; i < fake_play.Count; i++)
                    {
                        /*if (made_moves.Count > 0)
                        {
                            int shared = 0;
                            while (shared < made_moves.Count && shared < fake_play.Count && made_moves[shared] == fake_play[shared])
                                shared++;
                        }
                        else*/
                        timed_play.Add(new TimedMove(fake_play[i], 0, 0));
                        //made_moves.Push(fake_play[i]);
                    }

					// Undo all
					if (undo_method == VenueUndoMethod.UndoLast)
						for (int i = 0; i < fake_play.Count; i++)
							timed_play.Add(TimedMove.CreateUndoMove(0, 0));
					else  
						timed_play.Add(TimedMove.CreateUndoMove(0, 0));
                }

                int m = 0;
                // TODO: Some how check if current move and previous share some similar so not necessary undo all.

                foreach (Move move in hints[0].Play)
                    timed_play.Add(new TimedMove(move, 0, 0));
            }
            else
            {
                // No fake plays, add the optimal move.
                foreach (Move move in hints[0].Play)
                    timed_play.Add(new TimedMove(move, 0, 0));
            }
            

            int last_from = -2;
            bool was_last_undo = false;
            foreach (TimedMove timed_move in timed_play)
            {
                if (timed_move.IsUndo)
                {
                    if (was_last_undo)
                    {
                        timed_move.WaitBefore = Gaussian.Next(500, 200, 10000, 600, 1.5);
                        timed_move.WaitAfter = random.Next(200);
                    }
                    else
                    {
                        timed_move.WaitBefore = Gaussian.Next(1000, 500, 10000, 2000, 1.5);
                        timed_move.WaitAfter = Gaussian.Next(1000, 50, 10000, 2000, 1.5);
                    }
                    
                    last_from = -2;

                    was_last_undo = true;

                    continue;
                }

                was_last_undo = false;

                if (timed_move.From != last_from)
                {
                    //timed_move.WaitBefore = Gaussian.Next(500, 250, 2000, 500, 3.0);
                    //timed_move.WaitAfter = random.Next(300);
					timed_move.WaitBefore = Gaussian.Next(1000, 250, 2000, 1000, 3.0);
					timed_move.WaitAfter = random.Next(300);
                }
                else
                {
                    //timed_move.WaitBefore = Gaussian.Next(250, 100, 2000, 200, 3.0);
                    //timed_move.WaitAfter = random.Next(100);
					timed_move.WaitBefore = Gaussian.Next(500, 250, 2000, 200, 3.0);
					timed_move.WaitAfter = random.Next(250);
                }

                // Speedup for pure race or when we have a blockade.
                if (random.NextDouble() <= 0.9 && (gamestate.Board.IsPureRace() || HasBlockaded(gamestate) || obvious_move))
                {
					timed_move.WaitBefore = (int)(timed_move.WaitBefore * (0.4 + 0.3 * random.NextDouble()));
					timed_move.WaitAfter = (int)(timed_move.WaitAfter * (0.4 + 0.3 * random.NextDouble()));
                }

                last_from = timed_move.From;
            }

            // Normalize with coefficient.
            foreach (TimedMove timed_move in timed_play)
            {
                timed_move.WaitBefore = (int)(timed_move.WaitBefore * coefficient);
                timed_move.WaitAfter = (int)(timed_move.WaitAfter * coefficient);
            }

            return timed_play;
        }

		public override int TimeOnResignOffer(GameState gamestate, ResignResponseHint hint)
        {
            //  Parameter   Values
            //  Mean        3000 ms
            //  Minimum     1000 ms
            //  Maximum     8000 ms
            //  Deviation   2.0 (95.4% are within 2000 ms from mean)
            return (int)(coefficient * Gaussian.Next(3000, 1000, 8000, 2000, 2.0));
        }

		public override int TimeOnDoubleOffer(GameState gamestate, DoubleResponseHint hint)
        {
            //  Parameter   Values
            //  Mean        2000 ms
            //  Minimum     1000 ms
            //  Maximum     20000 ms
            //  Deviation   2.0 (95.4% are within 5000 ms from mean)
            return (int)(coefficient * Gaussian.Next(2000, 1000, 20000, 2000, 2.0));
        }

        // +-std    fraction in range
        // 1        0.682689492137
        // 2        0.954499736104
        // 3        0.997300203937
        // 4        0.999936657516

        public override int TimeOnRematchOffer()
        {
            //  Parameter   Values
            //  Mean        2000 ms
            //  Minimum     1000 ms
            //  Maximum     20000 ms
            //  Deviation   2.0 (95.4% are within 2000 ms from mean)
            return (int)(coefficient * Gaussian.Next(2000, 1000, 20000, 2000, 2.0));
        }
    }
}
