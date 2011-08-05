using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Threading;
using BjEval;
using GR.Gambling.Blackjack.Betting;

// only the ranks of the cards matter
// A=0,2=1,...,9=8,T=9,J=9,Q=9,K=9

// Ace is 1 or 11
namespace GR.Gambling.Blackjack
{
	class Program
	{
		interface ResetSystem
		{
			bool Reset(int cards_dealt, double shoe_ev);
		}

		class FixedReset : ResetSystem
		{
			double reset_ev;

			public FixedReset(double reset_ev)
			{
				this.reset_ev = reset_ev;
			}

			public bool Reset(int cards_dealt, double shoe_ev)
			{
				if (shoe_ev < reset_ev) return true;

				return false;
			}
		}

		class SmartReset : ResetSystem
		{
			double start_ev, end_ev;
			int max_dealt;

			public SmartReset(double start_ev, double end_ev, int max_dealt)
			{
				this.start_ev = start_ev;
				this.end_ev = end_ev;
				this.max_dealt = max_dealt;
			}

			public bool Reset(int cards_dealt, double shoe_ev)
			{
				if (cards_dealt > max_dealt) return false;

				double interpolation = cards_dealt / (double)max_dealt;

				double current_ev = start_ev + interpolation * (end_ev - start_ev);

				if (shoe_ev < current_ev) return true;

				return false;
			}
		}

		static void Main(string[] args)
		{

			/*for (int roll = 1000; roll <= 50000; roll+=1000)
			{
				Console.WriteLine("{0}: P: {1:######}  K: {2:######}", 
					roll, 
					system.BetSize(0, roll * 100) / 100, 
					kelly.BetSize(0.005, roll) / 100);
			}

			for (double ev = 0; ev <= 0.0085; ev += 0.0005)
			{
				Console.WriteLine("{0:0.0000}: {1}", ev, kelly.BetSize(ev, 25000) / 100);
			}

			return;
			*/
			/*Shoe shoe = new Shoe(8);
			shoe.Remove(new CardSet(new Card[] { new Card("5c"), new Card("8c"), new Card("3h") }));

			BjEval.Eval.CacheDealerProbs(5, shoe.ToArray());
			Console.WriteLine(BjEval.Eval.DoubleEv(new SHand() { Total = 11, Soft = false }, 5, 100, shoe.ToArray()));

			return;
			*/
			//GeneralTest();
			//CompareStrategies();
			//TestSuper();

			/*ProportionalBetting proportional = new ProportionalBetting(225, 25 * 100, 200 * 100);
			KellyBetting kelly = new KellyBetting(0.008, 25 * 100, 200 * 100);

			double ev_cutoff = 0.0015;

			Random random = new NPack.MersenneTwister();
			
			for (int i = 1; i <= 1000000; i++)
			{
				Console.WriteLine("STARTING TEST NUMBER " + i);
				Console.WriteLine();
				MakeTestRun(string.Format("kelly1_{0}.txt", i), random, 200000, ev_cutoff, kelly, new FixedReset(-0.0064));
				MakeTestRun(string.Format("kelly2_{0}.txt", i), random, 200000, ev_cutoff, kelly, new SmartReset(-0.0068, -0.0025, 70));
			}*/

			seed_gen = new NPack.MersenneTwister();

			Thread thread1 = new Thread(new ThreadStart(TestLoop1));
			thread1.Start();

			TestLoop1();
		}

		//static int loop_num = 1;
		/*
		static void BadLoop()
		{
			int num = loop_num;
			loop_num++;

			double ev_cutoff = 0.0;

			Random random = new NPack.MersenneTwister();

			for (int i = 1; i <= 1000000; i++)
			{
				MakeTestRun(string.Format("max200_{0}_{1}.txt", num, i), random, 200000, ev_cutoff, null, new FixedReset(-0.0068));
			}
		}*/

		static object lock_obj = new object();
		static int run_number = 1;

		static Random seed_gen;

