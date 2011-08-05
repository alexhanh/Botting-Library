#include "Dealer.h"

double Dealer::cachedProbs[5];
double Dealer::cachedBust;

/*double Dealer::ProbBust(int upcard, Shoe shoe, bool bj)
{
	double not_bust_prob = 0.0;

	for (int i=17; i<=21; i++)
		not_bust_prob += Dealer::ProbTotal(Hand(upcard), i, shoe);

	if (bj)
		not_bust_prob += Dealer::ProbNatural(upcard, shoe);

	return 1.0-not_bust_prob;
}*/

double Dealer::ProbNatural(int upcard, Shoe shoe)
{
	if (upcard == 1)
		return shoe.CardProb(10);
	else if (upcard == 10)
		return shoe.CardProb(1);
	else
		return 0.0;
}

double Dealer::ProbNatural(Shoe shoe)
{
	double total_prob = 0.0;
	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i);
		shoe.DealCard(i);
		total_prob += tmp*Dealer::ProbNatural(i, shoe);
		shoe.AddCard(i);
	}
	return total_prob;
}

double Dealer::ProbTotalBrute(Hand hand, int total, Shoe shoe, int depth)
{
	// stop on soft >= 18
	// stop on hard >= 17
	if (hand.Total() >= 17 && !hand.Soft())
	{
		if (hand.Total() == total)
			return 1.0;
		else
			return 0.0;
	}
	if (hand.Total() >= 18 && hand.Soft())
	{
		if (hand.Total() == total)
			return 1.0;
		else
			return 0.0;
	}

	double total_prob = 0.0;
	// only upcard dealt and it is A or 10
	// if it is A, next card cannot be 10
	// if it is 10, next card cannot be A
	if (depth == 0 && (hand.Total() == 10 || hand.Total() == 11))
	{
		if (hand.Total() == 10)
		{
			for (int i=2; i<=10; i++)
			{
				double tmp = shoe[i]/(double)(shoe.Total()-shoe[1]);
				if (tmp > 0.0)
				{
					shoe.DealCard(i);
					total_prob += tmp*Dealer::ProbTotalBrute(hand+i, total, shoe, depth+1);
					shoe.AddCard(i);
				}
			}
		}
		else
		{
			for (int i=1; i<=9; i++)
			{
				double tmp = shoe[i]/(double)(shoe.Total()-shoe[10]);
				if (tmp > 0.0)
				{
					shoe.DealCard(i);
					total_prob += tmp*Dealer::ProbTotalBrute(hand+i, total, shoe, depth+1);
					shoe.AddCard(i);
				}
			}
		}

		return total_prob;
	}

	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			total_prob += tmp*Dealer::ProbTotalBrute(hand+i, total, shoe, depth+1);
			shoe.AddCard(i);
		}
	}

	return total_prob;
}