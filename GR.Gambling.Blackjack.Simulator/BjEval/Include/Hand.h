#ifndef __HAND_H__
#define __HAND_H__


class Hand
{
	private:
		int total; 
		bool soft; // true if hand contains ace which is counted as 11

	public:
		Hand() {}
		Hand(int total, bool soft)
		{
			this->total = total;
			this->soft = soft;
		}

		Hand(int card);

		Hand& operator += (int card);
		bool operator <= (int total) { return (this->total <= total); }
		bool operator >= (int total) { return (this->total >= total); }
		bool operator == (int total) { return (this->total == total); }
		bool operator != (int total) { return (this->total != total); }
		bool operator < (int total) { return (this->total < total); }
		bool operator > (int total)	{ return (this->total > total);	}
		friend Hand operator + (Hand& hand, int card)
		{
			Hand tmp(hand.Total(), hand.Soft());
			return tmp += card;
		}

		inline int Total() { return total; }
		inline bool Soft() { return soft; }
		void Print();
};

#endif