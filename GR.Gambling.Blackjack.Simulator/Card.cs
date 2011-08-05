using System;
using System.Collections.Generic;
using System.Text;

namespace GR.Gambling.Blackjack
{
	public enum CardRank
	{
		Two = 0,
		Three = 1,
		Four = 2,
		Five = 3,
		Six = 4,
		Seven = 5,
		Eight = 6,
		Nine = 7,
		Ten = 8,
		Jack = 9,
		Queen = 10,
		King = 11,
		Ace = 12,
		Count = 13
	}

	public enum CardSuit
	{
		Hearts = 0,
		Diamonds = 1,
		Clubs = 2,
		Spades = 3,
		Count = 4,
	}

	/*
	Suits: 0=h, 1=d, 2=c, 3=s

	fully explicit card to integer conversions :
	2h =  0    2d = 13    2c = 26    2s = 39
	3h =  1    3d = 17    3c = 33    3s = 49
	4h =  2		   ...        ...        ...
	5h =  3
	6h =  4
	7h =  5
	8h =  6
	9h =  7
	Th =  8
	Jh =  9
	Qh =  10
	Kh =  11
	Ah =  12
	*/

	public class Card : IComparable
	{
		private int suit, rank;

		private static char[] suit_chars = { 'h', 'd', 'c', 's' };
		private static char[] rank_chars = { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };

		public static bool operator ==(Card card1, Card card2)
		{
			return card1.Equals(card2);
		}
		public static bool operator !=(Card card1, Card card2)
		{
			return !card1.Equals(card2);
		}

		public int Suit
		{
			get { return suit; }
			set { suit = value; }
		}

		public int Rank
		{
			get { return rank; }
			set { rank = value; }
		}

		public int Index
		{
			//this.suit = index / 13;
			//this.rank = index % 13;
			get { return suit * 13 + rank; }
		}

		// Blackjack
		public int PointValue
		{
			get 
			{
				if (rank == (int)CardRank.Ace)
					return 1;
				else if (IsTenValue())
					return 10;
				else
					return rank + 2;
			}
		}

		// true if cards rank is T,J,Q or K.
		public bool IsTenValue()
		{
			return (rank >= 8 && rank <= 11);
		}

		public bool IsAce()
		{
			return rank == (int)CardRank.Ace;
		}

		public Card(int suit, int rank)
		{
			this.suit = suit;
			this.rank = rank;
		}
	 
		public Card(char suit, char rank) : this(SuitFromChar(suit), RankFromChar(rank))
		{
		}

		public Card(string str) : this(str[1], str[0])
		{
		}

		public Card(int index)
		{
			this.suit = index / 13;
			this.rank = index % 13;
		}

		public override string ToString()
		{
			return RankChar().ToString() + SuitChar().ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj==null) return false;

			Card c = (Card)obj;

			if (Suit == c.Suit && Rank == c.Rank) return true;

			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public char SuitChar()
		{
			return SuitChar(Suit);
		}

		public char RankChar()
		{
			return RankChar(Rank);
		}

		public int CompareTo(object obj)
		{
			Card c = (Card)obj;

			if (rank != c.Rank)
			{
				if (c.Rank > rank)
					return 1;
				if (c.Rank < rank)
					return -1;

				return 0;
			}
			else
			{
				if (c.Suit > suit)
					return 1;
				if (c.Suit < suit)
					return -1;

				return 0;
			}
		}

		public object Clone()
		{
			return new Card(this.suit, this.rank);
		}

		public static char SuitChar(int suit)
		{
			if (suit < 0 || suit >= suit_chars.Length) return 'x';
			return suit_chars[suit];
		}

		public static char RankChar(int rank)
		{
			if (rank < 0 || rank >= rank_chars.Length) return 'X';
			return rank_chars[rank];
		}

		public static int SuitFromChar(char s)
		{
			for (int i = 0; i < suit_chars.Length; i++) if (suit_chars[i] == s) return i;
			return -1;
		}

		public static int RankFromChar(char r)
		{
			for (int i = 0; i < rank_chars.Length; i++) if (rank_chars[i] == r) return i;
			return -1;
		}

		public static bool IsCard(string card)
		{
			if (card.Length != 2)
				return false;

			if (card.IndexOfAny(rank_chars, 0, 1) != 0)
				return false;

			if (card.IndexOfAny(suit_chars, 1, 1) != 1)
				return false;

			return true;
		}
	}

	public class CardsFactory
	{
		public static CardSet StandardDeck
		{
			get
			{
				CardSet set = new CardSet();

				for (int i = 0; i < 52; i++)
				{
					set.Add(new Card(i));
				}

				return set;
			}
		}
	}
}
