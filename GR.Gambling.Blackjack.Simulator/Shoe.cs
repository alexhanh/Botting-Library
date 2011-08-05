using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public class Shoe
	{
		int decks;
		int[] counts = new int[10];
		int total;

		protected Shoe()
		{
		}

		public Shoe(int decks)
		{
			this.decks = decks;
			Reset();
		}

		public int this[int pointValue]
		{
			get { return counts[pointValue - 1]; }
		}

		public int CardCount
		{
			get { return total; }
		}

		public int FullCount
		{
			get { return decks * 52; }
		}

		public void Clear()
		{
			total = 0;
			for (int i = 0; i < 10; i++)
			{
				counts[i] = 0;
			}
		}

		public void Reset()
		{
			Clear();

			for (int deck = 0; deck < decks; deck++)
			{
				Add(CardsFactory.StandardDeck);
			}
		}

		public void Add(CardSet cards)
		{
			foreach (Card c in cards)
			{
				total++;
				counts[c.PointValue - 1]++;
			}
		}

		public void Remove(CardSet cards)
		{
			foreach (Card c in cards)
			{
				Remove(c);
			}
		}

		public void Remove(Card card)
		{
			total--;
			counts[card.PointValue - 1]--;
		}

		public int[] ToArray()
		{
			return (int[])counts.Clone();
		}

		public Shoe Copy()
		{
			Shoe copy = new Shoe();
			copy.decks = this.decks;
			copy.total = this.total;
			copy.counts = this.ToArray();

			return copy;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.Append(decks);

			for (int i = 0; i < 10; i++)
			{
				result.Append(" ");

				result.Append(counts[i]);
			}

			return result.ToString();
		}

		public static Shoe Parse(string s)
		{
			string[] parts = s.Split(new char[] { ' ' });
			if (parts.Length < 10)
			{
				throw new ArgumentException(string.Format("Error while parsing a shoe, too few counts ({0})", parts.Length));
			}

			Shoe shoe = new Shoe();

			if (parts.Length == 11) shoe.decks = int.Parse(parts[0]);
			else shoe.decks = 8;

			for (int i = 0; i < 10; i++)
			{
				int count = int.Parse(parts[(parts.Length - 10) + i]);
				shoe.counts[i] = count;
				shoe.total += count;
			}

			return shoe;
		}
	}
}
