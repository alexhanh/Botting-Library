#include "Hand.h"

//#include <iostream>

//using namespace std;

Hand::Hand(int card)
{
	if (card==1)
	{
		total = 11;
		soft = true;
	}
	else
	{
		total = card;
		soft = false;
	}
}

Hand& Hand::operator += (int card)
{
	if (soft)
	{
		if (total+card <= 21)
			total += card;
		else
		{
			total += card-10;
			soft = false;
		}
	}
	else
	{
		if (card == 1 && (total+11 <= 21))
		{
			total += 11;
			soft = true;
		}
		else
			total += card;
	}
	return *this;
}

void Hand::Print()
{
//	char c = soft?'S':'H';
//	cout << c << total;
}