#include "PDFDealer.h"

#include "Common.h"

#include <iostream>
#include <iomanip>

// Calculates the probability that the dealer will stop to e, where 17 <= e <= 21
double PDFDealer::DealerProb21(Hand d, int e, PDFDealer::DealerShoe& shoe)
{
	double total = 0.0;

	if (e==17) 
	{
		// dealer can only stop to hard 17
		return DealerHard17(d, 17, shoe);
	}
	else
	{
		// 17 soft -> e soft
		{
			int i = e-17;

			double tmp = shoe.CardProb(i);

			if (tmp > 0.000001)
			{
				shoe.DealCard(i);
				total+=tmp*DealerSoft17(d, 17, shoe);
				shoe.AddCard(i);
			}
		}

		// <=16 -> e
		for (int i=e-16; i<=min(e-d.Total(), 11); i++)
		{
			// We know the dealer doesn't have a natural
			if (d.Total() == 10 && i==11) continue;
			if (d.Total() == 11 && i==10) continue;

			int c = i;
			if (c == 11) c = 1;

			double tmp = shoe.CardProb(c);

			if (tmp>0.000001)
			{
				shoe.DealCard(c);
				double p = DealerProb17(d, e - i, shoe);
				total+=tmp * p;

				//cout << d.Total() << " " << c << " " <<  p << " " << tmp << endl;

				shoe.AddCard(c);
			}
		}
	}

	return total;
}


// The probability that the dealer will get from d to hard e
double PDFDealer::DealerHard17(Hand d, int e, PDFDealer::DealerShoe& shoe)
{
	//return 0;
	//cout << "DealerHard17 " << d.Total() << " " << e << endl;

	if (d.Soft()) return DealerSoft2Hard17(d.Total(), e, shoe);


	if (d == e) return 1.0;
	//if (d.Total() > e) return 0.0;


	double total = 0.0;

	double tmp = shoe.CardProb(1);

	if (tmp > 0.000001)
	{
		shoe.DealCard(1);
		if (d.Total()+11 <= 17) total += tmp * DealerSoft2Hard17(d.Total()+11, e, shoe);
		else if (d.Total()+11 >= 22)
		{
			total += tmp * CombProb(e-d.Total()-1, shoe);

			//cout << (d+1).Total() << endl;
			//if (d.Total()+1 < 11) total += tmp * DealerHard17(d+1, e, shoe);
			//else total += tmp * CombProb(e-d.Total()-1, shoe);
		}
		shoe.AddCard(1);
	}

	for (int i=2; i<=min(e-d.Total(), 10); i++)
	{
		double tmp = shoe.CardProb(i);

		//cout << i << " " << tmp << endl;

		if (tmp > 0.000001)
		{
			shoe.DealCard(i);

			if (d.Total()+i < 11) total += tmp * DealerHard17(d+i, e, shoe);
			else total += tmp * CombProb(e-d.Total()-i, shoe);

			shoe.AddCard(i);
		}
	}

	return total;
}

