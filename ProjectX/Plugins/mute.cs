using System;
using System.Linq;

namespace ProjectX.Plugins
{
    public class mute
    {

        public static void Add(Fougerite.Player targetuser, float time, Fougerite.Player netuser) {
            if (ProjectX.muteList.Contains(targetuser.UID))
            {
                if (netuser != null)
                {
                    netuser.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Este player já está multado.");
                }
            }
            else {
                ProjectX.muteList.Add(targetuser.UID);

                targetuser.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Você foi multado!");

                if (time <= 0) {
                    time = 300;//5 minutes
                }

                TimerEdit.TimerEvento.Once(time, () => {
                    mute.Remove(targetuser.UID, null);
                });

                if (netuser != null)
                {
                    netuser.MessageFrom(ProjectX.configServer.NameServer, "[color green] Multado com sucesso.");
                }
                ProjectX.BroadCast(ProjectX.configServer.NameServer, targetuser.Name + "[color yellow] Multado por [color #ffffff]" + (time / 60).ToString() + "[/color] minutos.");
            }
        }

        public static void Remove(ulong SteamID, Fougerite.Player netuser)
        {
            if (ProjectX.muteList.Contains(SteamID))
            {
                ProjectX.muteList.Remove(SteamID);
                if (netuser != null)
                {
                    netuser.MessageFrom(ProjectX.configServer.NameServer, "[color green] Player desmultado.");
                }
                Fougerite.Player cachePlayer = Fougerite.Server.GetServer().FindPlayer(SteamID.ToString());
                if(cachePlayer != null)
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color green] Você foi desmultado.");
                }
            }
            else {
                if (netuser != null)
                {
                    netuser.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Este player não está multado.");
                }
            }
        }

    }

    public class MuteCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    int timeMute = 600;
                    Fougerite.Player cachePlayer = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                    if (cachePlayer != null)
                    {
                        if (ChatArguments[1] != null) {
                            if (ChatArguments[1].All(char.IsDigit)) {
                                timeMute = Convert.ToInt32(ChatArguments[1]) * 60;
                            }
                        }

                        mute.Add(cachePlayer, timeMute, Fougerite.Server.Cache[Arguments.argUser.userID]);

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

    public class UnMuteCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    Fougerite.Player cachePlayer = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                    if (cachePlayer != null)
                    {
                        mute.Remove(cachePlayer.UID, Fougerite.Server.Cache[Arguments.argUser.userID]);
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
