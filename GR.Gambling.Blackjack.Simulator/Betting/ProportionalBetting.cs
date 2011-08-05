using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack.Betting
{
	public class ProportionalBetting : BettingSystem
	{
		int proportion, spacing, max_bet;

		public ProportionalBetting(int proportion, int spacing, int max_bet)
		{
			this.proportion = proportion;
			this.spacing = spacing;
			this.max_bet = max_bet;
		}

		// 15000 / 100 == 150
		//25, 50, 75, 100, 125, 150, 175 ja 200
		public override int BetSize(double ev, int roll)
		{
			int p = roll / proportion;

			int bet = (p / spacing) * spacing;

			return Math.Min(max_bet, Math.Max(spacing, bet));
		}
	}
}
