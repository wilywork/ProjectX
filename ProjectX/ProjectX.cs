using System.Collections.Generic;
using System;
using System.IO;
using Rust;
using Fougerite;
using Fougerite.Events;
using ProjectX.extensoes;
using UnityEngine;
using ProjectX.Plugins;
using TimerEdit;

namespace ProjectX
{
    public class ProjectX
    {
        //====================== Variaveis default ========================
        //used in teleport
        public static RustServerManagement management;

        public static string ConfigsFolder;
        //create folds defaults
        public static List<string> DefualtFolders = new List<string>() { "config", "data" };
        //itens do jogo
        public static Dictionary<string, ItemDataBlock> displaynameToDataBlock = new Dictionary<string, ItemDataBlock>();
        //mutes
        public static List<ulong> muteList = new List<ulong>();
        //friends
        public static Dictionary<ulong, List<string>> friendList = new Dictionary<ulong, List<string>>();
        //shares
        public static Dictionary<ulong, List<string>> shareList = new Dictionary<ulong, List<string>>();
        //homes
        public static Dictionary<ulong, Dictionary<string, string>> homeList = new Dictionary<ulong, Dictionary<string, string>>();
        //userCache
        public static Dictionary<ulong, Dictionary<string, string>> userCache = new Dictionary<ulong, Dictionary<string, string>>();
        //kits
        public static Dictionary<ulong, Dictionary<string, double>> cacheKits = new Dictionary<ulong, Dictionary<string, double>>();
        //permissoes
        public static Dictionary<ulong, List<string>> permList = new Dictionary<ulong, List<string>>();
        //time atual
        public static DateTime epoch = new System.DateTime(1970, 1, 1);

        //=========================== Config ===============================
        public class Config
        {
            public string NameServer;
            public string WarnNotPermission;
            public string WarnInvalidCommand;
            public string InventoryFull;
            public int totalDeDiasSemlogar;
            public float timerSaveFilesSeconds;
            public int limitFriends;
            public int limitShares;

            public Config Default()
            {
                NameServer = "Server";
                WarnNotPermission = "[color red]Você não tem permissão para usar este comando.";
                WarnInvalidCommand = "[color yellow] Este Comando não existe.";
                InventoryFull = "Inventário cheio!";
                totalDeDiasSemlogar = 5;
                timerSaveFilesSeconds = 3600;
                limitShares = 6;
                limitFriends = 5;
                return this;
            }
        }

        public static Config configServer = new Config();
        //=================================================================

        //============================ funcoes ============================

        public static double TimeSeconds()
        {
            return System.DateTime.UtcNow.Subtract(ProjectX.epoch).TotalSeconds;
        }

        public static DateTime DataAtual()
        {
            return DateTime.Now;
        }

        public static string QuoteSafe(string str) => "\"" + str.Replace("\"", "\\\"").TrimEnd(new char[] { '\\' }) + "\"";

        public static string GetDirection(Vector3 targetposition, Vector3 playerposition)
        {
            if (Vector3.Distance(targetposition, playerposition) < 10) return string.Empty;
            string northsouth;
            string westeast;
            string direction = string.Empty;
            if (playerposition.x < targetposition.x)
                northsouth = "Sul";
            else
                northsouth = "Norte";
            if (playerposition.z < targetposition.z)
                westeast = "Leste";
            else
                westeast = "Oeste";
            var diffx = Math.Abs(playerposition.x - targetposition.x);
            var diffz = Math.Abs(playerposition.z - targetposition.z);
            if (diffx / diffz <= 0.5) direction = westeast;
            if (diffx / diffz > 0.5 && diffx / diffz < 1.5) direction = northsouth + "-" + westeast;
            if (diffx / diffz >= 1.5) direction = northsouth;
            return direction;
        }

        public static void BroadCast(string prefix, string msg)
        {
            ConsoleNetworker.Broadcast($"chat.add {QuoteSafe(prefix)} { QuoteSafe(msg).Replace("\\\"", "\"")}");
        }

