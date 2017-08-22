namespace ProjectX.Plugins
{
    public class GodModeCommand
    {

        public static string notPermission = "[color red]Você não tem permissão para usar este comando";
        public static string godModeDisable = "[color red] God Mode foi desativado.";
        public static string godModeEnable = "[color green] God Mode foi ativado.";

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];
                if (ChatArguments.Length > 0)
                {
                    cachePlayer.PlayerClient.rootControllable.rootCharacter.takeDamage.SetGodMode(false);
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, godModeDisable);
                }
                else {
                    cachePlayer.PlayerClient.rootControllable.rootCharacter.takeDamage.SetGodMode(true);
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, godModeEnable);
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, notPermission);
            }
        }
    }
}
