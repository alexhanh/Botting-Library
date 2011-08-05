using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GR.Gambling.Blackjack.Betting;

namespace GR.Gambling.Blackjack
{
	class BasicStrategy : Agent
	{
		private int max_bet;
		private double ev_cutoff;
		private double pp_multiplier;

		private BettingSystem betting_system = null;

		private CardCounter card_counter;

		private Process shell;

		private int current_roll = 0;

		public int CurrentRoll { get { return current_roll; } set { current_roll = value; } }

		public BasicStrategy(int max_bet, double ev_cutoff, double pp_multiplier)
		{
			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "bjtest.exe";
			info.UseShellExecute = false;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;

			shell = Process.Start(info);

			// wait until it says "enter"
			string line = shell.StandardOutput.ReadLine();
			if (line.StartsWith("ready"))
				Console.WriteLine("evaluator ready");

			this.ev_cutoff = ev_cutoff;
			this.max_bet = max_bet;
			this.pp_multiplier = pp_multiplier;


			card_counter = new CardCounter(pp_multiplier);
		}

		public BasicStrategy(int max_bet, double ev_cutoff, double pp_multiplier, BettingSystem betting_system)
			: this(max_bet, ev_cutoff, pp_multiplier)
		{
			this.betting_system = betting_system;
		}

		public void Stop()
		{
			shell.Kill();
		}

		public override double ShoeEV()
		{
			return card_counter.CurrentEV;
		}

		private List<ActionEv> Evaluate(Card upcard, Hand hand)
		{
			shell.StandardInput.WriteLine(upcard.PointValue);
			shell.StandardInput.WriteLine(hand.Count);

			foreach (Card card in hand)
				shell.StandardInput.WriteLine(card.PointValue);

			string line = "";
			List<ActionEv> actions = new List<ActionEv>();
			while (true)
			{
				line = shell.StandardOutput.ReadLine();

				if (line.StartsWith("done"))
					break;

				ActionEv action_ev = new ActionEv();

				string[] param = line.Split(new char[] { ' ' });

				action_ev.Ev = double.Parse(param[1]);
				if (param[0] == "Surrender")
					action_ev.Action = ActionType.Surrender;
				else if (param[0] == "Hit")
					action_ev.Action = ActionType.Hit;
				else if (param[0] == "Stand")
					action_ev.Action = ActionType.Stand;
				else if (param[0] == "Double")
					action_ev.Action = ActionType.Double;
				else if (param[0] == "Split")
					action_ev.Action = ActionType.Split;
				else
					Console.WriteLine("BIG FUCKING UPS!!!!!");

				actions.Add(action_ev);
			}

			actions.Sort(delegate(ActionEv ae1, ActionEv ae2) { return ae2.Ev.CompareTo(ae1.Ev); });

			/*Console.WriteLine("Upcard: " + upcard + " Hand: " + hand);
			foreach (ActionEv ae in actions)
				Console.WriteLine(ae.Action + " " + ae.Ev);
			Console.ReadKey();*/

			return actions;
		}

		public override int Bet(Game game)
		{
			// unseen tells how many cards still to come before shuffle
			//int unseen = 84 - (game.Rules.Decks * 52 - game.Shoe.Count);
			//int true_count = (int)Math.Round(((double)running_count / (double)unseen));

			if (card_counter.CurrentEV > ev_cutoff)
			{
				if (betting_system != null)
				{
					return betting_system.BetSize(card_counter.CurrentEV, current_roll);
				}
				
				return max_bet;
			}
			else
				return game.Rules.MinBet;
		}


		public override void Showdown(Game game)
		{
			// update count
			foreach (Card card in game.DealerHand)
			{
				card_counter.RemoveCard(card.PointValue);
			}

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit())
					continue;

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

		public override ActionType GetBestAction(Game game)
		{
			List<ActionEv> actions = Evaluate(game.Upcard(), game.PlayerHandSet.ActiveHand);

			foreach (ActionEv ae in actions)
			{
				if (game.IsValidAction(ae.Action))
					return ae.Action;
				else
				{
					//Console.WriteLine("OMGGG " + ae.Action);
				}
			}
			Console.WriteLine("BIG FUCCCCKKK");

			return ActionType.Split;
		}

		// should +ev in some cases, study
		// insurance should be taken when probability of dealer having
		// blackjack is greater than 1/3 (0.3333333%), i guess because 
		// of how the payoff goes
		public override bool TakeInsurance(Game game)
		{
			// the number of seen tens
			int tens_count = card_counter[10];

			// check if newly dealt player hand has tens and add them to the count
			if (game.PlayerHandSet.ActiveHand[0].IsTenValue())
				tens_count++;
			if (game.PlayerHandSet.ActiveHand[1].IsTenValue())
				tens_count++;

			// switch to number of tens still in shoe
			tens_count = game.Rules.Decks * 4 * 4 - tens_count;

			// the +1 comes from the dealer's unknown which has been removed from the shoe
			if (((double)tens_count / (double)(game.Shoe.Count + 1)) + pp_multiplier * 0.0002 > (1.0 / 3.0))
				return true;

			return false;
		}
	}
}
