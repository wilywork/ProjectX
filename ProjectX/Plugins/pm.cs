using System.Linq;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    public class PmCommand
    {
        static string teal = "[color #ff00f5]";
        static string search;

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];
            if (ChatArguments.Length < 2)
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "Private Message Usage:  /pm playerName message");
                return;
            }
            search = ChatArguments[0];
            Fougerite.Player cachePlayer2 = Fougerite.Player.FindByName(search);
            if (cachePlayer2 == null)
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "Não foi possível localizar o jogador " + search);
                return;
            }
            List<string> wth = ChatArguments.ToList();
            wth.Remove(wth[0]);
            string message;
            try
            {
                message = string.Join(" ", wth.ToArray()).Replace(search, "").Trim(new char[] { ' ', '"' }).Replace('"', 'ˮ');
            }
            catch
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "Jogador não foi encontrado tente novamente.");
                return;
            }
            if (message == string.Empty)
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "Private Message Usage: /pm playerName message");
            }
            else
            {
                cachePlayer2.MessageFrom("PM de " + cachePlayer.Name, teal + message);
                cachePlayer.MessageFrom("PM para " + cachePlayer2.Name, teal + message);
                ActiveOldPm(cachePlayer.PlayerClient, cachePlayer2.PlayerClient);
            }
        }

        public static void ActiveOldPm(PlayerClient netUser, PlayerClient targetPlayer)
        {
            ProjectX.StoragePlayer cacheStorage = targetPlayer.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage == null) cacheStorage = targetPlayer.gameObject.AddComponent<ProjectX.StoragePlayer>();
            cacheStorage.oldUserPm = netUser.userID.ToString();

            cacheStorage = netUser.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage == null) cacheStorage = netUser.gameObject.AddComponent<ProjectX.StoragePlayer>();
            cacheStorage.oldUserPm = targetPlayer.userID.ToString();
        }

    }

    public class ReplyPmCommand
    {
        private static string mensagemPm;

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];

            if (ChatArguments.Length > 0)
            {
                ProjectX.StoragePlayer cacheStorage = cachePlayer.PlayerClient.GetComponent<ProjectX.StoragePlayer>();
                if (cacheStorage == null) cacheStorage = cachePlayer.PlayerClient.gameObject.AddComponent<ProjectX.StoragePlayer>();

                if (cacheStorage.oldUserPm != null)
                {
                    Fougerite.Player cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(cacheStorage.oldUserPm);
                    if (cachePlayer2 != null)
                    {
                        mensagemPm = "";

                        foreach (string value in ChatArguments)
                        {
                            mensagemPm = mensagemPm + " " + value;
                        }
                        cachePlayer.MessageFrom("PM para " + cachePlayer2.Name, "[color #ff00f5]" + mensagemPm);
                        cachePlayer2.MessageFrom("PM de " + cachePlayer.Name, "[color #ff00f5]" + mensagemPm);

                        PmCommand.ActiveOldPm(cachePlayer.PlayerClient, cachePlayer2.PlayerClient);
                    }
                    else
                    {
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color red] Player de destino não encontrado.");
                    }
                }
                else
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color red] Player de destino não encontrado.");
                }
            }
            else
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color red] Você deve informar uma mensgem.");
            }
        }

    }
}
