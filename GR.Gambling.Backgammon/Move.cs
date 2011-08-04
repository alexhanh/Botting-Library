using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GR.Gambling.Backgammon
{

	// We use a clever trick to indicate bar moves and bearoffs.
	// - I think the list of waypoints can be replaced with a single waypoint, because ambiguous sequences are only possible
	// with non-equal dice, and two dice can only create two sequences.
	/// <summary>
	/// A class to represent a chequer move from the perspective of the player on roll. 
	/// Since the player always races his checkers towards his home board (points 1-6), the starting point is always bigger than
	/// the ending point (since a player cannot move backwards). Additionally, a list of waypoints is stored to indicate a sequence
	/// of points in-between the starting and the ending point. This is needed in order to make it possible to store a move sequence,
	/// where the order of the moves matter. This situation arises when we have two dice that aren't equal in value (6-4 for example)
	/// and both are used for to move the same chequer. The first sequence is 6-4 and the second is 4-6. 
	/// Now, if there's an opponent chequer on either one of the 6 or 4 point, the sequences aren't transitive anymore. So it's important
	/// to store this information.
	/// 
	/// The 'from', 'to' and 'waypoint' use 0-based indexing of the points, so 0-23 are the accepted indices.
	/// </summary>

    public class Move
    {
        private class MoveComparer : IComparer<int>
        {
            /// <summary>
            /// Sorts from biggest to lowest.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }

        private SortedList<int, bool> points;

        public int From { get { return points.Keys[0]; } }
        public int To { get { return points.Keys[points.Count - 1]; } }


        public int Distance { get { return this.From - this.To; } }

        public bool IsBearOff { get { return To == -1; } }
        public bool IsEnter { get { return From == 24; } }

        public IEnumerable<int> Points { get { return points.Keys; } }
        public IEnumerable<int> Waypoints
        {
            get
            {
                for (int i = 1; i < points.Count - 1; i++)
                    yield return points.Keys[i];
            }
        }

        /// <summary>
        /// A move is a simple move if it doesn't contain any waypoints.
        /// </summary>
        public bool IsSimpleMove { get { return points.Count == 2; } }

        public bool HasHits { get { return points.Values.Contains(true); } }

        public Move()
        {
            points = new SortedList<int, bool>(5, new MoveComparer());
            // What to add here to the empty points?
        }

        public Move(int from, int to)
        {
            points = new SortedList<int, bool>(5, new MoveComparer());

            AddPoint(from);
            AddPoint(to);
        }

        public Move(Move move)
        {
            points = new SortedList<int, bool>(move.points, new MoveComparer());
        }

        public Move(IEnumerable<int> points)
        {
            this.points = new SortedList<int, bool>(new MoveComparer());

            foreach (int point in points)
                AddPoint(point);
        }

		public Move(string s)
		{
			this.points = new SortedList<int, bool>(new MoveComparer());

			string[] t = s.Split('/');

			int from = -2;
			if (t[0] == "bar")
				from = 24;
			else
				from = int.Parse(t[0]) - 1;

			AddPoint(from);

			int to = -2;
			if (t[t.Length - 1].StartsWith("off"))
				to = -1;
			else
				to = int.Parse(t[t.Length - 1].Trim('*')) - 1;

			if (t[t.Length - 1].Contains("*"))
				AddHitPoint(to);
			else
				AddPoint(to);

			for (int i = 1; i < (t.Length - 1); i++)
			{
				if (t[i].Contains("*"))
					AddHitPoint(int.Parse(t[i].Trim('*')) - 1);
				else
					AddPoint(int.Parse(t[i]) - 1);
			}
		}

		public void AddPoint(int point)
        {
            points[point] = false;
        }

        public void AddHitPoint(int point)
        {
            points[point] = true;
        }

        public void RemovePoint(int point)
        {
            points.Remove(point);
        }

        public bool IsHitPoint(int point)
        {
            return points[point];
        }

        /// <summary>
        /// Breaks a complex move with waypoints to a collection of simple moves without waypoints.
        /// </summary>
        /// <returns></returns>
        public List<Move> ToSimpleMoves() // DieMove better name? (it uses a single die)
        {
            List<Move> moves = new List<Move>();

            for (int i = 0; i < points.Keys.Count - 1; i++)
            {
                moves.Add(new Move(points.Keys[i], points.Keys[i + 1]));
                if (IsHitPoint(points.Keys[i + 1]))
                    moves[moves.Count - 1].AddHitPoint(points.Keys[i + 1]);
            }

            return moves;
        }

        /// <summary>
        /// A factory method for creating a bearoff move.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static Move CreateBearoffMove(int from)
        {
            return new Move(from, -1);
        }

        /// <summary>
        /// A factory method for creating a bearoff move.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Move CreateBearoffMove(int[] points)
        {
            Move move = new Move();

            foreach (int point in points)
                move.AddPoint(point);

            return move;
        }

        /// <summary>
        /// A factory method for creating a bar move.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Move CreateBarMove(int to)
        {
            return new Move(24, to);
        }

        /// <summary>
        /// A factory method for creating a bar move.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Move CreateBarMove(int[] points)
        {
            Move move = new Move();

            move.AddPoint(24);

            foreach (int point in points)
                move.AddPoint(point);

            return move;
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (From == 24)
                sb.Append("bar/");
            else
                sb.Append((From + 1) + "/");

            for (int i = 1; i < points.Count - 1; i++)
            {
                sb.Append((points.Keys[i] + 1));
                
                if (points[points.Keys[i]])
                    sb.Append("*");

                sb.Append("/");
            }

            if (To == -1)
                sb.Append("off");
            else
            {
                sb.Append("" + (To + 1));
                if (points[To])
                    sb.Append("*");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Two moves are equal if they share the same points.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) 
                return false;

            Move move = (Move)obj;
            if (points.Count != move.points.Count)
                return false;

            for (int i = 0; i < points.Count; i++)
                if (points.Keys[i] != move.points.Keys[i])
                    return false;

            return true;
        }

        private uint[] bit_masks = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4194304, 8388608, 16777216, 33554432, 67108864 };
        public override int GetHashCode()
        {
            uint hash = 0;

            foreach (int point in points.Keys)
                hash |= bit_masks[point + 1];

            return (int)hash;
        }
    }
}