// The probability that the dealer will get from d to soft e
double PDFDealer::DealerSoft17(Hand d, int e, PDFDealer::DealerShoe& shoe)
{
	//return 0;
	//cout << "DealerSoft17 " << d.Total() << " " << e << endl;

	if (d.Total() > e) return 0;

	if (!d.Soft())
	{
		double delta = e - d.Total();

		// no way to get soft e
		if (delta < 11) return 0;

		// we need an ace to be soft
		double tmp = shoe.CardProb(1);
		double total = 0;

		if (tmp > 0.000001)
		{
			shoe.DealCard(1);

			// Must count all permutations with an ace in it and with sum delta
			
			if (delta-11 == 0)		// 2 -> 13s, 3 -> 14s, 4 -> 15s, 5 -> 16s, 6 -> 17s
			{
				total = tmp;
			}
			else if (delta-11 == 1) // 2 -> 14s, 3 -> 15s, 4 -> 16s, 5 -> 17s
			{
				//       A + 1
				//   1 + A

				total = tmp * shoe.CardProb(1);
			}
			else if (delta-11 == 2) // 2 -> 15s, 3 -> 16s, 4 -> 17s
			{
				//       A + (2)
				//   1 + A + 1
				// (2) + A

				total = 
					2 * tmp * shoe.CardProb(2) + 
						tmp * PermProb(1, 1, shoe);
			}
			else if (delta-11 == 3) // 2 -> 16s, 3 -> 17s
			{
				//       A + (3)
				//   1 + A + (2)
				// (2) + A + 1
				// (3) + A

				total = 
						tmp * PermProb(1, 1, 1, shoe) + 
					3 * tmp * PermProb(2, 1, shoe) + 
					2 * tmp * shoe.CardProb(3);

			}
			else if (delta-11 == 4) // 2 -> 17s
			{
				//       A + (4)
				//   1 + A + (3)
				// (2) + A + (2)
				// (3) + A + 1
				// (4) + A

				total = 
						tmp * PermProb(1, 1, 1, 1, shoe) + 
					4 * tmp * PermProb(2, 1, 1, shoe) + 
					3 * tmp * PermProb(2, 2, shoe) + 
					3 * tmp * PermProb(3, 1, shoe) + 
					2 * tmp * shoe.CardProb(4);
			}


			shoe.AddCard(1);
		}

		return total;
	}
	else
	{
		if (d.Total() == e) return 1.0;

		return CombProb(e - d.Total(), shoe);
	}
}

// The probability that the dealer will get from soft d to hard e
double PDFDealer::DealerSoft2Hard17(int d, int e, PDFDealer::DealerShoe& shoe)
{
	//cout << "DealerSoft2Hard17 " << d << " " << e << endl;

	if (e<12) return 0;

	int targetSum = (e+10) - d;

	double total = 0.0;

	// grind up to 17
	for (int i = 1; i <= 17 - d; i++)
	{
		double tmp = shoe.CardProb(i);

		if (tmp>0.000001)
		{
			shoe.DealCard(i);
			total += tmp * DealerSoft2Hard17(d+i, e, shoe);
			shoe.AddCard(i);
		}
	}

	// jump at least to 22
	for (int i = min(10, targetSum); i >= 22 - d; i--)
	{
		int j = targetSum - i;

		double p = 0.0;
		double tmp = shoe.CardProb(i);

		if (tmp>0.000001)
		{
			if (j==0) p = tmp;
			else
			{
				shoe.DealCard(i);
				p = tmp * CombProb(j, shoe);
				shoe.AddCard(i); 
			}

			total += p;
		}

		//cout << i << " (" << j << ") " << p << endl;
	}

	return total;
}


// The probability that the next cards in the shoe will sum up to a (smart implementation)
double PDFDealer::CombProbSmart(int a, DealerShoe& shoe, int count, int last_card, int last_count, int permutations)
{
	if (a==0) return permutations;

	double total = 0.0;

	for (int i=last_card; i<=a; i++)
	{
		double tmp = shoe.CardProb(i);

		if (tmp > 0.000001)
		{
			int c = count+1;
			int lc, p = permutations;

			if (i==last_card) lc = last_count + 1;
			else lc = 1;

			p *= c;
			p /= lc;


			shoe.DealCard(i);
			total += tmp * CombProbSmart(a-i, shoe, c, i, lc, p);
			shoe.AddCard(i);
		}
	}

	return total;
}


// The probability that the next cards in the shoe will sum up to a
double PDFDealer::CombProbSimple(int a, PDFDealer::DealerShoe& shoe)
{
	if (a==0) return 1.0;

	double total = 0.0;

	for (int i=1; i<=a; i++)
	{
		double tmp = shoe.CardProb(i);

		if (tmp > 0.000001)
		{
			shoe.DealCard(i);
			total += tmp * CombProb(a-i, shoe);
			shoe.AddCard(i);
		}
	}

	return total;
}

