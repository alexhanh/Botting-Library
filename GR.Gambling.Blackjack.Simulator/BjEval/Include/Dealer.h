#ifndef __DEALER_H__
#define __DEALER_H__

#include "Hand.h"
#include "Shoe.h"
#include "PDFDealer.h"

class Hand;
class Shoe;

class Dealer
{
private:
	static double cachedProbs[5];
	static double cachedBust;

public:
	// Calculates the probability of dealer busting with given upcard and shoe.
	// 'bj' indicates wether dealer's probability of natural is included or not.
	// If it's known that the dealer cannot have a natural, bj must be false. This is the case after dealer has confirmed not to have a natural.
	// Otherwise it should be true. 
	// Note: ProbTotal() doesn't take into account the branches where dealer gets a natural
	//static double ProbBust(int upcard, Shoe shoe, bool bj = true);

	// Calculates the probability of dealer hitting natural (ie. 2-card blackjack) with a given upcard
	static double ProbNatural(int upcard, Shoe shoe);

	// Computes total probability of dealer hitting a natural with certain shoe.
	static double ProbNatural(Shoe shoe);

	static inline double ProbTotal(Hand hand, int total, Shoe& shoe)
	{
		return cachedProbs[total-17];
		//return PDFDealer::ProbTotal(hand, total, shoe);
	}

	static inline double ProbBust(int upcard, Shoe& shoe)
	{
		return cachedBust;
	}

	static void CacheProbs(int upcard, Shoe& shoe)
	{
		cachedBust = 1.0;

		for (int i=17; i<=21; i++)
		{
			cachedProbs[i-17] = PDFDealer::ProbTotal(upcard, i, shoe);
			cachedBust -= cachedProbs[i-17];
		}
	}

	// Brute-force version. Works for sure, un-optimized.
	// Calculates the probability of dealer finishing with a total score of 'total'.
	// Assumed rules: dealer must hit on soft 17, stand on hard 17.
	// 'hand' contains dealer's known cards.
	// 'shoe' contains all still uknown cards to come (all known cards, including dealer's hand, must be removed from the shoe)
	// 'depth' is used for tracing the recursion level, should be left to default by caller.
	// Doesn't calculate branches with dealer ending up with natural (2-card blackjack) (ie. A->10 or 10->A), this usually
	// is a correct assumption because dealer checks for natural before continuing the round.
	static double ProbTotalBrute(Hand hand, int total, Shoe shoe, int depth = 0);
};

#endif