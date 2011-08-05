using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace GR.Gambling.Blackjack
{
	public class GameLogger
	{
		private struct ActionInfo
		{
			public int hand_index;
			public CardSet player_cards;

			public ActionEv[] action_evs;
		}

		Shoe shoe;
		double shoe_ev;
		int bet_size;
		bool take_insurance = false;

		List<ActionInfo> action_history;

		public GameLogger()
		{
		}

		public void StartGame(Shoe shoe, double shoe_ev, int bet_size)
		{
			this.take_insurance = false;
			this.shoe = shoe.Copy();
			this.shoe_ev = shoe_ev;
			this.bet_size = bet_size;
			this.action_history = new List<ActionInfo>();
		}

		public void Action(Card dealer_upcard, CardSet[] player_hands, int active_hand_index, List<ActionEv> actions)
		{
			ActionInfo info = new ActionInfo() {
				hand_index = active_hand_index, 
				player_cards = player_hands[active_hand_index], 
				action_evs = actions.ToArray()
			};

			action_history.Add(info);
		}

		public void Insurance(bool take)
		{
			take_insurance = take;
		}

		public void Showdown(CardSet dealer_hand, CardSet[] player_hands, long game_id, int expected_money, int actual_money)
		{
			if (game_id <= 0)
			{
				long ticks = DateTime.Now.ToUniversalTime().Ticks;
				TimeSpan time = new TimeSpan(ticks);
				game_id = ((long)(time.TotalSeconds * 10)) % (int.MaxValue);
			}

			TextWriter file = new StreamWriter("gamelog.txt", true);

			file.WriteLine("GameStart {0}", game_id);
			file.WriteLine("Time {0}", DateTime.Now.ToUniversalTime().ToString(new CultureInfo("en-US", true)));
			file.WriteLine("Bet {0}", bet_size);
			file.WriteLine("Shoe {0}", shoe);
			file.WriteLine("ShoeEv {0}", shoe_ev);
			file.WriteLine("Insurance {0}", take_insurance);

			foreach (ActionInfo action in action_history)
			{
				file.Write("Action {0} | {1}", action.hand_index, action.player_cards);

				foreach (ActionEv ev in action.action_evs)
				{
					file.Write(" | {0} {1}", ev.Action, ev.Ev);
				}

				file.WriteLine();
			}

			for (int i=0; i<player_hands.Length; i++)
			{
				file.WriteLine("Showdown {0} | {1}", i, player_hands[i]);
			}

			file.WriteLine("Dealer {0}", dealer_hand);
			file.WriteLine("ExpectedMoney {0}", expected_money);
			file.WriteLine("ActualMoney {0}", actual_money);
			file.WriteLine("GameEnd {0}", game_id);
			file.WriteLine();

			file.Close();

			Console.WriteLine("Game ID: " + game_id);
			Console.WriteLine("Expected: " + expected_money);
			Console.WriteLine("Actual:   " + actual_money);
		}
	}
}
