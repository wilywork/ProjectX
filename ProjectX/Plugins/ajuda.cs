using System.Collections.Generic;

namespace ProjectX.Plugins
{
    public class HelpCommand
    {

        public class Config
        {
            public List<string> listHelps;

            public Config Default()
            {
                listHelps = new List<string>() { "help1..", "help2.." };
                return this;
            }
        }

        public static Config configAjuda = new Config();

        public static void Start()
        {
            configAjuda = ProjectX.ReadyConfigChecked<Config>(configAjuda.Default(), "config/help.json");
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];
            foreach (string help in configAjuda.listHelps) {
                player.MessageFrom(ProjectX.configServer.NameServer, help);
            }
        }
    }
}
