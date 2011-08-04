using System;

namespace GR.Gambling.Backgammon
{
    public enum BackgammonVariation
    {
        Standard,
        Hypergammon
    }

	public struct Cube
	{
		public int Value;
		public int Owner;

		public bool Centered { get { return Value == 1; } }

		public Cube(int value, int owner)
		{
			Value = value;
			Owner = owner;
		}
	}

	public enum GameType
	{
        /// <summary>
        /// Money game has a initial stake with a maximum monetary limit and is essentially a one-point match.
        /// </summary>
		Money,
        /// <summary>
        /// Match has a fixed monetary stake with a maximum points we play for.
        /// </summary>
		Match,
		// Add tourney?
        None
	} 

	public enum OfferType
	{
		None = 0,
		Double = 1,
		Resign = 2
	}

	public enum ResignValue
	{
		None = 0,
		Single = 1,
		Gammon = 2,
		Backgammon = 3
	};

	public enum DoubleResponse
	{
		None = 0,
		Pass = 1,
		Take = 2,
		Beaver = 4
	};

    public enum ResignResponse
    {
        None = 0,
        Accept = 1,
        Reject = 2
    }

    public enum DoubleAction
    {
        None = 0,
        NoDouble = 1,
        Double = 2,
        // Take = 3,
        // Beaver = 4,
        // Pass = 5,
        // TooGoodToDouble = 6
    };

	//?
	public enum PlayerDirection
	{
		Clockwise,
		CounterClockwise
	};
}