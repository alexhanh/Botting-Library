#include "Dealer.h"
#include "PDFDealer.h"
#include "Shoe.h"

#include <windows.h>

#include <iostream>
using namespace std;

#include <time.h>

void ProfileProbs()
{
	int seed = 1000;
	int repeats = 50;

	int iterations = 0;

	int shoeCount = 0;

	for (shoeCount=1; shoeCount<=50; shoeCount++)
	{
		Shoe shoe(8);
		shoe.Shuffle();
		shoe.RemoveRandom(rand()%84);

		Hand hand(1+rand()%10);


		for (int total=17; total<=21; total++)
		{
			iterations+=repeats;


			double prob2;
			for (int r=0; r<repeats; r++)
			{
				prob2 = PDFDealer::ProbTotal(hand, total, shoe);
			}

		}

		if (shoeCount%10==0)
		{
			cout << "Number of test cases: " << shoeCount << endl;
		}
	}
}

void TestProbs()
{
	int seed = time(0);
	srand(seed);
	int repeats = 1;

	double totalError = 0;

	int iterations = 0;

	int time1 = 0, time2 = 0;

	int shoeCount = 0;

	for (shoeCount=1; shoeCount<=100000; shoeCount++)
	{
		Shoe shoe(8);
		shoe.Shuffle();
		shoe.RemoveRandom(rand()%84);

		Hand hand(1+rand()%10);


		for (int total=17; total<=21; total++)
		{
			iterations+=repeats;

			double prob1, prob2;

			int start = GetTickCount();
			for (int r=0; r<repeats; r++)
			{
				prob1 = Dealer::ProbTotal(hand, total, shoe);
			}
			time1 += GetTickCount() - start;

			start = GetTickCount();
			for (int r=0; r<repeats; r++)
			{
				prob2 = PDFDealer::ProbTotal(hand, total, shoe);
			}
			time2 += GetTickCount() - start;

			double error = prob2-prob1;
			if (error < 0) error = -error;

			totalError+=error;

			if (error > 0.0000001)
			{

				cout << "Error detected!" << endl;
				cout << "Shoe ";
				for (int c=1; c<=10; c++) cout << shoe[c] << " ";
				cout << endl;

				cout << "Upcard: " << hand.Total() << " Total: " << total << endl;
				cout << "Prob1: " << prob1 << endl;
				cout << "Prob2: " << prob2 << endl << endl;
			}
		}

		if (totalError > 0.00001) break;

		if (repeats >= 1000 || shoeCount%(1000/repeats)==0)
		{
			cout << "Number of test cases: " << shoeCount << endl;
			cout << "Total error: " << totalError << endl;
			cout << "Method 1 average time: " << (time1/(double)iterations) << " ms" << endl;
			cout << "Method 2 average time: " << (time2/(double)iterations) << " ms" <<endl << endl;
		}
	}
}

void CompareTimes()
{
	int repeats = 20;
	int tests = 200;	
	
	int seed = time(0);

	double total1 = 0, total2 = 0;

	srand(seed);

	int iterations = 0;
	int start = GetTickCount();
	for (int shoeCount=1; shoeCount<=tests; shoeCount++)
	{
		Shoe shoe(8);
		shoe.Shuffle();
		shoe.RemoveRandom(rand()%84);

		Hand hand(1+rand()%10);


		for (int total=17; total<=21; total++)
		{
			iterations+=repeats;

			for (int r=0; r<repeats; r++)
			{
				total1 += PDFDealer::ProbTotal(hand, total, shoe);
			}
		}
	}

	int time = GetTickCount() - start;

	cout << "Repeats: " << iterations << ", Total time: " << time << " ms, Average time: " << (time/(double)iterations) << " ms" << endl;


	srand(seed);

	iterations = 0;
	start = GetTickCount();
	for (int shoeCount=1; shoeCount<=tests; shoeCount++)
	{
		Shoe shoe(8);
		shoe.Shuffle();
		shoe.RemoveRandom(rand()%370);

		Hand hand(1+rand()%10);


		for (int total=17; total<=21; total++)
		{
			iterations+=repeats;

			for (int r=0; r<repeats; r++)
			{
				total2 += Dealer::ProbTotal(hand, total, shoe);
			}
		}
	}

	time = GetTickCount() - start;

	cout << "Repeats: " << iterations << ", Total time: " << time << " ms, Average time: " << (time/(double)iterations) << " ms" << endl;

	cout << total1 << endl;
	cout << total2 << endl;
}

int main()
{
	/*for (int i=1; i<=6; i++)
	{
		cout << "Permutations for a = " << i << endl;
		PDFDealer::PrintProbACombinations(i);
		cout << endl;
	}*/

	CompareTimes();

	//ProfileProbs();

	TestProbs();

	return 0;
}