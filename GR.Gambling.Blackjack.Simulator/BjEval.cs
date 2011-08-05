using System.Runtime.InteropServices;

namespace BjEval
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SHand
	{
		public int Total;
		public bool Soft;
	}

	public class Eval
	{
		[DllImport("BjEvalDLL.dll")]
		public static extern void CacheDealerProbs(int upcard, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double StandEv(SHand hand, int upcard, [MarshalAs(UnmanagedType.LPArray, SizeConst= 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double HitEv(SHand player, int upcard, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double DoubleEv(SHand player, int upcard, int bet_size, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double InsuranceEv(int bet_size, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double SplitEv(int split_card, int upcard, int bet_size, int max_splits, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe);
		[DllImport("BjEvalDLL.dll")]
		public static extern double SurrenderEv();

		[DllImport("BjEvalDLL.dll")]
		public static extern double ShoeEv([MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe, int bet_size);

		[DllImport("BjEvalDLL.dll")]
		public static extern double DealEv(int player1, int player2, int upcard, [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] int[] shoe, int bet_size);

		[DllImport("BjEvalDLL.dll")]
		public static extern double Version();
	}
}