using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Win32;

namespace GR.Gambling.Backgammon
{
    public class JoinOffer : GameOffer
    {
        private readonly string joiner;
        private readonly Window window;

        public string Joiner { get { return joiner; } }
        public Window Window { get { return window; } }

        public JoinOffer(GameOffer game_offer, string joiner, Window window)
            : base(game_offer.Creator, game_offer.GameType, game_offer.MatchTo, game_offer.Stake, game_offer.Limit, DateTime.UtcNow)
        {
            this.joiner = joiner;
            this.window = window;
        }
    }
}
