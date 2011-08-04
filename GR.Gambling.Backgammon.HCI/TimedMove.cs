using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon;

namespace GR.Gambling.Backgammon.HCI
{
    public class TimedMove : Move
    {
        private int wait_before;
        private int wait_after;
        private bool undo; // Undo command

        public TimedMove(Move move, int wait_before, int wait_after)
            : base(move)
        {
            this.wait_before = wait_before;
            this.wait_after = wait_after;
            this.undo = false;
        }

        public bool IsUndo { get { return undo; } }
        public int WaitBefore { get { return wait_before; } set { wait_before = value; } }
        public int WaitAfter { get { return wait_after; } set { wait_after = value; } }

        public static TimedMove CreateUndoMove(int wait_before, int wait_after)
        {
            TimedMove timed_move = new TimedMove(new Move(0, 1), wait_before, wait_after);
            timed_move.undo = true;

            return timed_move;
        }

		public TimedMove(TimedMove move)
			: base(move)
		{
			this.wait_before = move.wait_before;
			this.wait_after = move.wait_after;
			this.undo = move.undo;
		}

		/*public TimedMove Clone()
		{
			return new TimedMove(this);
		}*/

		public static TimedMove Deserialize(string s)
		{
			string[] t = s.Split('{');

			t[1] = t[1].Replace("}", "");
			
			string[] times = t[1].Split(',');

			int wait_before = int.Parse(times[0]);
			int wait_after = int.Parse(times[1]);

			if (t[0] == "undo")
				return CreateUndoMove(wait_before, wait_after);

			return new TimedMove(new Move(t[0]), wait_before, wait_after);
		}

        public override string ToString()
        {
			if (undo)
				return "undo" + "{" + wait_before + "," + wait_after + "}";

			return base.ToString() + "{" + wait_before + "," + wait_after + "}";
        }
    }
}
