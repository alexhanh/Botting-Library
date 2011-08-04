using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
	public class GameStateDoubleAction : GameStateAction
	{
		public GameStateDoubleAction(GameState gs, long time, DoubleResponse response)
			: base(gs, time)
		{
			this.response = response;
		}

		private DoubleResponse response;

		public DoubleResponse Response { get { return response; } }

		public static string Serialize(GameStateDoubleAction gsda)
		{
			return GameState.Serialize(gsda.GameState) + "|" + gsda.time + " " + gsda.response.ToString();
		}

		public static GameStateAction Deserialize(string s)
		{
			string[] ss = s.Split(new char[] { '|' });

			GameState gs = GameState.Deserialize(ss[0]);

			ss = ss[1].Split(' ');

			int time = int.Parse(ss[0]);
			DoubleResponse response = (DoubleResponse)Enum.Parse(typeof(DoubleResponse), ss[1]);

			GameStateDoubleAction gsda = new GameStateDoubleAction(gs, time, response);

			return gsda;
		}
	}
}
