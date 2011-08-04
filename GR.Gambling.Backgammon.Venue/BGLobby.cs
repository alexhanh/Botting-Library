using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Backgammon.Venue
{
    public abstract class BGLobby
    {
        protected List<GameOffer> created_game_offers;

        public BGLobby()
        {
            created_game_offers = new List<GameOffer>();
        }

        /// <summary>
        /// Holds information about the current game offers created by calling CreateGameOffer().
        /// </summary>
        public List<GameOffer> CreatedGameOffers { get { return created_game_offers; } }

        // TODO:
        // public abstract List<GameOffer> GetMatchOffers();
        // public abstract List<GameOffer> GetMoneyOffers();
        // public abstract JoinGameOffer(GameOffer game_offer);
        public abstract List<GameOffer> GetOffers();
       
        /// <summary>
        /// This should try to create a game offer in the lobby with the given offer parameters. 
        /// Should add the game offer to created_game_offers upon succesful creation.
        /// </summary>
        /// <param name="game_offer"></param>
        public abstract void CreateGameOffer(GameOffer game_offer);

        /// <summary>
        /// This should be invoked when a new join offer is available by joining an offer or someone else joining our offer.
        /// The return value should tell whether we should accept the offer or not.
        /// </summary>
        public delegate bool JoinOfferedEventHandler(JoinOffer join_offer);
        public event JoinOfferedEventHandler JoinOffered;
        protected virtual bool OnJoinOffered(JoinOffer join_offer)
        {
            if (JoinOffered != null)
                return JoinOffered(join_offer);

            return false;
        }

        /// <summary>
        /// Should be invoked when a join request was declined by other player and there's information available of that.
        /// </summary>
        /// <param name="join_offer"></param>
        public delegate void JoinOfferDeclined(JoinOffer join_offer);
        public event JoinOfferDeclined JoinDeclined;
        protected virtual void OnJoinDeclined(JoinOffer join_offer)
        {
            if (JoinDeclined != null)
                JoinDeclined(join_offer);
        }

        /// <summary>
        /// This should be invoked when a created game offer expires in the lobby. It is in the implementato
        /// </summary>
        public delegate void CreatedGameOfferExpiredEventHandler(GameOffer game_offer);
        public event CreatedGameOfferExpiredEventHandler CreateGameOfferExpired;
        protected virtual void OnCreateGameOfferExpired(GameOffer game_offer)
        {
            /*lock (created_game_offers)
            {
                for (int i = 0; i < created_game_offers.Count; i++)
                {
                    if (created_game_offers[i].GetHashCode() == game_offer.GetHashCode())
                    {
                        created_game_offers.RemoveAt(i);
                        break;
                    }
                }
            }*/

            if (CreateGameOfferExpired != null)
                CreateGameOfferExpired(game_offer);
        }

        /// <summary>
        /// This should be the method upon when called, detects different situations and triggers applicable events.
        /// </summary>
        public abstract void Update();
    }
}
