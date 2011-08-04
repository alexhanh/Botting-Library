using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon
{
    /// <summary>
    /// Represents creatable and joinable game offers.
    /// </summary>
    public class GameOffer
    {
        protected readonly string creator;
        protected readonly GameType game_type;
        protected readonly int match_to;
        protected readonly int stake;
        protected readonly int limit;
        protected readonly DateTime time_created;

        public string Creator { get { return creator; } }
        public GameType GameType { get { return game_type; } }
        public int MatchTo { get { return match_to; } }
        public int Stake { get { return stake; } }
        public int Limit { get { return limit; } }
        public DateTime TimeCreated { get { return time_created; } }

        public GameOffer(string creator, GameType game_type, int match_to, int stake, int limit, DateTime time_created)
        {
            this.creator = creator;
            this.game_type = game_type;
            this.match_to = match_to;
            this.stake = stake;
            this.limit = limit;
            this.time_created = time_created;
        }

		public GameOffer(string creator, GameType game_type, int match_to, int stake, int limit)
		{
            this.creator = creator;
            this.game_type = game_type;
            this.match_to = match_to;
            this.stake = stake;
            this.limit = limit;
            this.time_created = DateTime.UtcNow;
		}

        public override int GetHashCode()
        {
            return time_created.GetHashCode();
        }

		/// <summary>
		/// Creates a deep-copy from existing offer and updates it's time stamp.
		/// </summary>
		/// <param name="offer"></param>
		/// <returns></returns>
		public static GameOffer FromOffer(GameOffer offer)
		{
			return new GameOffer(offer.creator, offer.game_type, offer.match_to, offer.stake, offer.limit);
		}

        public static GameOffer CreateMatchOffer(string creator, int match_to, int stake)
        {
            return new GameOffer(creator, GameType.Match, match_to, stake, stake, DateTime.UtcNow);
        }

        public static GameOffer CreateMoneyOffer(string creator, int stake, int limit)
        {
            return new GameOffer(creator, GameType.Money, 1, stake, limit, DateTime.UtcNow);
        }
    }
}
