#include "Strategy.h"
#include "Rakeback.h"

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <algorithm>
#include <iomanip>
using namespace std;

void Strategy::SetActions(int dealer, int player1, int player2, ActionList& actions)
{
	if (player1<player2) {
		int tmp = player1;
		player1 = player2;
		player2 = tmp;
	}

	strategy[dealer-1][player1-1][player2-1] = actions;
}

ActionList Strategy::GetActions(int dealer, int player1, int player2)
{
	if (player1<player2) {
		int tmp = player1;
		player1 = player2;
		player2 = tmp;
	}

	return strategy[dealer-1][player1-1][player2-1];
}

void Strategy::AddAction(int dealer, int player1, int player2, Action& action)
{
	if (player1<player2) {
		int tmp = player1;
		player1 = player2;
		player2 = tmp;
	}

	strategy[dealer-1][player1-1][player2-1].Add(action);
}

double Strategy::DealProb(int dealer, int player1, int player2)
{
	double total = shoe.CardProb(dealer);
	shoe.DealCard(dealer);
	total *= shoe.CardProb(player1);
	shoe.DealCard(player1);
	total *= shoe.CardProb(player2);

	shoe.AddCard(player1);
	shoe.AddCard(dealer);

	if (player1 != player2) total*=2;

	return total;
}

double Strategy::DealerBJProb(int dealer, int player1, int player2)
{
	if (dealer!=1 && dealer!=10) return 0;

	shoe.DealCard(dealer);
	shoe.DealCard(player1);
	shoe.DealCard(player2);

	double total;

	if (dealer == 1) total = shoe.CardProb(10);
	else total = shoe.CardProb(1);

	shoe.AddCard(player2);
	shoe.AddCard(player1);
	shoe.AddCard(dealer);

	return total;
}

double Strategy::TotalEV(int betSize)
{
	double total = Rakeback::Amount(betSize, betSize);

	for (int dealer = 1; dealer <= 10; dealer++)
	{
		for (int player1 = 1; player1 <= 10; player1++)
		{
			for (int player2 = 1; player2 <= player1; player2++)
			{
				double dealProb = DealProb(dealer, player1, player2);

				double ev = 0;

				ActionList actions = GetActions(dealer, player1, player2);

				if (actions.GetCount() == 0)
				{
					cout << "Missing actions: " << dealer << " | " << player1 << "," << player2 << endl;
				}
				else
				{
					double dealerBJ = DealerBJProb(dealer, player1, player2);

					if ((player1==10 && player2==1) || (player1==1 && player2==10))
					{
						ev = (1-dealerBJ) * 1.5;
					}
					else
					{
						double insuranceEV = 0.0;

						if (dealer == 1)
						{
							insuranceEV = 1.0*dealerBJ - 0.5*(1.0-dealerBJ) + Rakeback::Amount(betSize/2, betSize);

							if (insuranceEV < 0) insuranceEV = 0;
						}

						ev = dealerBJ * (-1) + (1-dealerBJ) * actions[0].ev + insuranceEV;
					}
				}

				total += dealProb * ev;
			}
		}
	}

	return total;
}

bool Strategy::Read(const char* filename)
{
	ifstream file(filename);
	if (!file.good()) 
	{
		cout << "File not found: " << filename << endl;
		file.close();
		return false;
	}
	Read(file);
	file.close();
	return true;
}

void Strategy::Write(const char* filename)
{
	ofstream file(filename);
	Write(file);
	file.close();
}

void Strategy::Read(std::istream& stream)
{
	int lineNumber = 1;

	bool shoeRead = false;

	while (!stream.eof())
	{
		string line;
		getline(stream, line);

		if (line.length() > 0)
		{
			std::istringstream lineStream(line);

			if (!shoeRead)
			{
				int counts[10];

				for (int i=0; i<10; i++)
				{
					lineStream >> counts[i];
					if (lineStream.fail()) { ParseError(lineNumber, "Shoe counts"); }
				}

				this->shoe = Shoe(counts);

				shoeRead = true;
			}
			else
			{
				if (!ReadLine(lineStream, lineNumber)) return;
			}
		}

		lineNumber++;
	}

	PrintMissingDeals();
}

bool Strategy::ReadLine(std::istream& stream, int line)
{
	//cout << "Reading line " << line << endl;

	int dealer, player1, player2;
	string player;

	stream >> dealer;
	if (stream.fail()) { ParseError(line, "Dealer card"); return false; }

	stream >> player;

	int comma_index = player.find_first_of(',');
	if (comma_index == string::npos) { ParseError(line, "Player cards (comma missing)"); return false; }

	if (!Parse<int>(player1, player.substr(0, comma_index))) { 
		ParseError(line, "Player card 1"); return false; 
	}
	if (!Parse<int>(player2, player.substr(comma_index+1, player.length()-(comma_index+1)))) { 
		ParseError(line, "Player card 2"); return false;
	}

	ActionList actions;

	double stand_ev, hit_ev, double_ev, surrender_ev;

	stream >> stand_ev;
	if (stream.fail()) { ParseError(line, "Stand EV"); return false; }

	stream >> hit_ev;
	if (stream.fail()) { ParseError(line, "Hit EV"); return false; }

	stream >> double_ev;
	if (stream.fail()) { ParseError(line, "Double EV"); return false; }

	stream >> surrender_ev;
	if (stream.fail()) { ParseError(line, "Surrender EV"); return false; }

	actions.Add(Action(Stand, stand_ev));
	actions.Add(Action(Hit, hit_ev));
	actions.Add(Action(Double, double_ev));
	actions.Add(Action(Surrender, surrender_ev));

	if (player1 == player2)
	{
		double split_ev;

		stream >> split_ev;
		if (stream.fail()) { ParseError(line, "Split EV"); return false; }

		actions.Add(Action(Split, split_ev));
	}

	SetActions(dealer, player1, player2, actions);
/*
	cout << "Dealer: " << dealer << endl;

	cout << "Player: " << player1 << " " << player2 << endl;

	cout << "Stand: " << stand_ev << endl;
	cout << "Hit: " << hit_ev << endl;
	cout << "Double: " << double_ev << endl;
*/

	return true;
}

