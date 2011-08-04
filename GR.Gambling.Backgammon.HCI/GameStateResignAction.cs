using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
	public class GameStateResignAction : GameStateAction
	{
		public GameStateResignAction(GameState gs, long time, ResignResponse response)
			: base(gs, time)
		{
			this.response = response;

		}

		private ResignResponse response;

		public ResignResponse Response { get { return response; } }

		public static string Serialize(GameStateResignAction gsra)
		{
			return GameState.Serialize(gsra.GameState) + "|" + gsra.time + " " + gsra.response.ToString();
		}

		public static GameStateResignAction Deserialize(string s)
		{
			string[] ss = s.Split(new char[] { '|' });

			GameState gs = GameState.Deserialize(ss[0]);

			ss = ss[1].Split(' ');

			int time = int.Parse(ss[0]);
			ResignResponse response = (ResignResponse)Enum.Parse(typeof(ResignResponse), ss[1]);

			GameStateResignAction gsra = new GameStateResignAction(gs, time, response);

			return gsra;
		}
	}
}
