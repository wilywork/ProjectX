namespace ProjectX.Plugins
{
    public class SuicideCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser != null && Arguments.argUser.playerClient != null) {
                Fougerite.Server.Cache[Arguments.argUser.playerClient.userID].Kill();
            }
        }
    }
}
