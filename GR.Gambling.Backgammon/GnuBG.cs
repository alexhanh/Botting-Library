using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using GR.Gambling.Backgammon;

namespace GR.Gambling.Backgammon.Tools
{
	public struct EvaluationInfo
	{
		public double Win;
		public double WinGammon;
		public double WinBackgammon;
		public double Lose;
		public double LoseGammon;
		public double LoseBackgammon;
	}

	public class GnuBg
	{
        private StringBuilder input_buffer;
        private Process process;
        private StreamReader std_out;
        private StreamWriter std_in;
        private char[] read_buf;
        private string prompt;

        private string path;
        private bool verbose;

		private Dictionary<string, DoubleAction> string2doubleaction;
        private Dictionary<string, DoubleResponse> string2doubleresponse;
        private Dictionary<string, ResignResponse> string2resignresponse = new Dictionary<string, ResignResponse>();
        private string[] double_strings = new string[] { "Your proper cube action:", "Proper cube action:" };

		// Holds the parameters with which the met has been generated.
		private int met_stake = -1;
		private int met_limit = -1;

        public GnuBg(string path)
        {
            input_buffer = new StringBuilder();
            process = new Process();
            read_buf = new char[4096];
            prompt = "<READY>";
            this.path = path;
			
            string2doubleaction = new Dictionary<string, DoubleAction>
            {
                { "no double", DoubleAction.NoDouble },
                { "no redouble", DoubleAction.NoDouble },
                { "too good to double", DoubleAction.NoDouble }, // I think this means we shouldn't double because of bigger value in trying to get a gammon or backgammon.
                { "too good to redouble", DoubleAction.NoDouble },
                { "never double", DoubleAction.NoDouble },
                { "never redouble", DoubleAction.NoDouble },
                { "double", DoubleAction.Double },
                { "redouble", DoubleAction.Double },
                { "beaver", DoubleAction.Double },
                { "optional double", DoubleAction.Double },
                { "optional redouble", DoubleAction.Double }
            };

            string2doubleresponse = new Dictionary<string, DoubleResponse>
            {
                { "take" , DoubleResponse.Take },
                { "pass" , DoubleResponse.Pass },
                { "beaver" , DoubleResponse.Beaver }
                /*{ "No double" , DoubleResponse.None },
                { "No redouble" , DoubleResponse.None },
                { "Too good to double" , DoubleResponse.None },
                { "double" , DoubleResponse.None },
                { "Double" , DoubleResponse.None }*/
            };

            string2resignresponse = new Dictionary<string, ResignResponse>
            {
                { "Accept", ResignResponse.Accept },
                { "Reject", ResignResponse.Reject }
            };
        }

        //public string Prompt { get; set; }

        public void Start()
        {
            if (!File.Exists(path))
            {
				Console.WriteLine(path);
                Console.WriteLine("invalid gnubg-cli.exe path, cannot start gnubg");
                return;
            }

			ProcessStartInfo psi = new ProcessStartInfo(path);
            psi.Arguments = "-q"; // start gnubg in quiet (no sound) mode
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            
            process.StartInfo = psi;
  
            process.Start();
            std_out = process.StandardOutput;
            std_in = process.StandardInput;

            SetOptions();

            Console.WriteLine("gnubg initialized");
        }

        private void SetOptions()
        {
            Command("set prompt <READY>");
            // Command("set sound enable off"); // not necessary when using "-q" start argument
            Command("set automatic game off");
            Command("set automatic roll off");
            Command("set automatic bearoff off");
            Command("set automatic crawford off");
            Command("set automatic doubles 0");
            Command("set automatic game off");
            Command("set output mwc off");
			Command("set output digits 6");

            // "No beavers allowed in money sessions."
            Command("set beavers 0");

            Command("set confirm new off");
			Command("set confirm default yes");
            Command("set player 0 human");
            Command("set player 1 human");

			Set0PlyEvaluation();
        }

