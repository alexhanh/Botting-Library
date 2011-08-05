using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Blackjack;
using BjEval;
using System.IO;
using GR.Gambling.Blackjack.Betting;
using GR.Gambling.Blackjack.Common;

namespace GR.Gambling.Blackjack
{
	public class BonusPairsAgent : IAgent
	{
		protected int min_bet;
		protected int max_bet;
		protected double ev_cutoff;

		protected int current_bet;
		protected Shoe shoe;

		protected BettingSystem betting_system;

		protected int max_splits = 3;
		protected int split_count;

		int action_count = 0;

		protected double shoe_ev = 0;
		bool ev_evaluated = false;

		bool[] hand_doubled = new bool[4];
		bool surrendered = false;
		bool insurance_taken = false;

		int roll_before = 0;

		long last_game_id = 0;
		protected GameLogger game_logger;

		public BonusPairsAgent(int min_bet, int max_bet, double ev_cutoff, BettingSystem betting_system)
		{
			this.min_bet = min_bet;
			this.max_bet = max_bet;
			this.current_bet = max_bet;
			this.ev_cutoff = ev_cutoff;
			this.betting_system = betting_system;

			this.shoe = new Shoe(8);

			game_logger = new GameLogger();

			InitializeRound();
		}

		private void InitializeRound()
		{
			Console.WriteLine();
			action_count = 0;
			split_count = 0;
			ev_evaluated = false;

			for (int i = 0; i < 4; i++) hand_doubled[i] = false;
			surrendered = false;
			insurance_taken = false;
		}

		// seen_cards include active_hand's cards
		public override ActionType GetAction(CardSet seen_cards, Card dealer_upcard, CardSet[] player_hands, int active_hand, List<ActionType> available_actions)
		{
			List<ActionEv> actions = GetActions(seen_cards, dealer_upcard, player_hands, active_hand, available_actions);

			ActionType best = actions[0].Action;

			if (best == ActionType.Split)
			{
				split_count++;
			}
			else if (best == ActionType.Double)
			{
				hand_doubled[active_hand] = true;
			}
			else if (best == ActionType.Surrender)
			{
				surrendered = true;
			}

			action_count++;

			return best;
		}

		public void ValidateActions(CardSet active_hand, List<ActionType> available_actions)
		{
			if (!available_actions.Contains(ActionType.Stand)) throw new Exception("ValidateActions failed: no stand");
			if (!available_actions.Contains(ActionType.Hit)) throw new Exception("ValidateActions failed: no hit");

			if (active_hand.Count == 2)
			{
				if (!available_actions.Contains(ActionType.Double)) throw new Exception("ValidateActions failed: no double");

				if (active_hand[0].PointValue == active_hand[1].PointValue && split_count < max_splits)
				{
					if (!available_actions.Contains(ActionType.Split)) throw new Exception("ValidateActions failed: no split");
				}

				if (split_count == 0)
				{
					if (!available_actions.Contains(ActionType.Surrender)) throw new Exception("ValidateActions failed: no surrender");
				}
			}
		}

		public List<ActionEv> GetActions(CardSet seen_cards, Card dealer_upcard, CardSet[] player_hands, int active_hand, List<ActionType> available_actions)
		{
			ValidateActions(player_hands[active_hand], available_actions);

			Shoe tmp_shoe = shoe.Copy();
			tmp_shoe.Remove(seen_cards);

			Eval.CacheDealerProbs(dealer_upcard.PointValue, tmp_shoe.ToArray());

			List<ActionEv> actions = new List<ActionEv>();

			foreach (ActionType a in available_actions)
			{
				double ev = GetActionEV(tmp_shoe, player_hands[active_hand], a, dealer_upcard);

				actions.Add(new ActionEv() { Action = a, Ev = ev });
			}

			actions.Sort(delegate(ActionEv ae1, ActionEv ae2) { return ae2.Ev.CompareTo(ae1.Ev); });

			game_logger.Action(dealer_upcard, player_hands, active_hand, actions);

			return actions;
		}

		public override bool TakeInsurance(CardSet seen_cards)
		{
			Shoe tmp_shoe = shoe.Copy();
			tmp_shoe.Remove(seen_cards);

			double insurance_ev = Eval.InsuranceEv(current_bet, tmp_shoe.ToArray());

			if (insurance_ev >= 0.0)
			{
				game_logger.Insurance(true);
				insurance_taken = true;
				return true;
			}

			return false;
		}

