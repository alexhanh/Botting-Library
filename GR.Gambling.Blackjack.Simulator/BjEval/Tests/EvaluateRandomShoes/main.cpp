#include <iostream>
#include <fstream>
#include <iomanip>
#include <time.h>
#include <vector>
#include <algorithm>

#include "Shoe.h"
#include "Player.h"
#include "Hand.h"
#include "Dealer.h"
#include "ShoeEV.h"

#include "Strategy.h"

using namespace std;

int main()
{
	srand(time(0));

	char filename[50];
	int shoeNumber = 1;

	while (true)
	{
		cout << "Generating random shoe.." << endl;
		Shoe shoe(8);
		shoe.RemoveRandom(rand()%85);


		for (int i=1; i<=10; i++) cout << shoe[i] << " ";
		cout << endl;
		cout << shoe.Total() << endl;

		cout << "Evaluating.." << endl;

		Strategy strategy = ShoeEV::GetStrategy(shoe, 5000);

		sprintf_s(filename, "ref%d.txt", shoeNumber);

		cout << "Saving strategy to " << filename << endl;

		strategy.Write(filename);

		cout << "Total EV: " << strategy.TotalEV(5000) << endl;

		shoeNumber++;
	}


	return 0;
}