using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using GR.Gambling.Backgammon.Tools;

namespace GR.Gambling.Backgammon.Bot
{
    public class GnuBgHintModule : HintModule
    {
        private GnuBg gnubg;
		private GnuBg resign_gnubg; // For errorless resign evals.
        private string path;

        public GnuBgHintModule(string path)
            : base(new string[0])
        {
            this.path = path;
        }

		public GnuBgHintModule(string path, IEnumerable<string> settings)
			: base(settings)
		{
			this.path = path;
		}

        public override void Initialize()
        {
            gnubg = new GnuBg(path);

            gnubg.Start();

			foreach (string setting in settings)
				gnubg.Command(setting);

			resign_gnubg = new GnuBg(path);

			resign_gnubg.Start();
        }

		/* Alternative development for introducing errors to play. 
		private List<PlayHint> Errorify(GameState gamestate, List<PlayHint> play_hints)
		{
			if (play_hints.Count <= 1)
				return play_hints;

			double best_equity = play_hints[0].Equity;

			double diff = best_equity - play_hints[1].Equity;
			if (diff > 0.015)
				return play_hints;
			
			List<PlayHint> swapped = new List<PlayHint>();

			double doubtful = 0.04;
			double bad = 0.08;
			double very_bad = 0.16;

			// Handle special cases like 6 or 5-prime completion, bearoff, hitting

			for (int i = 1; i < play_hints.Count; i++)
			{
				diff = best_equity - play_hints[i].Equity;

			}

			return swapped;
		}*/

        public override List<PlayHint> PlayHint(GameState gamestate, int max_hints)
        {
			Stopwatch sw = Stopwatch.StartNew();

            if (gamestate.Board.IsPureRace() && gamestate.Board.IsBearoff(gamestate.PlayerOnRoll))
            {
                List<PlayHint> play_hints = gnubg.PlayHint(gamestate, max_hints);

                for (int i = 0; i < play_hints.Count; i++)
                {
                    bool all_bearoffs = true;
                    foreach (Move move in play_hints[i].Play)
                        if (!move.IsBearOff)
                        {
                            all_bearoffs = false;
                            break;
                        }

                    if (all_bearoffs)
                    {
                        PlayHint tmp = play_hints[i];
                        play_hints[i] = play_hints[0];
                        play_hints[0] = tmp;
                    }
                }

				Console.WriteLine("[Play Hints] " + sw.ElapsedMilliseconds + "ms.");

                return play_hints;
            }

            List<PlayHint> hints = gnubg.PlayHint(gamestate, max_hints);

			Console.WriteLine("[Play Hints] " + sw.ElapsedMilliseconds + "ms.");

			return hints;
        }

        public override PlayHint PlayHint(GameState gamestate)
		{
            if (gamestate.Board.IsPureRace() && gamestate.Board.IsBearoff(gamestate.PlayerOnRoll))
            {
                List<PlayHint> play_hints = gnubg.PlayHint(gamestate, 500);

                foreach (PlayHint play_hint in play_hints)
                {
                    bool all_bearoffs = true;
                    foreach (Move move in play_hint.Play)
                        if (!move.IsBearOff)
                        {
                            all_bearoffs = false;
                            break;
                        }

                    if (all_bearoffs)
                        return play_hint;
                }
                
                // Didn't find anything qualifying, just return the first.
                return play_hints[0];
            }

			Stopwatch sw = Stopwatch.StartNew();
            
			PlayHint hint = gnubg.PlayHint(gamestate);

			Console.WriteLine("[Play Hint] " + sw.ElapsedMilliseconds + "ms.");

			return hint;
        }

        public override DoubleHint DoubleHint(GameState gamestate)
        {
			Stopwatch sw = Stopwatch.StartNew();
            
			DoubleHint hint = null;
			if (gamestate.CanDouble())
				hint = gnubg.DoubleHint(gamestate);

			Console.WriteLine("[Double Hint] " + sw.ElapsedMilliseconds + "ms.");
			
			return hint;
        }

        public override DoubleResponseHint DoubleResponseHint(GameState gamestate)
        {
			Stopwatch sw = Stopwatch.StartNew();

            DoubleResponseHint hint =  gnubg.DoubleResponseHint(gamestate);

			Console.WriteLine("[DoubleResponse Hint] " + sw.ElapsedMilliseconds + "ms.");

			return hint;
        }

