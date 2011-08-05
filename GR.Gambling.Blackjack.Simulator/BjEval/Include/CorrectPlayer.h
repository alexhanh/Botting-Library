#ifndef __CORRECTPLAYER_H__
#define __CORRECTPLAYER_H__

#include "Common.h"
#include "Hand.h"
#include "Shoe.h"
#include "Action.h"
#include "Player.h"

class CorrectPlayer
{
private:

	Hand currentHand;
	int dealer;
	Shoe shoe;
	int betSize;

	bool allBusted;
	int numBusted;


	double restEV_allBust, restEV_noAllBust;

	double Stand(Hand player, int upcard, Shoe& shoe, int numBets = 1);
	double Hit(Hand player, int upcard, Shoe& shoe, int rec = 5);

	double Double(Hand player, int upcard, Shoe& shoe, int betSize);

	double Split(int card, int dealer, Shoe& shoe, int betSize);

public:

	CorrectPlayer() : restEV_allBust(0), restEV_noAllBust(0) {}

	// activeHands - List of all unfinished hands. First hand of the list is the hand we are playing now.
	// bustedHands - Number of hands busted so far if all finished hands have busted.
	void Initialize(Hand activeHands[], int numActive, bool allBusted, int bustedHands, int dealer, Shoe& shoe, int betSize);

	double StandEV()
	{
		return Stand(currentHand, dealer, shoe);
	}

	double HitEV()
	{
		return Hit(currentHand, dealer, shoe);
	}

	double DoubleEV()
	{
		return Double(currentHand, dealer, shoe, betSize);
	}

	double SplitEV(int splitCard, int maxSplits)
	{
		return Player::Split(splitCard, dealer, shoe, maxSplits, betSize);
	}

	double SurrenderEV()
	{
		return -0.5;
	}

	double InsuranceEV()
	{
		return Player::Insurance(betSize, shoe);
	}

	double HandEV(Hand hand, int dealer, Shoe& shoe, int betSize, bool allBusted, int numBusted);
	ActionList GetActions(Hand hand, int dealer, Shoe& shoe, int betSize, bool allBusted, int numBusted);
};


#endif