		private void Set2PlyEvaluation()
		{
			// Make it 2-ply
			Command("set evaluation chequerplay evaluation plies 2");
			Command("set evaluation cubedecision evaluation plies 2");

			// Turn fast neural net pruning on
			Command("set evaluation chequerplay evaluation prune on");
			Command("set evaluation cubedecision evaluation prune on");

			// Turn off noise
			Command("set evaluation chequerplay evaluation deterministic on");
			Command("set evaluation chequerplay evaluation noise 0.0");

			Command("set evaluation cubedecision evaluation deterministic on");
			Command("set evaluation cubedecision evaluation noise 0.0");

			// Apply move filters
			Command("set evaluation movefilter 1 0  0 8 0.160000");
			Command("set evaluation movefilter 2 0  0 8 0.160000");
			Command("set evaluation movefilter 2 1 -1 0 0.000000");
			Command("set evaluation movefilter 3 0  0 8 0.160000");
			Command("set evaluation movefilter 3 1 -1 0 0.000000");
			Command("set evaluation movefilter 3 2  0 2 0.040000");
			Command("set evaluation movefilter 4 0  0 8 0.160000");
			Command("set evaluation movefilter 4 1 -1 0 0.000000");
			Command("set evaluation movefilter 4 2  0 2 0.040000");
			Command("set evaluation movefilter 4 3 -1 0 0.000000");
		}

		private void Set0PlyEvaluation()
		{
			// Make it 0-ply
			Command("set evaluation chequerplay evaluation plies 0");
			Command("set evaluation cubedecision evaluation plies 0");

			// Turn fast neural net pruning off
			//Command("set evaluation chequerplay evaluation prune off");
			//Command("set evaluation cubedecision evaluation prune off");

			// Turn off noise
			Command("set evaluation chequerplay evaluation deterministic on");
			Command("set evaluation chequerplay evaluation noise 0.0");

			Command("set evaluation cubedecision evaluation deterministic on");
			Command("set evaluation cubedecision evaluation noise 0.0");
		}

		private void Set0PlyEvaluation(double noise)
		{
			// Make it 0-ply
			Command("set evaluation chequerplay evaluation plies 0");
			Command("set evaluation cubedecision evaluation plies 0");

			// Turn fast neural net pruning off
			Command("set evaluation chequerplay evaluation prune off");
			Command("set evaluation cubedecision evaluation prune off");

			// Turn on noise
			Command("set evaluation chequerplay evaluation deterministic off");
			Command("set evaluation chequerplay evaluation noise " + noise);

			Command("set evaluation cubedecision evaluation deterministic off");
			Command("set evaluation cubedecision evaluation noise " + noise);
		}

		~GnuBg()
		{
			if (process != null && !process.HasExited)
				process.Kill();
		}

		/// <summary>
		/// Tries to close the gnubg process the "natural" way by using the exit command.
		/// If it doesn't exit in max 3000(ms), it kills the running process.
		/// </summary>
		public void Exit()
		{
			// TODO: For some reason, the "exit" command doesn't work
            
			// try close the natural way
			//std_in.Write("exit" + Environment.NewLine);

			//if (process.WaitForExit(10000) == false)
			//{
			if (!process.HasExited)
			{
				process.Kill();
				process.WaitForExit();
			}
			//}
		}

		/// <summary>
		/// Sends a command to the gnubg and returns gnubg's response.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public string Command(string s)
		{
			std_in.Write(s + Environment.NewLine);

			StringBuilder response = new StringBuilder();

			while (true)
			{
				int read = std_out.Read(read_buf, 0, read_buf.Length);

				response.Append(read_buf, 0, read);

				if (response.ToString().EndsWith(prompt))
					break;
			}

			//Console.WriteLine(response.ToString());

			return response.ToString();
		}

		/// <summary>
		/// Gnubg treats positive counts as for the player on turn, and negative for the other.
		/// The first slot is the captured count for the player on turn, last slot is for the capture count of the other.
		/// Gnubg calculates the finished pieces for both players automatically.
		/// 26 integers are represented the following way:
		/// #Captured count for the player on roll# #Slot 1# ... #Slot 24# #Captured count for the other player#
		/// The slots start from the home board of the player on roll.
		/// </summary>
		/// <param name="game_state"></param>
		private void SetBoardSimple(GameState gamestate)
		{
			string simple_board = "";

			simple_board += gamestate.Board.CapturedCount(gamestate.PlayerOnRoll);

			int[] board = gamestate.Board.BoardRelativeTo(gamestate.PlayerOnRoll);

			for (int i = 0; i < 24; i++)
				simple_board += " " + board[i].ToString();

			simple_board += " " + gamestate.Board.CapturedCount(1 - gamestate.PlayerOnRoll);

			//Console.WriteLine(simple_board);
			//Console.WriteLine(Command("set board simple " + simple_board));
			Command("set board simple " + simple_board);
		}


