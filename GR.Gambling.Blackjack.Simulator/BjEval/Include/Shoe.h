#ifndef __SHOE_H__
#define __SHOE_H__

#include "Random.h"

// card values as follows
// A = 1	8 = 8
// 2 = 2	9 = 9
// 3 = 3   10 = 10
// 4 = 4    J = 10
// 5 = 5    Q = 10
// 6 = 6    K = 10
class Shoe
{
    private:
		int decks;
		int count[10];
		int total;

	public:
		Shoe() {}
		Shoe(int decks);
		Shoe(const int count[10]);


		int operator [] (const int card)
		{
			return count[card-1];
		}

		inline int Total() { return total; }

		// restore counts
		void Shuffle();

		// Calculate the straightforward P[], no additional info
		// P[k] = count_k / total
		double CardProb(int card)
		{
			return (double)(count[card-1] / (double)total);
		}

		// Calculate the probability of getting 'card', knowing D_d != 22
		// only interesting in cases upcard = A or T && dealer doesn't have natural
		// if upcard != T or A, then P[k] = Q[k]
		// assumes upcard has been removed from the deck already
		double Shoe::CardProb(int card, int upcard);

		// card >= 1 and <= 10
		void AddCard(int card)
		{
			count[card-1]++;
			total++;
		}

		void DealCard(int card)
		{
			count[card-1]--;
			total--;
		}

		void SetCount(int card, int c)
		{
			total += c - count[card-1];
			count[card-1] = c;
		}


		// Calculate the probability of getting 'card' next, knowing for sure that it isn't 'exclude'
		double CardProbExclude(int card, int exclude)
		{
			if (exclude <= 0) return CardProb(card);
			if (card == exclude) return 0;
			return (double)(count[card-1] / (double)(total - count[exclude-1]));
		}

		// Removes a total of 'number' random cards from the shoe
		void RemoveRandom(Random& random, int number);

		// Removes a random card and returns it
		int DealRandom(Random& random);
};

#endif