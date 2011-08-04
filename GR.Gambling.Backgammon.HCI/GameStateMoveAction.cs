using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
	public class GameStateMoveAction : GameStateAction
	{
		public GameStateMoveAction(GameState gs, GameState original, long time, TimedMove move, List<TimedMove> madeMoves)
			: base(gs, time)
		{
			this.original = original;
			this.move = move;
			this.madeMoves = madeMoves;
		}

		private GameState original;
		private List<TimedMove> madeMoves = new List<TimedMove>();
		private TimedMove move;

		public TimedMove Move { get { return move; } }
		public List<TimedMove> MadeMoves { get { return madeMoves; } } // [0] is the move made first, [count-1] is the move made last (just before)
		public GameState Original { get { return original; } }

		public bool IsDone { get { return move == null; } }
		public bool IsUndo { get { return Move.IsUndo; } }
		public TimedMove LastMove { get { return MadeMoves[0]; } }

		public int GetTotalMadeMoves()
		{
			int count = 0;
			foreach (TimedMove m in madeMoves)
			{
				if (m.IsUndo)
					count--;
				else
					count++;
			}

			return count;
		}

		public static string Serialize(GameStateMoveAction gsma)
		{
			StringBuilder sb = new StringBuilder();

			string s = (gsma.Move == null) ? "X" : gsma.Move.ToString();

			sb.Append(GameState.Serialize(gsma.Original) + "|" + GameState.Serialize(gsma.GameState) + "|" + gsma.Time + "|");

			foreach (TimedMove m in gsma.MadeMoves)
				sb.Append(m.ToString() + " ");

			sb.Append("|" + s);

			return sb.ToString();
		}

		public static GameStateAction Deserialize(string s)
		{
			string[] t = s.Split('|');

			GameState original = GameState.Deserialize(t[0]);
			GameState gs = GameState.Deserialize(t[1]);
			long time = long.Parse(t[2]);

			/*Stack<Move> madeMoves = new Stack<Move>();
			foreach (string d in t[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Reverse())
			{
				Move m = new Move(d);
				madeMoves.Push(m);
			}*/
			List<TimedMove> madeMoves = new List<TimedMove>();
			foreach (string d in t[3].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
			{
				TimedMove m = TimedMove.Deserialize(d);
				madeMoves.Add(m);
			}

			TimedMove move = null;
			if (t[4] != "X")
				move = TimedMove.Deserialize(t[4]);

			return new GameStateMoveAction(gs, original, time, move, madeMoves);
		}
	}
}
