using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public class Version
	{
		public static string GetBJVersion()
		{
			return "BJ version 1.06";
		}

		public static string GetBJEvalVersion()
		{
			return string.Format("BJEval version {0:0.00}", BjEval.Eval.Version());
		}
	}
}
