using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Gambling.Backgammon.Tools;

namespace GR.Gambling.Backgammon.Bot
{
    public abstract class HintModule
    {
		protected List<string> settings = new List<string>();

        public HintModule(IEnumerable<string> settings)
        {
			this.settings.AddRange(settings);
        }

        public abstract void Initialize();

        public abstract PlayHint PlayHint(GameState gamestate);
        public abstract List<PlayHint> PlayHint(GameState gamestate, int max_hints);
        public abstract DoubleHint DoubleHint(GameState gamestate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gamestate"></param>
        /// <param name="moved">If the player on roll has used his dice to move.</param>
        /// <returns></returns>
        public abstract ResignHint ResignHint(GameState gamestate, bool has_moved);
        public abstract DoubleResponseHint DoubleResponseHint(GameState gamestate);
        public abstract ResignResponseHint ResignResponseHint(GameState gamestate);

        public virtual void Close() { }

        public abstract HintModule CreateNew();
    }      
}
