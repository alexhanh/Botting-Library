using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon
{
    public class Rules
    {
        public int ReDoubles { get; set; }

        /// <summary>
        /// True, if beavers are allowed. Beaver means the possibility to re-double immediatly after double.
        /// </summary>
        //public bool AllowBeavers { get; }

        /// <summary>
        /// True, if raccoons are allowed. Raccoon means the possibility to re-re-double immediatly after re-double.
        /// </summary>
        //public bool AllowRaccoons { get; }

        /// <summary>
        /// True, if the Jacoby rule is used. The Jacoby rule allows gammons and backgammons to count for 
        /// their respective double and triple values only if the cube has already been offered and accepted.
        /// The Jacoby rule is widely used in money play but is not used in match play.
        /// </summary>
        public bool JacobyRule { get; set; }

        /// <summary>
        /// True, if the Crawford rule is used. The Crawford rule requires that when a player first reaches a score one point short of winning, 
        /// neither player may use the doubling cube for the following game, called the Crawford game. After the Crawford game, 
        /// normal use of the doubling cube resumes. The Crawford rule is used in tournament match play.
        /// </summary>
        public bool CrawfordRule { get; set; }
    }
}
