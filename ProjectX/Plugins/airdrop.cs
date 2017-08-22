using Fougerite;
using System;
using TimerEdit;
using UnityEngine;

namespace ProjectX.Plugins
{
    class Airdrop
    {
        //airdrop 
        public static string AirdropBound = "";

        public class Config
        {
            public int timerAirdropMinutes;
            public string message;

            public Config Default()
            {
                timerAirdropMinutes = 180;
                message = "Airdrop caindo perto de {0} ( {1} {2} da sua posição )";
                return this;
            }
        }

        public static Config configAirdrop = new Config();

        public static void Start()
        {

            configAirdrop = ProjectX.ReadyConfigChecked<Config>(configAirdrop.Default(), "config/airdropBound.json");

            //start timer repeat airdrop 
            TimerEvento.Repeat(configAirdrop.timerAirdropMinutes * 60, 0, () =>
            {
                ConsoleSystem.Run("airdrop.drop");
            });
        }

        public static void HookOnAirdrop(Vector3 targetposition)
        {
            string location = string.Empty;
            string distance = string.Empty;
            string direction = string.Empty;


            location = LocCommand.FindLocationName(targetposition);

            if (location != null)
            {
                AirdropBound = location;
            } else
            {
                AirdropBound = "unknow";
            }

            try
            {
                foreach (PlayerClient player in PlayerClient.All)
                {
                    distance = Math.Ceiling(Math.Abs(Vector3.Distance(targetposition, player.lastKnownPosition))).ToString() + "m";

                    direction = ProjectX.GetDirection(targetposition, player.lastKnownPosition);

                    Fougerite.Server.Cache[player.userID].MessageFrom(ProjectX.configServer.NameServer, string.Format(configAirdrop.message, AirdropBound, distance, direction));
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[AirDrop] error. " + ex);
            }


        }
    }
}