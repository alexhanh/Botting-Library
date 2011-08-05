using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack.Betting
{
	public abstract class BettingSystem
	{
		public abstract int BetSize(double ev, int roll);
	}
}
