using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BjEval;

namespace GR.Gambling.Blackjack
{
	class SuperOptStrategy : Agent
	{
		private CardCounter cardCounter;

		private int max_bet;
		private double ev_cutoff;
		private double pp_multiplier;

		public SuperOptStrategy(int max_bet, double ev_cutoff, double pp_multiplier)
		{
			this.ev_cutoff = ev_cutoff;
			this.max_bet = max_bet;
			this.pp_multiplier = pp_multiplier;

			cardCounter = new CardCounter(pp_multiplier);
		}

		public override double ShoeEV()
		{
			return cardCounter.CurrentEV;
		}

		public override int Bet(Game game)
		{
			if (cardCounter.CurrentEV > 0.0)
				return max_bet;
			else
				return game.Rules.MinBet;
		}

		public override List<ActionEv> GetActions(Game game)
		{
			SuperEval.Initialize(game);

			int split_card = game.PlayerHandSet.ActiveHand[0].PointValue;
		
			List<ActionEv> actions = new List<ActionEv>();

			if (game.IsValidAction(ActionType.Stand))
				actions.Add(new ActionEv() { Action = ActionType.Stand, Ev = SuperEval.StandEv() });
			if (game.IsValidAction(ActionType.Hit))
				actions.Add(new ActionEv() { Action = ActionType.Hit, Ev = SuperEval.HitEv() });
			if (game.IsValidAction(ActionType.Double))
				actions.Add(new ActionEv() { Action = ActionType.Double, Ev = SuperEval.DoubleEv() });
			if (game.IsValidAction(ActionType.Surrender))
				actions.Add(new ActionEv() { Action = ActionType.Surrender, Ev = SuperEval.SurrenderEv() });
			if (game.IsValidAction(ActionType.Split))
				actions.Add(new ActionEv() { Action = ActionType.Split, Ev = SuperEval.SplitEv(split_card, game.Rules.Splits - game.SplitCount) });

			actions.Sort(delegate(ActionEv ae1, ActionEv ae2) { return ae2.Ev.CompareTo(ae1.Ev); });

			return actions;
		}

		public override ActionType GetBestAction(Game game)
		{
			List<ActionEv> actions = GetActions(game);

			return actions[0].Action;
		}

		public override bool TakeInsurance(Game game)
		{
			return SuperEval.TakeInsurance(game);
		}

		public override void Showdown(Game game)
		{
			foreach (Card card in game.DealerHand)
			{
				cardCounter.RemoveCard(card.PointValue);
			}

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit()) continue;

				foreach (Card card in hand)
				{
					cardCounter.RemoveCard(card.PointValue);
				}
			}
		}

		public override void ResetShoe(Game game)
		{
			cardCounter.Reset();
		}

		public override void DealCard(Card card)
		{
			cardCounter.RemoveCard(card.PointValue);
		}
	};
}
