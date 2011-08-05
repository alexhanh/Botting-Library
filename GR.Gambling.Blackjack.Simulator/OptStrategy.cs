using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BjEval;

namespace GR.Gambling.Blackjack
{
	class OptStrategy : Agent
	{
		private int max_bet;
		private double ev_cutoff;
		private double pp_multiplier;

		private CardCounter card_counter;

		public OptStrategy(int max_bet, double ev_cutoff, double pp_multiplier)
		{
			this.ev_cutoff = ev_cutoff;
			this.max_bet = max_bet;
			this.pp_multiplier = pp_multiplier;

			card_counter = new CardCounter(pp_multiplier);
		}

		public override double ShoeEV()
		{
			return card_counter.CurrentEV;
		}

		public override int Bet(Game game)
		{
			if (card_counter.CurrentEV > ev_cutoff)
				return max_bet;
			else
				return game.Rules.MinBet;
		}

		public override List<ActionEv> GetActions(Game game)
		{
			List<ActionEv> actions = new List<ActionEv>();

			int[] shoe = game.Shoe.Counts;
			/*int[] test = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			int test_count = 0;
			foreach (Card card in game.Shoe)
			{
				test[card.PointValue - 1]++;
				test_count++;
			}
			for (int i = 0; i < 10; i++)
			{
				if (test[i] != shoe[i])
				{
					Console.Write(game.Shoe.Count + "| ");
					for (int j = 0; j < 10; j++)
						Console.Write(shoe[j] + " ");
					Console.WriteLine();
					Console.Write(test_count + "| ");
					for (int j = 0; j < 10; j++)
						Console.Write(test[j] + " ");
					Console.WriteLine();

					Console.ReadLine();
					break;
				}
			}*/
			shoe[game.DealerHand[1].PointValue - 1]++;

			SHand shand;
			int soft_total = game.PlayerHandSet.ActiveHand.SoftTotal();
			if (soft_total <= 21 && game.PlayerHandSet.ActiveHand.HasAce())
			{
				shand.Total = soft_total;
				shand.Soft = true;
			}
			else
			{
				shand.Total = game.PlayerHandSet.ActiveHand.HardTotal();
				shand.Soft = false;
			}

			int upcard = game.DealerHand[0].PointValue;
			int split_card = game.PlayerHandSet.ActiveHand[0].PointValue;

			BjEval.Eval.CacheDealerProbs(upcard, shoe);
			/*
						for (int i = 0; i < 10; i++)
							Console.Write(shoe[i] + " ");
						Console.WriteLine();

						Console.WriteLine("SHand: " + shand.Total + " soft=" + shand.Soft);
						*/
			if (game.IsValidAction(ActionType.Stand))
				actions.Add(new ActionEv() { Action = ActionType.Stand, Ev = BjEval.Eval.StandEv(shand, upcard, shoe) });
			if (game.IsValidAction(ActionType.Hit))
				actions.Add(new ActionEv() { Action = ActionType.Hit, Ev = BjEval.Eval.HitEv(shand, upcard, shoe) });
			if (game.IsValidAction(ActionType.Double))
				actions.Add(new ActionEv() { Action = ActionType.Double, Ev = BjEval.Eval.DoubleEv(shand, upcard, max_bet, shoe) });
			if (game.IsValidAction(ActionType.Surrender))
				actions.Add(new ActionEv() { Action = ActionType.Surrender, Ev = BjEval.Eval.SurrenderEv() });
			if (game.IsValidAction(ActionType.Split))
				actions.Add(new ActionEv() { Action = ActionType.Split, Ev = BjEval.Eval.SplitEv(split_card, upcard, max_bet, game.Rules.Splits - game.SplitCount, shoe) });

			actions.Sort(delegate(ActionEv ae1, ActionEv ae2) { return ae2.Ev.CompareTo(ae1.Ev); });

			return actions;
		}

		public override ActionType GetBestAction(Game game)
		{
			/*Console.WriteLine("upcard: " + upcard + " hand: " + game.PlayerHandSet.ActiveHand);
			foreach (ActionEv ae in actions)
				Console.WriteLine(ae.Action + " " + ae.Ev);

			Console.ReadLine();*/

			List<ActionEv> actions = GetActions(game);

			return actions[0].Action;
		}

		public override bool TakeInsurance(Game game)
		{
			int[] shoe = game.Shoe.Counts;
			shoe[game.DealerHand[1].PointValue - 1]++;

			double insurance_ev = Eval.InsuranceEv(max_bet, shoe);

			if (insurance_ev >= 0.0)
				return true;
			else
				return false;

			/*
			// remember the unseen dealt second dealer card, which is still reduced from shoe!
			// take into account
			int seen_tens = card_counts[9];
			if (game.PlayerHandSet.ActiveHand[0].IsTenValue())
				seen_tens++;
			if (game.PlayerHandSet.ActiveHand[1].IsTenValue())
				seen_tens++;

			// add the dealer's second dealt card to unknown count (+1)
			double bj_prob = (double)(8*4*4-seen_tens) / (double)(game.Shoe.Count + 1);
			double insurance_ev = 1.0*bj_prob-0.5*(1.0-bj_prob);

			// we take insurance
			if (insurance_ev >= 0.0)
				return true;
			else
				return false;*/
		}

		public override void Showdown(Game game)
		{
			foreach (Card card in game.DealerHand)
			{
				card_counter.RemoveCard(card.PointValue);
			}

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit()) continue;

				foreach (Card card in hand)
				{
					card_counter.RemoveCard(card.PointValue);
				}
			}
		}

		public override void ResetShoe(Game game)
		{
			card_counter.Reset();
		}

		public override void DealCard(Card card)
		{
			card_counter.RemoveCard(card.PointValue);
		}
	};
}
