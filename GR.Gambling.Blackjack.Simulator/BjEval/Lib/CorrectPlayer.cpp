#include "CorrectPlayer.h"

#include "Dealer.h"
#include "Rakeback.h"
#include "Player.h"

#include <iostream>
using namespace std;

void CorrectPlayer::Initialize(Hand activeHands[], int numActive, bool allBusted, int bustedHands, int dealer, Shoe& shoe, int betSize)
{
	this->currentHand = activeHands[0];
	this->dealer = dealer;
	this->shoe = shoe;
	this->betSize = betSize;


	restEV_allBust = 0;
	restEV_noAllBust = 0;

	for (int i = numActive-1; i >= 1; i--)
	{
		if (allBusted)
		{
			restEV_allBust = HandEV(activeHands[i], dealer, shoe, betSize, true, bustedHands + i);
		}

		restEV_noAllBust = HandEV(activeHands[i], dealer, shoe, betSize, false, 0);
	}

	this->allBusted = allBusted;
	this->numBusted = bustedHands;
}



double CorrectPlayer::HandEV(Hand hand, int dealer, Shoe& shoe, int betSize, bool allBusted, int numBusted)
{
	ActionList actions = GetActions(hand, dealer, shoe, betSize, allBusted, numBusted);

	//cout << allBusted << " " << numBusted << " " << actions[0].GetTypeString() << " " << actions[0].ev << endl;

	return actions[0].ev;
}

ActionList CorrectPlayer::GetActions(Hand hand, int dealer, Shoe& shoe, int betSize, bool allBusted, int numBusted)
{
	this->allBusted = allBusted;
	this->numBusted = numBusted;

	ActionList actions;
	actions.Add(Action(ActionType::Stand, Stand(hand, dealer, shoe)));
	actions.Add(Action(ActionType::Hit, Hit(hand, dealer, shoe)));
	actions.Add(Action(ActionType::Double, Double(hand, dealer, shoe, betSize)));
	actions.Add(Action(ActionType::Surrender, SurrenderEV()));

	return actions;
}

double CorrectPlayer::Stand(Hand player, int upcard, Shoe& shoe, int numBets)
{
	if (player.Total() > 21)
	{
		double ev = -1.0 * numBets;

		if (allBusted) ev += restEV_allBust;
		else ev += 1.0*Dealer::ProbBust(upcard, shoe) + restEV_noAllBust;

		return ev;
	}

	double ev = 1.0;

	if (player.Total() < 17)
	{
		for (int i=17; i<=21; i++)
		{
			ev -= 2.0 * Dealer::ProbTotal(upcard, i, shoe);
		}
	}
	else
	{
		double prob_dealer_win = 0.0;

		for (int i=player.Total()+1; i<=21; i++)
		{
			prob_dealer_win += Dealer::ProbTotal(upcard, i, shoe);
		}

		double prob_tie = Dealer::ProbTotal(upcard, player.Total(), shoe);

		ev -= prob_tie + 2.0*prob_dealer_win;
	}

	ev *= numBets;

	if (allBusted) ev += numBusted * Dealer::ProbBust(upcard, shoe);
	ev += restEV_noAllBust;

	return ev;
}

double CorrectPlayer::Hit(Hand player, int upcard, Shoe& shoe, int rec)
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

double CorrectPlayer::Double(Hand player, int upcard, Shoe& shoe, int betSize)
{
	double total = Rakeback::Amount(betSize, betSize);

	for (int i=1; i<=10; i++)
	{
		double tmp = shoe.CardProb(i, upcard);
		if (tmp > 0.0)
		{
			shoe.DealCard(i);
			total += tmp * Stand(player+i, upcard, shoe, 2);
			shoe.AddCard(i);
		}
	}
	return total;
}

double CorrectPlayer::Split(int card, int dealer, Shoe& shoe, int betSize)
{
	double total = Rakeback::Amount(betSize, betSize);

	for (int i2 = 1; i2 <= 10; i2++)
	{
		Hand h2(card); h2+=i2;

		// Save the original values
		double oAllBust = restEV_allBust;
		double oNoAllBust = restEV_noAllBust;


		if (allBusted)
		{
			restEV_allBust = HandEV(h2, dealer, shoe, betSize, true, numBusted + 1);
		}

		restEV_noAllBust = HandEV(h2, dealer, shoe, betSize, false, 0);
		

		double tmp1 = shoe.CardProb(i2, dealer);
		shoe.DealCard(i2);

		for (int i1 = 1; i1 <= 10; i1++)
		{
			Hand h1(card); h1+=i1;

			double tmp2 = tmp1 * shoe.CardProb(i1, dealer);
			shoe.DealCard(i1);

			/// some magic here

			shoe.AddCard(i1);
		}

		shoe.AddCard(i2);

		// Restore the original values
		restEV_allBust = oAllBust;
		restEV_noAllBust = oNoAllBust;
	}

	return total;
}
