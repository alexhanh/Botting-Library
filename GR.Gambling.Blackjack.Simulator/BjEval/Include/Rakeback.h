#ifndef __RAKEBACK_H__
#define __RAKEBACK_H__

class Rakeback
{
public:
	// wager:	wager amount in cents
	// betSize: size of normal bet in cents
	// return:	amount of rakeback scaled to betSize
	static double Amount(int wager, int bet_size)
	{
		double party_points = 0 * (wager/2500.0);

		//double revenue_share = 0.0075 * wager;

		double rb_in_cents = party_points;// + revenue_share;

		return rb_in_cents/(double)bet_size;
	}
};

#endif