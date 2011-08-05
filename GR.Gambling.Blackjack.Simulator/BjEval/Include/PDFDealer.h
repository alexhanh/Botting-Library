#ifndef __PDFDEALER_H__
#define __PDFDEALER_H__

#include "Hand.h"
#include "Shoe.h"

class PDFDealer
{
public:

	// The following is an implementation of the dealer probability function as described in
	// http://arxiv.org/PS_cache/math/pdf/0412/0412311v1.pdf


	static double ProbTotal(Hand upcard, int total, Shoe shoe)
	{
		DealerShoe dShoe(shoe, upcard);
		return DealerProb21(upcard, total, dShoe);
	}

	// Used to pre-calculate the DealerSoft17 combinations
	static void PrintSoftPermutations(int sum, bool ace = false, int* cards = 0, int count = 0);

	// Used to pre-calculate prob(a) combinations
	static void PrintProbAPermutations(int a, int* cards = 0, int count = 0);	

	// Used to pre-calculate prob(a) combinations
	static void PrintProbACombinations(int a, int* cards = 0, int count = 0, int last_card = 0, int last_count = 0, int permutations = 0);	

private:

	// An auxiliary class for calculating the dealer's hidden card's probability correctly
	class DealerShoe
	{
		Shoe& shoe;
		Hand upcard;

		int numDealt;

		public:

		DealerShoe(Shoe& shoe, Hand& upcard) : shoe(shoe), upcard(upcard)
		{
			numDealt = 1;
		}

		double CardProb(int card)
		{
			if (numDealt == 1)
			{
				if (upcard.Total() == 10) return shoe.CardProbExclude(card, 1);
				else if (upcard.Total() == 11) return shoe.CardProbExclude(card, 10);
			}

			return shoe.CardProb(card);
		}

		void AddCard(int card)
		{
			numDealt--;
			shoe.AddCard(card);
		}

		void DealCard(int card)
		{
			numDealt++;
			shoe.DealCard(card);
		}
	};

	// Calculates the probability that the dealer will stop to e, where 17 <= e <= 21
	static double DealerProb21(Hand d, int e, DealerShoe& shoe);

	// Calculates the probability that dealer will from d to e, where e <= 17
	static double DealerProb17(Hand d, int e, DealerShoe& shoe)
	{
		return DealerSoft17(d, e, shoe) + DealerHard17(d, e, shoe);
	}

	// The probability that the dealer will get from d to hard e
	static double DealerHard17(Hand d, int e, DealerShoe& shoe);

	// The probability that the dealer will get from d to soft e
	static double DealerSoft17(Hand d, int e, DealerShoe& shoe);

	// The probability that the dealer will get from soft d to hard e
	static double DealerSoft2Hard17(int d, int e, DealerShoe& shoe);

	static inline double CombProb(int a, DealerShoe& shoe)
	{
		return CombProbEnumerated(a, shoe);
	}

	// The probability that the next cards in the shoe will sum up to a (smart implementation, not used)
	static double CombProbSmart(int a, DealerShoe& shoe, int count = 0, int last_card = 1, int last_count = 0, int permutations = 1);

	// The probability that the next cards in the shoe will sum up to a (sloooow, not used)
	static double CombProbSimple(int a, DealerShoe& shoe);

	// The probability that the next cards in the shoe will sum up to a (works only for a <= 6, fastest)
	static double CombProbEnumerated(int a, DealerShoe& shoe);

	// The probability that the next cards in the shoe will sum up to a and b (not used, slow implementation)
	//static double CombProb(int a, int b, DealerShoe& shoe);

	// The probability that the next cards in the shoe will be a and b (in the exact order)
	static double PermProb(int a, int b, DealerShoe& shoe);

	// The probability that the next cards in the shoe will be a, b and c (in the exact order)
	static double PermProb(int a, int b, int c, DealerShoe& shoe);

	// The probability that the next cards in the shoe will be a, b, c and d (in the exact order)
	static double PermProb(int a, int b, int c, int d, DealerShoe& shoe);

	// The probability that the next cards in the shoe will be a, b, c, d and e (in the exact order)
	static double PermProb(int a, int b, int c, int d, int e, DealerShoe& shoe);

	// The probability that the next cards in the shoe will be a, b, c, d, e and f (in the exact order)
	static double PermProb(int a, int b, int c, int d, int e, int f, DealerShoe& shoe);
};

#endif