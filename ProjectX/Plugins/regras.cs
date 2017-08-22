using System.Collections.Generic;

namespace ProjectX.Plugins
{
    public class RegrasCommand
    {

        public class Config
        {
            public List<string> listRules;

            public Config Default()
            {
                listRules = new List<string>() { "rule1..", "rule2.." };
                return this;
            }
        }

        public static Config configRules = new Config();
 
        public static void Start()
        {
            configRules = ProjectX.ReadyConfigChecked<Config>(configRules.Default(), "config/rules.json");
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];
            foreach (string rule in configRules.listRules)
            {
                player.MessageFrom(ProjectX.configServer.NameServer, rule);
            }
        }
    }
}
