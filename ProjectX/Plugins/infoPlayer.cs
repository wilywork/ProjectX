namespace ProjectX.Plugins
{
    public class InfoUserCommand
    {

        public class Config
        {
            public string warnSteamID;

            public Config Default()
            {
                warnSteamID = "[color green]Seu SteamID é:[/color] {0}";
                return this;
            }
        }

        public static Config configInfoUser = new Config();

        public static void Start()
        {
            configInfoUser = ProjectX.ReadyConfigChecked<Config>(configInfoUser.Default(), "config/infoUser.json");
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Server.Cache[Arguments.argUser.playerClient.userID].MessageFrom(ProjectX.configServer.NameServer, string.Format(configInfoUser.warnSteamID, Arguments.argUser.playerClient.userID));
        }
    }
}
