using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
	public class GameStateTurnAction : GameStateAction
	{
		public GameStateTurnAction(GameState gs, long time, TurnAction action)
			: base(gs, time)
		{
			this.turnAction = action;
		}

		private TurnAction turnAction;

		public TurnAction Action { get { return turnAction; } }

		public static string Serialize(GameStateTurnAction gsta)
		{
			return GameState.Serialize(gsta.GameState) + "|" + gsta.time + " " + gsta.turnAction.ToString();
		}

		public static GameStateTurnAction Deserialize(string s)
		{
			string[] ss = s.Split(new char[] { '|' });

			GameState gs = GameState.Deserialize(ss[0]);

			ss = ss[1].Split(' ');

			int time = int.Parse(ss[0]);
			TurnAction action = (TurnAction)Enum.Parse(typeof(TurnAction), ss[1]);

			GameStateTurnAction gsta = new GameStateTurnAction(gs, time, action);

			return gsta;
		}
	}
}
