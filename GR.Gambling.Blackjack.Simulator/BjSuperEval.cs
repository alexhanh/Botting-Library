using System.Runtime.InteropServices;
using GR.Gambling.Blackjack;
using System.Collections.Generic;

namespace BjEval
{
	public class SuperEval
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct SHand
		{
			public SHand(Hand hand)
			{
				int soft_total = hand.SoftTotal();
				if (soft_total <= 21 && hand.HasAce())
				{
					this.Total = soft_total;
					this.Soft = true;
				}
				else
				{
					this.Total = hand.HardTotal();
					this.Soft = false;
				}
			}

			int Total;
			bool Soft;
		}
		/*
	BJSUPERDLL_API void InitializeEvaluation(
		SHand activeHands[], int numActiveHands, bool allBusted, int numBusted, int dealerUpcard, const int shoeCounts[], int betSize);

	BJSUPERDLL_API double StandEv();
	BJSUPERDLL_API double HitEv();
	BJSUPERDLL_API double DoubleEv();
	BJSUPERDLL_API double InsuranceEv();
	BJSUPERDLL_API double SurrenderEv();
	BJSUPERDLL_API double SplitEv(int max_splits);
		*/

		[DllImport("BjSuperDLL.dll")]
		private static extern void InitializeEvaluation(
			[MarshalAs(UnmanagedType.LPArray)]	SHand[] activeHands, 
			int numActiveHands, 
			bool allBusted, 
			int numBusted, 
			int dealerUpcard,
			[MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoeCounts, 
			int betSize);

		[DllImport("BjSuperDLL.dll")]
		public static extern double StandEv();
		[DllImport("BjSuperDLL.dll")]
		public static extern double HitEv();
		[DllImport("BjSuperDLL.dll")]
		public static extern double DoubleEv();
		[DllImport("BjSuperDLL.dll")]
		public static extern double SplitEv(int split_card, int max_splits);
		[DllImport("BjSuperDLL.dll")]
		public static extern double SurrenderEv();

		[DllImport("BjSuperDLL.dll")]
		private static extern double InsuranceEv(int bet_size, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);


		public static void Initialize(Game game)
		{
			HandSet playerHands = game.PlayerHandSet;

			List<Hand> active = new List<Hand>();

			active.Add(playerHands.ActiveHand);

			bool allBusted = true;
			int numBusted = 0;

			foreach (Hand hand in game.PlayerHandSet)
			{
				if (!hand.Finished)
				{
					if (hand != playerHands.ActiveHand) active.Add(hand);
				}
				else
				{
					if (hand.IsBust())
					{
						numBusted++;
					}
					else
					{
						if (!hand.IsSplit()) allBusted = false;
					}
				}
			}

			SHand[] shands = new SHand[active.Count];
			for (int i = 0; i < shands.Length; i++)
			{
				shands[i] = new SHand(active[i]);
			}

			int[] shoe = game.Shoe.Counts;
			shoe[game.DealerHand[1].PointValue - 1]++;

			int upcard = game.DealerHand[0].PointValue;

			InitializeEvaluation(shands, shands.Length, allBusted, numBusted, upcard, shoe, game.Bet);
		}

		public static bool TakeInsurance(Game game)
		{
			int[] shoe = game.Shoe.Counts;
			shoe[game.DealerHand[1].PointValue - 1]++;

			double insurance_ev = Eval.InsuranceEv(game.Bet, shoe);

			if (insurance_ev >= 0.0)
				return true;
			else
				return false;
		}
	}
}