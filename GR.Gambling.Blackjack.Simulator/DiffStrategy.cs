using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	class DiffStrategy : Agent
	{
		public bool IsDiff = false;
		public double Diff = 0.0;

		private Agent primary, secondary;

		public DiffStrategy(Agent primary, Agent secondary)
		{
			this.primary = primary;
			this.secondary = secondary;
		}

		public override double ShoeEV()
		{
			return primary.ShoeEV();
		}

		public override ActionType GetBestAction(Game game)
		{
			ActionType a1 = primary.GetBestAction(game);
			ActionType a2 = secondary.GetBestAction(game);

			if (a1 != a2 && !IsDiff)
			{
				List<ActionEv> l1 = primary.GetActions(game);

				Diff = 0.0;

				if (l1 != null)
				{
					for (int i = 0; i < l1.Count; i++)
					{
						if (l1[i].Action == a2)
						{
							Diff = l1[0].Ev - l1[i].Ev;
							break;
						}
					}
				}

				IsDiff = true;

			}

			return a1;
		}



		public override bool TakeInsurance(Game game)
		{
			bool take1 = primary.TakeInsurance(game);
			bool take2 = secondary.TakeInsurance(game);

			if (take1 != take2)
			{
				Console.WriteLine("Insurance mismatch: " + take1 + " <> " + take2);
				Console.WriteLine(game.ToString());
			}

			return take1;
		}

		// called when the round has ended
		public override void Showdown(Game game)
		{
			primary.Showdown(game);
			secondary.Showdown(game);
		}

		public override void ResetShoe(Game game)
		{
			primary.ResetShoe(game);
			secondary.ResetShoe(game);
		}

		public override void DealCard(Card card)
		{
			primary.DealCard(card);
			secondary.DealCard(card);
		}

		// ask for a bet amount in the beginning of the round
		public override int Bet(Game game)
		{
			return primary.Bet(game);
		}
	}
}