        public static void TeleportPlayer(NetUser player, Vector3 pos)
        {
            if (player == null || player.playerClient == null)
                return;
            if (ProjectX.management == null)
            {
                ProjectX.management = RustServerManagement.Get();
            }

            BreakLegs(player.playerClient);

            ProjectX.management.TeleportPlayerToWorld(player.playerClient.netPlayer, pos);

            TimerEdit.TimerEvento.Once(2, () => {
                if (player != null && player.playerClient != null)
                {
                    ProjectX.management.TeleportPlayerToWorld(player.playerClient.netPlayer, pos);
                }
            });

            TimerEdit.TimerEvento.Once(3, () => {
                if (player != null && player.playerClient != null)
                {
                    UnbreakLegs(player.playerClient);
                }
            });

        }

        public static void BreakLegs(PlayerClient player)
        {
            if (player == null) return;
            if (player.controllable == null) return;
            player.controllable.GetComponent<FallDamage>().AddLegInjury(1);

        }

        public static void UnbreakLegs(PlayerClient player)
        {
            if (player == null) return;
            if (player.controllable == null) return;
            player.rootControllable.GetComponent<HumanBodyTakeDamage>().Bandage(1000.0f);
            player.controllable.GetComponent<FallDamage>().ClearInjury();
        }

        public static void SendPopup(NetUser netUser, string temp, string icon, string message)
        {
            ConsoleNetworker.SendClientCommand(netUser.networkPlayer, $"notice.popup {QuoteSafe(temp)} {QuoteSafe(icon)} {QuoteSafe(message)}");
        }

        public static void Destroyed(Entity item)
        {
            if (item != null)
            {
                if (item.IsDeployableObject())
                {
                    NetCull.Destroy(item.GetObject<DeployableObject>().gameObject);
                }
                else if (item.IsStructure())
                {
                    NetCull.Destroy(item.GetObject<StructureComponent>().gameObject);
                }
                else if (item.IsStructureMaster())
                {
                    NetCull.Destroy(item.GetObject<StructureMaster>().gameObject);
                }
            }
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(ProjectX.ConfigsFolder, fileName);
        }

