using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GR.Gambling.Blackjack
{
	public class CardSet
	{
		private List<Card> card_set;
		private int[] card_counts;

		public Card this[int index]
		{
			get { return (Card)card_set[index]; }
			set { card_set[index] = value; }
		}

		public static CardSet operator +(CardSet set1, CardSet set2)
		{
			CardSet tmp = new CardSet(set1);
			tmp.Add(set2);

			return tmp;
		}

		public static CardSet operator -(CardSet set1, CardSet set2)
		{
			CardSet tmp = new CardSet(set1);
			tmp.Remove(set2);

			return tmp;
		}

		public int Count
		{
			get { return card_set.Count; }
		}

		public int[] Counts
		{
			get { return (int[])card_counts.Clone(); }
		}

		public IEnumerator<Card> GetEnumerator()
		{
			return card_set.GetEnumerator();
		}

		public Card[] GetCards()
		{
			return (Card[])card_set.ToArray();
		}

		private void UpdateCount()
		{
			card_counts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			foreach (Card card in card_set)
				card_counts[card.PointValue - 1]++;
		}

		private void AddCount(Card card)
		{
			card_counts[card.PointValue - 1]++;
		}

		private void RemoveCount(Card card)
		{
			card_counts[card.PointValue - 1]--;
		}

		public CardSet() : this(0)
		{
			UpdateCount();
		}

		public CardSet(int num_cards)
		{
			this.card_set = new List<Card>(num_cards);
			UpdateCount();
		}

		public CardSet(Card[] cards) : this(cards.Length)
		{
			this.Add(cards);
			UpdateCount();
		}

		public CardSet(CardSet card_set)
		{
			this.card_set = new List<Card>(card_set.card_set);
			UpdateCount();
		}

		public CardSet(string cards) : this(0)
		{
			this.Add(cards);
			UpdateCount();
		}

		public void Add(Card card)
		{
			card_set.Add(card);
			AddCount(card);
		}

		public void Add(Card[] cards)
		{
			card_set.AddRange(cards);
			foreach (Card card in cards)
				AddCount(card);
		}

		public void Add(CardSet cards)
		{
			foreach (Card c in cards)
			{
				Add(c);
			}
		}

		public void Add(string cards)
		{
			string[] split = cards.Split(new char[] { ' ' });

			for (int i = 0; i < split.Length; i++)
			{
				Card c = new Card(split[i]);
				Add(c);
			}
		}

		public void Clear()
		{
			card_set.Clear();
			UpdateCount();
		}

		public void Remove(Card card)
		{
			if (card_set.Remove(card))
				RemoveCount(card);            
		}

		public void Remove(CardSet cards)
		{
			foreach (Card c in cards)
				if (card_set.Remove(c))
					RemoveCount(c);
		}

		public Card ExtractTop()
		{
			Card card = (Card)card_set[card_set.Count - 1];
			RemoveCount(card);
			card_set.RemoveAt(card_set.Count - 1);

			return card;
		}

		public CardSet ExtractTop(int count)
		{
			CardSet set = new CardSet();

			for (int i = 0; i < count; i++)
			{
				if (card_set.Count == 0)
					break;

				set.Add(ExtractTop());
			}

			return set;
		}

		public CardSet ExtractRandom(Random rand, int count)
		{
			CardSet set = new CardSet();

			for (int i = 0; i < count; i++)
			{
				if (card_set.Count == 0)
					break;

				int card_index = rand.Next(card_set.Count);
				set.Add((Card)card_set[card_index]);
				RemoveCount(card_set[i]);
				card_set.RemoveAt(card_index);
			}

			return set;
		}

		public void Shuffle(Random rand)
		{
			for (int i = 0; i < card_set.Count; i++)
			{
				Card c = card_set[i];

				int index = rand.Next(card_set.Count - i) + i;
				card_set[i] = card_set[index];

				card_set[index] = c;
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			for (int i=0; i<Count; i++)
			{
				if (i > 0) result.Append(" ");
				result.Append(this[i]);
			}

			return result.ToString();
		}

		public static CardSet Parse(string s)
		{
			return new CardSet(s);
		}

		public string ToCountsString()
		{
			StringBuilder result = new StringBuilder();

			for (int i = 0; i < card_counts.Length; i++)
			{
				if (i>0) result.Append(" ");
				result.Append(card_counts[i]);
			}

			return result.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			CardSet cards=(CardSet)obj;

			if (Count != cards.Count) return false;

			for (int i=0; i<Count; i++) if (!this[i].Equals(cards[i])) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public object Clone()
		{
			CardSet cloned_set = new CardSet();

			foreach (Card card in this.card_set)
				cloned_set.Add((Card)(card.Clone()));

			return cloned_set;
		}

		public void CheatToDealPair(Random rand)
		{
			int top = card_set[card_set.Count-1].PointValue;

			if (card_set[card_set.Count-3].PointValue != top)
			{
				int top_count = card_counts[top - 1] - 1;

				int random_number = rand.Next(top_count);

				int random_index = 0;

				int matches = 0;

				for (int i = card_set.Count-2; i >= 0; i--)
				{
					if (card_set[i].PointValue == top)
					{
						random_index = i;

						if (matches == random_number) break;

						matches++;
					}
				}


				Card tmp = card_set[card_set.Count - 3];
				card_set[card_set.Count - 3] = card_set[random_index];
				card_set[random_index] = tmp;
			}
		}
	}
}
