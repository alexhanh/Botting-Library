using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public abstract class IAgent
	{
		public virtual bool ResetShoe(bool forced) { return false; }

		// seen_cards include active_hand's cards
		public abstract ActionType GetAction(CardSet seen_cards, Card dealer_upcard, CardSet[] player_hands, int active_hand, List<ActionType> available_actions);

		public abstract bool TakeInsurance(CardSet seen_cards);

		// ask for a bet amount in the beginning of the round
		public abstract int Bet(int roll_before);

		// called when the round has ended
		public virtual void RoundOver(CardSet seen_cards, CardSet dealer_hand, CardSet[] player_hands, long game_id, int roll_after) { }
	}
}
