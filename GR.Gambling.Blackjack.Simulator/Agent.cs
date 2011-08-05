using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BjEval;

namespace GR.Gambling.Blackjack
{
	public abstract class Agent
	{
		public virtual double ShoeEV() { return 0; }

		public abstract ActionType GetBestAction(Game game);
		public virtual List<ActionEv> GetActions(Game game) { return null; }

		public abstract bool TakeInsurance(Game game);

		// called when the round has ended
		public abstract void Showdown(Game game);

		public virtual void ResetShoe(Game game) { }
		public virtual void DealCard(Card card) { }


		// ask for a bet amount in the beginning of the round
		public abstract int Bet(Game game);
	}

	public struct ActionEv
	{
		public ActionType Action;
		public double Ev;

		public override string ToString()
		{
			if (Action == ActionType.None) return "-";
			return string.Format("{0,-9} {1:0.00000}", Action, Ev);
		}
	}



	class CmdTester : Agent
	{
		public override void Showdown(Game game)
		{
			Console.WriteLine("##Showdown##");
			Console.WriteLine("Dealer's hand: " + game.DealerHand + " (" + game.DealerHand.PointCount() + ")");
			Console.WriteLine("Number of splits = " + game.SplitCount);
			int i=0;
			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit())
					Console.Write("(S) ");
				Console.WriteLine("Hand " + i + ": " + hand + " (" + hand.PointCount() + ")");
				i++;
			}
		}

		public override ActionType GetBestAction(Game game)
		{
			Console.WriteLine("Upcard: " + game.Upcard() + " | Hand: " +
							  game.PlayerHandSet.ActiveHand + " (" +
							  game.PlayerHandSet.ActiveHand.PointCount() + ")");

			if (game.IsValidAction(ActionType.Hit))
				Console.Write("(H)it ");
			if (game.IsValidAction(ActionType.Stand))
				Console.Write("(S)tand ");
			if (game.IsValidAction(ActionType.Surrender))
				Console.Write("Sur(r)ender ");
			if (game.IsValidAction(ActionType.Split))
				Console.Write("S(p)lit ");
			if (game.IsValidAction(ActionType.Double))
				Console.Write("(D)ouble ");

			string input = Console.ReadLine();

			if (input[0] == 'h') return ActionType.Hit;
			if (input[0] == 's') return ActionType.Stand;
			if (input[0] == 'r') return ActionType.Surrender;
			if (input[0] == 'p') return ActionType.Split;
			if (input[0] == 'd') return ActionType.Double;

			Console.WriteLine("Ops");
			return ActionType.Surrender;
		}

		public override bool TakeInsurance(Game game)
		{
			Console.WriteLine("Insurance offered, take?");
			string input = Console.ReadLine();

			if (input.StartsWith("y"))
				return true;

			return false;
		}

		public override int Bet(Game game)
		{
			return game.Rules.MinBet;
		}
	}



	class PseudoOptStrategy : Agent
	{
		private int[] counts = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		private Process shell;

		private struct ActionEv
		{
			public ActionType Action;
			public double Ev;
		}

		public PseudoOptStrategy()
		{
			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "bjopt.exe";
			info.UseShellExecute = false;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;

			shell = Process.Start(info);
		}

		private ActionEv Ev(string action)
		{
			ActionEv action_ev = new ActionEv();

			shell.StandardInput.WriteLine(action);

			string line = shell.StandardOutput.ReadLine();

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
				Console.WriteLine("OU OU");

			return action_ev;
		}

		private List<ActionEv> Evaluate(Game game)
		{
			shell.StandardInput.WriteLine("h");
			for (int i = 0; i < 9; i++)
				shell.StandardInput.WriteLine(32 - counts[i]);
			shell.StandardInput.WriteLine(128 - counts[9]);

			shell.StandardInput.WriteLine(game.Upcard().PointValue);

			shell.StandardInput.WriteLine(game.PlayerHandSet.ActiveHand.Count);

			foreach (Card card in game.PlayerHandSet.ActiveHand)
				shell.StandardInput.WriteLine(card.PointValue);

			List<ActionEv> actions = new List<ActionEv>();

			if (game.IsValidAction(ActionType.Hit))
				actions.Add(Ev("h"));
			if (game.IsValidAction(ActionType.Stand))
				actions.Add(Ev("t"));
			if (game.IsValidAction(ActionType.Double))
				actions.Add(Ev("d"));
			if (game.IsValidAction(ActionType.Split))
				actions.Add(Ev("s"));
			if (game.IsValidAction(ActionType.Surrender))
				actions.Add(Ev("S"));

			shell.StandardInput.WriteLine("q");

			actions.Sort(delegate(ActionEv ae1, ActionEv ae2) { return ae2.Ev.CompareTo(ae1.Ev); });

			/*Console.WriteLine("Upcard: " + upcard + " Hand: " + hand);
			foreach (ActionEv ae in actions)
				Console.WriteLine(ae.Action + " " + ae.Ev);
			Console.ReadKey();*/

			return actions;
		}

		public double shoe_expectation;
		public override int Bet(Game game)
		{
			shell.StandardInput.WriteLine("s");
			for (int i=0; i<9; i++)
				shell.StandardInput.WriteLine(32 - counts[i]);
			shell.StandardInput.WriteLine(128 - counts[9]);

			double shoe_ev = double.Parse(shell.StandardOutput.ReadLine());
			shoe_expectation = shoe_ev;
			Console.WriteLine("shoe ev: " + shoe_ev);

			if (shoe_ev <= 0.0)
				return game.Rules.MinBet;
			else
				return 5000;
		}

		public override void Showdown(Game game)
		{
			// update count
			foreach (Card card in game.DealerHand)
				counts[card.PointValue - 1]++;

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (hand.IsSplit())
					continue;

				foreach (Card card in hand)
					counts[card.PointValue - 1]++;
			}

			// reset count when enough cards dealt
			if ((game.Rules.Decks * 52 - game.Shoe.Count) >= 84)
			{
				for (int i = 0; i < 10; i++)
					counts[i] = 0;
			}
		}

		public override ActionType GetBestAction(Game game)
		{
			Console.WriteLine(game.PlayerHandSet.ActiveHand);
			List<ActionEv> actions = Evaluate(game);
			foreach (ActionEv ae in actions)
				Console.WriteLine(ae.Action + " " + ae.Ev);

			foreach (ActionEv ae in actions)
			{
				if (game.IsValidAction(ae.Action))
					return ae.Action;
				else
				{
					Console.WriteLine("OMGGG " + ae.Action);
				}
			}
			Console.WriteLine("BIG FUCCCCKKK");

			return ActionType.Split;
		}

		public override bool TakeInsurance(Game game)
		{
			// the number of seen tens
			int tens_count = counts[9];

			// check if newly dealt player hand has tens and add them to the count
			if (game.PlayerHandSet.ActiveHand[0].IsTenValue())
				tens_count++;
			if (game.PlayerHandSet.ActiveHand[1].IsTenValue())
				tens_count++;

			// switch to number of tens still in shoe
			tens_count = game.Rules.Decks * 4 * 4 - tens_count;

			// the -1 unknown comes from the ace we know the dealer has
			if (((double)tens_count / (double)(game.Shoe.Count - 1)) > (1.0 / 3.0))
				return true;

			return false;
		}
	}

	class AlwaysHitAgent : Agent
	{
		public override int Bet(Game game)
		{
			return game.Rules.MinBet;
		}

		public override ActionType GetBestAction(Game game)
		{
			return ActionType.Hit;
		}

		public override void Showdown(Game game)
		{
		}

		public override bool TakeInsurance(Game game)
		{
			return false;
		}
	}


}