        public static T ReadyConfigChecked<T>(T obj, string pathFile)
        {
            try
            {
                if (File.Exists(GetAbsoluteFilePath(pathFile)))
                {
                    return JsonHelper.ReadyFile<T>(GetAbsoluteFilePath(pathFile));
                }
                else
                {
                    JsonHelper.SaveFile(obj, GetAbsoluteFilePath(pathFile));
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Falha ao ler path: " + pathFile + "Error: " + ex);
                return default(T);
            }
        }

        public static void CreateFolderCheck(List<string> folders)
        {

            foreach (string folder in folders)
            {
                if (!Directory.Exists(GetAbsoluteFilePath(folder)))
                {
                    Directory.CreateDirectory(GetAbsoluteFilePath(folder));
                }
            }

        }

        public static void AddItemInventory(Fougerite.Player player, ItemDataBlock item, int qtd)
        {
            if (player != null && player.PlayerClient != null && player.PlayerClient.rootControllable != null)
            {

                Inventory inv = player.PlayerClient.rootControllable.idMain.GetComponent<Inventory>();
                if (inv != null)
                {
                    if (inv.occupiedSlotCount == 36)
                    {
                        SendPopup(player.PlayerClient.netUser, "5", "☢", configServer.InventoryFull);
                    }
                    else
                    {
                        inv.AddItemAmount(item, qtd);
                        player.InventoryNotice(qtd + " x " + item.name);
                    }
                }
            }
        }

        //============================ Classes ==================================

        public class StoragePlayer : MonoBehaviour
        {
            public string oldUserPm;
            public bool showDamange = true;
        }

        public class ResourcesItens
        {
            public static Dictionary<string, Dictionary<string, int>> IngredientesItem;

            public static void Init()
            {
                if(IngredientesItem == null)
                {
                    IngredientesItem = new Dictionary<string, Dictionary<string, int>>();
                    IngredientesItem.Add("Bed", new Dictionary<string, int>() { { "cloth", 40 }, { "metal fragments", 100 } });
                    IngredientesItem.Add("Camp Fire", new Dictionary<string, int>() { { "wood", 5 } });
                    IngredientesItem.Add("Furnace", new Dictionary<string, int>() { { "stones", 15 }, { "wood", 20 }, { "low grade fuel", 10 } });
                    IngredientesItem.Add("LargeSpikeWall", new Dictionary<string, int>() { { "wood", 200 } });
                    IngredientesItem.Add("LargeWoodStorage", new Dictionary<string, int>() { { "wood", 60 } });
                    IngredientesItem.Add("MetalCeiling", new Dictionary<string, int>() { { "low quality metal", 6 } });
                    IngredientesItem.Add("MetalDoor", new Dictionary<string, int>() { { "metal fragments", 200 } });
                    IngredientesItem.Add("MetalDoorFrame", new Dictionary<string, int>() { { "low quality metal", 4 } });
                    IngredientesItem.Add("MetalFoundation", new Dictionary<string, int>() { { "low quality metal", 8 } });
                    IngredientesItem.Add("MetalPillar", new Dictionary<string, int>() { { "low quality metal", 2 } });
                    IngredientesItem.Add("MetalRamp", new Dictionary<string, int>() { { "low quality metal", 5 } });
                    IngredientesItem.Add("MetalStairs", new Dictionary<string, int>() { { "low quality metal", 5 } });
                    IngredientesItem.Add("MetalWall", new Dictionary<string, int>() { { "low quality metal", 4 } });
                    IngredientesItem.Add("MetalWindowFrame", new Dictionary<string, int>() { { "low quality metal", 4 } });
                    //IngredientesItem.Add("MetalWindowBars", new Dictionary<string, int>() { { "stones",1} });
                    IngredientesItem.Add("Repair Bench", new Dictionary<string, int>() { { "stones", 12 }, { "wood", 60 }, { "metal fragments", 50 }, { "low grade fuel", 6 } });
                    IngredientesItem.Add("Sleeping Bag", new Dictionary<string, int>() { { "cloth", 15 } });
                    IngredientesItem.Add("Small Stash", new Dictionary<string, int>() { { "leather", 10 } });
                    IngredientesItem.Add("Spike Wall", new Dictionary<string, int>() { { "wood", 100 } });
                    IngredientesItem.Add("WoodBarricade", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("Wood Barricade", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("WoodCeiling", new Dictionary<string, int>() { { "wood planks", 6 } });
                    IngredientesItem.Add("WoodDoorFrame", new Dictionary<string, int>() { { "wood planks", 4 } });
                    IngredientesItem.Add("WoodenDoorFrame", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("WoodenDoor", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("WoodFoundation", new Dictionary<string, int>() { { "wood planks", 8 } });
                    IngredientesItem.Add("WoodGate", new Dictionary<string, int>() { { "wood", 120 } });
                    IngredientesItem.Add("WoodGateway", new Dictionary<string, int>() { { "wood", 400 } });
                    IngredientesItem.Add("WoodPillar", new Dictionary<string, int>() { { "wood planks", 2 } });
                    IngredientesItem.Add("WoodRamp", new Dictionary<string, int>() { { "wood planks", 5 } });
                    IngredientesItem.Add("WoodShelter", new Dictionary<string, int>() { { "wood", 50 } });
                    IngredientesItem.Add("WoodStairs", new Dictionary<string, int>() { { "wood planks", 5 } });
                    IngredientesItem.Add("WoodStorageBox", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("WoodWall", new Dictionary<string, int>() { { "wood planks", 4 } });
                    IngredientesItem.Add("WoodWindowFrame", new Dictionary<string, int>() { { "wood planks", 4 } });
                    IngredientesItem.Add("Workbench", new Dictionary<string, int>() { { "stones", 8 }, { "wood", 50 } });
                    IngredientesItem.Add("WoodBox", new Dictionary<string, int>() { { "wood", 30 } });
                    IngredientesItem.Add("WoodBoxLarge", new Dictionary<string, int>() { { "wood", 60 } });
                    IngredientesItem.Add("LargeWoodSpikeWall", new Dictionary<string, int>() { { "wood", 200 } });
                    IngredientesItem.Add("WoodSpikeWall", new Dictionary<string, int>() { { "wood", 100 } });
                }
            }
        }

        //=======================================================================

        //========================== ready e save files =========================

        public static void ReadyFile()
        {
            ProjectX.friendList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(ProjectX.friendList, "data/friendList.json");
            ProjectX.shareList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(ProjectX.shareList, "data/shareList.json");
            ProjectX.permList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(ProjectX.permList, "data/permList.json");
            ProjectX.homeList = ProjectX.ReadyConfigChecked<Dictionary<ulong, Dictionary<string, string>>>(ProjectX.homeList, "data/homeList.json");
            ProjectX.userCache = ProjectX.ReadyConfigChecked<Dictionary<ulong, Dictionary<string, string>>>(ProjectX.userCache, "data/userCache.json");
            ProjectX.cacheKits = ProjectX.ReadyConfigChecked<Dictionary<ulong, Dictionary<string, double>>>(ProjectX.cacheKits, "data/cacheKits.json");
        }

        public static void SaveFiles()
        {
            if (friendList.Count != 0)
            {
                Logger.Log("Saving friendList.");
                JsonHelper.SaveFile(friendList, GetAbsoluteFilePath("data/friendList.json"));
            }

            if (ProjectX.homeList.Count != 0)
            {
                Logger.Log("Saving homeList.");
                JsonHelper.SaveFile(homeList, GetAbsoluteFilePath("data/homeList.json"));
            }

            if (ProjectX.userCache.Count != 0)
            {
                Logger.Log("Saving userCache.");
                JsonHelper.SaveFile(userCache, GetAbsoluteFilePath("data/userCache.json"));
            }

            if (ProjectX.permList.Count != 0)
            {
                Logger.Log("Saving permissoes.");
                JsonHelper.SaveFile(permList, GetAbsoluteFilePath("data/permList.json"));
            }

            if (ProjectX.shareList.Count != 0)
            {
                Logger.Log("Saving shares.");
                JsonHelper.SaveFile(shareList, GetAbsoluteFilePath("data/shareList.json"));
            }

            if (ProjectX.cacheKits.Count != 0)
            {
                Logger.Log("Saving cacheKits.");
                JsonHelper.SaveFile(cacheKits, GetAbsoluteFilePath("data/cacheKits.json"));
            }

        }

        //=======================================================================
    }

    public class ProjectXModule : Fougerite.Module
    {
        public override string Name
        {
            get { return "ProjectX"; }
        }
        public override string Author
        {
            get { return "WilyWork"; }
        }
        public override string Description
        {
            get { return "ProjectX ToolsPlugins"; }
        }
        public override Version Version
        {
            get { return new Version("1.5.0"); }
        }

        //=================================================================

        public override void Initialize()
        {
            try
            {
                ProjectX.ConfigsFolder = ModuleFolder;

                //creat folders if not exist
                ProjectX.CreateFolderCheck(ProjectX.DefualtFolders);

                //init timer
                TimerEditStart.Init();

                //ready configServer
                ProjectX.configServer = ProjectX.ReadyConfigChecked<ProjectX.Config>(ProjectX.configServer.Default(), "configServer.json");

                try
                {
                    // start ready files json's config's
                    HelpCommand.Start();
                    Notices.Start();
                    GatherMultipler.Start();
                    PlayerConnection.Start();
                    CraftBlock.Start();
                    Chat.Start();
                    Airdrop.Start();
                    Kits.Start();
                    BuildConstructor.Start();
                    RegrasCommand.Start();
                    AntiGlith.Start();
                    ShowDamange.Start();
                }
                catch (Exception ex)
                {
                    Logger.Log("[Error] Start config's: " + ex);
                }


                ProjectX.ResourcesItens.Init();

                //disable autosave native, 11574 days
                ConsoleSystem.Run("save.autosavetime 999999999");

                //start timer repeat saveFiles
                Logger.Log("Sistema de salvamento a cada " + (ProjectX.configServer.timerSaveFilesSeconds / 60).ToString() + " minutes.");
                TimerEvento.Repeat(ProjectX.configServer.timerSaveFilesSeconds, 0, () =>
                {
                    Logger.Log("=== Savalndo configuracoes e mapa ===");
                    ProjectX.SaveFiles();
                    saveSystemCommand.SaveMap(ServerSaveManager.autoSavePath);
                });

                ProjectX.ReadyFile();

                Fougerite.Hooks.OnChatRaw += ChatReceived;
                Fougerite.Hooks.OnTablesLoaded += InitializeTable;
                Fougerite.Hooks.OnCrafting += CraftBlock.HookOnCrafting;
                Fougerite.Hooks.OnPlayerGathering += GatherMultipler.HookOnPlayerGathering;
                Fougerite.Hooks.OnDoorUse += ShareCommand.DoorUse;
                Fougerite.Hooks.OnEntityHurt += ShowDamange.EntityHurt;
                Fougerite.Hooks.OnPlayerConnected += PlayerConnection.HookPlayerConnect;
                Fougerite.Hooks.OnPlayerDisconnected += PlayerConnection.HookPlayerDisconnect;
                Fougerite.Hooks.OnPlayerHurt += ShowDamange.PlayerHurt;
                Fougerite.Hooks.OnPlayerKilled += ShowDamange.PlayerKilled;
                Fougerite.Hooks.OnNPCHurt += ShowDamange.NPCHurt;
                Fougerite.Hooks.OnEntityDestroyed += ShowDamange.OnEntityDestroyed;
                Fougerite.Hooks.OnServerShutdown += OnServerShutdown;
                Fougerite.Hooks.OnShowTalker += ShowTalker;
                Fougerite.Hooks.OnChat += Chat.HookChat;
                Fougerite.Hooks.OnAirdropCalled += Airdrop.HookOnAirdrop;
                Fougerite.Hooks.OnSteamDeny += OnSteamDeny;
                Fougerite.Hooks.OnEntityDeployedWithPlacer += AntiGlith.EntityDeployed;
                Fougerite.Hooks.OnPlayerSpawned += AntiGlith.OnPlayerSpawned;
            }
            catch (Exception ex)
            {
                Logger.Log("[Error] Initialize: " + ex);
            }

        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnChatRaw -= ChatReceived;
            Fougerite.Hooks.OnTablesLoaded -= InitializeTable;
            Fougerite.Hooks.OnCrafting -= CraftBlock.HookOnCrafting;
            Fougerite.Hooks.OnPlayerGathering -= GatherMultipler.HookOnPlayerGathering;
            Fougerite.Hooks.OnDoorUse -= ShareCommand.DoorUse;
            Fougerite.Hooks.OnEntityHurt -= ShowDamange.EntityHurt;
            Fougerite.Hooks.OnPlayerConnected -= PlayerConnection.HookPlayerConnect;
            Fougerite.Hooks.OnPlayerDisconnected -= PlayerConnection.HookPlayerDisconnect;
            Fougerite.Hooks.OnPlayerHurt -= ShowDamange.PlayerHurt;
            Fougerite.Hooks.OnPlayerKilled -= ShowDamange.PlayerKilled;
            Fougerite.Hooks.OnNPCHurt -= ShowDamange.NPCHurt;
            Fougerite.Hooks.OnEntityDestroyed -= ShowDamange.OnEntityDestroyed;
            Fougerite.Hooks.OnServerShutdown -= OnServerShutdown;
            Fougerite.Hooks.OnShowTalker -= ShowTalker;
            Fougerite.Hooks.OnChat -= Chat.HookChat;
            Fougerite.Hooks.OnAirdropCalled -= Airdrop.HookOnAirdrop;
            Fougerite.Hooks.OnSteamDeny -= OnSteamDeny;
            Fougerite.Hooks.OnEntityDeployedWithPlacer -= AntiGlith.EntityDeployed;
            Fougerite.Hooks.OnPlayerSpawned -= AntiGlith.OnPlayerSpawned;

            //disable timer
            TimerEditStart.DeInitialize();

            ProjectX.SaveFiles();
        }

        void ChatReceived(ref ConsoleSystem.Arg Arguments)
        {
            string displayname = Arguments.argUser.user.Displayname;
            if (String.IsNullOrEmpty(Arguments.GetString(0)) || Arguments.GetString(0) == "\"\"")
            {
                return;
            }
            string[] strArray = Arguments.GetString(0).Trim().Split(new char[] { ' ' });
            string cmd = strArray[0].Trim();
            if (cmd.IndexOf("/", 0, 1) == -1)
            {
                return;
            }
            string[] ChatArguments = new string[strArray.Length - 1];
            for (int i = 1; i < strArray.Length; i++)
            {
                ChatArguments[i - 1] = strArray[i];
            }

            switch (cmd)
            {
                case "/dmg":
                    DamangeCommand.DammangeActiveCommand(Arguments, ChatArguments);
                    break;
                case "/dmgoff":
                    DamangeCommand.DammangeDesactiveCommand(Arguments, ChatArguments);
                    break;
                case "/share":
                    ShareCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/shares":
                    shareListCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/unshare":
                    unShareCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/addfriend":
                    FriendCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/unfriend":
                    UnFriendCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/friends":
                    FriendsCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/sethome":
                    sethomeCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/home":
                    HomeCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/pm":
                    PmCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/r":
                    ReplyPmCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/perm":
                    PermissionCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/unperm":
                    UnPermissionCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/kit":
                    KitCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/kits":
                    KitsCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/ping":
                    PingInfoCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/loc":
                    LocCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/location":
                    LocCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/players":
                    PlayersCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/steamid":
                    InfoUserCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/regras":
                    RegrasCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/rules":
                    RegrasCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/ajuda":
                    HelpCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/help":
                    HelpCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/fps":
                    FpsCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/fpsoff":
                    FpsOffCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/die":
                    SuicideCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/suicide":
                    SuicideCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/god":
                    GodModeCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/owner":
                    OwnerCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/remove":
                    RemoveCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/tp":
                    TpAdmCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/tpr":
                    TprCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/tpa":
                    TpaCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/tpc":
                    TpcCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/mute":
                    MuteCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/unmute":
                    UnMuteCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/clear":
                    ClearCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/save":
                    saveSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/aviso":
                    avisoSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/reload":
                    reloadSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/findHome":
                    findHomeSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/unban":
                    unbanSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/restart":
                    restartSystemCommand.Execute(Arguments, ChatArguments);
                    break;
                case "/fougerite":
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color #ffca2a]ProjectX Version:[/color] " + Version.ToString());
                    break;
                case "/rbban":
                    break;
                case "/rustbuster":
                    break;
                case "/rbunban":
                    break;
                default:
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnInvalidCommand);
                    break;
            }
        }

        void InitializeTable(Dictionary<string, LootSpawnList> tables)
        {
            if (ProjectX.displaynameToDataBlock != null) {
                ProjectX.displaynameToDataBlock.Clear();
            }
            foreach (ItemDataBlock itemdef in DatablockDictionary.All)
            {
                ProjectX.displaynameToDataBlock.Add(itemdef.name.ToLower(), itemdef);
            }
        }
      
        void OnServerShutdown()
        {
            ProjectX.SaveFiles();
        }

        void ShowTalker(uLink.NetworkPlayer player, Fougerite.Player p)
        {
            if (Fougerite.Hooks.talkerTimers.ContainsKey(p.UID))
            {
                if ((Environment.TickCount - ((int)Fougerite.Hooks.talkerTimers[p.UID])) < 1500)
                    return;

                Fougerite.Hooks.talkerTimers[p.UID] = Environment.TickCount;
            }
            else
            {
                Fougerite.Hooks.talkerTimers.Add(p.UID, Environment.TickCount);
            }
            Notice.Inventory(player, "➤ " + p.Name);
        }

        void OnSteamDeny(SteamDenyEvent e)
        {
            if (e.ClientConnection != null && e.ClientConnection.UserID == 76561197960266962)
            {
                return;
            }

            if (e.ErrorNumber.ToString() == "Facepunch_Connector_Cancelled")
            {
                e.ForceAllow = true;
            }
        }
    }
}
