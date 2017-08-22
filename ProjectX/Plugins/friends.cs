namespace ProjectX.Plugins
{
    using System;
    using System.Collections.Generic;

    public class UnFriendCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Para remover um amigo use:[/color]  /unfriend Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (!ProjectX.friendList.ContainsKey(pl.UID) || ProjectX.friendList[pl.UID].Count == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você não tem amigos.");
            }
            else if (cachePlayer2 != null && ProjectX.friendList[pl.UID].Contains(cachePlayer2.SteamID))
            {
                ProjectX.friendList[pl.UID].Remove(cachePlayer2.SteamID);
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você removeu [color #ffffff]" + cachePlayer2.Name + "[/color] da sua lista de amigos.");

            } else if( playerName.Length == 17 && ProjectX.friendList[pl.UID].Contains(playerName) ) {

                ProjectX.friendList[pl.UID].Remove(playerName);
                if (ProjectX.userCache.ContainsKey(Convert.ToUInt64(playerName))) {
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

    public class FriendsCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];

            if (ProjectX.friendList.ContainsKey(pl.UID) && ProjectX.friendList[pl.UID].Count > 0)
            {
                foreach (string friend in ProjectX.friendList[pl.UID])
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, ProjectX.userCache[Convert.ToUInt64(friend)]["name"].ToString() + " - " + friend);
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você não tem amigos.");
            }

        }
    }

    public class FriendCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Para adicionar um amigo use:[/color]  /addfriend Nome");
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (cachePlayer2 != null)
            {
                if (ProjectX.friendList.ContainsKey(pl.UID))
                {
                    if (ProjectX.friendList[pl.UID].Count >= ProjectX.configServer.limitFriends)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você atingiu o numero máximo de amigos.");
                    }
                    else
                    {
                        ProjectX.friendList[pl.UID].Add(cachePlayer2.SteamID);
                        pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você adicionou o [color #ffffff]" + cachePlayer2.Name + "[/color] em sua lista de amigos.");
                    }
                }
                else
                {
                    ProjectX.friendList.Add(pl.UID, new List<string>() { cachePlayer2.SteamID });
                    pl.MessageFrom(ProjectX.configServer.NameServer, "[color green]Você adicionou o [color #ffffff]" + cachePlayer2.Name + "[/color] em sua lista de amigos.");
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Player não encontrado.");
            }

        }

        public static bool isFriend(ulong steamIdOwner, ulong steamId)
        {
            if (ProjectX.friendList.ContainsKey(steamIdOwner))
            {
                if (ProjectX.friendList[steamIdOwner].Contains(steamId.ToString()))
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