		static void TestLoop1()
		{
			KellyBetting kelly = new KellyBetting(0.008, 25 * 100, 25 * 100, 200 * 100);

			double ev_cutoff = 0.0015;

			Random random;

			lock (seed_gen) { random = new NPack.MersenneTwister(seed_gen.Next()); }

			int i = 0;

			while (true)
			{
				lock (lock_obj)
				{
					i = run_number++;
				}

				MakeTestRun(i, random, 200000, ev_cutoff, kelly, new FixedReset(-0.0068));
			}
		}

		/*static void TestLoop2()
		{
			KellyBetting kelly = new KellyBetting(0.008, 1 * 100, 25 * 100, 200 * 100);

			double ev_cutoff = 0.0015;

			Random random = new NPack.MersenneTwister();

			for (int i = 1; i <= 1000000; i++)
			{
				MakeTestRun(string.Format("kelly2_{0}.txt", i), random, 200000, ev_cutoff, kelly, new FixedReset(-0.0068));
			}
		}*/

		static void Write200kResult(double result)
		{
			TextWriter file = new StreamWriter("result200k.txt", true);
			file.WriteLine(result);
			file.Close();
		}

		static void WriteFinalResult(double result)
		{
			TextWriter file = new StreamWriter("result_final.txt", true);
			file.WriteLine(result);
			file.Close();
		}

		static void MakeTestRun(int run_number, Random random, int targetRuns, double ev_cutoff, BettingSystem betting_system, ResetSystem reset)
		{
			Console.WriteLine(run_number + " - EV cutoff " + ev_cutoff + " " + betting_system);
			TextWriter deal_file = new StreamWriter(string.Format("expected{0}.txt", run_number));
			TextWriter roll_file = new StreamWriter(string.Format("roll{0}.txt", run_number));

			double pp_multiplier = 0;

			Rules rules = new Rules { Decks = 8, MinBet = 100, MaxBet = 20000, Splits = 3 };

			//OptStrategy b = new OptStrategy(10000, ev_cutoff, pp_multiplier);
			BasicStrategy b = new BasicStrategy(20000, ev_cutoff, pp_multiplier, betting_system);

			Game game = new Game(rules, b, pp_multiplier, random);

			int start_roll = 20000 * 100;
			//game.PlayerMoney = 10000000; // 100000$
			game.PlayerMoney = start_roll;
			double expected_money = (double)game.PlayerMoney;
			game.Bet = 100; // 1$

			double lowest = TotalMoney(game), highest = TotalMoney(game);
			int runs = 0;

			double total_big_bet_ev = 0;
			double total_big_bet = 0;
			int num_big_bets = 0;
			int resets = 0;

			int reset_counter = 0;

			bool written_200k = false;

			while (true)
			{
				double total_money = TotalMoney(game);

				if (total_money < lowest) lowest = total_money;
				if (total_money > highest) highest = total_money;

				if (runs % 5000 == 0)
				{
					Console.WriteLine(runs + " " + (double)game.PlayerMoney / 100.0 + "$" + " pp: " + game.PartyPoints + " expected: " + expected_money / 100.0 + "$");
					Console.WriteLine("lowest: " + (double)lowest + "$" + " highest: " + (double)highest + "$");
					Console.WriteLine("Total: ${0}", game.PlayerMoney/100.0);
					Console.WriteLine("Expected: ${0}", expected_money/100.0);
					/*Console.WriteLine("Average big bet: {0:0.00} ({1:0.0000})", 
						(total_big_bet / 100.0) / num_big_bets,
						(total_big_bet_ev / 100.0) / num_big_bets);
					Console.WriteLine("Resets {0} ({1:0.000}%)", resets, resets / (double)runs);
					*/
					Console.WriteLine();
				}

				if (game.PlayerMoney <= 0)
				{
					deal_file.WriteLine(expected_money / 100.0);
					roll_file.WriteLine(game.PlayerMoney / 100.0);

					Write200kResult(0);
					WriteFinalResult(0);

					break;
				}

				if (runs % 1000 == 0)
				{
					//file.WriteLine(string.Format("{0} {1} {2} {3} {4} {5}", runs, game.PlayerMoney / 100.0, game.PartyPoints, total_money, lowest, highest));

					deal_file.WriteLine(expected_money / 100.0);
					roll_file.WriteLine(game.PlayerMoney / 100.0);

					if (runs >= targetRuns)
					{
						if (!written_200k)
						{
							Write200kResult(game.PlayerMoney / 100.0);
							written_200k = true;
						}

						if (game.PlayerMoney >= start_roll)
						{
							WriteFinalResult(game.PlayerMoney / 100.0);
							break;
						}
					}

					//if (game.party_points >= 20000) break;
				}

				if (reset_counter > 0)
				{
					reset_counter--;
					runs++;
					continue;
				}
				if (reset!=null && reset.Reset(52 * game.Rules.Decks - game.Shoe.Count, b.ShoeEV()))
				{
					resets++;
					runs++;
					reset_counter = 1;
					game.ResetShoe();

					continue;
				}


				b.CurrentRoll = game.PlayerMoney;

				Shoe shoe = new Shoe(8);
				shoe.Clear();
				shoe.Add(game.Shoe);

				game.StartRound();
				game.DealRound();

				Card p1 = game.PlayerHandSet[0][0], 
					 p2 = game.PlayerHandSet[0][1], 
					 d = game.DealerHand[0];

				shoe.Remove(p1);
				shoe.Remove(p2);
				shoe.Remove(d);

				BjEval.Eval.CacheDealerProbs(d.PointValue, shoe.ToArray());
				double deal_ev = BjEval.Eval.DealEv(
					p1.PointValue,
					p2.PointValue,
					d.PointValue,
					shoe.ToArray(),
					game.Bet);

				//Console.WriteLine("EV: {0} {1} {2} {3} {4}", p1.PointValue, p2.PointValue, d.PointValue, shoe.CardCount, deal_ev);

				expected_money += deal_ev * game.Bet;

				if (game.Bet > 100)
				{
					total_big_bet += game.Bet;
					total_big_bet_ev += game.Bet * deal_ev;
					num_big_bets++;
				}

				runs++;
			}

			deal_file.Close();
			roll_file.Close();

			b.Stop();
		}

