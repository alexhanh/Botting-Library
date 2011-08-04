using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.HCI
{
    public class TimedPlay
    {
        private List<TimedMove> timed_moves;

        public TimedPlay()
        {
            timed_moves = new List<TimedMove>();
        }

        public void Add(TimedMove timed_move)
        {
            timed_moves.Add(timed_move);
        }

        public IEnumerator<TimedMove> GetEnumerator()
        {
            return timed_moves.GetEnumerator();
        }

        public override string ToString()
        {
            if (timed_moves.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            sb.Append(timed_moves[0].ToString());
            for (int i = 1; i < timed_moves.Count; i++)
                sb.Append(" " + timed_moves[i].ToString());

            return sb.ToString();
        }
    }
}
