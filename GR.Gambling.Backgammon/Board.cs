using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;

namespace GR.Gambling.Backgammon
{
    /* Just a nice ASCII graph of a initial backgammon position
 +13-14-15-16-17-18------19-20-21-22-23-24-+
 | X           O    |   | O              X |
 | X           O    |   | O              X |
 | X           O    |   | O                |
 | X                |   | O                |
 | X                |   | O                |
v|                  |BAR|                  |
 | O                |   | X                |
 | O                |   | X                |
 | O           X    |   | X                |
 | O           X    |   | X              O |
 | O           X    |   | X              O |
 +12-11-10--9--8--7-------6--5--4--3--2--1-+

 Direction for player X is counterclockwise.
 Direction for player O is clockwise.
    	 
 Special index -1 is used to indicate non-existent player (none).
    	 
 */
    // TODO: rename slot to point to provide consistency in naming between Move and Board?
    public class Board
    {
        // Gnubg style representation, 0 is the ace-point of the player [player={0,1}][0..23 are slots, 25 finished count and 24 captured]
        // It's a representation of the board relative to the player
        // Jagged-arrays perform better? http://www.kerrywong.com/2005/10/26/performance-comparison-rectangular-array-vs-jagged-array-update/
        private int[][] board;
        private int[] captured;
        private int[] finished;

        public Board()
        {
            board = new int[2][];
            board[0] = new int[24];
            board[1] = new int[24];

            for (int p = 0; p < 24; p++)
                board[0][p] = board[1][p] = 0;

            captured = new int[2];
            finished = new int[2];
        }

        /// <summary>
        /// Deep-copy constructor.
        /// </summary>
        /// <param name="board"></param>
        public Board(Board board)
        {
            this.board = new int[2][];
            this.board[0] = new int[24];
            this.board[1] = new int[24];

            this.captured = new int[2];
            this.finished = new int[2];

            this.captured[0] = board.captured[0];
            this.captured[1] = board.captured[1];
            this.finished[0] = board.finished[0];
            this.finished[1] = board.finished[1];

            for (int p = 0; p < 24; p++)
            {
                this.board[0][p] = board.board[0][p];
                this.board[1][p] = board.board[1][p];
            }
        }

        public void InitializeBoard(BackgammonVariation variation)
        {
            if (variation == BackgammonVariation.Standard)
            {
                board[0][5] = board[1][5] = board[0][12] = board[1][12] = 5;
                board[0][7] = board[1][7] = 3;
                board[0][23] = board[1][23] = 2;

                captured[0] = captured[1] = 0;
                finished[0] = finished[1] = 0;
            }
            else if (variation == BackgammonVariation.Hypergammon)
            {
                board[0][23] = board[0][22] = board[0][21] = 1;
                board[1][23] = board[1][22] = board[1][21] = 1;

                captured[0] = captured[1] = 0;
                finished[0] = finished[1] = 0;
            }
            else
            {
                // Unknown variation
            }
        }

        public void AddToPoint(int player, int point, int count) { if (count > 0) board[player][point] += count; else board[1 - player][23 - point] += -1 * count; }
        public void RemoveFromPoint(int player, int point, int count) { board[player][point] -= count; }
        public void EmptyPoint(int player, int point) { board[player][point] = board[1 - player][23 - point] = 0; }
		public void Empty() { for (int p = 0; p < 24; p++) EmptyPoint(0, p); captured[0] = captured[1] = finished[0] = finished[1] = 0; }

        public void IncreaseCaptured(int player) { captured[player]++; }
        public void IncreaseCaptured(int player, int count) { captured[player] += count; }
        public void DecreaseCaptured(int player) { captured[player]--; }
        public void SetCaptured(int player, int count) { captured[player] = count; }
        public int CapturedCount(int player) { return captured[player]; }

        public void IncreaseFinished(int player) { finished[player]++; }
        public void IncreaseFinished(int player, int count) { finished[player] += count; }
        public void DecreaseFinished(int player) { finished[player]--; }
        public void SetFinished(int player, int count) { finished[player] = count; }
        public int FinishedCount(int player) { return finished[player]; }

