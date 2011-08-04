using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

using GR.Gambling.Backgammon;
using GR.Gambling.Backgammon.Utils;
using GR.Gambling.Backgammon.Bot;
using GR.Gambling.Backgammon.Tools;
using GR.Gambling.Backgammon.HCI;

namespace Bopycat
{
    public partial class Form1 : Form
    {
        // Parameters
        GameType gameType = GameType.Match;
        
        // Match
        int matchTo = 1;

		private List<GameStateDoubleAction> doubles = new List<GameStateDoubleAction>();
		private List<GameStateTurnAction> turns = new List<GameStateTurnAction>();
		private List<GameStateMoveAction> moves = new List<GameStateMoveAction>();
		private List<GameStateResignAction> resigns = new List<GameStateResignAction>();

        private GnuBgHintModule gnubg = new GnuBgHintModule("gnubg/ipoint.exe");

        private GameState currentGameState;
		private GameState originalGameState;

        private Random random = new Random();

		private List<BoundingChequer> bound = new List<BoundingChequer>();

		private Bitmap lastRender = null;

		private TimedThinker thinker;//ComplexTimedThinker(0.5, 0.03);

		private GameStatus status = GameStatus.Playing;

		private bool resignOfferMade = false;

		private Stopwatch watch;

        public Form1()
        {
			List<GameStateDoubleAction> doubles = new List<GameStateDoubleAction>();
			List<GameStateTurnAction> turns = new List<GameStateTurnAction>();
			List<GameStateMoveAction> moves = new List<GameStateMoveAction>();

			foreach (string line in File.ReadAllLines("doubles.txt"))
			{
				string s = line.Replace(Environment.NewLine, "");
				GameStateDoubleAction a = (GameStateDoubleAction)GameStateDoubleAction.Deserialize(s);
				doubles.Add(a);
				Console.WriteLine(a.GameState.ToString() + " " + a.Time);
			}

			foreach (string line in File.ReadAllLines("turns.txt"))
			{
				string s = line.Replace(Environment.NewLine, "");
				GameStateTurnAction a = (GameStateTurnAction)GameStateTurnAction.Deserialize(s);
				turns.Add(a);

			}

			foreach (string line in File.ReadAllLines("moves.txt"))
			{
				string s = line.Replace(Environment.NewLine, "");
				GameStateMoveAction a = (GameStateMoveAction)GameStateMoveAction.Deserialize(s);
				moves.Add(a);
				Console.WriteLine(GameStateMoveAction.Serialize(a));
			}

			thinker = new NeuralThinker(moves, doubles, new GameStateResignAction[] { }, turns);


            InitializeComponent();

			this.MouseClick += new MouseEventHandler(Form1_MouseClick);
			this.MouseDoubleClick += new MouseEventHandler(Form1_MouseDoubleClick);

            labelLeftName.Text = "Player";
            labelRightName.Text = "Computer";
            labelLeftPips.Text = "Pips: 0";
            labelRightPips.Text = "Pips: 0";

            gnubg.Initialize();

            random.Next();

            StartNew();
        }

		void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Form1_MouseClick(sender, e);
		}


