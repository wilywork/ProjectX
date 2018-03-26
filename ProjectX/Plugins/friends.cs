using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{

    class Friends
    {

        public class Config
        {
            public string helpRemove;
            public string helpAdd;
            public string warnRemove;
            public string warnAdd;
            public string noHaveFriends;
            public string playerNotFoundInList;
            public string playerNotFound;
            public string warnLimitFriends;
            public int limitFriends;

            public Config Default()
            {
                helpRemove = "To remove a friend use: /unfriend Nome";
                helpAdd = "To add a friend use: /addfriend Nome";
                warnRemove = "You removed {0} from your friends list.";
                warnAdd = "You added the {0} on your friends list.";
                noHaveFriends = "You do not have friends.";
                playerNotFoundInList = "No players on your list with this name: {0}, try using the SteamID.";
                playerNotFound = "Player not found.";
                warnLimitFriends = "You have reached the maximum number of friends.";
                limitFriends = 5;
                
                return this;
            }
        }

        public static Config configFriends = new Config();

        public static void Start()
        {
            configFriends = ProjectX.ReadyConfigChecked<Config>(configFriends.Default(), "config/friends.json");
        }
    }

    class UnFriendCommand
    {

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.helpRemove);
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (!ProjectX.friendList.ContainsKey(pl.UID) || ProjectX.friendList[pl.UID].Count == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.noHaveFriends);
            }
            else if (cachePlayer2 != null && ProjectX.friendList[pl.UID].Contains(cachePlayer2.SteamID))
            {
                ProjectX.friendList[pl.UID].Remove(cachePlayer2.SteamID);
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Friends.configFriends.warnRemove, cachePlayer2.Name) );

            } else if( playerName.Length == 17 && ProjectX.friendList[pl.UID].Contains(playerName) ) {

                ProjectX.friendList[pl.UID].Remove(playerName);
                if (ProjectX.userCache.ContainsKey(Convert.ToUInt64(playerName))) {
                    playerName = ProjectX.userCache[Convert.ToUInt64(playerName)]["name"];
                }
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Friends.configFriends.warnRemove, playerName) );

            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Friends.configFriends.playerNotFoundInList, playerName));
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
                pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.noHaveFriends);
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
                pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.helpAdd);
                return;
            }

            var cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(playerName);

            if (cachePlayer2 != null)
            {
                if (ProjectX.friendList.ContainsKey(pl.UID))
                {
                    if (ProjectX.friendList[pl.UID].Count >= Friends.configFriends.limitFriends)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.warnLimitFriends);
                    }
                    else
                    {
                        ProjectX.friendList[pl.UID].Add(cachePlayer2.SteamID);
                        pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Friends.configFriends.warnAdd, cachePlayer2.Name) );
                    }
                }
                else
                {
                    ProjectX.friendList.Add(pl.UID, new List<string>() { cachePlayer2.SteamID });
                    pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Friends.configFriends.warnAdd, cachePlayer2.Name) );
                }
            }
            else
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Friends.configFriends.playerNotFound);
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
