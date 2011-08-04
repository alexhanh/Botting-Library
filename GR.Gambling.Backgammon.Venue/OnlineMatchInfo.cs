using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.Venue
{
    // Idea is to store non-changing items of a money or match game into this structure so we can persist them and not need to screensrape the information everytime.
    // Create once into the xml and no need to change anything afterwards, just when restarting, first build a MatchInfoCollection to be querable, check if exists and so on..
    public class OnlineMatchInfo
    {
        private int match_to;
        private int stake;
        private int limit;
        private GameType game_type;
        private string[] players;
        private int creator; // Should equal to hero or opponent, null or empty if unknown.
        private int[] ratings;
        private string venue_id;
        private int id; // Unique id assigned by the casino.
        private DateTime timestamp; // When was this game played in UTC.

        public int MatchTo { get { return match_to; } }
        public int Stake { get { return stake; } }
        public int Limit { get { return limit; } }
        public GameType GameType { get { return game_type; } }
        public int Creator { get { return creator; } }
        public string VenueID { get { return venue_id; } }
        public int ID { get { return id; } }
        public DateTime Timestamp { get { return timestamp; } }

        public int Rating(int player) { return ratings[player]; }
        public string PlayerName(int player) { return players[player]; }
        // Create mets to ./temp folder and use "skinid_gameid.met" naming convention?

        public static OnlineMatchInfo CreateMoneyMatchInfo(string[] players, int creator, int[] ratings, int stake, int limit, 
                                                            string venue_id, int id, DateTime timestamp)
        {
            OnlineMatchInfo match_info = new OnlineMatchInfo();

            match_info.players = players;
            match_info.creator = creator;
            match_info.ratings = ratings;
            match_info.stake = stake;
            match_info.limit = limit;
            match_info.venue_id = venue_id;
            match_info.id = id;
            match_info.timestamp = timestamp;

            return match_info;
        }

        public bool HasPlayer(string player)
        {
            if (players[0] == player || players[1] == player)
                return true;

            return false;
        }

        public int Pot(int cube_value, int points)
        {
            return 2 * Math.Min(cube_value * points * stake, limit);
        }
    }
}