		/// <summary>
		/// Doesn't use the match id for setting up the position.
		/// </summary>
		/// <param name="gamestate"></param>
		public void SetGameState(GameState gamestate)
		{
			bool set_cube = true;
			if (gamestate.GameType == GameType.Money)
			{
				int wager = gamestate.Stake * gamestate.Cube.Value;
				bool capped = (wager >= gamestate.Limit);
				// Evaluate as a single point match if the game is capped or the cube is centered (jacoby rule) and leave the cube at the center.
				if (gamestate.DiceRolled && (capped || gamestate.Cube.Centered))
				{
//					Console.WriteLine("EVALUATING AS A MATCH");
					Command("new match 1");
					set_cube = false;
				}
				else
				{
					int max_points = (int)(gamestate.Limit / gamestate.Stake);

					if (max_points * gamestate.Stake < gamestate.Limit)
						max_points++;

					if (max_points < 1)
						max_points = 1;

					if (gamestate.Stake < gamestate.Limit && (met_stake != gamestate.Stake || met_limit != gamestate.Limit))
					{
//						Console.WriteLine("Setting money MET.");
						MatchEquityTable.CreateRakelessMet("gnubg/met.xml", gamestate.Stake, gamestate.Limit);

						Command("set met met.xml");

						met_stake = gamestate.Stake;
						met_limit = gamestate.Limit;
					}

					Command("new match " + max_points);
				}
			}
			else if (gamestate.GameType == GameType.Match) // What is only set in matches.
			{
				Command("new match " + gamestate.MatchTo);

				Command("set score " + gamestate.Score(0) + " " + gamestate.Score(1));
				// The score (after 0 games) is: gnubg 2, Administrator 2 (match to 3 points, post-Crawford play).

				// Need to be 1-away from match length to be able to set crawford game.
				if (gamestate.MatchTo - gamestate.Score(0) == 1 ||
					gamestate.MatchTo - gamestate.Score(1) == 1)
					Command("set crawford " + (gamestate.IsCrawford ? "true" : "false"));
				// Cannot set Crawford play for money sessions.
				// This game is the Crawford game (no doubling allowed).
			}
			else
			{
			}

			// This also clears the dice!
			Command("set turn " + gamestate.PlayerOnRoll);

			SetBoardSimple(gamestate);

			if (gamestate.OfferType == OfferType.Double)
			{
				Command("set turn " + gamestate.Cube.Owner);
				Command("set cube owner " + gamestate.Cube.Owner);// gamestate.PlayerOnRoll);
				Command("set cube value " + (gamestate.Cube.Value / 2));

				Command("double");
			}
			else if (gamestate.OfferType == OfferType.Resign)
			{
				if (gamestate.ResignOfferValue == ResignValue.None)
					Console.WriteLine("Resign offer but ResignValue == None!");

				Command("resign " + (int)gamestate.ResignOfferValue);
			}
			else
			{
				if (gamestate.DiceRolled)
					Command("set dice " + gamestate.Dice[0] + " " + gamestate.Dice[1]);
				// The dice have been set to 6 and 6.

				if (gamestate.Cube.Centered || !set_cube)
				{
					Command("set cube center");
					// The cube has been centred.
				}
				else
				{
					Command("set cube owner " + gamestate.Cube.Owner);
					// gnubg now owns the cube.

					Command("set cube value " + gamestate.Cube.Value);
					// The cube has been set to 2.
				}
			}
        }

		public EvaluationInfo Eval(GameState gamestate)
		{
			//Set2PlyEvaluation();

			SetGameState(gamestate);

			string s = Command("eval");

			int start = s.IndexOf("static:") + "static:".Length;//"2 ply:") + "2 ply:".Length;
			int end = s.IndexOf('\r', start);

			s = s.Substring(start, end - start);

			string[] t = s.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

			EvaluationInfo eval_info = new EvaluationInfo()
			{
				Win = double.Parse(t[0]),
				WinGammon = double.Parse(t[1]),
				WinBackgammon = double.Parse(t[2]),
				LoseGammon = double.Parse(t[3]),
				LoseBackgammon = double.Parse(t[4])
			};

			eval_info.Lose = 1.0 - eval_info.Win;

			//Set0PlyEvaluation(0.005);

			return eval_info;
		}

        public string Hint(GameState gamestate)
        {
            // Command("new match " + gamestate.MatchTo);
            // SetBoardSimple(gamestate);
            // string matchid_response = Command("set matchid " + MatchID(gamestate));

            SetGameState(gamestate);

            // TODO: Current results give no difference with or without clear hint, without it, there might be performance boost. Investigate further.
            //Command("clear hint");

            return Command("hint");
        }


