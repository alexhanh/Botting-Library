#include "ShoeEV.h"

#include <iostream>
#include <fstream>
#include <iomanip>
#include <time.h>

#include "Player.h"
#include "Dealer.h"
#include "Rakeback.h"

using namespace std;

/*
double ShoeEV::Evaluate(Shoe shoe)
{
	fstream file("results.txt", ios::out);
	fstream summary("summary.txt", ios::out);

	double total = 0.0;

	int hands = 0;

	// uses the information that the probabilities are the same for both permutations
	for (int upcard=1; upcard<=10; upcard++)
	{
		double p_upcard = shoe.CardProb(upcard);
		if (p_upcard > 0.0)
		{
			shoe.DealCard(upcard);

			double upcard_ev = 0.0;

			for (int player1=1; player1<=10; player1++)
			{
				double p_player1 = shoe.CardProb(player1);
				if (p_player1 > 0.0)
				{
					shoe.DealCard(player1);

					for (int player2=player1; player2<=10; player2++)
					{
						//if (player2!=player1) continue;

						double p_player2 = shoe.CardProb(player2);
						if (p_player2 > 0.0)
						{
							cout << "Evaluating " << upcard << " " << player1 << " " <<  player2 << endl;

							shoe.DealCard(player2);


							double combo_prob;
							if (player1 != player2)
							{
								combo_prob = 2 * p_upcard * p_player1 * p_player2;
								hands+=2;
							}
							else
							{
								combo_prob = p_upcard * p_player1 * p_player2;
								hands++;
							}

							file << upcard << "|" << player1 << "," << player2 << " %= " << combo_prob << endl;

							double ev = GameEV(shoe, player1, player2, upcard, file);
							upcard_ev += combo_prob * ev;

							file << endl;

							shoe.AddCard(player2);
						}
					}
					shoe.AddCard(player1);
				}
			}

			summary << "Upcard " << upcard << ": " << (upcard_ev/p_upcard) << " => " << upcard_ev << endl;

			total += upcard_ev;

			shoe.AddCard(upcard);
		}
	}

	summary << "total ev: " << total << endl;
	summary << "total hands: " << hands << endl;

	file.close();
	summary.close();

	return total;
}

double ShoeEV::GameEV(Shoe& shoe, int player1, int player2, int upcard, std::fstream& file)
{
	bool is_pair = (player1==player2)?true:false;

	Hand player(player1); player += player2;

	double stand_ev, hit_ev, double_ev, surrender_ev, split_ev, insurance_ev, max_ev;

	if (upcard >= 2 && upcard <= 9)
	{
		// player has blackjack
		if ((player1 == 1 && player2 == 10)||(player1 == 10 && player2 == 1))
		{
			file << "stand: " << 1.5 << endl;
			
			return 1.5;
		}
		else
		{
			stand_ev = Player::Stand(player, upcard, shoe);
			hit_ev = Player::Hit(player, upcard, shoe);
			double_ev = Player::Double(player, upcard, shoe);
			surrender_ev = Player::Surrender();
			split_ev = -10;

			max_ev = max(stand_ev, max(hit_ev, max(double_ev, surrender_ev)));
			if (is_pair)
			{
				split_ev = Player::Split(player1, upcard, shoe, 3);
				max_ev = max(max_ev, split_ev);
			}

			file << "stand: " << stand_ev << endl;
			file << "hit: " << hit_ev << endl;
			file << "double: " << double_ev << endl;
			file << "surrender: " << surrender_ev << endl;
			if (is_pair)
				file << "split: " << split_ev << endl;

			return max_ev;
		}
	}
	else
	{
		double dealer_bj = Dealer::ProbNatural(upcard, shoe);
		file << "dealer bj% = " << dealer_bj << endl;
		// player has blackjack
		if ((player1 == 1 && player2 == 10)||(player1 == 10 && player2 == 1))
		{
			return (1.0-dealer_bj) * 1.5;
		}
		else
		{
			stand_ev = Player::Stand(player, upcard, shoe);
			hit_ev = Player::Hit(player, upcard, shoe);
			double_ev = Player::Double(player, upcard, shoe);
			surrender_ev = Player::Surrender();
			split_ev = -10;
			insurance_ev = 0.0;
			if (upcard==1)
			{
				insurance_ev = Player::Insurance(shoe);
				file << "insurance: " << insurance_ev << endl;
				insurance_ev = max(0.0, insurance_ev);
			}
			
			file << "stand: " << stand_ev << endl;
			file << "hit: " << hit_ev << endl;
			file << "double: " << double_ev << endl;
			file << "surrender: " << surrender_ev << endl;

			max_ev = max(stand_ev, max(hit_ev, max(double_ev, surrender_ev)));
			if (is_pair)
			{
				split_ev = Player::Split(player1, upcard, shoe, 3);
				max_ev = max(max_ev, split_ev);
				file << "split: " << split_ev << endl;
			}

			return (1.0-dealer_bj)*max_ev - dealer_bj + insurance_ev;
		}
	}
}*/

