using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using GR.Gambling.Backgammon.Tools;
using GR.Win32;

namespace GR.Gambling.Backgammon.Venue
{
	public abstract class BGGameWindow
	{
		protected Window window;
        protected BGClient client;

		public string Title { get { return window.Title; } }

        public BGGameWindow(Window window, BGClient client)
        {
            this.window = window;
            this.client = client;
        }

		public virtual Bitmap Capture()
		{
			return window.Capture();
		}

        public override int GetHashCode()
        {
            return window.GetHashCode();
        }

        public abstract GameState GetGameState(Bitmap bitmap);

        public GameState GetGameState()
        {
			Bitmap bitmap = Capture();
            GameState gs = GetGameState(bitmap);

			bitmap.Dispose();

			return gs;
        }

        public BGClient Client { get { return client; } }

        public IntPtr Handle { get { return window.Handle; } }

        public Window Window { get { return window; } }

        public override string ToString()
        {
            return window.Title;
        }

        public abstract void MakeMove(Move move, GameState gamestate);

		/// <summary>
		/// This will undo last or undo all depending on which one is supported by the venue.
		/// </summary>
        public abstract void Undo();

        // TODO: provide decent implementation, think what player index here means
        public virtual int Rake(int player, int points) { return 0; }

        /// <summary>
        /// Checks the window and game status and raises the according events. Notice, the events are designed
        /// to support scenarios where one of the players is the observer.
        /// </summary>
        public abstract void Update();

        // public abstract bool UndoLast();

        /// <summary>
        /// Roll the dice.
        /// </summary>
        /// <returns></returns>
        public abstract bool Roll();

        /// <summary>
        /// Offer a double.
        /// </summary>
        /// <returns></returns>
        public abstract bool Double();

        /// <summary>
        /// Finish the current move turn.
        /// </summary>
        /// <returns></returns>
        public abstract bool Done();

        /// <summary>
        /// Offer a resign for a given value.
        /// </summary>
        /// <param name="resign_value"></param>
        /// <returns></returns>
        public abstract bool Resign(GameState gamestate, ResignValue resign_value);

        /// <summary>
        /// Close and leave the table.
        /// </summary>
        /// <returns></returns>
        public abstract bool Leave();

        /// <summary>
        /// Respond back to a double offer.
        /// </summary>
        /// <param name="double_response"></param>
        /// <returns></returns>
        public abstract bool RespondToDouble(DoubleResponse double_response);

        /// <summary>
        /// Respond back to a resign offer.
        /// </summary>
        /// <param name="resign_response"></param>
        /// <returns></returns>
        public abstract bool RespondToResign(ResignResponse resign_response);

        /// <summary>
        /// Respond back to a rematch offer.
        /// </summary>
        /// <param name="accept"></param>
        /// <returns></returns>
        public abstract bool RespondToRematch(bool accept);

        /// <summary>
        /// Writes a chat message to the game window chat.
        /// </summary>
        /// <param name="text">Text to write.</param>
        public abstract void Chat(string text);

        public delegate void ChatMessageEventHandler(string message);
        /// <summary>
        /// Assign to this event to receive notifications of new chat messages.
        /// </summary>
        public event ChatMessageEventHandler ChatMessage;
        protected virtual void OnChatMessage(string message)
        {
            if (ChatMessage != null)
                ChatMessage(message);
        }

        public delegate void SessionEndedEventHandler();
        public event SessionEndedEventHandler SessionEnded;
        protected virtual void OnSessionEnded()
        {
            if (SessionEnded != null)
                SessionEnded();
        }

        public delegate void TurnChangedEventHandler(GameState gamestate, DateTime stamp);
        /// <summary>
        /// This should be raised when a turn has been changed, but the dice haven't been rolled yet.
		/// Stamp is an optional parameter telling when the event was noticed.
        /// </summary>
        public event TurnChangedEventHandler TurnChanged;
        protected virtual void OnTurnChanged(GameState gamestate, DateTime stamp)
        {
            if (TurnChanged != null)
                TurnChanged(gamestate, stamp);
        }