double PDFDealer::CombProbEnumerated(int a, PDFDealer::DealerShoe& shoe)
{
	if (a == 0) return 1.0;

	if (a == 1)
	{
		// Permutations for a = 1
		// (1) 1

		return shoe.CardProb(1);
	}
	else if (a == 2)
	{
		// Permutations for a = 2
		// (1) 1 1
		// (1) 2

		return 
			PermProb(1, 1, shoe) + 
			shoe.CardProb(2);
	}
	else if (a == 3)
	{
		// Permutations for a = 3
		// (1) 1 1 1
		// (2) 1 2
		// (1) 3

		return 
				PermProb(1, 1, 1, shoe) + 
			2 * PermProb(2, 1, shoe) + 
				shoe.CardProb(3);
	}
	else if (a == 4)
	{
		// Permutations for a = 4
		// (1) 1 1 1 1
		// (3) 1 1 2
		// (2) 1 3
		// (1) 2 2
		// (1) 4

		return 
				PermProb(1, 1, 1, 1, shoe) + 
			3 * PermProb(2, 1, 1, shoe) + 
			2 * PermProb(3, 1, shoe) + 
				PermProb(2, 2, shoe) + 
				shoe.CardProb(4);
	}
	else if (a == 5)
	{
		// Permutations for a = 5
		// (1) 1 1 1 1 1
		// (4) 1 1 1 2
		// (3) 1 1 3
		// (3) 1 2 2
		// (2) 1 4
		// (2) 2 3
		// (1) 5

		return 
				PermProb(1, 1, 1, 1, 1, shoe) + 
			4 * PermProb(2, 1, 1, 1, shoe) + 
			3 * PermProb(3, 1, 1, shoe) + 
			3 * PermProb(2, 2, 1, shoe) + 
			2 * PermProb(4, 1, shoe) + 
			2 * PermProb(3, 2, shoe) + 
				shoe.CardProb(5);
	}
	else if (a == 6)
	{
		// Permutations for a = 6
		// (1) 1 1 1 1 1 1
		// (5) 1 1 1 1 2
		// (4) 1 1 1 3
		// (6) 1 1 2 2
		// (3) 1 1 4
		// (6) 1 2 3
		// (2) 1 5
		// (1) 2 2 2
		// (2) 2 4
		// (1) 3 3
		// (1) 6

		return 
				PermProb(1, 1, 1, 1, 1, 1, shoe) + 
			5 * PermProb(1, 1, 1, 1, 2, shoe) + 
			4 * PermProb(1, 1, 1, 3, shoe) + 
			6 * PermProb(1, 1, 2, 2, shoe) + 
			3 * PermProb(1, 1, 4, shoe) + 
			6 * PermProb(1, 2, 3, shoe) + 
			2 * PermProb(1, 5, shoe) + 
			1 * PermProb(2, 2, 2, shoe) + 
			2 * PermProb(2, 4, shoe) + 
				PermProb(3, 3, shoe) +
				shoe.CardProb(6);
	}

	return 0.0;
}

// The probability that the next cards in the shoe will sum up to a and b
/*double PDFDealer::CombProb(int a, int b, PDFDealer::DealerShoe& shoe)
{
	if (a==0) return CombProb(b, shoe);
	if (b==0) return CombProb(a, shoe);

	double total = 0.0;

	for (int i=1; i<=a; i++)
	{
		double tmp = shoe.CardProb(i);

		if (tmp > 0.0000001)
		{
			shoe.DealCard(i);
			total += tmp * CombProb(a-i, b, shoe);
			shoe.AddCard(i);
		}
	}

	return total;
}*/

// The probability that the next cards in the shoe will be a and b (in the exact order)
double PDFDealer::PermProb(int a, int b, PDFDealer::DealerShoe& shoe)
{
	double total = shoe.CardProb(a);
	shoe.DealCard(a);
	total*=shoe.CardProb(b);

	shoe.AddCard(a);

	return total;
}

// The probability that the next cards in the shoe will be a, b and c (in the exact order)
double PDFDealer::PermProb(int a, int b, int c, PDFDealer::DealerShoe& shoe)
{
	double total = shoe.CardProb(a);
	shoe.DealCard(a);
	total*=shoe.CardProb(b);
	shoe.DealCard(b);
	total*=shoe.CardProb(c);

	shoe.AddCard(b);
	shoe.AddCard(a);

	return total;
}

