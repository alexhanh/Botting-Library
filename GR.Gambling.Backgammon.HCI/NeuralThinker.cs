using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using GR.Gambling.Backgammon.HCI;
using GR.Gambling.Backgammon.Tools;
using GR.Gambling.Backgammon.Bot;
using GR.Gambling.Backgammon.Venue;

namespace GR.Gambling.Backgammon.HCI
{
	class Vector
	{
		private double[] inputs;

		public IEnumerator GetEnumerator()
		{
			return inputs.GetEnumerator();
		}

		public double Distance { get; set; }

		public double this[int i]
		{
			get { return inputs[i]; }
			set { inputs[i] = value; }
		}

		public Vector(params double[] inputs)
		{
			this.inputs = inputs;
		}

		public static double ComputeDistance(Vector v1, Vector v2)
		{
			double distance = 0.0;
			double diff = 0.0;
			for (int i = 0; i < v1.inputs.Length; i++)
			{
				diff = v1[i] - v2[i];
				distance += diff * diff;
			}

			return System.Math.Sqrt(distance);
		}
	}

	public class NeuralThinker : TimedThinker
	{
		Random random = new Random();

		// [0] take, [1] pass, [2] abs(take-pass)
		//List<KeyValuePair<double[], int>> doubles = new List<KeyValuePair<double[], int>>();

		// [0] doublediff (abs(min{ double/take, double/pass} - nodouble)), [1] wagerratio (min{cube*stake/limit}, 1), [2] 100.0 => can double, 0.0 => can't double
		//List<KeyValuePair<double[], int>> turns = new List<KeyValuePair<double[], int>>();

		List<KeyValuePair<GameStateAction, Vector>> doubles = new List<KeyValuePair<GameStateAction, Vector>>();
		List<KeyValuePair<GameStateAction, Vector>> resigns = new List<KeyValuePair<GameStateAction, Vector>>();
		List<KeyValuePair<GameStateAction, Vector>> turns = new List<KeyValuePair<GameStateAction, Vector>>();
		List<KeyValuePair<GameStateAction, Vector>> moves = new List<KeyValuePair<GameStateAction, Vector>>();

		private static int Compare(KeyValuePair<GameStateAction, Vector> v1, KeyValuePair<GameStateAction, Vector> v2)
		{
			return v1.Value.Distance.CompareTo(v2.Value.Distance);
		}

		public NeuralThinker(IEnumerable<GameStateMoveAction> moves,
							IEnumerable<GameStateDoubleAction> doubles,
							IEnumerable<GameStateResignAction> resigns,
							IEnumerable<GameStateTurnAction> turns)
		{
			GnuBgHintModule gnubg = new GnuBgHintModule(@"gnubg/ipoint.exe");

			gnubg.Initialize();

			foreach (GameStateMoveAction gsma in moves)
			{
				List<PlayHint> hints = gnubg.PlayHint(gsma.Original, 500);

				this.moves.Add(new KeyValuePair<GameStateAction, Vector>(gsma, ToMoveInput(gsma, hints)));
			}

			foreach (GameStateDoubleAction gsda in doubles)
			{
				DoubleResponseHint hint = gnubg.DoubleResponseHint(gsda.GameState);

				//Vector vector = new Vector(hint.TakeEq, hint.PassEq, System.Math.Abs(hint.PassEq - hint.TakeEq));
				this.doubles.Add(new KeyValuePair<GameStateAction, Vector>(gsda, ToDoubleInput(gsda.GameState, hint)));
			}

			foreach (GameStateResignAction gsra in resigns)
			{
				ResignResponseHint hint = gnubg.ResignResponseHint(gsra.GameState);

				Vector vector = new Vector();

				this.resigns.Add(new KeyValuePair<GameStateAction, Vector>(gsra, vector));
			}

			foreach (GameStateTurnAction gsta in turns)
			{
				DoubleHint hint = null;
				if (gsta.GameState.CanDouble())
					hint = gnubg.DoubleHint(gsta.GameState);

				this.turns.Add(new KeyValuePair<GameStateAction, Vector>(gsta, ToTurnInput(gsta.GameState, hint)));
			}

			random.Next();
			random.Next();
		}

		private Vector ToMoveInput(GameStateMoveAction gsma, List<PlayHint> hints)
		{
			double slotDistance = 0.0;
			int movesMade = gsma.GetTotalMadeMoves();
			
			
			return new Vector();
		}

		private Vector ToDoubleInput(GameState gs, DoubleResponseHint hint)
		{
			return new Vector(hint.TakeEq, hint.PassEq, System.Math.Abs(hint.PassEq - hint.TakeEq));
		}

