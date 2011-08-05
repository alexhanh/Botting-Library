#include <iostream>
#include <fstream>
#include <iomanip>
#include <time.h>

#include "Dealer.h"
#include "Shoe.h"
#include "Player.h"

using namespace std;

void dealer_probs(Shoe shoe)
{
	fstream file("results.txt", ios::out);

	// shoe distribution
	file << "Shoe distribution" << endl << endl;
	file << "Card" << '\t';
	for (int i=1; i<=10; i++)
		file << i << '\t';
	file << "Total" << endl;

	file << "Count" << '\t';
	for (int i=1; i<=10; i++)
		file << shoe[i] << '\t';
	file << shoe.Total() << endl;

	file << "P(Card)" << '\t';
	for (int i=1; i<=10; i++)
		file << setprecision(2) << shoe.CardProb(i) << '\t';

	file << endl << endl;
	file << "Dealer probabilities" << endl << endl;

	file << "Blackjack" << endl;
	file << "Upcard" << '\t' << '\t';
	for (int i=1; i<=10; i++)
		file << i << '\t';
	file << "Total" << endl;

	file << "P(Bj|Card)" << '\t';
	for (int i=1; i<=10; i++)
	{
		shoe.DealCard(i);
		file << Dealer::ProbNatural(i, shoe) << '\t';
		shoe.AddCard(i);
	}
	file << Dealer::ProbNatural(shoe) << endl;

	file.close();
}

int main()
{
	dealer_probs(Shoe(8));

	return 0;
}