		public override void RoundOver(CardSet seen_cards, CardSet dealer_hand, CardSet[] player_hands, long game_id, int roll_after)
		{
			Hand dealer = new Hand(dealer_hand);

			Hand[] player = new Hand[player_hands.Length];
			for (int i = 0; i < player.Length; i++)
			{
				player[i] = new Hand(player_hands[i]);
				player[i].Doubled = hand_doubled[i];
			}

			int actual_money = roll_after - roll_before;
			int expected_money = ExpectedMoney(dealer, player);

			if (game_id > 0 && game_id == last_game_id)
			{
				throw new Exception("game_id == last_game_id");
			}

			if (action_count == 0)
			{
				bool dealer_natural = dealer.IsNatural();
				bool player_natural = player.Count() == 1 && player[0].IsNatural();

				if (!dealer_natural && !player_natural)
				{
					throw new Exception("No actions made and no BJ");
				}
				else
				{
					if (actual_money == 0)
					{
						if (dealer_natural && insurance_taken)
						{
							// this is correct
						}
						else if (!(dealer_natural && player_natural))
						{
							throw new Exception("BJ but no roll change (and no push)");
						}
					}
				}
			}

			game_logger.Showdown(dealer_hand, player_hands, game_id, expected_money, actual_money);
			Console.WriteLine("Roll: " + roll_after);

			last_game_id = game_id;

			if (expected_money != actual_money)
			{
				if (Config.Current.GetBooleanProperty("ThrowMoneyException"))
				{
					throw new MoneyMismatchException(expected_money, actual_money);
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine("EXPECTED MONEY MISMATCH!");
					Console.WriteLine();
				}
			}

			shoe.Remove(seen_cards);

			if (shoe.FullCount - shoe.CardCount >= 84)
			{
				shoe.Reset();
			}

			Console.WriteLine("Seen cards: " + seen_cards);
			Console.WriteLine("Removed from shoe: {0} ({1})", shoe.FullCount - shoe.CardCount, seen_cards.Count);

			InitializeRound();
		}


		private void EvaluateEV()
		{
			shoe_ev = Eval.ShoeEv(shoe.ToArray(), max_bet);
			ev_evaluated = true;
			
			Console.WriteLine("Shoe EV: " + shoe_ev);
		}

		// ask for a bet amount in the beginning of
		public override int Bet(int roll_before)
		{
			this.roll_before = roll_before;

			if (!ev_evaluated)
			{
				EvaluateEV();
			}


			if (shoe_ev > ev_cutoff)
			{
				if (betting_system != null) current_bet = betting_system.BetSize(shoe_ev, 25000);
				else current_bet = max_bet;
			}
			else current_bet = min_bet;


			game_logger.StartGame(shoe, shoe_ev, 100 * current_bet);

			return current_bet;
		}

		public override bool ResetShoe(bool forced) 
		{
			if (!forced) EvaluateEV();

			if (forced || shoe_ev < -0.0068)
			{
				shoe.Reset();
				InitializeRound();

				return true;
			}

			return false;
		}

		private double GetActionEV(Shoe tmp_shoe, CardSet active_hand, ActionType action, Card dealer_upcard)
		{
			Hand hand = new Hand(active_hand);

			SHand shand;
			int soft_total = hand.SoftTotal();
			if (soft_total <= 21 && hand.HasAce())
			{
				shand.Total = soft_total;
				shand.Soft = true;
			}
			else
			{
				shand.Total = hand.HardTotal();
				shand.Soft = false;
			}

			int[] shoe_counts = tmp_shoe.ToArray();
			int upcard = dealer_upcard.PointValue;

			switch (action)
			{
				case ActionType.Stand:

					return Eval.StandEv(shand, upcard, shoe_counts);

				case ActionType.Hit:

					return Eval.HitEv(shand, upcard, shoe_counts);

				case ActionType.Double:

					return Eval.DoubleEv(shand, upcard, current_bet, shoe_counts);

				case ActionType.Split:

					return Eval.SplitEv(active_hand[0].PointValue, upcard, current_bet, max_splits - split_count, shoe_counts);

				case ActionType.Surrender:

					return Eval.SurrenderEv();
			}

			return -1;
		}

		private int ExpectedMoney(Hand dealer, Hand[] player)
		{
			int game_bet = 100 * current_bet;
			int expected_money = 0;

			if (insurance_taken)
			{
				expected_money -= game_bet / 2;

				if (dealer.IsNatural()) expected_money += 3 * game_bet / 2;
			}

			if (surrendered)
			{
				expected_money -= game_bet / 2;
			}
			else
			{
				foreach (Hand hand in player)
				{
					int hand_bet = hand.Doubled ? (game_bet * 2) : game_bet;

					expected_money -= hand_bet;

					if (dealer.IsNatural())
					{
						if (hand.IsNatural()) expected_money += hand_bet;
					}
					else if (split_count == 0 && hand.IsNatural())
					{
						expected_money += 2 * hand_bet + hand_bet / 2;
					}
					else if (dealer.IsBust())
					{
						if (!hand.IsBust())
						{
							expected_money += 2 * hand_bet;
						}
					}
					else if (!hand.IsBust())
					{
						int player_total = hand.PointCount();
						int dealer_total = dealer.PointCount();

						if (player_total < dealer_total)
						{
							// do nothing, we lose our bet
						} // we win, 2:1 payout
						else if (dealer_total < player_total)
						{
							expected_money += 2 * hand_bet;
						} // push
						else
						{
							expected_money += hand_bet;
						}
					}
				}
			}

			return expected_money;
		}
	}
}
