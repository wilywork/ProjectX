using System;
using UnityEngine;

namespace ProjectX.Plugins
{
    public class TpAdmCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin"))
            {
                if (ChatArguments.Length > 0)
                {
                    Fougerite.Player cachePlayer = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                    if (cachePlayer != null)
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].TeleportTo(cachePlayer);
                    } else if (ChatArguments.Length == 3)
                    {
                        ChatArguments[0] = ChatArguments[0].Replace(",", "");
                        ChatArguments[1] = ChatArguments[1].Replace(",", "");
                        ChatArguments[2] = ChatArguments[2].Replace(",", "");
                        ProjectX.TeleportPlayer(Arguments.argUser, new Vector3(Convert.ToInt32(ChatArguments[0]), Convert.ToInt32(ChatArguments[1]), Convert.ToInt32(ChatArguments[2])));
                    }
                    else
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Player não encontrado.");
                    }

                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Player não encontrado.");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }
}
