using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public class MoneyMismatchException : Exception
	{
		int expected, actual;

		public MoneyMismatchException(int expected, int actual)
		{
			this.expected = expected;
			this.actual = actual;
		}

		public override string ToString()
		{
			return string.Format("Expected: {0}, Actual: {1}", expected, actual);
		}
	}
}