		private Vector ToTurnInput(GameState gs, DoubleHint hint)
		{
			double ratio = 0.0;
			Vector vector = null;

			if (gs.CanDouble())
			{
				if (gs.GameType == GameType.Match)
				{
					ratio = System.Math.Min(
						(System.Math.Max(gs.Score(0), gs.Score(1)) + gs.Cube.Value) / (double)gs.MatchTo,
						1.0);
				}

				if (gs.GameType == GameType.Money)
				{
					ratio = System.Math.Min(gs.Cube.Value * gs.Stake / (double)gs.Limit, 1.0);
				}

				vector = new Vector(System.Math.Abs(System.Math.Min(hint.DoubleTakeEq, hint.DoublePassEq) - hint.NoDoubleEq),
										ratio, 100.0);
			}
			else
			{
				vector = new Vector(0.0, ratio, 0.0);
			}

			return vector;
		}

		public override int TimeOnTurnChanged(GameState gamestate, DoubleHint doubleHint, ResignHint resignHint)
		{
			Vector v = ToTurnInput(gamestate, doubleHint);

			foreach (KeyValuePair<GameStateAction, Vector> gv in turns)
			{
				gv.Value.Distance = Vector.ComputeDistance(gv.Value, v);
			}

			turns.Sort(Compare);


			foreach (KeyValuePair<GameStateAction, Vector> gv in turns)
			{
				Console.WriteLine(gv.Key.Time + " " + gv.Value.Distance);
			}
			Console.WriteLine(turns.Count);

			return (int)turns[0].Key.Time;
		}

		public override int TimeOnDiceRolled(GameState gamestate)
		{
			return 0;
		}

		public override int TimeOnStartingDiceRolled(GameState gamestate)
		{
			return 0;
		}

		public override TimedPlay TimedPlayOnRoll(GameState gamestate, Play play)
		{
			throw new NotImplementedException();
		}

		public override TimedPlay TimedPlayOnRoll(GameState gamestate, List<PlayHint> hints, VenueUndoMethod undo_method)
		{
			TimedPlay play = new TimedPlay();
			
			foreach (Move move in hints[0].Play)
			{
				play.Add(new TimedMove(move, (int)this.moves[random.Next(this.moves.Count)].Key.Time, 0));
			}
				

			return play;
		}

		public override int TimeOnResignOffer(GameState gamestate, ResignResponseHint hint)
		{
			if (hint.Response == ResignResponse.Reject)
			{
				if (random.Next(5) == 0)
					return random.Next(2000, 4000);

				return random.Next(1000, 3000);
			}

			if (random.Next(5) == 0)
				return random.Next(2000, 4000);

			return random.Next(1000, 2500);
		}

		// Where's the randomness? Possibilities:
		// Category the actions and pick one randomly from the cat's set of actions (or approximate by some probability distribution etc.)
		// Sort a list from best to worst distances and pick randomly from that list (top N candidates - uniform random or normal)

		public override int TimeOnDoubleOffer(GameState gamestate, DoubleResponseHint hint)
		{
			Vector v = ToDoubleInput(gamestate, hint);

			foreach (KeyValuePair<GameStateAction, Vector> gv in doubles)
			{
				gv.Value.Distance = Vector.ComputeDistance(gv.Value, v);
			}

			doubles.Sort(Compare);
						
			return (int)doubles[0].Key.Time;
		}

		private Dictionary<int, Point> slot2point = new Dictionary<int, Point> 
		{
			{ 12, new Point(0,11) }, { 13, new Point(1,11) }, { 14, new Point(2,11) },
			{ 15, new Point(3,11) }, { 16, new Point(4,11) }, { 17, new Point(5,11) },
			{ 18, new Point(7,11) }, { 19, new Point(8,11) }, { 20, new Point(9,11) },
			{ 21, new Point(10,11)}, { 22, new Point(11,11)}, { 23, new Point(12,11)},

			{ 11, new Point(0,0) },  { 10, new Point(1,0)  }, { 9, new Point(2,0) },
			{ 8, new Point(3,0)  },  { 7, new Point(4,0)   }, { 6, new Point(5,0) },
			{ 5, new Point(7,0)  },  { 4, new Point(8,0)   }, { 3, new Point(9,0) },
			{ 2, new Point(10,0) },  { 1, new Point(11,0)  }, { 0, new Point(12,0) },

			{ 24, new Point(6, 7) }
		};

		public double SlotDistance(int slot1, int slot2)
		{
			Point p1 = slot2point[slot1];
			Point p2 = slot2point[slot2];

			double xdiff = (double)(p1.X - p2.X);
			double ydiff = (double)(p1.Y - p2.Y);

			return System.Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
		}

		public override int TimeOnRematchOffer()
		{
			if (random.Next(5) == 0)
				return random.Next(2000, 4000);

			if (random.Next(10) == 0)
				return random.Next(3000, 8000);

			return random.Next(1000, 3000);
		}
	}
}
