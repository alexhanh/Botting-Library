// BjEvalDLL.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "BjEvalDLL.h"

#include "Shoe.h"
#include "ShoeEV.h"
#include "Strategy.h"
#include "Hand.h"
#include "Player.h"
#include "Dealer.h"
#include "CorrectPlayer.h"

#include <iostream>

extern "C"
{
	BJEVALDLL_API void CacheDealerProbs(int upcard, const int shoe[])
	{
		Dealer::CacheProbs(upcard, Shoe(shoe));
	}

	BJEVALDLL_API double ShoeEv(const int counts[], int betSize)
	{
		Shoe shoe(counts);
		Strategy strategy = ShoeEV::GetStrategy(shoe, betSize);

		return strategy.TotalEV(betSize);
	}

	BJEVALDLL_API double DealEv(int player1, int player2, int upcard, const int shoe[], int betSize)
	{
		return ShoeEV::GetEV(upcard, player1, player2, Shoe(shoe), betSize);
	}

	BJEVALDLL_API double StandEv(SHand hand, int upcard, const int shoe[])
	{
		return Player::Stand(Hand(hand.Total, hand.Soft), upcard, Shoe(shoe));
	}
	BJEVALDLL_API double HitEv(SHand player, int upcard, const int shoe[])
	{
		return Player::Hit(Hand(player.Total, player.Soft), upcard, Shoe(shoe), 5);
	}

	BJEVALDLL_API double DoubleEv(SHand player, int upcard, int bet_size, const int shoe[])
	{
		return Player::Double(Hand(player.Total, player.Soft), upcard, Shoe(shoe), bet_size);
	}

	BJEVALDLL_API double InsuranceEv(int bet_size, const int shoe[])
	{
		return Player::Insurance(bet_size, Shoe(shoe));
	}

	BJEVALDLL_API double SurrenderEv()
	{
		return Player::Surrender();
	}

	BJEVALDLL_API double SplitEv(int split_card, int upcard, int bet_size, int max_splits, const int shoe[])
	{
		return Player::Split(split_card, upcard, Shoe(shoe), max_splits, bet_size);
	}

	BJEVALDLL_API double Version()
	{
		return 1.01;
	}
}