Strategy ShoeEV::GetStrategy(Shoe shoe, int betSize)
{
	Strategy strategy(shoe);


	for (int dealer = 1; dealer <= 10; dealer++)
	{
		shoe.DealCard(dealer);

		for (int player1 = 1; player1 <= 10; player1++)
		{
			shoe.DealCard(player1);

			//Dealer::CacheProbs(dealer, shoe);

			for (int player2 = 1; player2 <= player1; player2++)
			{
				shoe.DealCard(player2);

				Dealer::CacheProbs(dealer, shoe);

				ActionList actions = GetActions(dealer, player1, player2, shoe, betSize);

				strategy.SetActions(dealer, player1, player2, actions);

				shoe.AddCard(player2);
			}

			shoe.AddCard(player1);
		}

		shoe.AddCard(dealer);
	}

	return strategy;
}

ActionList ShoeEV::GetActions(int dealer, int player1, int player2, Shoe& shoe, int betSize)
{
	//cout << "Evaluating " << dealer << " | " << player1 << "," << player2 << endl;

	Hand player(player1); player+=player2;

	ActionList actions;

	actions.Add(Action(Stand,		Player::Stand(player, dealer, shoe)));
	actions.Add(Action(Hit,			Player::Hit(player, dealer, shoe)));
	actions.Add(Action(Double,		Player::Double(player, dealer, shoe, betSize)));
	actions.Add(Action(Surrender,	Player::Surrender()));

	if (player1 == player2)
	{
		actions.Add(Action(Split,	Player::Split(player1, dealer, shoe, 3, betSize)));	
	}

	return actions;
}

double ShoeEV::GetEV(int dealer, int player1, int player2, Shoe& shoe, int betSize)
{
	double ev = Rakeback::Amount(betSize, betSize);

	double dealerBJ = 0;

	if (dealer == 1) dealerBJ = shoe.CardProb(10);
	else if (dealer == 10) dealerBJ = shoe.CardProb(1);

	if ((player1==10 && player2==1) || (player1==1 && player2==10))
	{
		ev += (1 - dealerBJ) * 1.5;
	}
	else
	{
		double insuranceEV = 0.0;

		if (dealer == 1)
		{
			insuranceEV = 1.0*dealerBJ - 0.5*(1.0-dealerBJ) + Rakeback::Amount(betSize/2, betSize);

			if (insuranceEV < 0) insuranceEV = 0;
		}

		Hand player(player1); player+=player2;

		ActionList actions;

		actions.Add(Action(Stand,		Player::Stand(player, dealer, shoe)));
		actions.Add(Action(Hit,			Player::Hit(player, dealer, shoe)));
		actions.Add(Action(Double,		Player::Double(player, dealer, shoe, betSize)));
		actions.Add(Action(Surrender,	Player::Surrender()));
		if (player1 == player2)
		{
			actions.Add(Action(Split,	Player::Split(player1, dealer, shoe, 3, betSize)));	
		}

		ev += dealerBJ * (-1) + (1-dealerBJ) * actions[0].ev + insuranceEV;
	}

	return ev;
}
