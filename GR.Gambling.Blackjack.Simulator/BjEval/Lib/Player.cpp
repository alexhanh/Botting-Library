#include "Player.h"

#include "Hand.h"
#include "Shoe.h"
#include "Dealer.h"
#include "Rakeback.h"

#include "Common.h"


double Player::Hit(Hand player, int upcard, Shoe& shoe, int rec)
{
	double total = 0.0;
	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			if (player+i >= 21 || rec <= 0)
				total += tmp*Stand(player+i, upcard, shoe);
			else
				total += tmp*max(Stand(player+i, upcard, shoe), Hit(player+i,upcard, shoe, rec-1));
			shoe.AddCard(i);
		}
	}
	return total;
}


double Player::Double(Hand player, int upcard, Shoe& shoe, int betSize)
{
	double total = Rakeback::Amount(betSize, betSize);

	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			total += tmp * 2.0 * Stand(player+i, upcard, shoe);
			shoe.AddCard(i);
		}
	}
	return total;
}

// are initial bet's pps affected with surrendering?
double Player::Surrender()
{
	return -0.5;
}

double Player::Stand(Hand player, Hand upcard, Shoe& shoe)
{
	if (player.Total() > 21)
		return -1.0;
	if (player.Total() < 17)
	{
		double ev = 1.0;
		
		for (int i=17; i<=21; i++)
		{
			ev -= 2.0 * Dealer::ProbTotal(upcard, i, shoe);
		}

		return ev;
	}
	else
	{
		double prob_dealer_win = 0.0;

		for (int i=player.Total()+1; i<=21; i++)
		{
			prob_dealer_win += Dealer::ProbTotal(upcard, i, shoe);
		}

		double prob_tie = Dealer::ProbTotal(upcard, player.Total(), shoe);

		return 1.0-prob_tie-2.0*prob_dealer_win;
	}
}

double Player::SplitAces(int upcard, Shoe& shoe, int betSize)
{
	double total = Rakeback::Amount(betSize, betSize);

	Hand ace(1);
	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			total += tmp * 2.0 * Stand(ace+i, upcard, shoe);
			shoe.AddCard(i);
		}
	}

	return total;
}


double Player::NoSplit(Hand hand, int upcard, Shoe& shoe, bool das, int betSize)
{
	if (hand >= 21)
		return Stand(hand, upcard, shoe);
	else
		if (das)
			return max(Stand(hand, upcard, shoe), max(Double(hand, upcard, shoe, betSize), Hit(hand, upcard, shoe)));
		else
			return max(Stand(hand, upcard, shoe), Hit(hand, upcard, shoe));
}


double Player::SplitNonAces(Hand hand, int upcard, Shoe& shoe, bool das, int maxReSplits, int betSize, const double noSplitEV[])
{
	double total = Rakeback::Amount(betSize, betSize);

	if (maxReSplits == 0)
	{
		/*
		for (int i=1; i<=10; i++)
		{
			double tmp = shoe.CardProb(i, upcard);

			if (tmp > 0.000001)
			{
				shoe.DealCard(i);
				total += 2 * tmp * NoSplit(hand+i, upcard, shoe, das, betSize);
				shoe.AddCard(i);
			}
		}
		*/

		for (int i=1; i<=10; i++)
		{
			double tmp = shoe.CardProb(i, upcard);

			total += 2 * tmp * noSplitEV[i];
		}
	}
	else
	{
		/*double noSplitEV[11];
		for (int i=1; i<=10; i++)
		{
			if (shoe[i] > 0)
			{
				shoe.DealCard(i);

				noSplitEV[i] = NoSplit(hand+i, upcard, shoe, das, betSize);

				shoe.AddCard(i);
			}
			else noSplitEV[i] = 0;
		}*/

		double split[2] = {0};
		if (maxReSplits>=1)
		{
			int i = hand.Total();

			if (shoe[i] > 0)
			{
				shoe.DealCard(i);

				split[0] = SplitNonAces(hand, upcard, shoe, das, 0, betSize, noSplitEV);

				if (maxReSplits>=2 && shoe[i] > 0)
				{
					shoe.DealCard(i);
					split[1] = SplitNonAces(hand, upcard, shoe, das, 1, betSize, noSplitEV);
					shoe.AddCard(i);
				}

				shoe.AddCard(i);
			}
		}


		for (int i=1; i<=10; i++)
		{
			double tmp_i = shoe.CardProb(i, upcard);
			shoe.DealCard(i);

			for (int j=i; j<=10; j++)
			{
				double tmp = tmp_i * shoe.CardProb(j, upcard);

				if (i!=j) tmp *= 2;

				if (tmp < 0.000001) continue;

				double p;

				if (i==j && i == hand.Total() && maxReSplits==2)
				{
					//p = 2 * split[0];
					p = 2 * max(noSplitEV[i], split[0]);
				}
				else
				{
					if (i == hand.Total()) // && maxReSplits>=1
					{
						//p = split[maxReSplits-1] + no_split[j];
						p = max(noSplitEV[i], split[maxReSplits-1]) + noSplitEV[j];
					}
					else if (j == hand.Total()) // && maxReSplits>=1
					{
						//p = split[maxReSplits-1] + no_split[i];
						p = max(noSplitEV[j], split[maxReSplits-1]) + noSplitEV[i];
					}
					else
					{
						p = noSplitEV[i] + noSplitEV[j];
					}
				}

				total += tmp * p;
			}

			shoe.AddCard(i);
		}
	}

	return total;
}

