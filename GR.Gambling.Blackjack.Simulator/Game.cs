using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public enum ActionType
	{
		None,
		Stand,
		Hit,
		Double,
		Split,
		Surrender
	}

	public class Rules
	{
		public int Decks { get; set; }

		// total of allowed splits (split + resplits)
		public int Splits { get; set; }

		public int MinBet { get; set; }
		public int MaxBet { get; set; }
	}

	public class Game
	{
		private Hand dealer_hand;
		private HandSet player_handset;
		private CardSet shoe;

		// the playing ai agent
		private Agent agent;
		// player's total money on table in cents
		private int player_money;
		// initial wager in this round
		private int bet;

		private double pp_multiplier;
		public double party_points;
		private int split_count;

		private Rules rules;

		private Random random;

		public Game(Rules rules, Agent agent, double pp_multiplier, Random random)
		{
			dealer_hand = new Hand();
			player_handset = new HandSet();
			shoe = new CardSet();

			this.rules = rules;
			this.agent = agent;
			this.pp_multiplier = pp_multiplier;
			this.random = random;

			this.player_money = 0;
			this.bet = rules.MinBet;
			this.split_count = 0;

			party_points = 0;

			ResetShoe();
		}

		public Hand DealerHand { get { return dealer_hand; } }
		public HandSet PlayerHandSet { get { return player_handset; } }
		public CardSet Shoe { get { return shoe; } }
		public int Bet { get { return bet; } set { bet = value; } }
		public int SplitCount { get { return split_count; } }
		public int PlayerMoney { get { return player_money; } set { player_money = value; } }
		public Rules Rules { get { return rules; } }
		public Random Random { get { return random; } set { random = value; } }

		public int PartyPoints { get { return (int)party_points; } }
		public int TotalPartyPoints { get { return (int)(party_points * pp_multiplier); } }

		public bool CanSplit()
		{
			// split is available as long as split cap hasn't been
			// reached and there's no other action done and the hand is a pair
			if (split_count < rules.Splits &&
				!player_handset.ActiveHand.Acted &&
				player_handset.ActiveHand.IsPair())
				return true;
			return false;
		}

		private void AddPartyPoints(int wager)
		{
			party_points += wager/2500.0;
		}

		public bool CanSurrender()
		{
			// surrender only available on the first two cards 
			// and when no other action has been yet made
			if (player_handset.HandCount == 1 && !player_handset.ActiveHand.Acted)
				return true;

			return false;
		}

		public Card Upcard()
		{
			return dealer_hand[0];
		}

		public void StartRound()
		{
			dealer_hand.Reset();
			player_handset.Reset();

			// ask the player how much he wants to bet
			bet = agent.Bet(this);

			// collect initial wager from the player
			player_money -= bet;
			AddPartyPoints(bet);

			split_count = 0;
		}

		public void DealRound()
		{
			player_handset.ActiveHand.AddCard(shoe.ExtractTop());
			dealer_hand.AddCard(shoe.ExtractTop());
			player_handset.ActiveHand.AddCard(shoe.ExtractTop());
			dealer_hand.AddCard(shoe.ExtractTop());


			bool dealer_natural;
			dealer_natural = dealer_hand.IsNatural();

			// check if player has blackjack
			if (player_handset.ActiveHand.IsNatural())
			{
				if (dealer_natural)
				{
					// push, return initial bet
					player_money += bet;
					EndRound();
					return;
				}
				else
				{
					// player wins with blackjack, pay out 1.5:1 (with 10$ at stake, return 15$)
					player_money += (int)(1.5 * bet) + bet;
					EndRound();
					return;
				}
			}

			int insurance = 0;
			// offer insurance
			if (Upcard().IsAce())
			{
				if (agent.TakeInsurance(this))
				{
					insurance = (int)(0.5 * bet);
					player_money -= insurance;
					AddPartyPoints(insurance);
				}
			}

			// dealer has blackjack
			if (dealer_natural)
			{
				if (insurance > 0)
				{
					//??????
					// insurance bet pays 2:1, initial bet is lost
					player_money += (2 * insurance)+insurance;
					EndRound();
					return;
				}
				else
				{
					// dealer wins with blackjack
					EndRound();
					return;
				}
			}

			// main loop
			// keeps asking player for actions for unfinished hands until
			// no unfinished hands remains
			while (!player_handset.AllFinished())
			{
				ActionType action = agent.GetBestAction(this);

				if (!IsValidAction(action))
				{
					Console.WriteLine("Player made invalid action!");
					return;
				}

				// player decides to stand, hand is over, move to next one
				if (ActionType.Stand == action)
				{
					player_handset.ActiveHand.Stand();
				} 
				else if (ActionType.Hit == action)
				{
					player_handset.ActiveHand.Hit(shoe.ExtractTop());
				}
				else if (ActionType.Surrender == action)
				{
					player_handset.ActiveHand.Surrender();
				}
				else if (ActionType.Double == action)
				{
					player_handset.ActiveHand.Double(shoe.ExtractTop());
					player_money -= bet;
					AddPartyPoints(bet);
				}
				else if (ActionType.Split == action)
				{
					player_handset.Split(shoe.ExtractTop(), shoe.ExtractTop());
					player_money -= bet;
					AddPartyPoints(bet);
					split_count++;
				}
				else
				{
					Console.WriteLine("Unknown action");
				}

				player_handset.NextActiveHand();
			}

			// each hand should be here in either some of these states:
			// busted, standed, doubled, surrendered

			// if all hands are busted, dealer doesn't draw, only reveals his hand and the round is over
			if (player_handset.AllBusted())
			{
				EndRound();
				return;
			}

			// player surrendered
			if (player_handset.ActiveHand.Surrendered)
			{
				// return half of the initial bet
				player_money += (int)(0.5 * bet);
				EndRound();
				return;
			}

			// handle the dealer logic
			while (true)
			{
				// must hit on soft 17's
				if (dealer_hand.HasAce() && dealer_hand.SoftTotal() == 17)
				{
					dealer_hand.AddCard(shoe.ExtractTop());
					continue;
				}

				// finish condition
				if (dealer_hand.PointCount() >= 17)
				{
					break;
				}

				dealer_hand.AddCard(shoe.ExtractTop());
			}

			// payouts
			foreach (Hand hand in player_handset)
			{
				// skip split hands
				if (hand.IsSplit())
					continue;

				int hand_bet = (hand.Doubled) ? (bet * 2) : bet;

				if (dealer_hand.IsBust())
				{
					/*
					// both busted, it's a push
					if (hand.IsBust())
						player_money += hand_bet;
					else // not busted hand wins dealer's busted hand 2:1 payout
						player_money += 2 * hand_bet;
					 */

					if (!hand.IsBust())
					{
						player_money += 2 * hand_bet;
					}
				}
				else
				{
					// do nothing, we lose our bet
					if (hand.IsBust())
					{
					}
					else
					{
						int player_total = hand.PointCount();
						int dealer_total = dealer_hand.PointCount();

						if (player_total < dealer_total)
						{
							// do nothing, we lose our bet
						} // we win, 2:1 payout
						else if (dealer_total < player_total)
						{
							player_money += 2 * hand_bet;
						} // push
						else
						{
							player_money += hand_bet;
						}
					}
				}
			}

			// round is over
			EndRound();
		}

		public void EndRound()
		{
			agent.Showdown(this);

			// shuffle if the condition met
			// 20% of cards used by the end of the hand
			if ((rules.Decks * 52 - shoe.Count) >= 84)
				ResetShoe();
		}

		public bool IsValidAction(ActionType action)
		{
			// can always hit
			if (action == ActionType.Hit)
				return true;

			// can always stand
			if (action == ActionType.Stand)
				return true;

			// surrender only available on the first two cards 
			// and when no other action has been yet made
			if (action == ActionType.Surrender)
			{
				if (player_handset.HandCount == 1 && !player_handset.ActiveHand.Acted)
					return true;

				return false;
			}

			// double is available as a first action on hand
			// and the hand isn't blackjack, or on splits 
			if (action == ActionType.Double)
			{
				if (!player_handset.ActiveHand.Acted)
					return true;

				return false;
			}

			// split is available as long as split cap hasn't been
			// reached and there's no other action done and the hand is a pair
			if (action == ActionType.Split)
			{
				if (split_count < rules.Splits &&
					!player_handset.ActiveHand.Acted &&
					player_handset.ActiveHand.IsPair())
					return true;
				return false;
			}

			// should never reach here
			return false;
		}

		public void ResetShoe()
		{
			shoe.Clear();

			int decks = rules.Decks;
			for (int i = 0; i < decks; i++)
				shoe.Add(CardsFactory.StandardDeck);

			shoe.Shuffle(random);

			agent.ResetShoe(this);
		}

		public void RemoveCards(int count)
		{
			for (int i = 0; i < count; i++)
			{
				Card c = shoe.ExtractTop();
				agent.DealCard(c);
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Shoe: " + shoe.ToCountsString());
			result.AppendLine();

			result.Append(string.Format("Dealer:   {0,-12} = {1}", dealer_hand, dealer_hand.PointCount()));
			if (dealer_hand.IsNatural()) result.Append(" (BJ)");
			if (dealer_hand.IsBust()) result.Append(" (busted)");
			result.AppendLine();

			result.AppendLine();

			result.AppendLine(player_handset.ToString());

			return result.ToString();
		}
	}
}
