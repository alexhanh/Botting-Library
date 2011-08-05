using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Blackjack
{
	// class for a played hand, holds the cards, the bet amount and the action(s)
	public class Hand
	{
		private CardSet cards;

		private bool doubled;
		private bool split;
		private bool stood;
		private bool surrendered;

		private int hit_count;

		// true if this is a split hand coming from splitting aces
		private bool from_aces;

		public Hand()
		{
			cards = new CardSet();
			Reset();
		}

		public Hand(CardSet cards) : this()
		{
			foreach (Card c in cards)
			{
				Hit(c);
			}
		}

		public void Reset()
		{
			doubled = false;
			split = false;
			stood = false;
			surrendered = false;
			from_aces = false;
			hit_count = 0;
			cards.Clear();
		}

		public CardSet Cards
		{
			get { return cards; }
		}

		public Card this[int index]
		{
			get { return (Card)cards[index]; }
		}

		public int Count
		{
			get { return cards.Count; }
		}

		// splits this hand to two new hands
		public Hand[] Split(Card card1, Card card2)
		{
			Hand[] hands = new Hand[] { new Hand(), new Hand() };

			hands[0].AddCard(cards[0]);
			hands[1].AddCard(cards[1]);

			hands[0].AddCard(card1);
			hands[1].AddCard(card2);

			//hands[0].Bet = this.bet;
			//hands[1].Bet = this.bet;

			if (cards[0].IsAce() && cards[1].IsAce())
			{
				hands[0].from_aces = true;
				hands[1].from_aces = true;
			}

			this.split = true;

			return hands;
		}

		public void Stand()
		{
			stood = true;
		}

		public void Hit(Card card)
		{
			cards.Add(card);
			hit_count++;
		}

		public void Surrender()
		{
			surrendered = true;
		}

		public void Double(Card card)
		{
			cards.Add(card);
			doubled = true;
		}

		public int HitCount
		{
			get { return hit_count; }
		}

		// true if any action has been made (hit, doubled, surrendered, split)
		public bool Acted
		{
			get
			{
				if (doubled || split || stood || surrendered || hit_count > 0)
					return true;

				return false;
			}
		}

		// for the hand to be played or finished, it needs to 
		// be stood or
		// be busted or
		// be split or 
		// be 21 (we make an assumption of player standing on 21)
		public bool Finished 
		{
			get
			{
				if (stood || IsBust() || split || doubled || surrendered || from_aces || PointCount() == 21)
					return true;
				else
					return false;
			}
		}

		// any point value combination exceeds 21
		public bool IsBust()
		{
			return PointCount() > 21;
		}

		public bool IsSplit()
		{
			return split;
		}

		public bool Doubled { get { return doubled; } set { doubled = value; } }
		public bool Surrendered { get { return surrendered; } }

		public bool HasAce()
		{
			foreach (Card c in cards)
				if (c.IsAce())
					return true;
			return false;
		}

		// _one_ ace is counted as 11
		public int SoftTotal()
		{
			int points = 0;
			bool ace_counted = false;

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i].IsAce())
				{
					if (ace_counted)
					{
						points += 1;
					}
					else
					{
						points += 11;
						ace_counted = true;
					}
				}
				else
				{
					points += cards[i].PointValue;
				}
			}

			return points;
		}

		// all aces are counted as 1
		public int HardTotal()
		{
			int points = 0;

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i].IsAce())
				{
					points += 1;
				}
				else
				{
					points += cards[i].PointValue;
				}
			}

			return points;
		}

		// returns the highest possible point value less than 22, if possible
		public int PointCount()
		{
			int ace_count = 0;
			int points = 0;

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i].IsAce())
				{
					points += 11;
					ace_count++;
				}
				else
				{
					points += cards[i].PointValue;
				}
			}

			while (ace_count > 0 && points > 21)
			{
				points -= 10;
				ace_count--;
			}

			return points;
		}

		public bool IsPair()
		{
			return (cards[0].PointValue == cards[1].PointValue);
		}

		public bool IsNatural()
		{
			if ((cards[0].IsTenValue() && cards[1].IsAce()) ||
				(cards[1].IsTenValue() && cards[0].IsAce()))
				return true;

			return false;
		}

		public void AddCard(Card card)
		{
			cards.Add(card);
		}

		public IEnumerator<Card> GetEnumerator()
		{
			return cards.GetEnumerator();
		}

		public override string ToString()
		{
			return cards.ToString();
		}
	}
}