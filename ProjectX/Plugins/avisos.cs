using System.Collections.Generic;


namespace ProjectX.Plugins
{
    public class Avisos
    {

        public class Config
        {
            public List<string> listAvisos;

            public Config Default()
            {
                listAvisos = new List<string>() { "aviso 1..", "aviso 2.." };
                return this;
            }
        }

        public static Config configAvisos = new Config();



        public static void Start()
        {
            configAvisos = ProjectX.ReadyConfigChecked<Config>(  configAvisos.Default(), "config/avisos.json");
        }




        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];


            foreach (string help in configAvisos.listAvisos)
            {
                player.MessageFrom(ProjectX.configServer.NameServer, help);
            }

        }
    }
}