void Strategy::ParseError(int line, const char* message)
{
	cout << "Parse error on line " << line << ": " << message << endl;
}

void Strategy::PrintMissingDeals()
{
	for (int dealer = 1; dealer <= 10; dealer++)
	{
		for (int player1 = 1; player1 <= 10; player1++)
		{
			for (int player2 = 1; player2 <= player1; player2++)
			{
				if (GetActions(dealer, player1, player2).GetCount() == 0)
				{
					cout << "Missing: " << dealer << " | " << player1 << "," << player2 << endl;
				}
			}
		}
	}
}

void Strategy::Write(std::ostream& stream)
{
	for (int i=1; i<=10; i++)
	{
		stream << shoe[i] << " ";
	}
	stream << endl;


	for (int dealer = 1; dealer <= 10; dealer++)
	{
		for (int player1 = 1; player1 <= 10; player1++)
		{
			for (int player2 = 1; player2 <= player1; player2++)
			{
				if (player1==player2) continue;

				WriteLine(stream, dealer, player1, player2);
				stream << endl;
			}
		}
	}

	for (int dealer = 1; dealer <= 10; dealer++)
	{
		for (int player = 1; player <= 10; player++)
		{
			WriteLine(stream, dealer, player, player);
			stream << endl;
		}
	}
}

void Strategy::WriteLine(std::ostream& stream, int dealer, int player1, int player2)
{
	stream << dealer << " " << player1 << "," << player2;

	ActionList actions = GetActions(dealer, player1, player2);
	Action action;

	if (actions.GetAction(Stand, action)) stream << " " << action.ev;
	else stream << " -1.0";

	if (actions.GetAction(Hit, action)) stream << " " << action.ev;
	else stream << " -1.0";

	if (actions.GetAction(Double, action)) stream << " " << action.ev;
	else stream << " -1.0";

	if (actions.GetAction(Surrender, action)) stream << " " << action.ev;
	else stream << " -1.0";

	if (player1==player2)
	{
		if (actions.GetAction(Split, action)) stream << " " << action.ev;
		else stream << " -1.0";
	}
}

void StrategyAnalyzer::Compare(Strategy &s1, Strategy &s2, int betSize)
{
	vector<DealInfo> deals;

	for (int dealer = 1; dealer <= 10; dealer++)
	{
		for (int player1 = 1; player1 <= 10; player1++)
		{
			for (int player2 = 1; player2 <= player1; player2++)
			{
				if ((player1==10 && player2==1) || (player1==1 && player2==10)) continue;

				DealInfo d(dealer, player1, player2);

				ActionList list1 = s1.GetActions(dealer, player1, player2);
				ActionList list2 = s2.GetActions(dealer, player1, player2);

				if (list1.GetCount() == 0)
				{
					cout << "Strategy 1 missing an action for " << dealer << " | " << player1 << "," << player2 << endl;
				}
				else if (list2.GetCount() == 0) 
				{
					cout << "Strategy 2 missing an action for " << dealer << " | " << player1 << "," << player2 << endl;
				}
				else
				{
					d.action1 = list1[0];
					d.action2 = list2[0];

					d.error = d.action2.ev - d.action1.ev;

					deals.push_back(d);
				}
			}
		}
	}

	sort(deals.begin(), deals.end());

	cout << setiosflags(ios::left);

	for (int i=0; i<min(20, (int)deals.size()); i++)
	{
		if (abs(deals[i].error) < 0.0001) break;

		cout << setw(2) << deals[i].dealer << " " << setw(2) << deals[i].player1 << "," << setw(2) << deals[i].player2;

		cout << " Error: " << setw(11) << deals[i].error;
		cout << "  S1: " << setw(9) << deals[i].action1.GetTypeString() << " " << setw(11) << deals[i].action1.ev;
		cout << endl;
		cout << setw(27) << " ";
		cout << "  S2: " << setw(9) << deals[i].action2.GetTypeString() << " " << setw(11) << deals[i].action2.ev;
		cout << endl << endl;
	}

	cout << setiosflags(ios::right);

	double ev1 = s1.TotalEV(betSize);
	double ev2 = s2.TotalEV(betSize);

	cout << "Strategy 1 total EV: " << ev1 << endl;
	cout << "Strategy 2 total EV: " << ev2 << endl;
	cout << "Total error:         " << ev2-ev1 << endl;
}