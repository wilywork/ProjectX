namespace ProjectX.Plugins
{
    public class SuicideCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser != null && Arguments.argUser.playerClient != null) {
                Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];
                if (player.PlayerClient != null && player.PlayerClient.rootControllable != null) {
                    Fougerite.Server.Cache[Arguments.argUser.playerClient.userID].Kill();
                }
            }
        }
    }
}
