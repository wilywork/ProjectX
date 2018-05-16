using Fougerite.Events;
using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{

    class Share
    {

        public class Config
        {
            public string helpRemove;
            public string helpAdd;
            public string warnRemove;
            public string warnNotShare;
            public string warnAdd;
            public string noHaveShares;
            public string playerNotFoundInList;
            public string playerNotFound;
            public string warnLimitShares;
            public int limitShares;

            public Config Default()
            {
                helpRemove = "[color yellow] Para remover um share use:[/color]  /unshare Nome";
                helpAdd = "[color yellow] Para adicionar share use:[/color]  /share Nome";
                warnNotShare = "[color yellow]Você não tem amigos na lista de share.";
                warnRemove = "[color green]Você removeu [color #ffffff]{0}[/color] da sua lista de share.";
                warnAdd = "[color green]Você adicionou o [color #ffffff]{0}[/color] em sua lista de share.";
                noHaveShares = "[color yellow]Você não tem players na lista de share.";
                playerNotFoundInList = "[color yellow]Nenhum player da sua lista com este nome: [color #ffffff]{0}[/color], tente usar o SteamID.";
                playerNotFound = "[color yellow]Player não encontrado.";
                warnLimitShares = "[color yellow]Você atingiu o numero máximo de share.";
                limitShares = 5;

                return this;
            }
        }

        public static Config configShare = new Config();

        public static void Start()
        {
            configShare = ProjectX.ReadyConfigChecked<Config>(configShare.Default(), "config/share.json");
        }
    }

    class unShareCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.helpRemove);
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (!ProjectX.shareList.ContainsKey(pl.UID) || ProjectX.shareList[pl.UID].Count == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.warnNotShare);
            }
            else if (cachePlayer2 != null && ProjectX.shareList[pl.UID].Contains(cachePlayer2.SteamID))
            {
                ProjectX.shareList[pl.UID].Remove(cachePlayer2.SteamID);
                pl.MessageFrom(ProjectX.configServer.NameServer, String.Format(Share.configShare.warnRemove, cachePlayer2.Name));
            }
            else if (playerName.Length == 17 && ProjectX.shareList[pl.UID].Contains(playerName)) {
                ProjectX.shareList[pl.UID].Remove(playerName);
                if (ProjectX.userCache.ContainsKey(Convert.ToUInt64(playerName)))
                {
                    playerName = ProjectX.userCache[Convert.ToUInt64(playerName)]["name"];
                }
                pl.MessageFrom(ProjectX.configServer.NameServer, String.Format(Share.configShare.warnRemove, playerName));
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Share.configShare.playerNotFound, playerName));
                return;
            }
        }
    }

    public class shareListCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];

            if (ProjectX.shareList.ContainsKey(pl.UID) && ProjectX.shareList[pl.UID].Count > 0)
            {
                foreach (string share in ProjectX.shareList[pl.UID])
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, ProjectX.userCache[Convert.ToUInt64(share)]["name"].ToString() + " - " + share);
                }
            }
            else {
                pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.noHaveShares);
            }

        }
    }

    public class ShareCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.helpAdd);
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (cachePlayer2 != null)
            {
                if (ProjectX.shareList.ContainsKey(pl.UID))
                {
                    if (ProjectX.shareList[pl.UID].Count >= Share.configShare.limitShares)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.warnLimitShares);
                    }
                    else
                    {
                        ProjectX.shareList[pl.UID].Add(cachePlayer2.SteamID);
                        pl.MessageFrom(ProjectX.configServer.NameServer, String.Format(Share.configShare.warnAdd, cachePlayer2.Name));
                    }
                }
                else
                {
                    ProjectX.shareList.Add(pl.UID, new List<string>() { cachePlayer2.SteamID });
                    pl.MessageFrom(ProjectX.configServer.NameServer, String.Format(Share.configShare.warnAdd, cachePlayer2.Name));
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Share.configShare.playerNotFound);
            }

        }


        public static bool isShare(ulong steamIdOwner, ulong steamId)
        {
            if (ProjectX.shareList.ContainsKey(steamIdOwner))
            {
                if (ProjectX.shareList[steamIdOwner].Contains(steamId.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void DoorUse(Fougerite.Player p, DoorEvent de)
        {
            if (de.Entity.UOwnerID == p.UID || p.Admin)
            {
                de.Open = true;
            }
            else if (ShareCommand.isShare(de.Entity.UOwnerID, p.UID))
            {
                de.Open = true;
            }
            else
            {
                de.Open = false;
            }
        }
    }
}
