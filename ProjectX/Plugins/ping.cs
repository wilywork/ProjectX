namespace ProjectX.Plugins
{
    public class PingInfoCommand
    {
        public static Fougerite.Player cachePlayer;

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (ChatArguments.Length > 0)
            {
                if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "mod"))
                {
                    cachePlayer = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                    if (cachePlayer != null)
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Player:[/color] "+ cachePlayer.Name);
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]SteamID:[/color] " + cachePlayer.SteamID);
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]IP:[/color] " + cachePlayer.IP);
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Ping:[/color] " + cachePlayer.Ping);
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Tempo online:[/color] " + cachePlayer.TimeOnline);
                    }
                    else {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Player não encontrado.");
                    }
                }
                else {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
                }
            }
            else {
                Fougerite.Server.Cache[Arguments.argUser.userID].Notice("Seu ping " + Fougerite.Server.Cache[Arguments.argUser.userID].NetworkPlayer.averagePing + "ms");
            }

        }
    }
}