		/// <summary>
		/// Assumes dice have been rolled and game is still going.
		/// </summary>
		/// <param name="state"></param>
		/// <returns>Returns null if there are no legal moves.</returns>
		public List<PlayHint> PlayHint(GameState gamestate, int max_hints)
		{
            SetGameState(gamestate);

            // TODO: Current results give no difference with or without clear hint, without it, there might be performance boost. Investigate further.
            //Command("clear hint");

			return ParseMoveHints(Command("hint " + max_hints), gamestate.Dice, gamestate);
		}

        public PlayHint PlayHint(GameState gamestate)
        {
            List<PlayHint> play_hints = PlayHint(gamestate, 1);
            if (play_hints == null)
                return null;

            return play_hints[0];
        }

		public DoubleHint DoubleHint(GameState gamestate)
		{
			return GetDoubleHint(gamestate);//new DoubleHint() { Action = ParseDoubleAction(Hint(gamestate)) };
		}

		DoubleHint GetDoubleHint(GameState gamestate)
		{
			string hint = Hint(gamestate);

			Console.WriteLine(gamestate);

			int start = hint.IndexOf("No double") + 9;
			int end = hint.IndexOf("\r\n", start);

			string h = hint.Substring(start, end - start);
			if (h.Contains("("))
				h = h.Substring(0, h.IndexOf("("));
			double noDoubleEq = double.Parse(h.Trim());

			start = hint.IndexOf("Double, pass") + 12;
			end = hint.IndexOf("\r\n", start);
			h = hint.Substring(start, end - start);
			if (h.Contains("("))
				h = h.Substring(0, h.IndexOf("("));

			double doubePassEq = double.Parse(h.Trim());

			start = hint.IndexOf("Double, take") + 12;
			end = hint.IndexOf("\r\n", start);
			h = hint.Substring(start, end - start);
			if (h.Contains("("))
				h = h.Substring(0, h.IndexOf("("));

			double doubeTakeEq = double.Parse(h.Trim());

            start = hint.LastIndexOf(":");
            string sub = hint.Substring(start + 1);
            if (sub.Contains("("))
                sub = sub.Remove(sub.LastIndexOf("("));

            if (sub.Contains("\r\n\r\n" + this.prompt))
                sub = sub.Remove(sub.LastIndexOf("\r\n\r\n" + this.prompt));

            if (sub.Contains(","))
                sub = sub.Remove(sub.IndexOf(","));
            
            sub = sub.Trim();
			sub = sub.ToLowerInvariant();

			// We should double when the cube is centered and it's a money game.
			if ((gamestate.GameType == GameType.Money && gamestate.Cube.Centered) && (sub == "too good to double" || sub == "too good to redouble"))
			{
				return new DoubleHint(DoubleAction.Double, doubeTakeEq, doubePassEq, noDoubleEq);
			}

			return new DoubleHint(string2doubleaction[sub], doubeTakeEq, doubePassEq, noDoubleEq);
		}

        public DoubleResponseHint DoubleResponseHint(GameState gamestate)
        {
			return ParseDoubleResponse(Hint(gamestate));
        }

        public ResignResponseHint ResignResponseHint(GameState gamestate)
        {
			return ParseResignResponse(Hint(gamestate));
        }

        private List<PlayHint> ParseMoveHints(string hints, int[] dice, GameState gamestate)
        {
            if (hints.StartsWith("There are no legal moves."))
                return null;

			List<PlayHint> move_hints = new List<PlayHint>();

			//string[] hs = hints.Split(new char[] { ']' });
			string[] hs = hints.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
            string hint = "";

			// Skip the first one.
            for (int i = 1; i < hs.Length; i++)
            {
                hint = hs[i];
                int s = hint.IndexOf("-ply");
                if (s < 0)
                    continue;
                
                int e = hint.IndexOf("Eq.:");
                if (e < 0)
                {
                    e = hint.IndexOf("MWC");
                }
				if (e < 0)
				{
					e = hint.IndexOf("uity");
				}
                
                // Parse equity.
                int eq_e = hint.IndexOfAny(new char[] { '(', '\r' }, e + 4);
                double eq = double.Parse(hint.Substring(e + 4, eq_e - (e + 4)));

                hint = hint.Substring(s + 4, e - s - 4).Trim();

                // Console.WriteLine("GnuBg: " + hint);
				string[] moves_strings = hint.Split(new char[] { ' ' });
				int count = 0;
				List<Move> moves = new List<Move>();
				foreach (string move_string in moves_strings)
				{
					Move move = ParseMoveHint(move_string, out count, moves_strings.Length, dice, gamestate);
					for (int c = 0; c < count; c++)
						moves.Add(move);
				}

                PlayHint play_hint = new PlayHint(moves, eq);

                play_hint.Play.SortHiToLow();

                move_hints.Add(play_hint);
            }

			return move_hints;
        }

