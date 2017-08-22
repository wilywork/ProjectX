namespace ProjectX.Plugins
{
    public class InfoUserCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Server.Cache[Arguments.argUser.playerClient.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Seu SteamID é:[/color] "+ Arguments.argUser.playerClient.userID);
        }
    }
}
