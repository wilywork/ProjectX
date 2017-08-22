namespace ProjectX.Plugins
{
    public class PlayersCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];

            pl.MessageFrom(ProjectX.configServer.NameServer, PlayerClient.All.Count + "/100 players onlines:");
            int num = 0;
            string str = "";
            foreach (Fougerite.Player client in Fougerite.Server.GetServer().Players)
            {

                if (str == "")
                {
                    str = client.Name;
                }
                else
                {
                    str = str + " [color green]|[/color] "+ client.Name;
                }

                if (num == 4)
                {
                    num = 0;
                    pl.MessageFrom(ProjectX.configServer.NameServer, str);
                    str = "";
                }
                else
                {
                    num++;
                }
            }
            if (num != 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, str);
            }
        }
    }
}
