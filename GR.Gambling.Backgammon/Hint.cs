using System;
using System.Collections.Generic;
using System.Linq;

using GR.Gambling.Backgammon;

namespace GR.Gambling.Backgammon.Tools
{
	public class DoubleHint
	{
		private DoubleAction action;
		private double doubleTakeEq;
		private double doublePassEq;
		private double noDoubleEq;

		public DoubleAction Action { get { return action; } set { action = value; } }

		public double DoubleTakeEq { get { return doubleTakeEq; } set { doubleTakeEq = value; } }
		public double DoublePassEq { get { return doublePassEq; } set { doublePassEq = value; } }
		public double NoDoubleEq { get { return noDoubleEq; } set { noDoubleEq = value; } }

		public override string ToString()
		{
			return Action.ToString();
		}

		public DoubleHint(DoubleAction action, double takeEq, double passEq, double noDoubleEq)
		{
			this.action = action;
			this.doubleTakeEq = takeEq;
			this.doublePassEq = passEq;
			this.noDoubleEq = noDoubleEq;
		}
	}

    public class DoubleResponseHint
    {
		private DoubleResponse response;
		private double takeEq;
		private double passEq;

		public double TakeEq { get { return takeEq; } }
		public double PassEq { get { return passEq; } }
		public DoubleResponse Response { get { return response; } }

		public DoubleResponseHint(DoubleResponse response, double takeEq, double passEq)
		{
			this.response = response;
			this.takeEq = takeEq;
			this.passEq = passEq;
		}

		public override string ToString()
		{
			return response.ToString();
		}
    }

    public class ResignHint
    {
		private ResignValue value;

		public ResignValue Value { get { return value; } }

        public override string ToString()
        {
            return Value.ToString();
        }

		public ResignHint(ResignValue value)
		{
			this.value = value;
		}
    }

    public class ResignResponseHint
    {
		private ResignResponse response;

		public ResignResponse Response { get { return response; } }

		public ResignResponseHint(ResignResponse response)
		{
			this.response = response;
		}
    }

	public class PlayHint
	{
        private Play play;
        private double equity;

		public PlayHint()
		{
			//moves = new List<Move>();
            play = new Play();
            equity = double.NaN;
		}

		public PlayHint(IEnumerable<Move> moves)
		{
			//this.moves = moves;
            play = new Play(moves);
            equity = double.NaN;
		}

        public PlayHint(IEnumerable<Move> moves, double equity)
        {
            play = new Play(moves);
            this.equity = equity;
        }

        public double Equity { get { return equity; } }

		//public List<Move> Moves { get { return moves; } }
        public Play Play { get { return play; } }

		public override string ToString()
		{
			string hint = "";
			foreach (Move move in play)
				hint += move + " ";
            if (!double.IsNaN(equity))
                hint += "[" + equity.ToString() + "]";

			return hint;
		}
	}
}