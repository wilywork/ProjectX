using Fougerite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectX.Plugins
{
    class PlayerConnection
    {

        public class Config
        {
            public string joinServer;
            public string leaveServer;
            public string nameShortBroadCast;
            public string nameShort;
            public string nameIlegalBroadCast;
            public string nameIlegal;
            public string colorWarn;
            public string nameBlockedBroadCast;
            public string nameBlocked;
            public List<string> blockNames;

            public Config Default()
            {
                joinServer = "entrou";
                leaveServer = "saiu";
                nameShortBroadCast = "será kikado por nick menor que 2 caracteres!";
                nameShort = " Seu nick precisa ser maior que 2 caracteres!";
                nameIlegalBroadCast = "será kikado por nick conter caracteres especiais!";
                nameIlegal = " Você foi kikado por nick proibido ou conter caracteres especiais!";
                colorWarn = "[color red]";
                nameBlockedBroadCast = "será kikado por está com nick inapropiado ou bloqueado.";
                nameBlocked = " Você será kikado por está com nick inapropiado ou bloqueado.";
                blockNames = new List<string>() { "fougerite", "console", "server" };
                return this;
            }
        }

        public static Config configConnection = new Config();

        public static void Start()
        {
            configConnection = ProjectX.ReadyConfigChecked<Config>(configConnection.Default(), "config/playerConnection.json");
        }

        public static void HookPlayerConnect(Fougerite.Player player)
        {
            string cacheNameJoinPlayer = player.Name.ToLower();

            //check name short
            if (cacheNameJoinPlayer.Length < 3)
            {
                KickPlayer(player, configConnection.nameShort, configConnection.nameShortBroadCast);
                return;
            }

            //check ilegal character
            string cacheNameJoinPlayerValid = Regex.Replace(cacheNameJoinPlayer, "[^0-9a-zA-Z| |\\]|\\[|\\-|!|.|_|?]+", "");
            if (cacheNameJoinPlayer != cacheNameJoinPlayerValid)
            {
                KickPlayer(player, configConnection.nameIlegal, configConnection.nameIlegalBroadCast);
                return;
            }

            //autoadmin
            if (Permission.HasPermission(player.UID, "admin"))
            {
                player.PlayerClient.netUser.SetAdmin(true);
            }

            //check name blocked
            if (!player.Admin)
            {
                foreach (string value in configConnection.blockNames)
                {
                    if (cacheNameJoinPlayer.IndexOf(value) != -1)
                    {
                        KickPlayer(player, configConnection.nameBlocked, configConnection.nameBlockedBroadCast);
                        return;
                    }
                }
            }

            ProjectX.BroadCast(ProjectX.configServer.NameServer, player.Name + configConnection.joinServer);

            //save userCache
            if (ProjectX.userCache.ContainsKey(player.UID))
            {
                ProjectX.userCache[player.UID]["name"] = player.Name;
                ProjectX.userCache[player.UID]["date"] = ProjectX.DataAtual().ToString();
                ProjectX.userCache[player.UID]["ip"] = player.IP;
            }
            else
            {
                ProjectX.userCache.Add(player.UID, new SerializableDictionary<string, string>());
                ProjectX.userCache[player.UID].Add("name", player.Name);
                ProjectX.userCache[player.UID].Add("date", ProjectX.DataAtual().ToString());
                ProjectX.userCache[player.UID].Add("ip", player.IP);
            }

            if(player.PlayerClient.GetComponent<ProjectX.StoragePlayer>() == null)
            {
                player.PlayerClient.gameObject.AddComponent<ProjectX.StoragePlayer>();
            }
            
        }

        public static void HookPlayerDisconnect(Fougerite.Player player)
        {
            ProjectX.BroadCast(ProjectX.configServer.NameServer, player.Name + configConnection.leaveServer);
            //save userCache
            if (ProjectX.userCache.ContainsKey(player.UID))
            {
                ProjectX.userCache[player.UID]["name"] = player.Name;
                ProjectX.userCache[player.UID]["date"] = ProjectX.DataAtual().ToString();
            }
            else
            {
                ProjectX.userCache.Add(player.UID, new SerializableDictionary<string, string>());
                ProjectX.userCache[player.UID].Add("name", player.Name);
                ProjectX.userCache[player.UID].Add("date", ProjectX.DataAtual().ToString());
            }
        }

        public static void KickPlayer(Fougerite.Player player, string message, string messageBroadCast) {
            ProjectX.BroadCast(ProjectX.configServer.NameServer, player.Name +" "+ configConnection.colorWarn + messageBroadCast);
            player.Notice("☢", message, 25f);
            player.Message(configConnection.colorWarn + message);
            player.Message(configConnection.colorWarn + message);
            player.Disconnect();
        }

    }
}
