using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon
{
    /// <summary>
    /// Represent a complete list of moves for a player for one turn. Because of this, the order in which the moves are put into the sequence, matters.
    /// Breaks added complex moves into simple moves.
    /// Source: http://www.bkgm.com/gloss/lookup.cgi?play
    /// </summary>
    public class Play
    {
        private class MoveComparer : IComparer<Move>
        {
            /// <summary>
            /// Sorts from biggest to lowest.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(Move m1, Move m2)
            {
                return m2.From.CompareTo(m1.From);
            }
        }

        private List<Move> moves;
        private MoveComparer comparer;

        public Move this[int index]
        {
            get
            {
                return moves[index];
            }
        }

        /// <summary>
        /// Returns the total number of simple moves this move sequence contains.
        /// </summary>
        /// <returns></returns>
        public int Count { get { return moves.Count; } }

        public Play()
        {
            this.moves = new List<Move>();
            this.comparer = new MoveComparer();
        }

        public Play(IEnumerable<Move> moves)
        {
            this.moves = new List<Move>();
            this.comparer = new MoveComparer();

            foreach (Move move in moves)
                Add(move);
        }

        /// <summary>
        /// Adds a move to the end of the move sequence.
        /// </summary>
        /// <param name="move"></param>
        public void Add(Move move)
        {
            foreach (Move simple_move in move.ToSimpleMoves())
                moves.Add(simple_move);
        }

        public bool Contains(Move move)
        {
            return moves.Contains(move);
        }

        public void Remove(Move move)
        {
            moves.Remove(move);
        }

        /// <summary>
        /// Reverse the order of moves.
        /// </summary>
        public void Reverse()
        {
            moves.Reverse();
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return moves.GetEnumerator();
        }

        /// <summary>
        /// Sorts the moves by their 'from' points.
        /// </summary>
        public void SortHiToLow()
        {
            moves.Sort(comparer);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Play play = (Play)obj;
            foreach (Move move in play)
                if (!this.Contains(move))
                    return false;
            return true;
        }

        public override string ToString()
        {
            if (moves.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            
            sb.Append(moves[0].ToString());
            for (int i = 1; i < moves.Count; i++)
                sb.Append(" " + moves[i].ToString());

            return sb.ToString();
        }
    }
}