double Player::Split(int split_card, int upcard, Shoe& shoe, int maxSplits, int betSize)
{
	if (split_card == 1) return SplitAces(upcard, shoe, betSize);
	else
	{
		Hand hand(split_card);
		double noSplitEV[11];
		for (int i=1; i<=10; i++)
		{
			if (shoe[i] > 0)
			{
				shoe.DealCard(i);

				noSplitEV[i] = NoSplit(hand+i, upcard, shoe, true, betSize);

				shoe.AddCard(i);
			}
			else noSplitEV[i] = 0;
		}

		return SplitNonAces(split_card, upcard, shoe, true, min(3, maxSplits)-1, betSize, noSplitEV);
	}
}

double Player::Insurance(int bet_size, Shoe& shoe)
{
	double bj_prob = shoe.CardProb(10);

	return 1.0*bj_prob - 0.5*(1.0-bj_prob) + Rakeback::Amount(bet_size/2, bet_size);
}

/*
double Player::Split(int split_card, int upcard, Shoe shoe)
{
	if (split_card == 1)
		return SplitAces(upcard, shoe, false);
	else
		return SplitNonAces(split_card, upcard, shoe, true, true);
}*/

/*
double Player::SplitNonAces(Hand hand, int upcard, Shoe shoe, bool das, bool rsp)
{
	double total = 0.0;

	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			if (rsp && hand == i)
				total += tmp*max(NoSplit(hand+i, upcard, shoe, das), SplitNonAces(hand, upcard, shoe, das, false));
			else
				total += tmp*NoSplit(hand+i, upcard, shoe, das);

			shoe.AddCard(i);
		}
	}

	return 2.0*total;
}*/

/*double Player::SplitNonAces(Hand hand, int upcard, Shoe shoe, bool das, int maxReSplits)
{
	double no_split[11];
	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.000001)
		{
			shoe.DealCard(i);

			no_split[i] = NoSplit(hand+i, upcard, shoe, das);

			shoe.AddCard(i);
		}
		else no_split[i] = 0;
	}

	double split[2] = {0};
	if (maxReSplits>=1)
	{
		int i = hand.Total();
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.000001)
		{
			shoe.DealCard(i);

			split[0] = SplitNonAces(hand, upcard, shoe, das, 0);

			if (maxReSplits>=2)
			{
				split[1] = SplitNonAces(hand, upcard, shoe, das, 1);
			}

			shoe.AddCard(i);
		}
	}

	double total = 0.0;

	for (int i=1; i<=10; i++)
	{
		if (i == hand.Total()) continue;

		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.000001)
		{
			total += 2 * tmp * no_split[i];
		}
	}


	int i = hand.Total();
	double tmp = shoe.CardProb(i, upcard);

	if (maxReSplits == 0)
	{
		total += 2 * tmp * no_split[i];
	}
	else
	{
		for (int j=1; j<=10; j++)
		{
			double tmp2 = tmp * shoe.CardProb(j, upcard); // sums up to tmp for j=1...10

			if (tmp2 > 0.0000001)
			{
				if (i!=j || maxReSplits == 1)
				{
					double ev = 
						max(no_split[i], split[maxReSplits-1]) + 
						no_split[j];

					if (i!=j) ev*=2;

					total += tmp2 * ev;
				}
				else
				{
					total += 2 * tmp2 * max(no_split[i], split[0]);
				}
			}
		}
	}

	return total;
}*/

/*
double Player::SplitAces(int upcard, Shoe shoe, bool rsa, int betSize)
{
	Hand ace(1);
	double total = 0.0;
	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			if (rsa && i==1)
				total += tmp*max(Stand(ace+i, upcard, shoe), SplitAces(upcard, shoe, false));
			else
				total += tmp*Stand(ace+i, upcard, shoe);
			shoe.AddCard(i);
		}
	}

	return 2.0*total;
}
*/
