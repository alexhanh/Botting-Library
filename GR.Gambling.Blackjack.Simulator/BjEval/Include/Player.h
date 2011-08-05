#ifndef __PLAYER_H__
#define __PLAYER_H__

class Hand;
class Shoe;

class Player
{
private:
	//static double SplitAces(int upcard, Shoe shoe, bool rsa, int betSize);
	static double SplitAces(int upcard, Shoe& shoe, int betSize);
	//static double SplitNonAces(Hand hand, int upcard, Shoe shoe, bool das, bool rsp);
	static double NoSplit(Hand hand, int upcard, Shoe& shoe, bool das, int betSize);


public:
	static double Hit(Hand player, int upcard, Shoe& shoe, int rec = 4);
	static double Double(Hand player, int upcard, Shoe& shoe, int betSize);
	//static double Split(int split_card, int upcard, Shoe shoe);
	static double Insurance(int bet_size, Shoe& shoe);
	static double Surrender();
	static double Stand(Hand player, Hand upcard, Shoe& shoe);

	static double SplitNonAces(Hand hand, int upcard, Shoe& shoe, bool das, int maxReSplits, int betSize, const double noSplitEV[]);
	static double Split(int split_card, int upcard, Shoe& shoe, int maxSplits, int betSize);
};

#endif