        /// <summary>
        /// Returns the count of this player's perspective. Count is negative if opponent has checkers on the given slot.
        /// </summary>
        /// <param name="player">Relative player from whos perspective the query is made.</param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public int PointCount(int player, int point)
        {
            int opponent = 1 - player;
            int flipped = 23 - point;
            if (board[player][point] > 0)
                return board[player][point];
            else if (board[opponent][flipped] > 0)
                return -board[opponent][flipped];
            else
                return 0;
        }

        /// <summary>
        /// Sets the given slot to count from player's perspective.
        /// </summary>
        /// <param name="player">The player from whose perspective the slot is mapped.</param>
        /// <param name="slot"></param>
        /// <param name="count">Positive count is for the player, negative for the opponent. Zero count empties the slot for both players.</param>
        public void SetPoint(int player, int point, int count)
        {
            if (player == -1)
                return;

            int opponent = 1 - player;
            int flipped = 23 - point;
            if (count > 0)
            {
                board[player][point] = count;
                board[opponent][flipped] = 0;
            }
            else if (count == 0)
            {
                board[player][point] = 0;
                board[opponent][flipped] = 0;
            }
            else
            {
                board[player][point] = 0;
                board[opponent][flipped] = -1 * count;
            }
        }

        /// <summary>
        /// Returns the board relative to the player, so that the slots 0-5 match the player's home board. Negative counts indicate opponents checkers.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int[] BoardRelativeTo(int player)
        {
            int[] relative_board = new int[24];

            int opponent = 1 - player;
            for (int i = 0; i < 24; i++)
            {
                int flipped = 23 - i;
                if (board[player][i] > 0)
                    relative_board[i] = board[player][i];
                else if (board[opponent][flipped] > 0)
                    relative_board[i] = -board[opponent][flipped];
                else
                    relative_board[i] = 0;
            }

            return relative_board;
        }

        /// <summary>
        /// Returns the point of the last chequer of the player from the player's perspective. Returns 24 for captured and -1 if all checkers have been bearoffed.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int LastChequer(int player)
        {
            if (CapturedCount(player) > 0)
                return 24;

            int point = -1;
            for (point = 23; point >= 0; point--)
                if (board[player][point] > 0)
                    return point;

            return point;
        }

