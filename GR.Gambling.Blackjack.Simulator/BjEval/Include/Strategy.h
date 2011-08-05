#ifndef __STRATEGY_H__
#define __STRATEGY_H__

#include <math.h>
#include <iostream>
#include <sstream>

#include "Hand.h"
#include "Shoe.h"
#include "Action.h"

class Strategy
{
public:
	Shoe GetShoe() { return shoe; }

	Strategy() : shoe(0) {}

	Strategy(const Shoe& shoe) : shoe(shoe)
	{
	}

	bool Read(const char* filename);
	void Write(const char* filename);

	void Read(std::istream& stream);
	void Write(std::ostream& stream);

	void SetActions(int dealer, int player1, int player2, ActionList& actions);
	ActionList GetActions(int dealer, int player1, int player2);

	void AddAction(int dealer, int player1, int player2, Action& action);

	double DealProb(int dealer, int player1, int player2);
	double DealerBJProb(int dealer, int player1, int player2);

	double TotalEV(int betSize);

private:

	bool ReadLine(std::istream& stream, int line);
	void WriteLine(std::ostream& stream, int dealer, int player1, int player2);

	template <class T> bool Parse(T& t, const std::string& s)//, std::ios_base& (*f)(std::ios_base&))
	{
	  std::istringstream iss(s);
	  return !(iss >> std::dec >> t).fail();
	}

	void ParseError(int line, const char* message);

	void PrintMissingDeals();

	Shoe shoe;

	// dealer upcard, player1, player2
	ActionList strategy[10][10][10];
};

class StrategyAnalyzer
{
public:

	static void Compare(Strategy& s1, Strategy& s2, int betSize);

private:

	struct DealInfo
	{
		DealInfo() {}
		DealInfo(int dealer, int player1, int player2) : dealer(dealer), player1(player1), player2(player2) {}

		int dealer, player1, player2;
		Action action1, action2;

		double error;

		bool operator < (const DealInfo& a) { return (abs(this->error) > abs(a.error)); }
	};
};


#endif