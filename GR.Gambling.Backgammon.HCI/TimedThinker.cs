using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;
using GR.Gambling.Backgammon.Venue;

namespace GR.Gambling.Backgammon.HCI
{
    public abstract class TimedThinker
    {
        public abstract int TimeOnTurnChanged(GameState gamestate, DoubleHint doubleHint, ResignHint resignHint);
        public abstract int TimeOnDiceRolled(GameState gamestate);
        public abstract int TimeOnStartingDiceRolled(GameState gamestate);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gamestate"></param>
        /// <param name="play">The actual play we want to time and input.</param>
        /// <returns></returns>
        public abstract TimedPlay TimedPlayOnRoll(GameState gamestate, Play play);
        //public abstract TimedPlay TimedPlayOnRoll(GameState gamestate, List<PlayHint> hints);
		public abstract TimedPlay TimedPlayOnRoll(GameState gamestate, List<PlayHint> hints, VenueUndoMethod undo_method);
        public abstract int TimeOnResignOffer(GameState gamestate, ResignResponseHint hint);
        public abstract int TimeOnDoubleOffer(GameState gamestate, DoubleResponseHint hint);
        public abstract int TimeOnRematchOffer();

    }
}