        /// <summary>
        /// Returns true if both player's checkers have "passed" each others so that it's impossible to hit or block anymore.
        /// Source: http://www.bkgm.com/gloss/lookup.cgi?pure+race
        /// </summary>
        /// <returns></returns>
        public bool IsPureRace()
        {
            if (CapturedCount(0) > 0 || CapturedCount(1) > 0)
                return false;

            int player = -2, opponent = -2;
            for (int i = 23; i >= 0; i--)
            {
                if (board[0][i] > 0)
                {
                    player = i;
                    break;
                }
            }

            for (int i = 23; i >= 0; i--)
            {
                if (board[1][i] > 0)
                {
                    opponent = i;
                    break;
                }
            }

            if (player == -2 || opponent == -2)
                return false;

            // Flip the index
            opponent = 23 - opponent;

            if (player > opponent)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if player's all chequers are on his home board and is bearing off.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsBearoff(int player)
        {
            return LastChequer(player) < 6;
        }

        /// <summary>
        /// Returns the current resign value for the player based on the last chequer's position.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public ResignValue ResignationValue(int player)
        {
            if (FinishedCount(player) > 0)
                return ResignValue.Single;

            int last_chequer_point = LastChequer(player);
            if (last_chequer_point > 17)
                return ResignValue.Backgammon;

            return ResignValue.Gammon;
        }

        public int PipCount(int player)
        {
            int pip_count = 0;
            for (int i = 0; i < 24; i++)
            {
                if (board[player][i] > 0)
                    pip_count += (i + 1) * board[player][i];
            }
            pip_count += 25 * CapturedCount(player);

            return pip_count;
        }

		public bool IsLegal()
		{
			int total = 0;
			for (int s = 0; s < 24; s++)
				total += board[0][s];
			total += finished[0] + captured[0];

			if (total != 15)
				return false;

			total = 0;
			for (int s = 0; s < 24; s++)
				total += board[1][s];
			total += finished[1] + captured[1];

			if (total != 15)
				return false;

			return true;
		}

		/// <summary>
		/// Returns if the given move for the given player is a valid move.
		/// </summary>
		/// <param name="player">The player for whom the move is made.</param>
		/// <param name="move">The move to be made, slot indices must be relative to the player making the move.</param>
		/// <param name="die">The die with which the move is made.</param>
		/// <returns></returns>
		public bool IsLegalMove(int player, Move move, int die)
        {
            if (CapturedCount(player) > 0 && move.IsEnter == false)
                return false;

            // The second check if for bearoff moves where move distance is smaller than the die but the bearoffed chequer isn't the last one.
            if (move.IsBearOff && (LastChequer(player) > 5 || (move.Distance < die && LastChequer(player) > move.From)))
                return false;

            // There should be player's chequer at whatever slot we are moving from.
            if (move.IsEnter == true)
            {
                if (CapturedCount(player) == 0)
                    return false;
            }
            else
            {
                if (board[player][move.From] == 0)
                    return false;
            }

            // Next, there should be no opponent "stacks" at waypoints or the move destination slot.
            int opponent = 1 - player;

            foreach (int waypoint in move.Waypoints)
                if (board[opponent][23 - waypoint] >= 2)
                    return false;

            if (!move.IsBearOff)
            {
                if (board[opponent][23 - move.To] >= 2)
                    return false;
            }

            return true;
        }

        // TODO: Test this heavily out. Idea: Setup random positions, get 0-ply (for speed) list of plays and compare results to the ones given by legal moves
        /// <summary>
        /// This returns all the possible legal moves for the player with given dice. It sorts the dice from highest to lowest, so it produces same results regardless of the dice order.
        /// It also finds correctly moves according to following rules: "For any roll, if a player can move both dice, that player is compelled to do so. 
        /// If it is possible to move either die, but not both, the higher number must be played. 
        /// Further, if one die is unable to be moved, but such a move is made possible by the moving of the other die, that move is compulsory."
        /// Source: http://en.wikipedia.org/wiki/Backgammon
        /// </summary>
        /// <param name="player"></param>
        /// <param name="dice"></param>
        /// <returns></returns>
        public List<Play> LegalPlays(int player, int[] dice)
        {
            List<Play> plays = new List<Play>(); // Contains the legal move sequences.
            List<Play> partial_plays = new List<Play>(); // Contains move sequences, which leave one or more unused dice.
            List<Move> moves_made = new List<Move>(); // Contains the moves made so far in the recursion.
            List<int> free_dice = new List<int>(); // Contains unused dice so far in the recursion.
            HashSet<string> board_hash = new HashSet<string>(); // Contains the hashes of already traversed boards.
            
            free_dice.Add(Math.Max(dice[0], dice[1]));
            free_dice.Add(Math.Min(dice[0], dice[1]));

            if (dice[0] == dice[1])
            {
                free_dice.Add(dice[0]);
                free_dice.Add(dice[1]);
            }

            Recursion(player, free_dice, this, moves_made, ref plays, ref partial_plays, ref board_hash);
            //Recursion(player, free_dice, this, moves_made, ref move_hints);

            if (plays.Count == 0 && partial_plays.Count > 0)
            {
                if (partial_plays.Count == 1)
                {
                    plays.Add(partial_plays[0]);
                }
                else
                {
                    if (dice[0] != dice[1])
                    {
                        foreach (Play partial_move_sequence in partial_plays)
                            if (partial_move_sequence[0].Distance == Math.Max(dice[0], dice[1]))
                                plays.Add(partial_move_sequence);
                    }
                    else
                    {
                        // I believe this should never happen
                        Console.WriteLine("LegalMoves Partialmovehints problem!");
                    }
                }
            }

            return plays;
        }

        // A recursion which finds all possible move sequences, including permutations.
        /*private static void Recursion(int player, List<int> free_dice, Board board, List<Move> moves_made, ref List<MoveHint> move_hints)
        {
            if (free_dice.Count == 0 || board.FinishedCount(player) == 15)
            {
                move_hints.Add(new MoveHint(new List<Move>(moves_made)));
                return;
            }

            if (board.CapturedCount(player) > 0)
            {
                for (int i = 0; i < free_dice.Count; i++)
                {
                    int free_die = free_dice[i];
                    int to = 24 - free_die;
                    Move move = Move.CreateBarMove(to);

                    if (board.IsLegalMove(player, move))
                    {
                        if (board.SlotCount(player, to) == -1)
                            move.AddHitPoint(to);

                        free_dice.RemoveAt(i);

                        moves_made.Add(move);

                        board.Makemove(player, move);

                        Recursion(player, free_dice, board, moves_made, ref move_hints);

                        board.UndoMove(player, move);

                        moves_made.RemoveAt(moves_made.Count - 1);

                        free_dice.Insert(i, free_die);
                    }
                }

                return;
            }

            for (int slot = 23; slot >= 0; slot--)
            {
                if (board.SlotCount(player, slot) > 0)
                {
                    for (int i = 0; i < free_dice.Count; i++)
                    {
                        int free_die = free_dice[i];
                        int to = slot - free_die;
                        Move move = (to >= 0) ? new Move(slot, to) : Move.CreateBearoffMove(slot);

                        if (board.IsLegalMove(player, move))
                        {
                            if (board.SlotCount(player, to) == -1)
                                move.AddHitPoint(to);

                            free_dice.RemoveAt(i);

                            moves_made.Add(move);

                            board.Makemove(player, move);

                            Recursion(player, free_dice, board, moves_made, ref move_hints);

                            board.UndoMove(player, move);

                            moves_made.RemoveAt(moves_made.Count - 1);

                            free_dice.Insert(i, free_die);
                        }
                    }
                }
            }

        }*/

        // A recursion which finds all legal moves, but only adds only one move per different end board position.
        // This doesn't always find all bearoff moves, like with D31 on |_ _ _ O _ _|, it'll find 3/off but not 3/2/off. This is because the hash already contains the end position after 3/off.
        private static void Recursion(
            int player, 
            List<int> free_dice, 
            Board board, 
            List<Move> moves_made, 
            ref List<Play> plays,
            ref List<Play> partial_plays,
            ref HashSet<string> board_hash)
        {
            string hash = board.HashString();
            if (board_hash.Contains(hash))
                return;

            board_hash.Add(hash);

            if (free_dice.Count == 0)
            {
                plays.Add(new Play(moves_made));
                return;
            }

            bool further_moves = false;
            if (board.CapturedCount(player) > 0)
            {

                for (int i = 0; i < free_dice.Count; i++)
                {
                    int free_die = free_dice[i];
                    int to = 24 - free_die;
                    Move move = Move.CreateBarMove(to);

                    if (board.IsLegalMove(player, move, free_die))
                    {
                        further_moves = true;

                        if (board.PointCount(player, to) == -1)
                            move.AddHitPoint(to);

                        free_dice.RemoveAt(i);

                        moves_made.Add(move);

                        board.MakeMove(player, move);

                        Recursion(player, free_dice, board, moves_made, ref plays, ref partial_plays, ref board_hash);

                        board.UndoMove(player, move);

                        moves_made.RemoveAt(moves_made.Count - 1);

                        free_dice.Insert(i, free_die);
                    }
                }

                // No need to check for further moves here because bar moves are forced and if there aren't any, we can't move at all

                if (!further_moves && plays.Count == 0 && moves_made.Count > 0)
                {
                    partial_plays.Add(new Play(moves_made));
                }

                return;
            }

            further_moves = false;
            for (int slot = 23; slot >= 0; slot--)
            {
                if (board.PointCount(player, slot) > 0)
                {
                    for (int i = 0; i < free_dice.Count; i++)
                    {
                        int free_die = free_dice[i];
                        int to = slot - free_die;
                        Move move = (to >= 0) ? new Move(slot, to) : Move.CreateBearoffMove(slot);

                        if (board.IsLegalMove(player, move, free_die))
                        {
                            further_moves = true;

                            if (!move.IsBearOff && board.PointCount(player, to) == -1)
                                move.AddHitPoint(to);

                            free_dice.RemoveAt(i);

                            moves_made.Add(move);

                            board.MakeMove(player, move);

                            Recursion(player, free_dice, board, moves_made, ref plays, ref partial_plays, ref board_hash);

                            board.UndoMove(player, move);

                            moves_made.RemoveAt(moves_made.Count - 1);

                            free_dice.Insert(i, free_die);
                        }
                    }
                }
            }

            if (!further_moves && plays.Count == 0 && moves_made.Count > 0)
            {
                partial_plays.Add(new Play(moves_made));
            }
        }

        /// <summary>
        /// A move is a forced move if it must be made before other moves and every move hint share the same forced move.
        /// </summary>
        /// <param name="move_hints"></param>
        /// <returns></returns>
        public static List<Move> ForcedMoves(List<Play> legal_move_sequences)
        {
            List<Move> forced_moves = new List<Move>();

            if (legal_move_sequences.Count == 0)
                return forced_moves;

            int i = 0;
            bool go = true;
            // Since LegalMoves() computes in terms of simple moves, every legal_move_sequence should have the same number of simple moves.
            for (i = 0; i < legal_move_sequences[0].Count && go; i++)
            {
                int from = legal_move_sequences[0][i].From;
                int to = legal_move_sequences[0][i].To;

                foreach (Play move_sequence in legal_move_sequences)
                {
                    if (move_sequence[i].From != from ||
                        move_sequence[i].To != to)
                    {
                        go = false;
                        break;
                    }
                }

                if (go)
                    forced_moves.Add(legal_move_sequences[0][i]);
            }

            return forced_moves;
        }

        public bool IsStartingPosition(BackgammonVariation variation)
        {
            Board board = new Board();
            board.InitializeBoard(variation);

            return this.Equals(board);
        }

        /*/// <summary>
        /// Forced moves should be removed from moves.
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="forced_moves"></param>
        /// <returns></returns>
        public static List<Move> SortAndTimeMoves(List<Move> moves, List<Move> forced_moves)
        {
            List<Move> timed_moves = new List<Move>();

            foreach (Move move in forced_moves)
            {

            }

            return timed_moves;
        }*/

        /// <summary>
        /// Applys a given move to this board. This method assumes the move is valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="move"></param>
        public void MakeMove(int player, Move move)
        {
            int opponent = 1 - player;

            // Check for captures
            foreach (int waypoint in move.Waypoints)
            {
                if (PointCount(player, waypoint) == -1)
                {
                    EmptyPoint(player, waypoint);
                    IncreaseCaptured(opponent);
                }
            }

            if (move.IsBearOff == false)
            {
                if (PointCount(player, move.To) == -1)
                {
                    EmptyPoint(player, move.To);
                    IncreaseCaptured(opponent);
                }
            }

            if (move.IsEnter == true)
            {
                DecreaseCaptured(player);
                AddToPoint(player, move.To, 1);
            }
            else if (move.IsBearOff == true)
            {
                RemoveFromPoint(player, move.From, 1);
                IncreaseFinished(player);
            }
            else
            {
                RemoveFromPoint(player, move.From, 1);
                AddToPoint(player, move.To, 1);
            }
        }

        public void MakeMoves(int player, IEnumerable<Move> moves)
        {
            foreach (Move move in moves)
                MakeMove(player, move);
        }

        public void MakePlay(int player, Play play)
        {
            foreach (Move move in play)
                MakeMove(player, move);
        }

        /// <summary>
        /// Reverses a given move that was made. Notice that if you are reversing a made move sequence, you need to do it in reverse order,
        /// that is, undo the last made move first, then the second last, and so on.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="move"></param>
        public void UndoMove(int player, Move move)
        {
            if (move.IsBearOff)
            {
                AddToPoint(player, move.From, 1);
                DecreaseFinished(player);
            }
            else if (move.IsEnter)
            {
                IncreaseCaptured(player);
                board[player][move.To]--;
            }
            else
            {
                board[player][move.From]++;
                board[player][move.To]--;
            }

            int opponent = 1 - player;
            foreach (int point in move.Points)
                if (move.IsHitPoint(point))
                {
                    board[opponent][23 - point]++;
                    DecreaseCaptured(opponent);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="play">The play in which moves are in the order they were applied to the board and not reversed.</param>
        public void UndoPlay(int player, Play play)
        {
            for (int i = play.Count - 1; i >= 0; i--)
                UndoMove(player, play[i]);
        }

        public bool Equals(Board board)
        {
            if (this.captured[0] != board.captured[0] ||
                this.captured[1] != board.captured[1] ||
                this.finished[0] != board.finished[0] ||
                this.finished[1] != board.finished[1])
                return false;

            for (int i = 0; i < 24; i++)
            {
                if (this.board[0][i] != board.board[0][i] ||
                    this.board[1][i] != board.board[1][i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a deep-copy of this board.
        /// </summary>
        /// <returns></returns>
        public Board Clone()
        {
            return new Board(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int player = 0; player < 2; player++)
            {
                sb.Append("[P" + player + "] C: " + CapturedCount(player) + " F: " + FinishedCount(player) + " ");
                sb.AppendLine();
                for (int i = 0; i < 24; i++)
                    sb.Append(board[player][i] + " ");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Provides a unique hash string for each different board position.
        /// </summary>
        /// <returns></returns>
        // TODO: one slot, captured or finished count is deductable from the others and can be removed.
        public string HashString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(finished[0] + " ");
            for (int slot = 0; slot < 24; slot++)
                sb.Append(board[0][slot] + " ");
            sb.Append(captured[0] + " ");

            sb.Append(finished[1] + " ");
            for (int slot = 0; slot < 24; slot++)
                sb.Append(board[1][slot] + " ");
            sb.Append(captured[1]);

            return sb.ToString();
        }

        /// <summary>
        /// Gnubg style rendering of the board from the perspective of player. X is always the player's checkers, O opponent's.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public string ToString(int player)
        {
            int opponent = 1 - player;

            StringBuilder sb = new StringBuilder();

            sb.Append("+");
            for (int slot = 13; slot <= 18; slot++)
                sb.Append(slot + "-");
            sb.Append("-----");
            for (int slot = 19; slot <= 24; slot++)
                sb.Append(slot + "-");
            sb.AppendLine("+");

            for (int depth = 1; depth <= 5; depth++)
            {
                sb.Append("|");
                for (int slot = 12; slot < 18; slot++)
                {
                    int count = PointCount(player, slot);
                    int pos_count = Math.Abs(count);
                    if (pos_count >= depth)
                    {
                        if (depth < 5 || pos_count == depth)
                        {
                            if (count > 0)
                                sb.Append(" X ");
                            else
                                sb.Append(" O ");
                        }
                        else
                            sb.Append(" " + (pos_count>=10?pos_count.ToString():pos_count.ToString() + " "));
                    }
                    else
                        sb.Append("   ");
                }

                int captured = CapturedCount(player);
                if (depth == 1 && captured > 0)
                {
                    if (captured < 10)
                        sb.Append("|X " + captured + "|");
                    else
                        sb.Append("|X" + captured + "|");
                }
                else
                    sb.Append("|   |");

                for (int slot = 18; slot < 24; slot++)
                {
                    int count = PointCount(player, slot);
                    int pos_count = Math.Abs(count);
                    if (pos_count >= depth)
                    {
                        if (depth < 5 || pos_count == depth)
                        {
                            if (count > 0)
                                sb.Append(" X ");
                            else
                                sb.Append(" O ");
                        }
                        else
                            sb.Append(" " + (pos_count >= 10 ? pos_count.ToString() : pos_count.ToString() + " "));
                    }
                    else
                        sb.Append("   ");
                }

                int finished = FinishedCount(1 - player);
                if (depth == 1 &&  finished > 0)
                    sb.AppendLine("| O" + finished.ToString());
                else
                    sb.AppendLine("|");
            }

            sb.AppendLine("|                  |BAR|                  |");

            for (int depth = 5; depth >= 1; depth--)
            {
                sb.Append("|");
                for (int slot = 11; slot >= 6; slot--)
                {
                    int count = PointCount(player, slot);
                    int pos_count = Math.Abs(count);
                    if (pos_count >= depth)
                    {
                        if (depth < 5 || pos_count == depth)
                        {
                            if (count > 0)
                                sb.Append(" X ");
                            else
                                sb.Append(" O ");
                        }
                        else
                            sb.Append(" " + (pos_count >= 10 ? pos_count.ToString() : pos_count.ToString() + " "));
                    }
                    else
                        sb.Append("   ");
                }

                int captured = CapturedCount(opponent);
                if (depth == 1 && captured > 0)
                {
                    if (captured < 10)
                        sb.Append("|O " + captured + "|");
                    else
                        sb.Append("|O" + captured + "|");
                }
                else
                    sb.Append("|   |");

                for (int slot = 5; slot >= 0; slot--)
                {
                    int count = PointCount(player, slot);
                    int pos_count = Math.Abs(count);
                    if (pos_count >= depth)
                    {
                        if (depth < 5 || pos_count == depth)
                        {
                            if (count > 0)
                                sb.Append(" X ");
                            else
                                sb.Append(" O ");
                        }
                        else
                            sb.Append(" " + (pos_count >= 10 ? pos_count.ToString() : pos_count.ToString() + " "));
                    }
                    else
                        sb.Append("   ");
                }

                int finished = FinishedCount(player);
                if (depth == 1 && finished > 0)
                    sb.AppendLine("| X" + finished.ToString());
                else
                    sb.AppendLine("|");
            }

            sb.Append("+");
            for (int slot = 12; slot >= 10; slot--)
                sb.Append(slot + "-");
            for (int slot = 9; slot >= 7; slot--)
                sb.Append("-" + slot + "-");
            sb.Append("-----");
            for (int slot = 6; slot >= 1; slot--)
                sb.Append("-" + slot + "-");
            sb.Append("+");

            return sb.ToString();
        }

		public static string Serialize(Board board)
		{
			StringBuilder sb = new StringBuilder();

			// Player1 #captured# #finished# #slot0# ... #slot23#
			// Player2 #captured# #finished# #slot0# ... #slot23#
			sb.Append(board.captured[0] + " " + board.finished[0]);
			for (int p = 0; p < 24; p++)
				sb.Append(" " + board.board[0][p]);

			sb.Append(" " + board.captured[1] + " " + board.finished[1]);
			for (int p = 0; p < 24; p++)
				sb.Append(" " + board.board[1][p]);

			return sb.ToString();
		}

		public static Board Deserialize(string s)
		{
			string[] ss = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			int i = 0;

			Board board = new Board();
			board.captured[0] = int.Parse(ss[i]);
			i++;

			board.finished[0] = int.Parse(ss[i]);
			i++;

			while (i < 26)
			{
				board.board[0][i - 2] = int.Parse(ss[i]);
				i++;
			}

			board.captured[1] = int.Parse(ss[i]);
			i++;

			board.finished[1] = int.Parse(ss[i]);
			i++;

			while (i < 52)
			{
				board.board[1][i - 28] = int.Parse(ss[i]);
				i++;
			}

			return board;
		}
    }
}