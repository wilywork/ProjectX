using Fougerite.Events;
using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    class unShareCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Para remover um share use:[/color]  /unshare Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (!ProjectX.shareList.ContainsKey(pl.UID) || ProjectX.shareList[pl.UID].Count == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você não tem amigos na lista de share.");
            }
            else if (cachePlayer2 != null && ProjectX.shareList[pl.UID].Contains(cachePlayer2.SteamID))
            {
                ProjectX.shareList[pl.UID].Remove(cachePlayer2.SteamID);
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você removeu [color #ffffff]" + cachePlayer2.Name + "[/color] da sua lista de share.");
            }
            else if (playerName.Length == 17 && ProjectX.shareList[pl.UID].Contains(playerName)) {
                ProjectX.shareList[pl.UID].Remove(playerName);
                if (ProjectX.userCache.ContainsKey(Convert.ToUInt64(playerName)))
                {
                    playerName = ProjectX.userCache[Convert.ToUInt64(playerName)]["name"];
                }
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você removeu [color #ffffff]" + playerName + "[/color] da sua lista de amigos.");
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format("[color yellow]Nenhum player da sua lista com este nome: [color #ffffff]{0}[/color], tente usar o SteamID.", playerName));
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
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você não tem players na lista de share.");
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
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Para adicionar share use:[/color]  /share Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (cachePlayer2 != null)
            {
                if (ProjectX.shareList.ContainsKey(pl.UID))
                {
                    if (ProjectX.shareList[pl.UID].Count >= ProjectX.configServer.limitShares)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você atingiu o numero máximo de share.");
                    }
                    else
                    {
                        ProjectX.shareList[pl.UID].Add(cachePlayer2.SteamID);
                        pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você adicionou o [color #ffffff]" + cachePlayer2.Name + "[/color] em sua lista de share.");
                    }
                }
                else
                {
                    ProjectX.shareList.Add(pl.UID, new List<string>() { cachePlayer2.SteamID });
                    pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você adicionou o [color #ffffff]" + cachePlayer2.Name + "[/color] em sua lista de share.");
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Player não encontrado.");
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
