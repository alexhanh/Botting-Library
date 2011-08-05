using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	class DualStrategy : Agent
	{
		public bool IsDiff = false;
		public double Diff = 0.0;

		private Agent primary, secondary;

		public DualStrategy(Agent primary, Agent secondary)
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
				List<ActionEv> l2 = secondary.GetActions(game);


				int m = 0;
				if (l1 != null) m = l1.Count;
				if (l2 != null && l2.Count > m) m = l2.Count;

				Console.WriteLine("STRATEGY MISMATCH");
				Console.WriteLine();
				Console.WriteLine(game.ToString());

				Console.WriteLine(string.Format("{0,-25}    {1,-25}", primary.GetType(), secondary.GetType()));
				Console.WriteLine();

				Console.WriteLine(string.Format("{0,-25}    {1,-25}", a1, a2));
				Console.WriteLine();

				for (int i = 0; i < m; i++)
				{
					ActionEv e1 = new ActionEv(), e2 = new ActionEv();

					if (l1 != null && i < l1.Count) e1 = l1[i];
					if (l2 != null && i < l2.Count) e2 = l2[i];

					Console.WriteLine(string.Format("{0,-25}    {1,-25}", e1, e2));
				}

				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine();
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