		static void GeneralTest()
		{
			Random random = new NPack.MersenneTwister();

			Rules rules = new Rules { Decks = 8, MinBet = 100, MaxBet = 20000, Splits = 3 };

			int max_bet = 5000;
			double pp_multiplier = 4.0;
			double ev_cutoff = 0.0015;

			OptStrategy b1 = new OptStrategy(max_bet, ev_cutoff, pp_multiplier);
			//OptStrategy b2 = new OptStrategy(max_bet, ev_cutoff, pp_multiplier);
			BonusPairsStrategy b2 = new BonusPairsStrategy(100, max_bet, ev_cutoff);

			DualStrategy b = new DualStrategy(b1, b2);

			//PseudoOptStrategy b = new PseudoOptStrategy();

			Game game = new Game(rules, b, pp_multiplier, random);

			game.PlayerMoney = 0;
			double expected_money = (double)game.PlayerMoney;
			game.Bet = 100; // 1$


			int lowest = game.PlayerMoney, highest = game.PlayerMoney;
			int runs = 0;

			while (true)
			{
				double total_money = (game.PlayerMoney + pp_multiplier * game.PartyPoints) / 100.0;
				if (runs % 10000 == 0)
				{
					Console.WriteLine("runs: " + runs + " win: " + (double)game.PlayerMoney / 100.0 + "$" + " pp: " + game.party_points);
					Console.WriteLine("lowest: " + (double)lowest / 100.0 + "$" + " highest: " + (double)highest / 100.0 + "$");
					Console.WriteLine("total: " + total_money + "$ | expected: " + expected_money / 100.0 + "$");
					Console.WriteLine();
				}

				game.StartRound();
				game.DealRound();

				expected_money += b.ShoeEV() * game.Bet;

				if (game.PlayerMoney < lowest)
					lowest = game.PlayerMoney;
				if (game.PlayerMoney > highest)
					highest = game.PlayerMoney;

				runs++;
			}
		}