		public override ResignHint ResignHint(GameState gamestate, bool has_moved)
		{
			int lost_with_single, lost_with_gammon, lost_with_backgammon;

			if (gamestate.GameType == GameType.Match)
			{
				int opp_score = gamestate.Score(1 - gamestate.PlayerOnTurn);
				lost_with_backgammon = Math.Min(gamestate.MatchTo, opp_score + gamestate.Cube.Value * 3);
				lost_with_gammon = Math.Min(gamestate.MatchTo, opp_score + gamestate.Cube.Value * 2);
				lost_with_single = Math.Min(gamestate.MatchTo, opp_score + gamestate.Cube.Value);
			}
			else
			{
				lost_with_backgammon = gamestate.Cube.Centered ? gamestate.Stake : Math.Min(gamestate.Limit, gamestate.Stake * gamestate.Cube.Value * 3);
				lost_with_gammon = gamestate.Cube.Centered ? gamestate.Stake : Math.Min(gamestate.Limit, gamestate.Stake * gamestate.Cube.Value * 2);
				lost_with_single = gamestate.Cube.Centered ? gamestate.Stake : Math.Min(gamestate.Limit, gamestate.Stake * gamestate.Cube.Value);
			}

			if (!has_moved)
			{
				// Dice not rolled yet
				EvaluationInfo eval_info = resign_gnubg.Eval(gamestate);

				/*Console.WriteLine(eval_info.Win);
				Console.WriteLine(eval_info.WinGammon);
				Console.WriteLine(eval_info.WinBackgammon);
				Console.WriteLine(eval_info.LoseGammon);
				Console.WriteLine(eval_info.LoseBackgammon);
				Console.WriteLine(eval_info.Lose);*/

				if (eval_info.LoseBackgammon > 0.995)
					return new ResignHint(ResignValue.Backgammon);

				if (eval_info.LoseGammon > 0.995 && (eval_info.LoseBackgammon <= 0.0000001))// || (int)capped_loss >= 2))
					return new ResignHint(ResignValue.Gammon);

				if (eval_info.Lose > 0.995 && ((eval_info.LoseGammon <= 0.0000001 && eval_info.LoseBackgammon <= 0.0000001)))// || (int)capped_loss >= 1))
					return new ResignHint(ResignValue.Single);

				if (eval_info.Lose > 0.995 && lost_with_single == lost_with_gammon && lost_with_single == lost_with_backgammon && lost_with_gammon == lost_with_backgammon)
					return new ResignHint(ResignValue.Single);

				if (eval_info.Lose > 0.995 && lost_with_single == lost_with_gammon)
					return new ResignHint(ResignValue.Single);
			}
			else
			{
				// The optimal move has been made, change the turn to opp to see the effect of the move on eval
				int player_on_roll = gamestate.PlayerOnRoll;
				int player_on_turn = gamestate.PlayerOnTurn;
				int[] dice = new int[2];
				Array.Copy(gamestate.Dice, dice, 2);

				gamestate.ChangeTurn();

				EvaluationInfo eval_info = resign_gnubg.Eval(gamestate);

				/*Console.WriteLine(eval_info.Win);
				Console.WriteLine(eval_info.WinGammon);
				Console.WriteLine(eval_info.WinBackgammon);
				Console.WriteLine(eval_info.LoseGammon);
				Console.WriteLine(eval_info.LoseBackgammon);
				Console.WriteLine(eval_info.Lose);*/

				gamestate.SetDice(dice[0], dice[1]);
				gamestate.PlayerOnTurn = player_on_turn;
				gamestate.PlayerOnRoll = player_on_roll;

				if (eval_info.WinBackgammon > 0.995)
					return new ResignHint(ResignValue.Backgammon);

				if (eval_info.WinGammon > 0.995 && (eval_info.WinBackgammon <= 0.0000001))// || (int)capped_loss >= 2))
					return new ResignHint(ResignValue.Gammon);

				if (eval_info.Win > 0.995 && ((eval_info.WinGammon <= 0.0000001 && eval_info.WinBackgammon <= 0.0000001)))// || (int)capped_loss >= 1))
					return new ResignHint(ResignValue.Single);

				if (eval_info.Win > 0.995 && lost_with_single == lost_with_gammon && lost_with_single == lost_with_backgammon && lost_with_gammon == lost_with_backgammon)
					return new ResignHint(ResignValue.Single);

				if (eval_info.Win > 0.995 && lost_with_single == lost_with_gammon)
					return new ResignHint(ResignValue.Single);
			}

			return new ResignHint(ResignValue.None);
		}

