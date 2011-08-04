using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;

namespace GR.Gambling.Backgammon.HCI
{
	public class BgbTimedThinker : TimedThinker
	{
		private Random random = new Random();

		public override int TimeOnTurnChanged(GameState gamestate, DoubleHint doubleHint, ResignHint resignHint)
		{
			if (random.Next(15) == 0) 
				return 300 + random.Next(800);

			return 200;
		}

		public override int TimeOnDiceRolled(GameState gamestate)
		{
			return 0;
		}

		public override int TimeOnStartingDiceRolled(GameState gamestate)
		{
			return random.Next(500, 1500);
		}

		public override TimedPlay TimedPlayOnRoll(GameState gamestate, Play play)
		{
			throw new NotImplementedException();
		}

		public override TimedPlay TimedPlayOnRoll(GameState gamestate, List<GR.Gambling.Backgammon.Tools.PlayHint> hints, GR.Gambling.Backgammon.Venue.VenueUndoMethod undo_method)
		{
			bool is_race = gamestate.Board.IsPureRace();
			bool is_bearoff = (gamestate.Board.LastChequer(gamestate.PlayerOnRoll) < 6);

			int total_sleep_before_first = 0;
			if (gamestate.Board.CapturedCount(gamestate.PlayerOnRoll) != hints[0].Play.Count)
			{
				if (!is_race)
				{
					total_sleep_before_first += random.Next(750, 1750);

					if (random.Next(10) == 0)
						total_sleep_before_first += random.Next(2500);
				}
				else
				{
					if (!is_bearoff)
						total_sleep_before_first += random.Next(250, 750);

					if (random.Next(20) == 0)
						total_sleep_before_first += random.Next(1000);
					if (random.Next(40) == 0)
						total_sleep_before_first += random.Next(2500);
					if (random.Next(60) == 0)
						total_sleep_before_first += random.Next(5000);
				}
			}

			int last_slot = -5;
			int last_dest = -5;

			int on_bar = gamestate.Board.CapturedCount(gamestate.PlayerOnRoll);

			TimedPlay play = new TimedPlay();

			int[] dice = gamestate.Dice;
			Play p = hints[0].Play;
			for (int i = 0; i < p.Count; i++)
			{
				int slot = p[i].From;
				int before = 0;
				int after = 0;
				int die = p[i].From - p[i].To;

				if (!(die == dice[0] || die == dice[1]))
					die = System.Math.Max(dice[0], dice[1]);

				if (i == 0)
					before += total_sleep_before_first;

				if (i == (p.Count - 1) && random.Next(15) == 0)
					after += random.Next(300, 1300);

				if (on_bar > 0)
				{
					if (random.Next(8) == 0)
						before += random.Next(500, 1000);
				}
				else if (!is_bearoff && i > 0 && slot != last_slot && (slot - die >= 0) && slot != last_dest && !p[i].IsEnter && (slot - die != last_dest))
				{
					int diff = System.Math.Abs(slot - last_slot);

					if ((last_slot >= 12 && slot < 12) || (slot >= 12 && last_slot < 12))
						diff = 10;

					if (diff <= 1)
						before += random.Next(25, 75);
					else if (diff <= 3)
						before += random.Next(50, 350);
					else if (diff <= 6)
						before += random.Next(100, 500);
					else
						before += random.Next(150, 900);
				}
				else
				{
				}

				play.Add(new TimedMove(hints[0].Play[i], before, after));

				last_slot = slot;
				last_dest = p[i].To;
				on_bar--;
			}

			return play;
		}

		public override int TimeOnResignOffer(GameState gamestate, ResignResponseHint hint)
		{
			return random.Next(500, 1500);
		}

		public override int TimeOnDoubleOffer(GameState gamestate, DoubleResponseHint hint)
		{
			int total = 0;

			if (random.Next(10) == 0)
				total += random.Next(8000);

			if (random.Next(5) == 0)
				total += random.Next(4000);

			return random.Next(500, 1500);
		}

		public override int TimeOnRematchOffer()
		{
			return random.Next(1000, 5000);
		}
	}
}
