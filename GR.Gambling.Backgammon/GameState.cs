using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GR.Win32;

namespace GR.Gambling.Backgammon
{
    public enum DieSide : int
    {
        None = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6
    }

	public class GameState
	{
		private int[] dice;
        private Board board;
		private int player_on_roll;
		private int player_on_turn;
		private Cube cube;
		private OfferType offer;
		private ResignValue resign_offer_value;

        private int[] score;
        private int match_to;
        private bool crawford;

        private int stake;
        private int limit;

        private string[] names;

        private GameType game_type;

		public GameState(GameType game_type)
		{
			dice = new int[2];
			SetDice(0, 0);
            board = new Board();

			player_on_roll = -1;
			player_on_turn = -1;
			cube.Value = 1;
			cube.Owner = -1;
			offer = OfferType.None;
			resign_offer_value = ResignValue.None;
            score = new int[] { 0, 0 };
            match_to = 1;
            crawford = false;
            stake = 0;
            limit = 0;
            names = new string[] { "", "" };

            this.game_type = game_type;
		}

        public GameState Clone()
        {
            GameState gamestate = new GameState(this.game_type);
            gamestate.board = this.board.Clone();
            gamestate.crawford = this.crawford;
            gamestate.cube.Owner = this.cube.Owner;
            gamestate.cube.Value = this.cube.Value;
            gamestate.dice[0] = this.dice[0];
            gamestate.dice[1] = this.dice[1];
            gamestate.limit = this.limit;
            gamestate.match_to = this.match_to;
            gamestate.offer = this.offer;
            gamestate.player_on_roll = this.player_on_roll;
            gamestate.player_on_turn = this.player_on_turn;
            gamestate.resign_offer_value = this.resign_offer_value;
            gamestate.score[0] = this.score[0];
            gamestate.score[1] = this.score[1];
            gamestate.stake = this.stake;
            gamestate.names = this.names;

            return gamestate;
        }

		public Cube Cube { get { return cube; } set { cube = value; } }

		public void SetDice(int first_die, int second_die)
		{
			dice[0] = first_die;
			dice[1] = second_die;
		}

        /// <summary>
        /// Reset the dice to tray.
        /// </summary>
        public void Unroll()
        {
            dice[0] = 0;
            dice[1] = 0;
        }

        public void SetScore(int player0, int player1)
        {
            score[0] = player0;
            score[1] = player1;
        }

		public void SetCube(int value, int owner)
		{
			cube.Value = value;
			cube.Owner = owner;
		}

        public void CenterCube()
        {
            cube = GameState.CenteredCube;
        }

        /// <summary>
        /// Flips the current player on roll and sets the roller also as the player on turn, resets the dice.
        /// </summary>
        public void ChangeTurn()
        {
            Unroll();
            player_on_roll = 1 - player_on_roll;
            player_on_turn = player_on_roll;
        }

		public void SetResignOffer(ResignValue value)
		{
			offer = OfferType.Resign;
			resign_offer_value = value;
		}

		public void SetDoubleOffer()
		{
			offer = OfferType.Double;
		}

        public void Double()
        {
            if (!CanDouble())
            {
                Console.WriteLine("GameState: Trying to double when it is not possible.");
                throw new InvalidOperationException("Cannot double.");
            }

            offer = OfferType.Double;
            cube.Value *= 2;
            player_on_turn = 1 - player_on_turn;
        }

        public void Take()
        {
            if (offer != OfferType.Double)
                throw new InvalidOperationException("Cannot take when there's no double offer.");

            offer = OfferType.None;
            cube.Owner = player_on_turn;
            player_on_turn = 1 - player_on_turn;
        }

        public void Resign(ResignValue resign_value)
        {
            offer = OfferType.Resign;
            resign_offer_value = resign_value;
            player_on_turn = 1 - player_on_turn;
        }

        public void Reject()
        {
            if (offer != OfferType.Resign)
                throw new InvalidOperationException("Cannot reject when there's no resign offer.");

            offer = OfferType.None;
            player_on_turn = 1 - player_on_turn;
        }

		public bool HasOffer { get { return !(offer == OfferType.None); } }
        public OfferType OfferType { get { return offer; } set { offer = value; } }

        public ResignValue ResignOfferValue { get { return resign_offer_value; } set { resign_offer_value = value; } }

		public int[] Dice { get { return dice; } }
        public Board Board { get { return board; } set { board = value; } }
        public bool IsCrawford { get { return crawford; } set { crawford = value; } }
        public bool DiceRolled { get { return dice[0] > 0; } }

        public int PlayerOnRoll { get { return player_on_roll; } set { player_on_roll = value; } }
        public int PlayerOnTurn { get { return player_on_turn; } set { player_on_turn = value; } }

        public int MatchTo { get { return match_to; } set { match_to = value; } }

        public int Stake { get { return stake; } set { stake = value; } }
        public int Limit { get { return limit; } set { limit = value; } }

        public string GetName(int player)
        {
            return names[player];
        }

        public void SetName(int player, string name)
        {
            names[player] = name;
        }

        public GameType GameType { get { return game_type; } set { game_type = value; } }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">The player who's current match score is returned.</param>
        /// <returns></returns>
        public int Score(int player)
        {
            return score[player];
        }

        /// <summary>
        /// Tries to deduce if the player on turn can double.
        /// </summary>
        /// <returns></returns>
        public bool CanDouble()
        {
            if (crawford || (!cube.Centered && cube.Owner != player_on_roll) || cube.Value >= 64 || DiceRolled) // should be player on turn
                return false;

            // Match.. Could double, but pointless, even might cause harm?
            if (GameType == GameType.Match && Score(player_on_roll) + cube.Value >= match_to)
                return false;

            // Money game
            if (GameType == GameType.Money && limit > stake)
            {
                if (stake * cube.Value >= limit)
                    return false;
            }

            return true;
        }

