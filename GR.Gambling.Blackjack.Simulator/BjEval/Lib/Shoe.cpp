#include "Shoe.h"

#include <stdlib.h>
#include <iostream>

Shoe::Shoe(int decks)
{
	this->decks = decks;
	Shuffle();
}

Shoe::Shoe(const int count[10])
{
	this->total = 0;
	for (int i=0; i<10; i++)
	{
		this->count[i] = count[i];
		this->total += count[i];
	}
}

void Shoe::Shuffle()
{
	for (int i = 0; i < 9; i++)
		count[i] = 4*decks;
	count[9] = 4*4*decks;
	total = 52*decks;
}

double Shoe::CardProb(int card, int upcard)
{
	if (upcard == 10)
	{
		if (card == 1)
			return (double)count[0] / (double)(total-1);
		else
			return (double)(count[card-1]*(total-count[0]-1) / (double)((total-1)*(total-count[0])));
	} // upcard = 1
	else if (upcard == 1)
	{
		if (card == 10)
			return (double)count[9] / (double)(total-1);
		else
			return (double)(count[card-1]*(total-count[9]-1) / (double)((total-1)*(total-count[9])));
	}
	else
		return CardProb(card);
}



void Shoe::RemoveRandom(Random& random, int number)
{
	for (int i=0; i<number; i++)
	{
		DealRandom(random);
	}
}

int Shoe::DealRandom(Random& random)
{
	double number = random.uniform();

	double sum = 0;

	for (int i=1; i<=10; i++)
	{
		sum += CardProb(i);

		if (number <= sum) 
		{
			DealCard(i);
			return i;
		}
	}

	std::cout << "Should never get here" << std::endl;
	exit(0);

	return -1;
}