using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack.Betting
{
	public class KellyBetting : BettingSystem
	{
		double max_ev;
		int min_bet;
		int max_bet;
		int spacing;

		public KellyBetting(double max_ev, int spacing, int min_bet, int max_bet)
		{
			this.max_ev = max_ev;
			this.min_bet = min_bet;
			this.max_bet = max_bet;
			this.spacing = spacing;
		}

		public override int BetSize(double ev, int roll)
		{
			int bet = (int)(max_bet * ev / max_ev);

			bet = (bet / spacing) * spacing;

			return Math.Min(max_bet, Math.Max(bet, min_bet));
		}

		public override string ToString()
		{
			return "KellyBetting " + max_ev + " " + max_bet + " " + spacing;
		}
	}
}