        // This currently works when the dice haven't been rolled yet in sense that it assumes it has the chance to get 66.
        // Less than 66 could be sufficient enough to save us from the situation.
        /*public override ResignHint ResignHint(GameState gamestate, bool has_moved)
        {
            if (!gamestate.Board.IsPureRace())
            {
                return new ResignHint() { Value = ResignValue.None };
            }

            int player = gamestate.PlayerOnRoll;
            int opponent = 1 - player;

            // The maximum rolls the opponent will need in the worst-case scenario (rolling 2+1 = 3 everytime)
            int max_opponent_rolls = 0;

            // Bob Koca method http://www.bgonline.org/forums/webbbs_config.pl?read=39729
            int a = gamestate.Board.PointCount(opponent, 0);
            if (a < 0) a = 0;

            int b = 0;
            for (int point = 1; point <= 5; point += 2)
            {
                int count = gamestate.Board.PointCount(opponent, point);
                if (count > 0)
                    b += (point + 1) * count;
            }
            b /= 2;
            int c = gamestate.Board.PointCount(opponent, 4);
            if (c < 0) c = 0;

            int w = -1;
            int det = a - (b + c);
            if (det <= 0)
                w = 0;
            else
            {
                if (det % 2 == 0)
                    w = det / 2;
                else
                    w = 1 + (det + 1) / 2;
            }

            int pip_count = gamestate.Board.PipCount(opponent);
            max_opponent_rolls = (int)Math.Ceiling((pip_count + w) / 3.0);

            if (has_moved)
                max_opponent_rolls--;

            Console.WriteLine("max_rolls: " + max_opponent_rolls);

            if (gamestate.Board.FinishedCount(player) == 0)
            {
                // How many checkers we have in the backgammon zone (in the opponents homeboard)?
                int backgammon_count = 0;
                for (int point = 18; point < 24; point++)
                {
                    int count = gamestate.Board.PointCount(player, point);
                    if (count > 0)
                        backgammon_count += count;
                }

                if (backgammon_count > 0)
                {
                    // The minimum rolls the player will need to get out of the backgammon zone
                    // Assuming player rolls 66 everytime, he gets to remove 4 checkers out of the backgammon zone per roll.
                    int min_player_rolls = (int)Math.Ceiling(backgammon_count / 4.0);
                    Console.WriteLine("min: " + min_player_rolls);

                    // This assumes that we have the opportunity to roll or have rolled dice that will save us
                    if ((min_player_rolls - max_opponent_rolls) > 0)//if (min_player_rolls > max_opponent_rolls)
                        return new ResignHint() { Value = ResignValue.Backgammon };
                }

                int min_gammon_rolls = 0;
                for (int point = 6; point < 24; point++)
                {
                    int count = gamestate.Board.PointCount(player, point);
                    if (count > 0)
                        min_gammon_rolls += count * (int)Math.Ceiling((point + 1 - 6) / 6.0);
                }
                // The one we need to bearoff for not to get gammoned.
                min_gammon_rolls++;

                min_gammon_rolls = (int)Math.Ceiling(min_gammon_rolls / 4.0);



                if (min_gammon_rolls > max_opponent_rolls)
                    return new ResignHint() { Value = ResignValue.Gammon };
            }

            // Minimum number of rolls to bearoff everything.
            int min_rolls = 0;
            for (int point = 0; point < 24; point++)
            {
                int count = gamestate.Board.PointCount(player, point);
                if (count > 0)
                    min_rolls += count * (int)Math.Ceiling((point + 1) / 6.0);
            }

            min_rolls = (int)Math.Ceiling(min_rolls / 4.0);

            if (min_rolls > max_opponent_rolls)
                return new ResignHint() { Value = ResignValue.Single };

            return new ResignHint() { Value = ResignValue.None };
        }*/

        public override ResignResponseHint ResignResponseHint(GameState gamestate)
        {
			// TODO: Should add a gamestate.Rules.Jacoby flag to see if the Jacoby rule is in effect. We now assume it is (as it is most of the time with money games), but you never know.
			if (gamestate.GameType == GameType.Money && gamestate.Cube.Centered)
				return new ResignResponseHint(ResignResponse.Accept);

            return resign_gnubg.ResignResponseHint(gamestate);
        }

        public override void Close()
        {
            base.Close();

            gnubg.Exit();
			resign_gnubg.Exit();
        }

        // This is a request from the generic bot, return *this* or create new
        public override HintModule CreateNew()
        {
            return new GnuBgHintModule(path, this.settings);
        }
    }
}
