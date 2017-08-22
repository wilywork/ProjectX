namespace ProjectX.Plugins
{
    public class DamangeCommand
    {
        public static void DammangeActiveCommand(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            ProjectX.StoragePlayer cacheStorage = Fougerite.Server.Cache[Arguments.argUser.userID].PlayerClient.gameObject.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage != null)
                cacheStorage.showDamange = true;

            Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green] Show Damange ativado!");
        }

        public static void DammangeDesactiveCommand(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            ProjectX.StoragePlayer cacheStorage = Fougerite.Server.Cache[Arguments.argUser.userID].PlayerClient.gameObject.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage != null)
                cacheStorage.showDamange = false;

            Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Show Damange desativado!");
        }
    }
}
