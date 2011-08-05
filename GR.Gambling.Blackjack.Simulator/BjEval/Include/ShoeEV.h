#ifndef __SHOE_EV_H__
#define __SHOE_EV_H__

#include "Common.h"
#include "Shoe.h"
#include "Hand.h"
#include "Strategy.h"

#include <fstream>

class ShoeEV
{
public:
//	static double Evaluate(Shoe shoe);

//	static double GameEV(Shoe& shoe, int player1, int player2, int upcard, std::fstream& file);

	static Strategy GetStrategy(Shoe shoe, int betSize);

	static ActionList GetActions(int dealer, int player1, int player2, Shoe& shoe, int betSize); 

	static double GetEV(int dealer, int player1, int player2, Shoe& shoe, int betSize);

private:
};


#endif