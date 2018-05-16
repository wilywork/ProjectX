namespace ProjectX.Plugins
{
    public class GodModeCommand
    {

        public class Config
        {
            public string godModeDisable;
            public string godModeEnable;

            public Config Default()
            {
                godModeDisable = "[color red] God Mode foi desativado.";
                godModeEnable = "[color green] God Mode foi ativado.";
                return this;
            }
        }

        public static Config configGodMode = new Config();

        public static void Start()
        {
            configGodMode = ProjectX.ReadyConfigChecked<Config>(configGodMode.Default(), "config/godMode.json");
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];
                if (ChatArguments.Length > 0)
                {
                    cachePlayer.PlayerClient.rootControllable.rootCharacter.takeDamage.SetGodMode(false);
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, configGodMode.godModeDisable);
                }
                else {
                    cachePlayer.PlayerClient.rootControllable.rootCharacter.takeDamage.SetGodMode(true);
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, configGodMode.godModeEnable);
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }
}
