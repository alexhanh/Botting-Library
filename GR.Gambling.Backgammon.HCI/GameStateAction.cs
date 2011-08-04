using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
	public enum GameStateActionType
	{
		ResignResponse,
		DoubleResponse,
		TurnChanged,
		Move
	}

	public enum TurnAction
	{
		Resign,
		Double,
		Roll,
		None
	}

	public abstract class GameStateAction
	{
		protected long time;
		protected GameState gs;

		public GameStateAction(GameState gs, long time)
		{
			this.gs = gs;
			this.time = time;
		}



		public long Time { get { return time; } }
		public GameState GameState { get { return gs; } }
	}
}