		protected virtual void OnTurnChanged(GameState gamestate)
		{
			if (TurnChanged != null)
			{
				TurnChanged(gamestate, DateTime.UtcNow);
			}
		}

        public delegate void OppDiceRolledEventHandler(int[] dice, bool opening_roll);
        /// <summary>
        /// This should be raised when the opponent rolls the dice. The opening_roll indicates whether this is the first roll of the game.
        /// </summary>
        public event OppDiceRolledEventHandler OppDiceRolled;
        protected virtual void OnOppDiceRolled(int[] dice, bool opening_roll)
        {
            if (OppDiceRolled != null)
                OppDiceRolled(dice, opening_roll);
        }

        public delegate void DiceRolledEventHandler(GameState gamestate, DateTime stamp);
        /// <summary>
        /// This should be raised when the dice have been rolled.
        /// </summary>
        public event DiceRolledEventHandler DiceRolled;
        protected virtual void OnDiceRolled(GameState gamestate, DateTime stamp)
        {
            if (DiceRolled != null)
                DiceRolled(gamestate, stamp);
        }

		protected virtual void OnDiceRolled(GameState gamestate)
		{
			if (DiceRolled != null)
				DiceRolled(gamestate, DateTime.UtcNow);
		}

        public delegate void DoubleOfferedEventHandler(GameState gamestate, DateTime stamp);
        /// <summary>
        /// Venues that support getting the full gamestate on double offer should pass the gamestate,
        /// pass a null value otherwise, and the bot will try to figure itself out the current gamestate.
        /// </summary>
        public event DoubleOfferedEventHandler DoubleOffered;
        protected virtual void OnDoubleOffered(GameState gamestate, DateTime stamp)
        {
            if (DoubleOffered != null)
                DoubleOffered(gamestate, stamp);
        }

		protected virtual void OnDoubleOffered(GameState gamestate)
		{
			if (DoubleOffered != null)
				DoubleOffered(gamestate, DateTime.UtcNow);
		}

        public delegate void RematchOfferedEventHandler();
        public event RematchOfferedEventHandler RematchOffered;
        protected virtual void OnRematchOffered()
        {
            if (RematchOffered != null)
                RematchOffered();
        }

        public delegate void ResignOfferedEventHandler(ResignValue resign_value, DateTime stamp);
        public event ResignOfferedEventHandler ResignOffered;
        protected virtual void OnResignOffered(ResignValue resign_value, DateTime stamp)
        {
            if (ResignOffered != null)
                ResignOffered(resign_value, stamp);
        }

		protected virtual void OnResignOffered(ResignValue resign_value)
		{
			if (ResignOffered != null)
				ResignOffered(resign_value, DateTime.UtcNow);
		}

        public delegate void ResignOfferDeclinedEventHandler();
        public event ResignOfferDeclinedEventHandler ResignOfferDeclined;
        protected virtual void OnResignOfferDeclined()
        {
            if (ResignOfferDeclined != null)
                ResignOfferDeclined();
        }

        /// <summary>
        /// Should be thrown when the first dice have been rolled and it's the starting position.
        /// </summary>
        /// <param name="gamestate"></param>
        public delegate void NewGameStartedEventHandler(GameState gamestate, DateTime stamp);
        public event NewGameStartedEventHandler NewGameStarted;
        protected virtual void OnNewGameStarted(GameState gamestate, DateTime stamp)
        {
            if (NewGameStarted != null)
                NewGameStarted(gamestate, stamp);
        }

		protected virtual void OnNewGameStarted(GameState gamestate)
		{
			if (NewGameStarted != null)
				NewGameStarted(gamestate, DateTime.UtcNow);
		}

        /// <summary>
        /// Should be called when time taken on our decision has taken too long.
        /// </summary>
        public delegate void TimeoutEventHandler();
        public event TimeoutEventHandler Timeout;
        protected virtual void OnTimeout()
        {
            if (Timeout != null)
                Timeout();
        }
    }
}
