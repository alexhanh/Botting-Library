#include <iostream>
#include <fstream>
#include <iomanip>
#include <time.h>

#include "Shoe.h"
#include "Player.h"
#include "Hand.h"
#include "Dealer.h"

using namespace std;

double ShoeEv(Shoe shoe)
{
/*	fstream file("results.txt", ios::out);

	double total = 0.0;
	double p_upcard, p_player1, p_player2;
	bool is_pair;
	double combo_prob;
	int hands = 0;
	int upcard, player1, player2;
	double stand_ev, hit_ev, double_ev, surrender_ev, split_ev, insurance_ev, max_ev;
	// uses the information that the probabilities are the same for both permutations
	for (upcard=1; upcard<=10; upcard++)
	{
		p_upcard = shoe.CardProb(upcard);
		if (p_upcard > 0.0)
		{
			shoe.DealCard(upcard);
			for (player1=1; player1<=10; player1++)
			{
				p_player1 = shoe.CardProb(player1);
				if (p_player1 > 0.0)
				{
					shoe.DealCard(player1);
					for (player2=player1; player2<=10; player2++)
					{
						p_player2 = shoe.CardProb(player2);
						if (p_player2 > 0.0)
						{
							shoe.DealCard(player2);

							Hand player(player1); player += player2;
							is_pair = (player1==player2)?true:false;

							combo_prob = p_upcard*p_player1*p_player2*2.0;
							hands+=2;
							if (is_pair)
							{
								combo_prob *= 0.5;
								hands--;
							}

							file << upcard << "|" << player1 << "," << player2 << " %= " << combo_prob << endl;

							if (upcard >= 2 && upcard <= 9)
							{
								// player has blackjack
								if ((player1 == 1 && player2 == 10)||(player1 == 10 && player2 == 1))
								{
									file << "stand: " << 1.5 << endl;
									total += combo_prob*1.5;
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
										split_ev = Player::Split(player1, upcard, shoe);
										max_ev = max(max_ev, split_ev);
									}

									file << "stand: " << stand_ev << endl;
									file << "hit: " << hit_ev << endl;
									file << "double: " << double_ev << endl;
									file << "surrender: " << surrender_ev << endl;
									if (is_pair)
										file << "split: " << split_ev << endl;

									total += combo_prob*max_ev;
								}
							}
							else
							{
								double dealer_bj = Dealer::ProbNatural(upcard, shoe);
								file << "dealer bj% = " << dealer_bj << endl;
								// player has blackjack
								if ((player1 == 1 && player2 == 10)||(player1 == 10 && player2 == 1))
								{
									total += combo_prob*((1.0-dealer_bj)*1.5);
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
										split_ev = Player::Split(player1, upcard, shoe);
										max_ev = max(max_ev, split_ev);
										file << "split: " << split_ev << endl;
									}

									total += combo_prob*((1.0-dealer_bj)*max_ev - dealer_bj + insurance_ev);
									//total += combo_prob*(-dealer_bj + (1.0-dealer_bj)*max_ev);
									//total += combo_prob*(1.0-dealer_bj)*max_ev - dealer_bj;

								}
							}
							shoe.AddCard(player2);
							file << endl;
						}
					}
					shoe.AddCard(player1);
				}
			}
			shoe.AddCard(upcard);
		}
	}

	file << "total ev: " << total << endl;
	file << "total hands: " << hands << endl;

	file.close();

	return total;*/

return 0;
}


int main()
{
	ShoeEv(Shoe(8));

	return 0;
}