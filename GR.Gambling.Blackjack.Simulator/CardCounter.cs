using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public class CardCounter
	{
		double baseEV;
		double[] tagValues;

		int[] removedCounts = new int[10];
		double currentEV = 0;

		public int this[int cardValue]
		{
			get { return removedCounts[cardValue - 1]; }
		}

		public CardCounter(double baseEV, double[] tagValues)
		{
			this.baseEV = baseEV;
			this.tagValues = tagValues;

			Reset();
		}

		public CardCounter(double ppMultiplier)
		{
			if (ppMultiplier == 0.0)
			{
				baseEV = -0.00557853;
				tagValues = new double[] {
					-0.000709974,
					0.000602768,
					0.000736658,
					0.00100567,
					0.00120795,
					0.000690184,
					0.000318784,
					-0.000104576,
					-0.000378185,
					-0.000816215
				};
			}
			else if (ppMultiplier == 3.0)
			{
				baseEV = -0.00421498;
				tagValues = new double[] {
					-0.000715645,
					0.000601558,
					0.000734085,
					0.000997088,
					0.00121018,
					0.000693555,
					0.0003251,
					-0.000102365,
					-0.000378124,
					-0.000817202
				};
			}
			else if (ppMultiplier == 4.0)
			{
				baseEV = -0.00376047;
				tagValues = new double[] {
					-0.000719235,
					0.000595044,
					0.00073417,
					0.00100608,
					0.00120119,
					0.000686878,
					0.000321906,
					-0.000101043,
					-0.000383272,
					-0.000829224
				};
			}
			else
			{
				throw new Exception("Invalid PP multiplier");
			}

			Reset();
		}

		public void RemoveCard(int cardValue)
		{
			removedCounts[cardValue - 1]++;
			currentEV += tagValues[cardValue - 1];
		}

		public void Reset()
		{
			for (int i = 0; i < 10; i++)
			{
				removedCounts[i] = 0;
			}

			currentEV = baseEV;
		}

		public double CurrentEV
		{
			get { return currentEV; }
		}
	}
}