		List<TimedMove> moveHistory = new List<TimedMove>();
		Stack<Move> madeMoves = new Stack<Move>();
		List<Play> legalPlays = new List<Play>();
		List<int> unusedDice = new List<int>();
		Stack<int> usedDice = new Stack<int>();
		void Form1_MouseClick(object sender, MouseEventArgs e)
		{
			if (currentGameState.PlayerOnTurn == 0 && !currentGameState.HasOffer && currentGameState.DiceRolled && legalPlays.Count > 0 && madeMoves.Count < legalPlays[0].Count)
			{
				BoundingChequer chequer = ClickTest(e.Location);
				if (chequer == null)
					return;

				int[] dice = currentGameState.Dice;

				int from = chequer.Slot;

				if (dice[0] != dice[1])
				{
					int die = -1;
					if (unusedDice.Count == 2)
					{
						if (e.Button == MouseButtons.Left)
							die = Math.Max(unusedDice[0], unusedDice[1]);
						else if (e.Button == MouseButtons.Right)
							die = Math.Min(unusedDice[0], unusedDice[1]);
						else
							return;
					}
					else
						die = unusedDice[0];

					int to = chequer.Slot - die;
					if (to < -1)
						to = -1;

					Move move = new Move(from, to);

					if (currentGameState.Board.IsLegalMove(currentGameState.PlayerOnRoll, move, die))
					{
						if (to >= 0 && currentGameState.Board.PointCount(currentGameState.PlayerOnRoll, to) == -1)
							move.AddHitPoint(to);

						watch.Stop();
						
						/*Stack<Move> s = new Stack<Move>();
						foreach (Move m in madeMoves.Reverse())
							s.Push(m.Clone());*/

						List<TimedMove> h = new List<TimedMove>();
						foreach (TimedMove m in moveHistory)
							h.Add(new TimedMove(m));

						TimedMove tmove = new TimedMove(move, (int)watch.ElapsedMilliseconds, 0);
						
						moves.Add(new GameStateMoveAction(currentGameState.Clone(), originalGameState, watch.ElapsedMilliseconds, tmove, h));

						currentGameState.Board.MakeMove(currentGameState.PlayerOnRoll, move);
						madeMoves.Push(move);
						moveHistory.Add(tmove);

						unusedDice.Remove(die);
						usedDice.Push(die);

						UpdateControls();
					}
					else
					{
						if (unusedDice.Count == 2)
						{
							die = (dice[0] == die) ? dice[1] : dice[0];

							to = chequer.Slot - die;
							if (to < -1)
								to = -1;

							move = new Move(from, to);

							if (currentGameState.Board.IsLegalMove(currentGameState.PlayerOnRoll, move, die))
							{
								if (to >= 0 && currentGameState.Board.PointCount(currentGameState.PlayerOnRoll, to) == -1)
									move.AddHitPoint(to);

								watch.Stop();
								/*Stack<Move> s = new Stack<Move>();
								foreach (Move m in madeMoves.Reverse())
									s.Push(m.Clone());*/

								//moves.Add(new GameStateMoveAction(currentGameState.Clone(), watch.ElapsedMilliseconds, move.Clone(), s));

								List<TimedMove> h = new List<TimedMove>();
								foreach (TimedMove m in moveHistory)
									h.Add(new TimedMove(m));

								TimedMove tmove = new TimedMove(move, (int)watch.ElapsedMilliseconds, 0);

								moves.Add(new GameStateMoveAction(currentGameState.Clone(), originalGameState, watch.ElapsedMilliseconds, tmove, h));

								currentGameState.Board.MakeMove(currentGameState.PlayerOnRoll, move);
								madeMoves.Push(move);
								moveHistory.Add(tmove);

								unusedDice.Remove(die);
								usedDice.Push(die);

								UpdateControls();
							}
						}
						//else
						//Console.WriteLine("Illegal move " + from + "/" + to);
					}
				}
				else
				{
					int to = chequer.Slot - dice[0];
					if (to < -1)
						to = -1;
					Move move = new Move(from, to);

					if (currentGameState.Board.IsLegalMove(currentGameState.PlayerOnRoll, move, dice[0]))
					{
						if (to >= 0 && currentGameState.Board.PointCount(currentGameState.PlayerOnRoll, to) == -1)
							move.AddHitPoint(to);

						watch.Stop();

						/*Stack<Move> s = new Stack<Move>();
						foreach (Move m in madeMoves.Reverse())
							s.Push(m.Clone());

						moves.Add(new GameStateMoveAction(currentGameState.Clone(), watch.ElapsedMilliseconds, move.Clone(), s));*/

						List<TimedMove> h = new List<TimedMove>();
						foreach (TimedMove m in moveHistory)
							h.Add(new TimedMove(m));

						TimedMove tmove = new TimedMove(move, (int)watch.ElapsedMilliseconds, 0);

						moves.Add(new GameStateMoveAction(currentGameState.Clone(), originalGameState, watch.ElapsedMilliseconds, tmove, h));

						currentGameState.Board.MakeMove(currentGameState.PlayerOnRoll, move);
						madeMoves.Push(move);
						moveHistory.Add(tmove);

						unusedDice.Remove(dice[0]);
						usedDice.Push(dice[0]);

						UpdateControls();
					}
				}
				// Fix problem when making an illegal move the watch resets itsefl
			}
		}

		private BoundingChequer ClickTest(Point point)
		{
			Point p = new Point(point.X - 100, point.Y - 100);
			foreach (BoundingChequer chequer in bound)
			{
				if (chequer.IsHit(p))
					return chequer;
			}

			return null;
		}

		private void SetTurn(int player)
		{
			if (player == 0)
			{
				labelLeftName.BackColor = Color.BlanchedAlmond;
				labelRightName.BackColor = Color.Empty;
			}
			else
			{
				labelLeftName.BackColor = Color.Empty;
				labelRightName.BackColor = Color.BlanchedAlmond;
			}
		}

