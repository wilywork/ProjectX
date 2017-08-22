namespace ProjectX.Plugins
{
    using System;
    using System.Collections.Generic;

    public class unShareCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "Para remover um share use:  /unshare Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (!ProjectX.shareList.ContainsKey(pl.UID) || ProjectX.shareList[pl.UID].Count == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "Você não tem amigos na lista de share.");
            }
            else if (cachePlayer2 != null && ProjectX.shareList[pl.UID].Contains(cachePlayer2.SteamID))
            {
                ProjectX.shareList[pl.UID].Remove(cachePlayer2.SteamID);
                pl.MessageFrom(ProjectX.configServer.NameServer, "Você removeu " + cachePlayer2.Name + " da sua lista de share.");
            }
            else if (playerName.Length == 17 && ProjectX.shareList[pl.UID].Contains(playerName)) {
                ProjectX.shareList[pl.UID].Remove(playerName);
                if (ProjectX.userCache.ContainsKey(Convert.ToUInt64(playerName)))
                {
                    playerName = ProjectX.userCache[Convert.ToUInt64(playerName)]["name"];
                }
                pl.MessageFrom(ProjectX.configServer.NameServer, "Você removeu " + playerName + " da sua lista de amigos.");
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format("Nenhum player da sua lista com este nome: {0}, tente usar o SteamID.", playerName));
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
                pl.MessageFrom(ProjectX.configServer.NameServer, "Você não tem players na lista de share.");
            }

        }
    }

    public class shareCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, " Para adicionar share use: /share Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (cachePlayer2 != null)
            {
                if (ProjectX.shareList.ContainsKey(pl.UID))
                {
                    if (ProjectX.shareList[pl.UID].Count >= ProjectX.configServer.limitShares)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, "Você atingiu o numero máximo de share.");
                    }
                    else
                    {
                        ProjectX.shareList[pl.UID].Add(cachePlayer2.SteamID);
                        pl.MessageFrom(ProjectX.configServer.NameServer, "Você adicionou o " + cachePlayer2.Name + " em sua lista de share.");
                    }
                }
                else
                {
                    ProjectX.shareList.Add(pl.UID, new List<string>() { cachePlayer2.SteamID });
                    pl.MessageFrom(ProjectX.configServer.NameServer, "Você adicionou o " + cachePlayer2.Name + " em sua lista de share.");
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "Player não encontrado.");
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
    }
}
