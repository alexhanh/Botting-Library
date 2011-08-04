using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon
{
    public class GameOfferSet
    {
        private List<GameOffer> offers;

        public GameOfferSet()
        {
            offers = new List<GameOffer>();
        }

        public GameOfferSet(IEnumerable<GameOffer> offers)
        {
            this.offers = new List<GameOffer>();
            Add(offers);
        }

        public void Add(IEnumerable<GameOffer> offers)
        {
            this.offers.AddRange(offers);
        }

        public void Add(GameOffer offer)
        {
            this.offers.Add(offer);
        }

        /// <summary>
        /// Removes offers of a given creator from this collection.
        /// </summary>
        /// <param name="creator"></param>
        public void Remove(string creator)
        {
            this.offers.RemoveAll(m => m.Creator == creator);
        }

        public IEnumerator<GameOffer> GetEnumerator()
        {
            return offers.GetEnumerator();
        }
    }
}
