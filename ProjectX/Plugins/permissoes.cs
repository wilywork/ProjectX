using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{

    public class Permission
    {

        public static bool HasPermission(ulong idUser, string permission)
        {
            if (ProjectX.permList.ContainsKey(idUser))
            {
                if (ProjectX.permList[idUser].Contains(permission))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool UnPermission(ulong idUser, string permission)
        {
            if (ProjectX.permList.ContainsKey(idUser))
            {
                if (ProjectX.permList[idUser].Contains(permission))
                {
                    ProjectX.permList[idUser].Remove(permission);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class PermissionCommand
    {

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            List<string> permissoes = new List<string>();

            Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];
            if (cachePlayer.Admin)
            {
                if (ChatArguments[0] == null || ChatArguments[1] == null)
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] Para dar permissao use:[/color]  /perm playerName permissao");
                    return;
                }

                Fougerite.Player cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                if (cachePlayer2 != null)
                {
                    //verifica se tem instancia permissao
                    if (ProjectX.permList.ContainsKey(cachePlayer2.UID))
                    {
                        permissoes = ProjectX.permList[cachePlayer2.UID];
                    }
                    else
                    {
                        ProjectX.permList.Add(cachePlayer2.UID, new List<string>());
                        permissoes.Clear();
                    }

                    if (permissoes.Count == 0)
                    {
                        permissoes.Add(ChatArguments[1]);
                        ProjectX.permList[cachePlayer2.UID] = permissoes;
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] Permissão:[color #ffffff] " + ChatArguments[1] + "[/color] adicionada para:[color #ffffff] " + cachePlayer2.Name + "[/color] com sucesso!");
                        return;
                    }

                    if (permissoes.Contains(ChatArguments[1]))
                    {
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] O Player [color #ffffff]" + ChatArguments[0] + "[/color] já tem a permissão: [/color]" + ChatArguments[1]);
                    }
                    else
                    {
                        ProjectX.permList[cachePlayer2.UID].Add(ChatArguments[1]);
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] Permissão:[color #ffffff] " + ChatArguments[1] + "[/color] adicionada para:[color #ffffff] " + cachePlayer2.Name + "[/color] com sucesso!");
                    }


                }
                else if (ChatArguments[0].Length == 17)
                {

                    ulong SteamIdPlayer = Convert.ToUInt64(String.Join("", System.Text.RegularExpressions.Regex.Split(ChatArguments[0], @"[^\d]")));

                    //verifica se tem instancia permissao
                    if (ProjectX.permList.ContainsKey(SteamIdPlayer))
                    {
                        permissoes = ProjectX.permList[SteamIdPlayer];
                    }
                    else
                    {
                        permissoes.Clear();
                    }

                    if (permissoes.Count == 0)
                    {
                        permissoes.Add(ChatArguments[1]);
                        ProjectX.permList[SteamIdPlayer] = permissoes;
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color green] Permissão:[color #ffffff] " + ChatArguments[1] + "[/color] adicionada para:[color #ffffff] " + SteamIdPlayer + "[/color] com sucesso!");
                        return;
                    }

                    if (permissoes.Contains(ChatArguments[1]))
                    {
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] O Player [color #ffffff]" + ChatArguments[0] + "[/color] já tem a permissão: [/color]" + ChatArguments[1]);
                    }
                    else
                    {
                        ProjectX.permList[SteamIdPlayer].Add(ChatArguments[1]);
                        cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color green] Permissão:[color #ffffff] " + ChatArguments[1] + "[/color] adicionada para:[color #ffffff] " + SteamIdPlayer + "[/color] com sucesso!");
                    }

                }
                else
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] Player não encontrado:[/color] " + ChatArguments[0]);
                }

            }
            else
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }

        }
    }

    public class UnPermissionCommand
    {

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            ulong SteamIdPlayer = new ulong();

            Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];
            if (cachePlayer.Admin)
            {
                if (ChatArguments.Length < 2 || ChatArguments[1] == null)
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] Para remover permissão use:[/color]  /unperm playerName permissao.");
                    return;
                }

                Fougerite.Player cachePlayer2 = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                if (cachePlayer2 != null)
                {
                    SteamIdPlayer = cachePlayer2.UID;
                } else if(ChatArguments[0].Length == 17)
                {
                    SteamIdPlayer = Convert.ToUInt64(String.Join("", System.Text.RegularExpressions.Regex.Split(ChatArguments[0], @"[^\d]")));
                }

                if (Permission.UnPermission(SteamIdPlayer, ChatArguments[1]))
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color green] Permissão:[color #ffffff] " + ChatArguments[1] + "[/color] removida do player:[color #ffffff] " + ChatArguments[0] + " | " + SteamIdPlayer + "[/color] com sucesso!");
                }
                else
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, "[color orange] O player: [color #ffffff]" + ChatArguments[0] + "[/color] não tem a permissão: [color #ffffff]" + ChatArguments[1]);
                }
            }
            else
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }
}
