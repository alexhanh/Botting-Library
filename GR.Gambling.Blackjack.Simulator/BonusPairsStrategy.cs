using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	class BonusPairsStrategy : Agent
	{
		BonusPairsAgent agent;

		public BonusPairsStrategy(int min_bet, int max_bet, double ev_cutoff)
		{
			agent = new BonusPairsAgent(min_bet/100, max_bet/100, ev_cutoff, null);
		}

		private CardSet GetSeenCards(Game game, bool showdown)
		{
			CardSet seen_cards = new CardSet();

			if (showdown)
			{
				foreach (Card card in game.DealerHand)
				{
					seen_cards.Add(card);
				}
			}
			else
			{
				seen_cards.Add(game.DealerHand[0]);
			}

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit()) continue;

				foreach (Card card in hand)
				{
					seen_cards.Add(card);
				}
			}

			return seen_cards;
		}

		public override ActionType GetBestAction(Game game)
		{
			return GetActions(game)[0].Action;
		}

		public override List<ActionEv> GetActions(Game game)
		{
			List<ActionType> allowed_actions = new List<ActionType>();

			if (game.IsValidAction(ActionType.Stand)) allowed_actions.Add(ActionType.Stand);
			if (game.IsValidAction(ActionType.Hit)) allowed_actions.Add(ActionType.Hit);
			if (game.IsValidAction(ActionType.Double)) allowed_actions.Add(ActionType.Double);
			if (game.IsValidAction(ActionType.Split)) allowed_actions.Add(ActionType.Split);
			if (game.IsValidAction(ActionType.Surrender)) allowed_actions.Add(ActionType.Surrender);


			CardSet some_cards = new CardSet();

			//Hand active_hand = game.PlayerHandSet.ActiveHand;

			List<CardSet> player_hands = new List<CardSet>();
			foreach (Hand hand in game.PlayerHandSet)
			{
				player_hands.Add(hand.Cards);
			}

			return agent.GetActions(GetSeenCards(game, false), game.DealerHand[0], player_hands.ToArray(), game.PlayerHandSet.ActiveIndex, allowed_actions);
		}

		public override bool TakeInsurance(Game game)
		{
			return agent.TakeInsurance(GetSeenCards(game, false));
		}

		// called when the round has ended
		public override void Showdown(Game game)
		{
			//agent.RoundOver(GetSeenCards(game, true));
		}

		// ask for a bet amount in the beginning of the round
		public override int Bet(Game game)
		{
			return agent.Bet(10000) * 100;
		}
	}
}
