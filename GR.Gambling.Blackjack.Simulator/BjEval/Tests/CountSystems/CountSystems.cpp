#include "Shoe.h"
#include "ShoeEv.h"

#include <iostream>

using namespace std;

class CountSystem
{
private:
	double tag_values[10];
	int card_counts[10];

	// the running count
	double rc;

	void UpdateRc()
	{
		rc = 0.0;
		for (int i=0; i<10; i++)
			rc += card_counts[i]*tag_values[i];
	}
public:
	CountSystem(double tag_values[10], int card_counts[10])
	{
		for (int i=0; i<10; i++)
		{
			this->tag_values[i] = tag_values[i];
			this->card_counts[i] = card_counts[i];
		}

		UpdateRc();
	}

	CountSystem(double tag_values[10])
	{
		int counts[10] = {0,0,0,0,0,0,0,0,0,0};
		for (int i=0; i<10; i++)
		{
			this->tag_values[i] = tag_values[i];
			this->card_counts[i] = counts[i];
		}

		UpdateRc();
	}

	// 'card' 1..10
	int operator [] (const int card)
	{
		return tag_values[card-1];
	}

	void SetCount(int card_counts[10])
	{
		for (int i=0; i<10; i++)
			this->card_counts[i] = card_counts[i];

		UpdateRc();
	}

	void AddCard(int card)
	{
		card_counts[card-1]++;
		rc += tag_values[card-1];
	}

	void ResetCount()
	{
		for (int i=0; i<10; i++)
			card_counts[i] = 0;

		rc = 0.0;
	}

	double RunningCount()
	{
		return rc;
	}
};

#define MAX_RUNS 100

int main()
{
	// revere apc
	//double tag_values[10] = {-4,2,3,3,4,3,2,0,-1,-3}; 
	// uston ss 
	//double tag_values[10] = {-2,2,2,2,3,2,1,0,-1,-2}; 
	// revere point count
	//double tag_values[10] = {-2,1,2,2,2,2,1,0,0,-2};
	double tag_values[10] = { -0.000715645, 0.000601558,0.000734085,0.000997088,0.00121018,0.000693555,0.0003251,-0.000102365,-0.000378124, -0.000817202}; 
	CountSystem count_system(tag_values);

	int matches = 0;
	for (int runs = 0; runs < MAX_RUNS; runs++)
	{
		Shoe shoe(8);
		int card_counts[10] = {0};

		shoe.RemoveRandom(rand()%85);
		for (int card=0; card<9; card++)
			card_counts[card] = 8*4 - shoe[card+1];
		card_counts[9] = 4*8*4 - shoe[10];

		count_system.SetCount(card_counts);

		Strategy strategy = ShoeEV::GetStrategy(shoe, 5000);
		double shoe_ev = strategy.TotalEV(5000);

		if ((shoe_ev > 0.0 && count_system.RunningCount() > 0.0)||
			(shoe_ev < 0.0 && count_system.RunningCount() <= 0.0))
		{
			matches++;
		}
		else
		{
			
		}

		cout << runs << " " << shoe_ev << " " << count_system.RunningCount() << endl;
	}

	cout << "non matches: " << MAX_RUNS-matches << endl; 

	return 0;
}