		/*
		static void PrintGameState(Game game)
		{
			double total_money = (game.PlayerMoney + pp_multiplier * game.PartyPoints) / 100.0;

			Console.WriteLine("runs: " + runs + " win: " + (double)game.PlayerMoney / 100.0 + "$" + " pp: " + game.party_points);
			Console.WriteLine("total: " + total_money + "$ | expected: " + expected_money / 100.0 + "$");
			Console.WriteLine();
		}*/

		static double TotalMoney(Game game)
		{
			double total_money = (game.PlayerMoney + game.TotalPartyPoints) / 100.0;

			return total_money;
		}

		static void CompareStrategies()
		{
			Random masterRandom = new NPack.MersenneTwister();
			Random random1 = new NPack.MersenneTwister();
			Random random2 = new NPack.MersenneTwister();


			Rules rules = new Rules { Decks = 8, MinBet = 100, MaxBet = 20000, Splits = 3 };

			int max_bet = 5000;
			double pp_multiplier = 4.0;
			double ev_cutoff = 0.0015;

			DiffStrategy b1 = new DiffStrategy(
				new SuperOptStrategy(max_bet, ev_cutoff, pp_multiplier),
				new OptStrategy(max_bet, ev_cutoff, pp_multiplier)
				);
			OptStrategy b2 = new OptStrategy(max_bet, ev_cutoff, pp_multiplier);

			Game game1 = new Game(rules, b1, pp_multiplier, random1);
			Game game2 = new Game(rules, b2, pp_multiplier, random2);

			int runs = 0;
			double expected = 0;
			double expected_diff = 0;
			int diff_count = 0;

			while (true)
			{
				if (runs % 1000 == 0)
				{
					double total1 = TotalMoney(game1);
					double total2 = TotalMoney(game2);

					Console.WriteLine();
					Console.WriteLine("Runs:                {0}", runs);
					Console.WriteLine("Expected total:      $ {0:0.00}", expected);
					Console.WriteLine("SuperOpt total:      $ {1:0.00} ({2:0.00} c/hand)", b1.GetType(), total1, total1 * 100.0 / (double)runs);
					Console.WriteLine("Opt total:           $ {1:0.00} ({2:0.00} c/hand)", b2.GetType(), total2, total2 * 100.0 / (double)runs);
					Console.WriteLine("Different choices:   {0:0} ({1:0.00}%)", diff_count, 100.0 * diff_count / (double)runs);
					Console.WriteLine("Difference:          $ {0:0.00}", total1 - total2);
					Console.WriteLine("Expected difference: $ {0:0.00}", expected_diff);

					TextWriter writer = new StreamWriter("compare.txt", true);

					writer.WriteLine(string.Format("{0} {1} {2} {3}", runs, expected, TotalMoney(game1), TotalMoney(game2)));

					writer.Close();
				}

				int seed = masterRandom.Next();

				game1.Random = new NPack.MersenneTwister(seed);
				game2.Random = new NPack.MersenneTwister(seed);	

				game1.ResetShoe();
				game2.ResetShoe();

				int remove_count = masterRandom.Next(84);

				game1.RemoveCards(remove_count);
				game2.RemoveCards(remove_count);

				expected += b1.ShoeEV() * b1.Bet(game1) / 100.0;

				game1.StartRound();
				game2.StartRound();

				game1.DealRound();
				game2.DealRound();

				if (b1.IsDiff)
				{
					expected_diff += game1.Bet * b1.Diff / 100.0;
					diff_count++;
					b1.IsDiff = false;
				}

				runs++;
			}
		}

