using Facepunch.Utility;
using RustProto;
using System;
using UnityEngine;
using ProjectX.Plugins;

public class console : ConsoleSystem
{
    [Admin]
    public static void saveall(ref ConsoleSystem.Arg arg)
    {
        try
        {
            ProjectX.ProjectX.SaveFiles();
            saveSystemCommand.SaveMap(ServerSaveManager.autoSavePath);
            arg.ReplyWith("Salvando mapa.");
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex);
        }

    }

    [Admin]
    public static void unbanall(ref ConsoleSystem.Arg arg)
    {
        arg.ReplyWith("Cache Fougerite Liberado.");
        try
        {
            unbanSystemCommand.UnbanAll();
        }
        catch (Exception ex)
        {
            Debug.Log("Error: "+ ex);
        }
    }

    [Admin]
    public static void restart(ref ConsoleSystem.Arg arg)
    {
        arg.ReplyWith("Executando desligamento.");
        try
        {
            ProjectX.ProjectX.SaveFiles();
            restartSystemCommand.restartServer();
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex);
        }
    }

}

