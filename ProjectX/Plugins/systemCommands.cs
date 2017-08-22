using Facepunch.Clocks.Counters;
using Fougerite;
using RustProto;
using RustProto.Helpers;
using System;
using System.IO;
using UnityEngine;

namespace ProjectX.Plugins
{
    public class saveSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    if (ChatArguments[0] == "all")
                    {
                        ProjectX.SaveFiles();
                        ConsoleSystem.Run("save.all");
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Save all salvo com sucesso.");
                    }
                    else if (ChatArguments[0] == "files")
                    {
                        ProjectX.SaveFiles();
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Save files salvo com sucesso.");
                    }
                    else if (ChatArguments[0] == "all2")
                    {
                        ProjectX.SaveFiles();
                        SaveMap(ServerSaveManager.autoSavePath);
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Save all salvo com sucesso.");                   
                    }
                    else
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /save all e /save files.");
                    }
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /save all e /save files.");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
        public static void SaveMap(string path)
        {
           // Loom.ExecuteInBiggerStackThread(() => {
                try
                {
                    SystemTimestamp restart;
                    SystemTimestamp systemTimestamp;
                    WorldSave worldSave;
                    SystemTimestamp restart1 = SystemTimestamp.Restart;

                    if (path == string.Empty)
                    {
                        path = "savedgame.sav";
                    }
                    if (!path.EndsWith(".sav"))
                    {
                        path = string.Concat(path, ".sav");
                    }
                    if (ServerSaveManager._loading)
                    {
                        UnityEngine.Debug.LogError(string.Concat("Currently loading, aborting save to ", path));
                        return;
                    }
                    UnityEngine.Debug.Log(string.Concat("Saving to '", path, "'"));
                    if (!ServerSaveManager._loadedOnce)
                    {
                        if (File.Exists(path))
                        {
                            string str = string.Concat(new string[] { path, ".", ServerSaveManager.DateTimeFileString(File.GetLastWriteTime(path)), ".", ServerSaveManager.DateTimeFileString(DateTime.Now), ".bak" });
                            File.Copy(path, str);
                            UnityEngine.Debug.LogError(string.Concat("A save file exists at target path, but it was never loaded!\n\tbacked up:", Path.GetFullPath(str)));
                        }
                        ServerSaveManager._loadedOnce = true;
                    }
                    using (Recycler<WorldSave, WorldSave.Builder> recycler = WorldSave.Recycler())
                    {
                        WorldSave.Builder builder = recycler.OpenBuilder();
                        restart = SystemTimestamp.Restart;
                        ServerSaveManager.Get(false).DoSave(ref builder);
                        restart.Stop();
                        systemTimestamp = SystemTimestamp.Restart;
                        worldSave = builder.Build();
                        systemTimestamp.Stop();
                    }
                    int sceneObjectCount = worldSave.SceneObjectCount + worldSave.InstanceObjectCount;

                    SystemTimestamp systemTimestamp1 = SystemTimestamp.Restart;
                    SystemTimestamp systemTimestamp2 = systemTimestamp1;
                    SystemTimestamp systemTimestamp3 = systemTimestamp1;
                    using (FileStream fileStream1 = File.Open(string.Concat(path, ".new"), FileMode.Create, FileAccess.Write))
                    {
                        worldSave.WriteTo(fileStream1);
                        fileStream1.Flush();
                    }
                    systemTimestamp2.Stop();
                    if (File.Exists(string.Concat(path, ".old.5")))
                    {
                        File.Delete(string.Concat(path, ".old.5"));
                    }
                    for (int i = 5; i >= 0; i--)
                    {
                        if (File.Exists(string.Concat(path, ".old.", i)))
                        {
                            File.Move(string.Concat(path, ".old.", i), string.Concat(path, ".old.", i + 1));
                        }
                    }
                    if (File.Exists(path))
                    {
                        File.Move(path, string.Concat(path, ".old.0"));
                    }
                    if (File.Exists(string.Concat(path, ".new")))
                    {
                        File.Move(string.Concat(path, ".new"), path);
                    }
                    systemTimestamp3.Stop();
                    restart1.Stop();
                    if (!save.profile)
                    {
                        Logger.LogDebug("[SaveFiles] Saved  objs:" + sceneObjectCount + " " + restart1.ElapsedSeconds + "seconds, data: "+ ProjectX.DataAtual().ToString());
                        ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color green]Mapa salvo, [color #ffffff]" + restart1.ElapsedSeconds  + " segundos.");
                        UnityEngine.Debug.Log("[SaveFiles] Saved  objs:" + sceneObjectCount + " " + restart1.ElapsedSeconds + "seconds, data: " + ProjectX.DataAtual().ToString());
                    }
                    else
                    {
                        UnityEngine.Debug.Log(string.Format(" Saved {0} Object(s) [times below are in elapsed seconds]\r\n  Logic:\t{1,-16:0.000000}\t{2,7:0.00%}\r\n  Build:\t{3,-16:0.000000}\t{4,7:0.00%}\r\n  Stream:\t{5,-16:0.000000}\t{6,7:0.00%}\r\n  All IO:\t{7,-16:0.000000}\t{8,7:0.00%}\r\n  Total:\t{9,-16:0.000000}\t{10,7:0.00%}", new object[] { sceneObjectCount, restart.ElapsedSeconds, restart.ElapsedSeconds / restart1.ElapsedSeconds, systemTimestamp.ElapsedSeconds, systemTimestamp.ElapsedSeconds / restart1.ElapsedSeconds, systemTimestamp2.ElapsedSeconds, systemTimestamp2.ElapsedSeconds / restart1.ElapsedSeconds, systemTimestamp3.ElapsedSeconds, systemTimestamp3.ElapsedSeconds / restart1.ElapsedSeconds, restart1.ElapsedSeconds, restart1.ElapsedSeconds / restart1.ElapsedSeconds }));
                    }
                }
                catch (Exception ex)
                {
                    ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Falha ao salvar o mapa, servidor será reiniciado!!");
                    ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Falha ao salvar o mapa, servidor será reiniciado!!");
                    ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Falha ao salvar o mapa, servidor será reiniciado!!");
                    Logger.LogDebug("[SaveFiles] Erro save map:" + ex);
                    TimerEdit.TimerEvento.Once(5, () => {
                        restartSystemCommand.restartServer();
                    });
                }
           // });
        }

    }

    public class restartSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    if (ChatArguments[0] == "ok")
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Contagem para reiniciamento ativada.");
                        restartServer();
                    }
                    else
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você precisa confirmar o reiniciamento usando /restart ok.");
                    }
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Você precisa confirmar o reiniciamento usando /restart ok.");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }

        public static void restartServer() {
            ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Servidor será reinicado em [color #ffffff]30[/color] segundos.");
            Debug.Log("Servidor será reinicado em 30 segundos.");

            TimerEdit.TimerEvento.Once(10, () => {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Servidor será reinicado em [color #ffffff]20[/color] segundos.");
                Debug.Log("Servidor será reinicado em 20 segundos.");
            });
            TimerEdit.TimerEvento.Once(20, () => {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Servidor será reinicado em [color #ffffff]10[/color] segundos.");
                Debug.Log("Servidor será reinicado em 10 segundos.");
            });
            TimerEdit.TimerEvento.Once(25, () => {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Servidor será reinicado em [color #ffffff]5[/color] segundos.");
                Debug.Log("Servidor será reinicado em 5 segundos.");
            });
            TimerEdit.TimerEvento.Once(30, () => {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color red]Servidor será reinicado.");
                ConsoleSystem.Run("quit");
            });
        }
    }

    public class avisoSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    string mensagem = "";
                    foreach (string value in ChatArguments)
                    {
                        mensagem = mensagem + " " + value;
                    }

                    foreach (var player in PlayerClient.All)
                    {
                        if (player.netUser != null)
                        {
                            ProjectX.SendPopup(player.netUser, "15", "☢", mensagem);
                        }
                    }
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Informe uma mensagem para enviar o aviso.");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }

    public class reloadSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    if (ChatArguments[0] == "ok")
                    {
                        ProjectX.SaveFiles();
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Save configs salva com sucesso.");
                        TimerEdit.TimerEvento.Once(20, () => {
                            ConsoleSystem.Run("Fougerite.reload");
                            Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Plugins recarregados");
                        });
                    }
                    else
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /reload ok");
                    }
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /reload ok");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }

    public class findHomeSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if ( Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") )
            {
                if (ChatArguments.Length > 0)
                {
                    Vector3 oldLocation = new Vector3(0, 0, 0);

                    foreach (var struc in Resources.FindObjectsOfTypeAll(typeof(StructureComponent)))
                    {
                        try
                        {
                            if ((struc as StructureComponent)._master != null)
                            {
                                if (ChatArguments[0] == (struc as StructureComponent)._master.ownerID.ToString())
                                {
                                    if (Math.Floor(Vector3.Distance((struc as StructureComponent)._master.transform.position, oldLocation)) > 150)
                                    {
                                        oldLocation = (struc as StructureComponent)._master.transform.position;
                                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Location : [/color]" + (struc as StructureComponent)._master.transform.position.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    foreach (var strucMaster in Resources.FindObjectsOfTypeAll(typeof(StructureMaster)))
                    {
                        try
                        {
                            if (ChatArguments[0] == (strucMaster as StructureMaster).ownerID.ToString())
                            {
                                if (Math.Floor(Vector3.Distance((strucMaster as StructureMaster).transform.position, oldLocation)) > 150)
                                {
                                    oldLocation = (strucMaster as StructureMaster).transform.position;
                                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]Location : [/color]" + (strucMaster as StructureMaster).transform.position.ToString());
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Fim.");

                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /findHome SteamID");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }

    public class unbanSystemCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin") || Permission.HasPermission(Arguments.argUser.userID, "mod"))
            {
                if (ChatArguments.Length > 0)
                {
                    if (ChatArguments[0] == "all")
                    {
                        UnbanAll();
                        Fougerite.DataStore.GetInstance().Save();
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green]unban all executado com sucesso!");
                    }
                    else
                    {
                        Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /unban all");
                    }
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Comandos válidos: /unban all");
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }

        public static void UnbanAll() {
            //unbanall ids
            if (Fougerite.DataStore.GetInstance() != null)
            {
                var listaIds = Fougerite.DataStore.GetInstance().Keys("Ids");
                if (listaIds != null && listaIds.Length > 0)
                {
                    foreach (var ids in listaIds)
                    {
                        Fougerite.DataStore.GetInstance().Remove("Ids", ids);
                    }
                }
                //unbanall ips
                Fougerite.DataStore.GetInstance().Keys("Ips");
                var listaIps = Fougerite.DataStore.GetInstance().Keys("Ips");
                if (listaIps != null && listaIps.Length > 0)
                {
                    foreach (var ips in listaIps)
                    {
                        Fougerite.DataStore.GetInstance().Remove("Ips", ips);
                    }
                }
            }
        }

    }

}