		public bool HasFinished
		{
			get
			{
				if (board.FinishedCount(0) == 15 || board.FinishedCount(1) == 15)
					return true;

				return false;
			}
		}

		/*public Rules Rules { get; }*/

		public override string ToString()
		{
            string s = "";
            s += player_on_turn != -1? Board.ToString(player_on_turn) + Environment.NewLine : "";
            s += GameType.ToString();
            //if (GameType == GameType.Match)
            //    s += " Length: " + match_to + " Stake: " + stake + " Score: " + Score(player_on_turn) + "-" + Score(1 - player_on_turn) + " Crawford: " + IsCrawford.ToString() + Environment.NewLine;
            //if (GameType == GameType.Money)
            //    s += " Stake: " + stake + " Limit: " + limit + Environment.NewLine;
            s += " |Len {" + match_to + "} |Stakes {" + stake + "(" + limit + ")" + "} |Score {" + Score(player_on_turn) + "-" + Score(1 - player_on_turn) + "} |Crawford {" + IsCrawford.ToString() + "}" +  Environment.NewLine;

            if (DiceRolled)
                s += "D: " + Math.Max(dice[0], dice[1]) + "" + Math.Min(dice[0], dice[1]) + " ";
            s += "C {" + cube.Value + "@" + cube.Owner + "} ";
            s += "On roll {" + player_on_roll + "} |Turn {" + player_on_turn + "} |Offer {" + offer.ToString() + "} |Can double {" + CanDouble().ToString() + "}" + " |Resign value {" + (int)resign_offer_value + "}" + Environment.NewLine;

		/*	string s = player_on_turn != -1?Board.ToString(player_on_turn) + Environment.NewLine : "";
			s += "Cube: value " + cube.Value + " owner " + cube.Owner + Environment.NewLine;
			s += "Dice: " + dice[0] + " " + dice[1] + Environment.NewLine;
            s += "Player on roll: " + player_on_roll + Environment.NewLine;
            s += "Player on turn: " + player_on_turn + Environment.NewLine;
            s += "Offer: " + offer.ToString();*/
			return s;
		}

		public static string Serialize(GameState gs)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(gs.crawford.ToString());
			sb.Append(" " + gs.cube.Owner);
			sb.Append(" " + gs.cube.Value);

			sb.Append(" " + gs.dice[0]);
			sb.Append(" " + gs.dice[1]);

			sb.Append(" " + gs.game_type.ToString());

			sb.Append(" " + gs.limit);
			sb.Append(" " + gs.match_to);
			sb.Append(" " + gs.offer.ToString());
			sb.Append(" " + gs.player_on_roll);
			sb.Append(" " + gs.player_on_turn);
			sb.Append(" " + gs.resign_offer_value);
			sb.Append(" " + gs.score[0]);
			sb.Append(" " + gs.score[1]);
			sb.Append(" " + gs.stake);

			sb.Append("#" + Board.Serialize(gs.board));

			return sb.ToString();
		}

		public static GameState Deserialize(string s)
		{
			string[] ss = s.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

			GameState gs = new GameState(GameType.Match);

			gs.board = Board.Deserialize(ss[1]);

			ss = ss[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			
			int i=0;
			gs.crawford = bool.Parse(ss[i]);
			i++;

			gs.cube.Owner = int.Parse(ss[i]);
			i++;

			gs.cube.Value = int.Parse(ss[i]);
			i++;

			gs.dice[0] = int.Parse(ss[i]);
			i++;

			gs.dice[1] = int.Parse(ss[i]);
			i++;

			gs.game_type = (GameType)Enum.Parse(typeof(GameType), ss[i]);
			i++;

			gs.limit = int.Parse(ss[i]);
			i++;

			gs.match_to = int.Parse(ss[i]);
			i++;

			gs.offer = (OfferType)Enum.Parse(typeof(OfferType), ss[i]);
			i++;

			gs.player_on_roll = int.Parse(ss[i]);
			i++;
			
			gs.player_on_turn = int.Parse(ss[i]);
			i++;

			gs.resign_offer_value = (ResignValue)Enum.Parse(typeof(ResignValue), ss[i]);
			i++;

			gs.score[0] = int.Parse(ss[i]);
			i++;

			gs.score[1] = int.Parse(ss[i]);
			i++;

			gs.stake = int.Parse(ss[i]);

			return gs;
		}

        /// <summary>
        /// Compares this to another game state and determines if they are the same.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Equals(GameState state)
        {
            if (this.player_on_roll != state.PlayerOnRoll)
                return false;
            if (this.PlayerOnTurn != state.PlayerOnTurn)
                return false;
            if (this.Dice[0] != state.Dice[0] || this.Dice[1] != state.Dice[1])
                return false;
            if (this.Cube.Owner != state.Cube.Owner || this.Cube.Value != state.Cube.Value)
                return false;
            if (this.ResignOfferValue != state.ResignOfferValue)
                return false;

            return this.Board.Equals(state.Board);
        }

		public static int[] EmptyDice { get { return new int[2] { 0, 0 }; } }
		public static Cube CenteredCube { get { return new Cube(1, -1); } }
		public static int[] EmptyScore { get { return new int[2] { (int)DieSide.None, (int)DieSide.None }; } }
	}
}