        private void StartNew()
        {
            currentGameState = new GameState(GameType.Money);
			currentGameState.Stake = 1;
			currentGameState.Limit = 8;
            currentGameState.Board.InitializeBoard(BackgammonVariation.Standard);
            currentGameState.PlayerOnRoll = 0;
            currentGameState.PlayerOnTurn = 0;

            int[] startRoll = new int[2] { 0, 0 };
            while (startRoll[0] == startRoll[1])
            {
                startRoll[0] = random.Next(1, 7);
                startRoll[1] = random.Next(1, 7);
            }
            currentGameState.SetDice(startRoll[0], startRoll[1]);

            currentGameState.PlayerOnRoll = currentGameState.PlayerOnTurn = random.Next(2);
			SetTurn(currentGameState.PlayerOnTurn);

			if (currentGameState.PlayerOnTurn == 1)
				HandleAI();
			else
			{
				unusedDice.AddRange(currentGameState.Dice);

				if (currentGameState.Dice[0] == currentGameState.Dice[1])
					unusedDice.AddRange(currentGameState.Dice);

				legalPlays = currentGameState.Board.LegalPlays(currentGameState.PlayerOnRoll, currentGameState.Dice);

				originalGameState = currentGameState.Clone();
			}

            UpdateControls();
        }

        private void UpdateControls()
        {
            labelLeftPips.Text = "Pips: " + currentGameState.Board.PipCount(0);
            labelRightPips.Text = "Pips: " + currentGameState.Board.PipCount(1);

			SetTurn(currentGameState.PlayerOnTurn);

			if (currentGameState.Board.FinishedCount(0) == 15 || currentGameState.Board.FinishedCount(1) == 15 || status == GameStatus.GameOver)
			{
				Render();
				this.Refresh();

				MessageBox.Show("Gameover");

				return;
			}

			if (madeMoves.Count > 0)
			{
				buttonUndo.Enabled = true;
				buttonUndo.Visible = true;
			}

			if (currentGameState.PlayerOnTurn == 0 && currentGameState.PlayerOnRoll == 0 && currentGameState.CanDouble())
			{
				buttonDouble.Enabled = true;
				buttonDouble.Visible = true;
			}
			else
			{
				buttonDouble.Enabled = false;
				buttonDouble.Visible = false;

				buttonDouble.Refresh();
			}

			if (currentGameState.PlayerOnTurn == 0 && currentGameState.PlayerOnRoll == 0 && currentGameState.DiceRolled && legalPlays.Count > 0 && legalPlays[0].Count > 0)
			{
				if (madeMoves.Count == legalPlays[0].Count)
				{
					buttonDone.Enabled = true;
					buttonDone.Visible = true;
				}
				else
				{
					buttonDone.Enabled = false;
					buttonDone.Visible = false;
				}
			}

            if (currentGameState.PlayerOnTurn == 0 && !currentGameState.DiceRolled)
            {
				watch = Stopwatch.StartNew();

				textBoxLog.Text += "Watch reset for move " + Environment.NewLine;
				textBoxLog.SelectionStart = textBoxLog.Text.Length;
				textBoxLog.ScrollToCaret();

                buttonRoll.Enabled = true;
                buttonRoll.Visible = true;
            }

			if (currentGameState.PlayerOnRoll == 0 && currentGameState.PlayerOnTurn == 0 && currentGameState.DiceRolled)
			{
				watch = Stopwatch.StartNew();

				textBoxLog.Text += "Watch reset for move " + Environment.NewLine;
				textBoxLog.SelectionStart = textBoxLog.Text.Length;
				textBoxLog.ScrollToCaret();
			}

            if (currentGameState.PlayerOnTurn == 1)
                HandleAI();

			Render();

			this.Refresh();
            //this.Invalidate();
        }

		private void Render()
		{
			lastRender = GameStateRenderer.Render(0, 700, 400, currentGameState, ref bound);
		}

