using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.Venue
{
    public class OnlineMatchResult : OnlineMatchInfo
    {
        private int winner;
        private int winner_score; // Cube * Value, -1 if unknown.
        private int rake_paid;

        public int NetWin
        {
            get
            {
                return  rake_paid;
            }
        }
    }
}
