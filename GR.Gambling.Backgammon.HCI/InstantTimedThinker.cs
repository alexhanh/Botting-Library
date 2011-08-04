using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;
using GR.Gambling.Backgammon.Venue;

namespace GR.Gambling.Backgammon.HCI
{
    public class InstantTimedThinker : TimedThinker
    {
        private Random random;

        public InstantTimedThinker()
            : base()
        {
            random = new Random();
        }

		public override int TimeOnTurnChanged(GameState gamestate, DoubleHint doubleHint, ResignHint resignHint)
        {
            return 0;
        }

        public override int TimeOnDiceRolled(GameState gamestate)
        {
            return 0;
        }

        public override int TimeOnStartingDiceRolled(GameState gamestate)
        {
            return 0;
        }

        public override TimedPlay TimedPlayOnRoll(GameState gamestate, Play play)
        {
            //Console.WriteLine("Sorting moves...");
            List<Play> legal_plays = gamestate.Board.LegalPlays(gamestate.PlayerOnRoll, gamestate.Dice);
            List<Move> forced_moves = Board.ForcedMoves(legal_plays);
            //Console.Write("Forced moves: ");
            //foreach (Move move in forced_moves)
            //    Console.Write(move + " ");
            //Console.WriteLine();

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



            TimedPlay timed_play = new TimedPlay();

            if (gamestate.Board.IsPureRace())
            {
                foreach (Move move in play)
                {
                    timed_play.Add(new TimedMove(move, random.Next(100, 500), random.Next(200)));
                }
            }
            else
            {
                foreach (Move move in play)
                {
                    timed_play.Add(new TimedMove(move, random.Next(250, 500), random.Next(250)));
                }
            }

            return timed_play;
        }

        public override TimedPlay TimedPlayOnRoll(GameState gamestate, List<PlayHint> hints, VenueUndoMethod undo_method)
        {
            return TimedPlayOnRoll(gamestate, hints[0].Play);
        }

		public override int TimeOnResignOffer(GameState gamestate, ResignResponseHint hint)
        {
            return 0;
        }

		public override int TimeOnDoubleOffer(GameState gamestate, DoubleResponseHint hint)
        {
            return 0;
        }

        public override int TimeOnRematchOffer()
        {
            return 0;
        }
    }
}