        private void HandleAI()
        {
            if (currentGameState.PlayerOnTurn == 1)
            {
                if (currentGameState.HasOffer)
                {
                }
                else
                {
                    if (currentGameState.DiceRolled)
                    {
						Thread.Sleep(thinker.TimeOnDiceRolled(currentGameState));

                        List<PlayHint> hints = gnubg.PlayHint(currentGameState, 500);

						if (hints != null)
						{
							TimedPlay play = thinker.TimedPlayOnRoll(currentGameState, hints, GR.Gambling.Backgammon.Venue.VenueUndoMethod.UndoLast);

							Stack<Move> oppMadeMoves = new Stack<Move>();
							//currentGameState.Board.MakePlay(1, hints[0].Play);
							foreach (TimedMove move in play)
							{
								Thread.Sleep(move.WaitBefore);

								if (move.IsUndo)
								{
									Move m = oppMadeMoves.Pop();
									currentGameState.Board.UndoMove(1, m);
								}
								else
								{
									oppMadeMoves.Push(move);
									currentGameState.Board.MakeMove(1, move);
								}

								Render();
								this.Refresh();

								//this.Invalidate();

								Thread.Sleep(move.WaitAfter);
							}
						}

						if (hints == null)
							Thread.Sleep(1000);

						Thread.Sleep(500);

						currentGameState.ChangeTurn();
                    }
                    else
                    {
						DoubleHint doubleHint = gnubg.DoubleHint(currentGameState);
						Thread.Sleep(thinker.TimeOnTurnChanged(currentGameState, doubleHint, null));

                        if (currentGameState.CanDouble())
                        {
                            if (doubleHint.Action == DoubleAction.Double)
                            {
								watch = Stopwatch.StartNew();

								DialogResult res = MessageBox.Show("Your opponent doubles..", "Double offer", MessageBoxButtons.YesNo);

								watch.Stop();

								GameState gs = currentGameState.Clone();
								gs.Double();
								GameStateDoubleAction gsda = new GameStateDoubleAction(gs, watch.ElapsedMilliseconds, (res == DialogResult.Yes) ? DoubleResponse.Take : DoubleResponse.Pass);
								doubles.Add(gsda);

								textBoxLog.Text += "Double response added " + gs.OfferType.ToString() + " " + watch.ElapsedMilliseconds + "ms." + Environment.NewLine;
								textBoxLog.SelectionStart = textBoxLog.Text.Length;
								textBoxLog.ScrollToCaret();

								if (res == DialogResult.Yes)
								{
									currentGameState.SetCube(currentGameState.Cube.Value * 2, 0);

									Render();
									this.Refresh();
								}
								else
								{
									status = GameStatus.GameOver;
								}

                                UpdateControls();

                                return;
                            }
                        }

						if (!resignOfferMade)
						{
							ResignHint resignHint = gnubg.ResignHint(currentGameState, false);
							if (resignHint.Value != ResignValue.None)
							{
								watch = Stopwatch.StartNew();
								DialogResult res = MessageBox.Show("Your opponent wants to resign for " + resignHint.Value.ToString(), "Resign offer", MessageBoxButtons.YesNo);
								watch.Stop();

								GameState gs = currentGameState.Clone();
								gs.Resign(resignHint.Value);
								GameStateResignAction gsra = new GameStateResignAction(gs, watch.ElapsedMilliseconds, (res == DialogResult.Yes) ? ResignResponse.Accept : ResignResponse.Reject);
								resigns.Add(gsra);

								textBoxLog.Text += "Resign response added " + gs.OfferType.ToString() + " " + watch.ElapsedMilliseconds + "ms." + Environment.NewLine;
								textBoxLog.SelectionStart = textBoxLog.Text.Length;
								textBoxLog.ScrollToCaret();

								if (res == DialogResult.Yes)
								{
									status = GameStatus.GameOver;

									Render();
									this.Refresh();
								}
								else
								{
									resignOfferMade = true;
								}

								UpdateControls();

								return;
							}
						}

                        // Roll
                        currentGameState.SetDice(random.Next(1, 7), random.Next(1, 7));
                    }
                }

                UpdateControls();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
			e.Graphics.DrawImage(lastRender, 100, 100);

            base.OnPaint(e);
        }

        private void buttonRoll_Click(object sender, EventArgs e)
        {
			watch.Stop();

			GameStateTurnAction gsta = new GameStateTurnAction(currentGameState.Clone(), watch.ElapsedMilliseconds, TurnAction.Roll);

			turns.Add(gsta);

			textBoxLog.Text += "Turn action added" + " " + watch.ElapsedMilliseconds + "ms." + Environment.NewLine;
			textBoxLog.SelectionStart = textBoxLog.Text.Length;
			textBoxLog.ScrollToCaret();

            buttonRoll.Enabled = false;
            buttonRoll.Visible = false;

            currentGameState.SetDice(random.Next(1, 7), random.Next(1, 7));
			originalGameState = currentGameState.Clone();
			
			legalPlays = currentGameState.Board.LegalPlays(currentGameState.PlayerOnRoll, currentGameState.Dice);

			if (legalPlays.Count == 0)
			{
				this.Render();
				this.Refresh();

				legalPlays.Clear();
				madeMoves.Clear();
				unusedDice.Clear();

				currentGameState.ChangeTurn();

				Thread.Sleep(1000);

				UpdateControls();

				return;
			}

			unusedDice.AddRange(currentGameState.Dice);

			if (currentGameState.Dice[0] == currentGameState.Dice[1])
				unusedDice.AddRange(currentGameState.Dice);


            UpdateControls();
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
			watch.Stop();
			/*Stack<Move> s = new Stack<Move>();
			foreach (Move m in madeMoves.Reverse())
				s.Push(m.Clone());

			moves.Add(new GameStateMoveAction(currentGameState.Clone(), watch.ElapsedMilliseconds, null, s));
			*/
			List<TimedMove> h = new List<TimedMove>();
			foreach (TimedMove m in moveHistory)
				h.Add(new TimedMove(m));

			moves.Add(new GameStateMoveAction(currentGameState.Clone(), originalGameState, watch.ElapsedMilliseconds, null, h));
					
			
            buttonDone.Enabled = false;
            buttonDone.Visible = false;

			buttonUndo.Enabled = false;
			buttonUndo.Visible = false;

			legalPlays.Clear();
			madeMoves.Clear();
			unusedDice.Clear();
			usedDice.Clear();
			moveHistory.Clear();

            currentGameState.ChangeTurn();

			Render();

			this.Refresh();

            UpdateControls();
        }

		private void buttonUndo_Click(object sender, EventArgs e)
		{
			if (madeMoves.Count > 0)
			{
				watch.Stop();

				List<TimedMove> h = new List<TimedMove>();
				foreach (TimedMove m in moveHistory)
					h.Add(new TimedMove(m));

				TimedMove tmove = TimedMove.CreateUndoMove((int)watch.ElapsedMilliseconds, 0);
				moves.Add(new GameStateMoveAction(currentGameState.Clone(), originalGameState, watch.ElapsedMilliseconds, tmove, h));

				moveHistory.Add(tmove);
	
				Move move = madeMoves.Pop();

				currentGameState.Board.UndoMove(currentGameState.PlayerOnRoll, move);

				int[] dice = currentGameState.Dice;

				unusedDice.Add(usedDice.Pop());
			}
			
			if (madeMoves.Count == 0)
			{
				buttonUndo.Visible = false;
				buttonUndo.Enabled = false;
			}

			UpdateControls();
		}

		private void buttonDouble_Click(object sender, EventArgs e)
		{
			watch.Stop();

			GameStateTurnAction gsta = new GameStateTurnAction(currentGameState.Clone(), watch.ElapsedMilliseconds, TurnAction.Double);

			turns.Add(gsta);

			textBoxLog.Text += "Turn action added" + " " + watch.ElapsedMilliseconds + "ms." + Environment.NewLine;
			textBoxLog.SelectionStart = textBoxLog.Text.Length;
			textBoxLog.ScrollToCaret();

			currentGameState.Double();

			Render();
			this.Refresh();

			DoubleResponseHint hint = gnubg.DoubleResponseHint(currentGameState);

			Thread.Sleep(thinker.TimeOnDoubleOffer(currentGameState, hint));

			if (hint.Response == DoubleResponse.Pass)
			{
				status = GameStatus.GameOver;
				return;
			}

			if (hint.Response == DoubleResponse.Take)
			{
				currentGameState.Take();
			}

			UpdateControls();
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			foreach (GameStateResignAction gsra in resigns)
				sb.AppendLine(GameStateResignAction.Serialize(gsra));
			File.AppendAllText("resigns.txt", sb.ToString());

			sb = new StringBuilder();

			foreach (GameStateDoubleAction gsda in doubles)
				sb.AppendLine(GameStateDoubleAction.Serialize(gsda));
			File.AppendAllText("doubles.txt", sb.ToString());

			sb = new StringBuilder();
			foreach (GameStateTurnAction gsta in turns)
				sb.AppendLine(GameStateTurnAction.Serialize(gsta));
			File.AppendAllText("turns.txt", sb.ToString());

			sb = new StringBuilder();
			foreach (GameStateMoveAction gsma in moves)
				sb.AppendLine(GameStateMoveAction.Serialize(gsma));
			File.AppendAllText("moves.txt", sb.ToString());
		}
    }

	public enum GameStatus
	{
		GameOver,
		Playing
	}
}
