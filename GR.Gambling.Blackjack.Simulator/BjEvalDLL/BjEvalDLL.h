// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the BJEVALDLL_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// BJEVALDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef BJEVALDLL_EXPORTS
#define BJEVALDLL_API __declspec(dllexport)
#else
#define BJEVALDLL_API __declspec(dllimport)
#endif

extern "C" {

	struct BJEVALDLL_API SHand
	{
		int Total;
		bool Soft;
	};
	BJEVALDLL_API void CacheDealerProbs(int upcard, const int shoe[]);
	BJEVALDLL_API double ShoeEv(const int counts[], int betSize);

	BJEVALDLL_API double DealEv(int player1, int player2, int upcard, const int shoe[], int betSize);

	BJEVALDLL_API double StandEv(SHand hand, int upcard, const int shoe[]);
	BJEVALDLL_API double HitEv(SHand player, int upcard, const int shoe[]);
	BJEVALDLL_API double DoubleEv(SHand player, int upcard, int bet_size, const int shoe[]);
	BJEVALDLL_API double InsuranceEv(int bet_size, const int shoe[]);
	BJEVALDLL_API double SurrenderEv();
	BJEVALDLL_API double SplitEv(int split_card, int upcard, int bet_size, int max_splits, const int shoe[]);

	BJEVALDLL_API double Version();	
}