// The probability that the next cards in the shoe will be a, b, c and d (in the exact order)
double PDFDealer::PermProb(int a, int b, int c, int d, PDFDealer::DealerShoe& shoe)
{
	double total = shoe.CardProb(a);
	shoe.DealCard(a);
	total*=shoe.CardProb(b);
	shoe.DealCard(b);
	total*=shoe.CardProb(c);
	shoe.DealCard(c);
	total*=shoe.CardProb(d);

	shoe.AddCard(c);
	shoe.AddCard(b);
	shoe.AddCard(a);

	return total;
}

// The probability that the next cards in the shoe will be a, b, c, d and e (in the exact order)
double PDFDealer::PermProb(int a, int b, int c, int d, int e, PDFDealer::DealerShoe& shoe)
{
	double total = shoe.CardProb(a);
	shoe.DealCard(a);
	total*=shoe.CardProb(b);
	shoe.DealCard(b);
	total*=shoe.CardProb(c);
	shoe.DealCard(c);
	total*=shoe.CardProb(d);
	shoe.DealCard(d);
	total*=shoe.CardProb(e);

	shoe.AddCard(d);
	shoe.AddCard(c);
	shoe.AddCard(b);
	shoe.AddCard(a);

	return total;
}

// The probability that the next cards in the shoe will be a, b, c, d, e and f (in the exact order)
double PDFDealer::PermProb(int a, int b, int c, int d, int e, int f, PDFDealer::DealerShoe& shoe)
{
	double total = shoe.CardProb(a);
	shoe.DealCard(a);
	total*=shoe.CardProb(b);
	shoe.DealCard(b);
	total*=shoe.CardProb(c);
	shoe.DealCard(c);
	total*=shoe.CardProb(d);
	shoe.DealCard(d);
	total*=shoe.CardProb(e);
	shoe.DealCard(e);
	total*=shoe.CardProb(f);

	shoe.AddCard(e);
	shoe.AddCard(d);
	shoe.AddCard(c);
	shoe.AddCard(b);
	shoe.AddCard(a);

	return total;
}
// Used to pre-calculate the DealerSoft17 combinations
void PDFDealer::PrintSoftPermutations(int sum, bool ace, int* cards, int count)
{
	if (cards == NULL)
	{
		cards = new int[10];
		PrintSoftPermutations(sum, false, cards, 0);
		delete cards;
	}
	else
	{
		if (sum == 0)
		{
			if (!ace) return;

			for (int i=0; i<count; i++)
			{
				std::cout << cards[i] << " ";
			}
			std::cout << std::endl;
		}
		else
		{
			for (int i=1; i<=sum; i++)
			{
				bool a = ace;
				if (i==1) a = true;

				cards[count] = i;

				PrintSoftPermutations(sum-i, a, cards, count+1);
			}
		}
	}
}

void PDFDealer::PrintProbAPermutations(int a, int* cards, int count)
{
	if (cards == NULL)
	{
		cards = new int[10];
		PrintProbAPermutations(a, cards, 0);
		delete cards;
	}
	else
	{
		if (a==0)
		{
			for (int i=0; i<count; i++)
			{
				std::cout << cards[i] << " ";
			}
			std::cout << std::endl;
		}

		for (int i=1; i<=a; i++)
		{
			cards[count] = i;

			PrintProbAPermutations(a-i, cards, count+1);
		}
	}
}

void PDFDealer::PrintProbACombinations(int a, int* cards, int count, int last_card, int last_count, int permutations)
{
	if (cards == NULL)
	{
		cards = new int[10];
		PrintProbACombinations(a, cards, 0, 1, 0, 1);
		delete cards;
	}
	else
	{
		if (a==0)
		{
			std::cout << "(" << permutations << ") "; 
			
			for (int i=0; i<count; i++)
			{
				std::cout << cards[i] << " ";
			}
			std::cout << std::endl;
		}

		for (int i=last_card; i<=a; i++)
		{
			cards[count] = i;

			int c = count+1;
			int lc, p = permutations;

			if (i==last_card) lc = last_count + 1;
			else lc = 1;

			p *= c;
			p /= lc;

			PrintProbACombinations(a-i, cards, c, i, lc, p);
		}
	}
}