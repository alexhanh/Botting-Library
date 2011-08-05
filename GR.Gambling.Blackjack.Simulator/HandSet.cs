using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public class HandSet
	{
		private List<Hand> hands;
		private int active_index;

		public HandSet()
		{
			hands = new List<Hand>();
			hands.Add(new Hand());
			active_index = 0;
		}

		public Hand this[int index]
		{
			get { return (Hand)hands[index]; }
		}

		public Hand ActiveHand
		{
			get { return hands[active_index]; }
		}

		public int ActiveIndex
		{
			get { return active_index; }
		}

		public int HandCount
		{
			get
			{
				return hands.Count;
			}
		}

		// updates the active hand index to point to next unfinished hand
		public void NextActiveHand()
		{
			for (int i = 0; i < hands.Count; i++)
			{
				if (!hands[i].Finished)
				{
					active_index = i;
					return;
				}
			}

			// all hands are finished
		}

		// true if no hands are left to be played out by the player
		public bool AllFinished()
		{
			for (int i = 0; i < hands.Count; i++)
			{
				if (!hands[i].Finished)
					return false;
			}

			return true;
		}

		public bool AllBusted()
		{
			for (int i = 0; i < hands.Count; i++)
			{
				if (!hands[i].IsBust())
					return false;
			}

			return true;
		}

		// splits the active hand to two new hands
		public void Split(Card card1, Card card2)
		{
			Hand[] split_hands = ActiveHand.Split(card1, card2);

			hands.Add(split_hands[0]);
			hands.Add(split_hands[1]);
		}

		public void Reset()
		{
			hands.Clear();
			hands.Add(new Hand());
			active_index = 0;
		}

		public IEnumerator<Hand> GetEnumerator()
		{
			return hands.GetEnumerator();
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			for (int i = 0; i < hands.Count; i++)
			{
				result.Append(string.Format("Hand {0,1}:   {1,-12}", i+1, hands[i]));


				result.Append(string.Format(" = {0} ", hands[i].PointCount()));

				if (hands[i].IsSplit()) result.Append("(split)");
				else if (hands[i].Doubled) result.Append("(doubled)");
				else if (hands[i].IsBust()) result.Append("(busted)");
				else if (hands[i].IsNatural()) result.Append("(BJ)");
				else if (hands[i].Surrendered) result.Append("(surrendered)");
				else
				{
					if (active_index == i && !hands[i].Finished) result.Append("(*)");
				}

				result.AppendLine();
			}

			return result.ToString();
		}
	}
}
