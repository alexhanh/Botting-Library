using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Data;

namespace GR.Gambling.Backgammon.Venue
{
    /*public class BGPersistedData
    {
        private string GameID;
        private string Player1;
        private string Player2;
        private int MatchTo;
        private int Stake;
        private int Limit;
        private int Score1;
        private int Score2;
        private GameType GameType;
        private DateTime PlayStarted;
        private DateTime BotSentMessageStamp;

        private Dictionary<string, string> OtherParameters;

        public BGPersistedData()
        {

        }
    }*/
    
    public sealed class BGPersistor : PersistantFileStorage
    {
        static readonly BGPersistor instance = new BGPersistor("BGPersistedStorage.txt");

        static BGPersistor()
        {
        }

        private BGPersistor(string filepath)
            : base(filepath)
        {
            AutoCommitOnChange = true;
        }

        public static BGPersistor Instance
        {
            get
            {
                return instance;
            }
        }

        public static string[] GetPlayerNames(string id)
        {
            if (instance.Contains(id, "Player1") && instance.Contains(id, "Player2"))
            {
                return new string[] { (string)instance[id, "Player1"], (string)instance[id, "Player2"] };
            }

            return new string[] { "", "" };
        }

        public static GameType GetGameType(string id)
        {
            if (instance.Contains(id, "GameType"))
            {
                object gametype = instance[id, "GameType"];
                if (gametype.GetType() == typeof(GameType))
                    return (GameType)gametype;
                else
                {
                    GameType gt = (GameType)Enum.Parse(typeof(GameType), (string)gametype);

                    // Cache the conversion
                    instance[id, "GameType"] = gt;

                    return gt;
                }
            }

            return GameType.None;
        }
    }
}