        // total_hints = the number of parseble move hints in the gnubg string, ie. 6/off contains one, bar/23 6/3 contains two
        private Move ParseMoveHint(string move_string, out int count, int total_hints, int[] dice, GameState gamestate)
        {
            // from/to
            // from/to(count)
            // 'bar'/to
            // 'bar'/to(count)
            // from/'off'
            // from/'off'(count)
			// from/to*/to*
			// from/to*/to*/to*
            // from/'off'

			// get count and remove it from the string
			count = 1;
			int count_start_index = move_string.IndexOf('(');
			if (count_start_index > 0)
			{
				int count_end_index = move_string.IndexOf(')');
				count = int.Parse(move_string.Substring(count_start_index + 1, count_end_index - count_start_index - 1));
				move_string = move_string.Substring(0, count_start_index);
			}

			string[] point_strings = move_string.Split(new char[] { '/' });

			List<int> points = new List<int>();
            HashSet<int> hitpoints = new HashSet<int>();
			bool is_number;
			foreach (string point_string in point_strings)
			{
                bool hit = point_string.Contains("*");
                string clean_point_string = point_string;
                if (hit)
                    clean_point_string = clean_point_string.Replace("*", "");
				
                is_number = true;
				foreach (char c in clean_point_string)
					if (!char.IsDigit(c))
					{
						is_number = false;
						break;
					}
				if (is_number)
                {
                    int point = int.Parse(clean_point_string) - 1;
			        points.Add(point);
                    if (hit)
                        hitpoints.Add(point);
                }
			}

			if (point_strings[0][0] == 'b')
				points.Insert(0, 24);

			if (point_strings[point_strings.Length - 1][0] == 'o')
				points.Insert(points.Count, -1);
			// points should be sorted from highest to lowest at this point


			// calculate the missing way points
			if (dice[0] != dice[1])
			{
				if (count == 1 && points.Count == 2)
				{
                    int distance = points[0] - points[1];
                    int bigger_die = Math.Max(dice[0], dice[1]);
                    int smaller_die = Math.Min(dice[0], dice[1]);
					if (distance > bigger_die)//(points[0] - points[1]) == (dice[0] + dice[1]))
					{
                        if (distance == (dice[0] + dice[1]))
                        {
                            // Must not contain hits, because gnubg tells about the hit points seperately.
                            if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, points[0] - bigger_die) >= 0)
                                points.Insert(0, (points[0] - bigger_die));
                            else
                                points.Insert(0, (points[0] - smaller_die));
                        }
                        else if (points[0] - bigger_die < smaller_die) // bearoff from 6 with 52, need first to do the smaller one.
                        {
							if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, points[0] - smaller_die) >= 0) // Was broken without this condition on _ _ 8x 2o 2o 2o with dice 43
								points.Insert(0, (points[0] - smaller_die));
							else if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, points[0] - smaller_die) == -1)
								points.Insert(0, (points[0] - bigger_die));
							else
							{
							}
                        }
                        else
                        {
                            throw new InvalidOperationException("Cannot handle this. Help!");
                        }
                        /*if (gamestate.Board.PointCount(gamestate.PlayerOnRoll, points[0] - bigger_die) > -2)
                            points.Insert(0, (points[0] - bigger_die));
                        else
                            points.Insert(0, (points[0] - smaller_die));*/
					} // This is for lonely bearoffs like |_ _ x _ x x| with 41, gnubg gives one of the hints as '4/off'.
                        // We want atleast 2 chequers on the field because otherwise adding a waypoint is unnecessary inbetween.
					else if (total_hints == 1 && gamestate.Board.FinishedCount(gamestate.PlayerOnRoll) <= 13 && points[1] == -1 && (points[0] - smaller_die >= 0) && gamestate.Board.PointCount(gamestate.PlayerOnRoll, points[0] - smaller_die) >= 0 && gamestate.Board.LastChequer(gamestate.PlayerOnRoll) <= points[0])
                    {
                        points.Insert(0, (points[0] - smaller_die));
                    }
                    else
                    {

                    }
				}
			}
			else
			{
				for (int i = 0; i < points.Count - 1; i++)
				{
                    // This was broken with bearoffs without this condition (ie. 6/off with 44, the below else condition should handle this situation)
                    /*if (points[i + 1] > -1)
                    {*/
                        int gaps = (points[i] - points[i + 1]) / dice[0];
                        int mod = (points[i] - points[i + 1]) % dice[0];
                        if (mod > 0)
                            gaps++;

                        if (gaps > 1)
                        {
                            for (int c = 1; c < gaps; c++)
                            {
                                points.Insert(i + 1, points[i] - dice[0]);
                                i++;
                            }
                        }
                    /*}
                    else
                    {
                        int gaps = (points[i] - points[i + 1]) / dice[0];
                        int mod = (points[i] - points[i + 1]) % dice[0];
                        if (mod > 0)
                        {
                            for (int c = 0; c < gaps; c++)
                            {
                                points.Insert(i + 1, points[i] - dice[0]);
                                i++;
                            }
                        }
                    }*/
				}
			}

            Move move = new Move();
            foreach (int point in points)
            {
                if (hitpoints.Contains(point))
                    move.AddHitPoint(point);
                else
                    move.AddPoint(point);
            }



			return move;
		}

        // file:///C:/Users/Administrator/Desktop/Projects/Gambling/gbg90_may/doc/gnubg.html#gnubg-playing_double
		private DoubleAction ParseDoubleAction(string hint)
		{
            int start = hint.LastIndexOf(":");
            string sub = hint.Substring(start + 1);
            if (sub.Contains("("))
                sub = sub.Remove(sub.LastIndexOf("("));

            if (sub.Contains("\r\n\r\n" + this.prompt))
                sub = sub.Remove(sub.LastIndexOf("\r\n\r\n" + this.prompt));

            if (sub.Contains(","))
                sub = sub.Remove(sub.IndexOf(","));
            
            sub = sub.Trim();

            return string2doubleaction[sub.ToLowerInvariant()];
		}

        private DoubleResponseHint ParseDoubleResponse(string hint)
        {
			//Console.WriteLine(hint);

			int start = hint.IndexOf("Double, pass") + 12;
			int end = hint.IndexOf("\r\n", start);
			string h = hint.Substring(start, end - start);
			if (h.Contains("("))
				h = h.Substring(0, h.IndexOf("("));

			double doubePassEq = double.Parse(h.Trim());

			start = hint.IndexOf("Double, take") + 12;
			end = hint.IndexOf("\r\n", start);
			h = hint.Substring(start, end - start);
			if (h.Contains("("))
				h = h.Substring(0, h.IndexOf("("));

			double doubeTakeEq = double.Parse(h.Trim());

            start = hint.LastIndexOf(":");
            string sub = hint.Substring(start + 1);
            if (sub.Contains("("))
                sub = sub.Remove(sub.LastIndexOf("("));

            if (sub.Contains("\r\n\r\n" + this.prompt))
                sub = sub.Remove(sub.LastIndexOf("\r\n\r\n" + this.prompt));

            if (sub.Contains(","))
                sub = sub.Substring(sub.IndexOf(",") + 1);

            sub = sub.Trim();

			return new DoubleResponseHint(string2doubleresponse[sub.ToLowerInvariant()], doubeTakeEq, doubePassEq);
        }

        private ResignResponseHint ParseResignResponse(string hint)
        {
            string correct = "Correct resign decision : ";
            int start = hint.IndexOf(correct) + correct.Length;
            string answer = hint.Substring(start).Trim();

            if (answer.Contains("\r\n\r\n" + this.prompt))
                answer = answer.Replace("\r\n\r\n" + this.prompt, "");

			return new ResignResponseHint(string2resignresponse[answer]);
        }

        [DllImport(@"C:\Users\Administrator\Desktop\Projects\Backgammon\Play65\Common\GRLibrary\Release\GnuBgDLL.dll")]
        private static extern string MatchID(
            int die1,
            int die2,
            int turn,
            int resigned,
            int doubled,
            int move,
            int cube_owner,
            int crawford,
            int match_to,
            int score1,
            int score2,
            int cube,
            int gamestate);

        private enum GnuBGGameState : int
        {
            GAME_NONE = 0, 
            GAME_PLAYING = 1, 
            GAME_OVER = 2, 
            GAME_RESIGNED = 3, 
            GAME_DROP = 4
        }

        /// <summary>
        /// TODO: Solve the "I'm sorry, but SetMatchID cannot handle positions where a double has been offered" bullshit.
        /// </summary>
        /// <param name="gamestate"></param>
        /// <returns></returns>
        public static string MatchID(GameState gamestate)
        {
            Console.WriteLine("Cube owner: " + gamestate.Cube.Owner);
            return MatchID(
                gamestate.Dice[0], 
                gamestate.Dice[1], 
                gamestate.PlayerOnTurn, 
                (int)gamestate.ResignOfferValue,
                (gamestate.OfferType == OfferType.Double) ? 1 : 0, 
                gamestate.PlayerOnRoll, 
                (gamestate.Cube.Owner >= 0) ? gamestate.Cube.Owner : 2,
                gamestate.IsCrawford ? 1 : 0, 
                gamestate.MatchTo, 
                gamestate.Score(0), 
                gamestate.Score(1), 
                gamestate.Cube.Value, 
                (int)GnuBGGameState.GAME_PLAYING
                );
        }

        public static string ToPositionID(Board board, int player_on_roll)
        {
            StringBuilder bit_string = new StringBuilder();

            for (int p = 0; p < 24; p++)
            {
                for (int c = 0; c < board.PointCount(player_on_roll, p); c++)
                    bit_string.Append("1");
                bit_string.Append("0");
            }

            for (int c = 0; c < board.CapturedCount(player_on_roll); c++)
                bit_string.Append("1");
            bit_string.Append("0");

            for (int p = 0; p < 24; p++)
            {
                for (int c = 0; c < board.PointCount(1 - player_on_roll, p); c++)
                    bit_string.Append("1");
                bit_string.Append("0");
            }

            for (int c = 0; c < board.CapturedCount(1 - player_on_roll); c++)
                bit_string.Append("1");
            bit_string.Append("0");

            while (bit_string.Length < 80)
                bit_string.Append("0");
            Console.WriteLine(bit_string.ToString().Length);

            return bit_string.ToString();
        }

        private static int[] powers2 = new int[] { 1, 2, 4, 8, 16, 32, 64 };
        private static string base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        public static Board BoardFromPositionID(string position_id, ref string error)
        {
            Board board = new Board();

            error = "";
            if (position_id.Length != 14)
            {
                if (position_id.Length < 12)
                    error = "Position ID length too short.";
                else
                    error = "Position ID length too long.";
                return null;
            }

            List<bool> bits2 = new List<bool>();
            List<bool> bits = new List<bool>();
            foreach (char c in position_id)
            {
                if (!base64.Contains(c))
                {
                    error = "Position ID contains an invalid character.";
                    return null;
                }

                int dec = base64.IndexOf(c);
                for (int i = 5; i >= 0; i--)
                {
                    bits.Add((powers2[i] & dec) == powers2[i]);
                    //bool bit = (powers2[i] & dec) == powers2[i];
                    //Console.Write(bit?1:0);

                    if (bits.Count == 8)
                    {
                        bits.Reverse();
                        bits2.AddRange(bits);
                        bits.Clear();
                    }
                }
            }

            int total_chequers = 0;
            int slot = 0;
            int player = 0;
            foreach (bool bit in bits2)
            {
                if (bit)
                {
                    if (slot == 24)
                        board.IncreaseCaptured(player);
                    else
                        board.AddToPoint(player, slot, 1);

                    total_chequers++;
                }
                else
                {
                    slot++;
                }

                if (slot == 25)
                {
                    board.SetFinished(player, 15 - total_chequers);

                    player = 1 - player;
                    total_chequers = 0;
                    slot = 0;
                }
            }
            //Console.WriteLine(board.ToString(0));

            return board;
        }

        public static GameState GnuBGIDToGameState(string gnubg_id, ref string error)
        {
            GameState gs = new GameState(GameType.Match);

            error = "";

            if (!gnubg_id.Contains(':') || gnubg_id.Length != 27) // 14 (position id) + 12 (match id) + 1 (':')
            {
                error = "Invalid GNUBG ID.";
                return null;
            }

            string[] tmp = gnubg_id.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (tmp.Length != 2)
                return null;

            string pos_id = tmp[0];
            string match_id = tmp[1];

            if (match_id.Length != 12)
            {
                if (match_id.Length < 12)
                    error = "Match ID length too short.";
                else
                    error = "Match ID length too long.";
                return null;
            }

            Board board = BoardFromPositionID(pos_id, ref error);
            if (board == null)
                return null;

            gs.Board = board;

            List<bool> bits2 = new List<bool>();
            List<bool> bits = new List<bool>();
            foreach (char c in match_id)
            {
                if (!base64.Contains(c))
                {
                    error = "Match ID contains an invalid character.";
                    return null;
                }

                int dec = base64.IndexOf(c);
                for (int i = 5; i >= 0; i--)
                {
                    bits.Add((powers2[i] & dec) == powers2[i]);
                    bool bit = (powers2[i] & dec) == powers2[i];
					Console.Write(bit ? 1 : 0);

                    if (bits.Count == 8)
                    {
                        bits.Reverse();
                        bits2.AddRange(bits);
                        bits.Clear();
                    }
                }
            }

            // 1-4 cube
            int cube = BoolListToInt(bits2.Take(4).Reverse());
            if (cube > 15)
            {
                error = "Invalid cube value.";
                return null;
            }

            // 5-6 cube owner
            int cube_owner = BoolListToInt(bits2.Skip(4).Take(2).Reverse());

            if (cube_owner == 3)
                gs.CenterCube();
            else
                gs.SetCube((int)Math.Pow(2, cube), cube_owner);

            // 7 player on roll
            gs.PlayerOnRoll = bits2[6] ? 1 : 0;

            // 8 crawford
            gs.IsCrawford = bits2[7];
            // TODO, check if moneygame and this is set, return null
            
            // 9-11 game state, 000 for no game started, 001 for playing a game, 010 if the game is over, 011 if the game was resigned, or 100 if the game was ended by dropping a cube.
            int game_state = BoolListToInt(bits2.Skip(8).Take(2).Reverse());
            if (game_state < 0 || game_state > 4)
            {
                error = "Invalid game state in Match ID.";
                return null;
            }
            // Do nothing with game state for now.
            // TODO: handle game_state this or not?
            
            // 12 player on turn
            gs.PlayerOnTurn = bits2[11] ? 1 : 0;

            // 13 double offered
            bool double_offered = bits2[12];

            // 14-15 resignation offered
            int resign_value = BoolListToInt(bits2.Skip(13).Take(2).Reverse());

            if (double_offered && resign_value > 0)
            {
                error = "Cannot offer double and resign at the same time.";
                return null;
            }
            
            int[] dice = new int[2];
            // 16-18 first die
            dice[0] = BoolListToInt(bits2.Skip(15).Take(3).Reverse());
            // 19-21 first die
            dice[1] = BoolListToInt(bits2.Skip(18).Take(3).Reverse());

            if ((dice[0] == 0 && dice[1] != 0) || (dice[1] == 0 && dice[0] != 0) || dice[0] > 6 || dice[1] > 6)
            {
                error = "Invalid dice.";
                return null;
            }

            if (dice[0] > 0 && double_offered)
            {
                error = "Cannot offer double when dice have been thrown.";
                return null;
            }
            
            gs.SetDice(dice[0], dice[1]);

            if (double_offered)
            {
                gs.OfferType = OfferType.Double;
                gs.SetCube(2 * gs.Cube.Value, gs.Cube.Owner); // This is because GameState was designed so that on double offer, the cube value stored in gamestate is the actual offer value. In contrast, Gnubg stores the non-doubled value.
            }

            if (resign_value > 0)
            {
                gs.OfferType = OfferType.Resign;
                gs.ResignOfferValue = (ResignValue)resign_value;
            }

            // 22-36 match length, zero indicates a money game
            int match_length = BoolListToInt(bits2.Skip(21).Take(15).Reverse());

            // TODO: What do we do with money game, ie. match length = 0?
            gs.MatchTo = match_length;

            // 37-51 player 0 score
            int score0 = BoolListToInt(bits2.Skip(36).Take(15).Reverse());
            // 52-66 player 1 score
            int score1 = BoolListToInt(bits2.Skip(51).Take(15).Reverse());

            gs.SetScore(score0, score1);

            /*Console.WriteLine();
            foreach (bool bit in bits2)
                Console.Write(bit ? 1 : 0);

            Console.WriteLine();*/
            
            //Console.WriteLine(gs);

            return gs;
        }

        public static int BoolListToInt(IEnumerable<bool> bools)
        {
            int n = 0;

            foreach (bool bit in bools)
            {
                n = n << 1;
                n = n | (bit ? 1 : 0);
            }

            return n;
        }
        // TODO:
        /*public static string ToMatchID(GameState gamestate)
        {
        }

        public static string ToGnuBgID(GameState gamestate)
        {

        }
         
        */
	}
}