		static void CompareStrategies2()
		{
			Random masterRandom = new NPack.MersenneTwister();


			Rules rules = new Rules { Decks = 8, MinBet = 100, MaxBet = 20000, Splits = 3 };

			int max_bet = 5000;
			double pp_multiplier = 4.0;
			double ev_cutoff = 0.0015;

			SuperOptStrategy b1 = new SuperOptStrategy(max_bet, ev_cutoff, pp_multiplier);
			OptStrategy b2 = new OptStrategy(max_bet, ev_cutoff, pp_multiplier);
			BasicStrategy b3 = new BasicStrategy(max_bet, ev_cutoff, pp_multiplier);

			Game game1 = new Game(rules, b1, pp_multiplier, new NPack.MersenneTwister());
			Game game2 = new Game(rules, b2, pp_multiplier, new NPack.MersenneTwister());
			Game game3 = new Game(rules, b3, pp_multiplier, new NPack.MersenneTwister());

			int runs = 0;
			double expected = 0;

			while (true)
			{
				if (runs % 10000 == 0)
				{
					double total1 = TotalMoney(game1);
					double total2 = TotalMoney(game2);
					double total3 = TotalMoney(game3);

					Console.WriteLine();
					Console.WriteLine("Runs:           {0}", runs);
					Console.WriteLine("Expected total: $ {0:0.00}", expected);
					Console.WriteLine("SuperOpt total: $ {1:0.00} ({2:0.00} c/hand)", b1.GetType(), total1, total1 * 100.0 / (double)runs);
					Console.WriteLine("Opt total:      $ {1:0.00} ({2:0.00} c/hand)", b2.GetType(), total2, total2 * 100.0 / (double)runs);
					Console.WriteLine("Basic total:    $ {1:0.00} ({2:0.00} c/hand)", b3.GetType(), total3, total3 * 100.0 / (double)runs);

					TextWriter writer = new StreamWriter("compare.txt", true);

					writer.WriteLine(string.Format("{0} {1} {2} {3} {4}", runs, expected, total1, total2, total3));

					writer.Close();
				}

				int seed = masterRandom.Next();

				game1.Random = new NPack.MersenneTwister(seed);
				game2.Random = new NPack.MersenneTwister(seed);
				game3.Random = new NPack.MersenneTwister(seed);

				game1.ResetShoe();
				game2.ResetShoe();
				game3.ResetShoe();

				int remove_count = masterRandom.Next(84);

				game1.RemoveCards(remove_count);
				game2.RemoveCards(remove_count);
				game3.RemoveCards(remove_count);

				expected += b1.ShoeEV() * b1.Bet(game1) / 100.0;

				game1.StartRound();
				game2.StartRound();
				game3.StartRound();

				game1.DealRound();
				game2.DealRound();
				game3.DealRound();

				runs++;
			}
		}

		static void TestSuper()
		{
			Random random = new NPack.MersenneTwister();

			Rules rules = new Rules { Decks = 8, MinBet = 100, MaxBet = 20000, Splits = 3 };

			int max_bet = 5000;
			double pp_multiplier = 4.0;
			double ev_cutoff = 0.0015;

			SuperOptStrategy b = new SuperOptStrategy(max_bet, ev_cutoff, pp_multiplier);


			//PseudoOptStrategy b = new PseudoOptStrategy();

			Game game = new Game(rules, b, pp_multiplier, random);

			game.PlayerMoney = 0;
			double expected_money = (double)game.PlayerMoney;
			game.Bet = 100; // 1$


			int lowest = game.PlayerMoney, highest = game.PlayerMoney;
			int runs = 0;

			while (true)
			{
				double total_money = (game.PlayerMoney + pp_multiplier * game.PartyPoints) / 100.0;
				if (runs % 10000 == 0)
				{
					Console.WriteLine("runs: " + runs + " win: " + (double)game.PlayerMoney / 100.0 + "$" + " pp: " + game.party_points);
					Console.WriteLine("lowest: " + (double)lowest / 100.0 + "$" + " highest: " + (double)highest / 100.0 + "$");
					Console.WriteLine("total: " + total_money + "$ | expected: " + expected_money / 100.0 + "$");
					Console.WriteLine();
				}

				game.StartRound();
				game.DealRound();


				expected_money += b.ShoeEV() * game.Bet;

				if (game.PlayerMoney < lowest)
					lowest = game.PlayerMoney;
				if (game.PlayerMoney > highest)
					highest = game.PlayerMoney;

				runs++;
			}
		}
	}
}
