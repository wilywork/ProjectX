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
    public class ProjectX : Fougerite.Module
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
            get { return new Version("1.3.9"); }
        }
        //====================== Variaveis default ========================
        //used in teleport
        public static RustServerManagement management;

        public static string ConfigsFolder;
        //create folds defaults
        public static List<string> DefualtFolders = new List<string>() { "config","data" };
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
                NameServer = "Death Of Rust";
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
            return System.DateTime.UtcNow.Subtract(epoch).TotalSeconds;
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
            if (management == null)
            {
                management = RustServerManagement.Get();
            }

            BreakLegs(player.playerClient);

            management.TeleportPlayerToWorld(player.playerClient.netPlayer, pos);

            TimerEdit.TimerEvento.Once(2, () => {
                if (player != null && player.playerClient != null)
                {
                    management.TeleportPlayerToWorld(player.playerClient.netPlayer, pos);
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
            return Path.Combine(ConfigsFolder, fileName);
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
                if (!Directory.Exists(ProjectX.GetAbsoluteFilePath(folder)))
                {
                    Directory.CreateDirectory(ProjectX.GetAbsoluteFilePath(folder));
                }
            }

        }

        public static void AddItemInventory(Fougerite.Player player, string item, int qtd) {
            if (player.Inventory.InternalInventory.occupiedSlotCount == 36)
            {
                SendPopup(player.PlayerClient.netUser, "5", "☢", configServer.InventoryFull);
            }
            else
            {
                player.Inventory.AddItem(item, qtd);
                player.InventoryNotice(qtd + " x " + item);
            }
        }

        //=================================================================

        public override void Initialize()
        {
            ConfigsFolder = ModuleFolder;

            //creat folders if not exist
            CreateFolderCheck(DefualtFolders);

            //init timer
            TimerEditStart.Init();

            //ready configServer
            configServer = ProjectX.ReadyConfigChecked<Config>(configServer.Default(), "configServer.json");

            // start ready files json's config's
            HelpCommand.Start();
            Notices.Start();
            GatherMultipler.Start();
            PlayerConnection.Start();
            CraftBlock.Start();
            Chat.Start();
            Airdrop.Start();
            Kits.Start();
            RegrasCommand.Start();

            ResourcesItens.Init();

            //disable autosave native, 11574 days
            ConsoleSystem.Run("save.autosavetime 999999999");

            //start timer repeat saveFiles
            Logger.Log("Sistema de salvamento a cada " + (configServer.timerSaveFilesSeconds/60).ToString() + " minutos.");
            TimerEvento.Repeat(configServer.timerSaveFilesSeconds, 0, () =>
            {
                Logger.Log("=== Savalndo configuracoes e mapa ===");
                SaveFiles();
                saveSystemCommand.SaveMap(ServerSaveManager.autoSavePath);
            });

            ReadyFile();

            Fougerite.Hooks.OnChatRaw += ChatReceived;
            Fougerite.Hooks.OnTablesLoaded += InitializeTable;
            Fougerite.Hooks.OnCrafting += CraftBlock.HookOnCrafting;
            Fougerite.Hooks.OnPlayerGathering += GatherMultipler.HookOnPlayerGathering;
            Fougerite.Hooks.OnDoorUse += DoorUse;
            Fougerite.Hooks.OnEntityHurt += EntityHurt;
            Fougerite.Hooks.OnPlayerConnected += PlayerConnection.HookPlayerConnect;
            Fougerite.Hooks.OnPlayerDisconnected += PlayerConnection.HookPlayerDisconnect;
            Fougerite.Hooks.OnPlayerHurt += PlayerHurt;
            Fougerite.Hooks.OnPlayerKilled += PlayerKilled;
            Fougerite.Hooks.OnServerShutdown += OnServerShutdown;
            //Fougerite.Hooks.OnShowTalker += ShowTalker;
            Fougerite.Hooks.OnChat += Chat.HookChat;
            Fougerite.Hooks.OnAirdropCalled += Airdrop.HookOnAirdrop;
            Fougerite.Hooks.OnSteamDeny += OnSteamDeny;
            Fougerite.Hooks.OnEntityDeployedWithPlacer += antiGlith.EntityDeployed;
            Fougerite.Hooks.OnPlayerSpawned += antiGlith.OnPlayerSpawned;
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnChatRaw -= ChatReceived;
            Fougerite.Hooks.OnTablesLoaded -= InitializeTable;
            Fougerite.Hooks.OnCrafting -= CraftBlock.HookOnCrafting;
            Fougerite.Hooks.OnPlayerGathering -= GatherMultipler.HookOnPlayerGathering;
            Fougerite.Hooks.OnDoorUse -= DoorUse;
            Fougerite.Hooks.OnEntityHurt -= EntityHurt;
            Fougerite.Hooks.OnPlayerConnected -= PlayerConnection.HookPlayerConnect;
            Fougerite.Hooks.OnPlayerDisconnected -= PlayerConnection.HookPlayerDisconnect;
            Fougerite.Hooks.OnPlayerHurt -= PlayerHurt;
            Fougerite.Hooks.OnPlayerKilled -= PlayerKilled;
            Fougerite.Hooks.OnServerShutdown -= OnServerShutdown;
            //Fougerite.Hooks.OnShowTalker -= ShowTalker;
            Fougerite.Hooks.OnChat -= Chat.HookChat;
            Fougerite.Hooks.OnAirdropCalled -= Airdrop.HookOnAirdrop;
            Fougerite.Hooks.OnSteamDeny -= OnSteamDeny;
            Fougerite.Hooks.OnEntityDeployedWithPlacer -= antiGlith.EntityDeployed;
            Fougerite.Hooks.OnPlayerSpawned -= antiGlith.OnPlayerSpawned;
        }

        //void OnEntityDestroyed(DestroyEvent de)
        //{
        //    Debug.Log("teste");
        //    NetUser killeruser = de.DamageEvent.attacker.client?.netUser ?? null;

        //    if (killeruser != null && killeruser.playerClient != null)
        //    {
        //        try
        //        {
        //            if (de.DamageEvent.victim.idMain != null)
        //            {
        //                var structure = de.DamageEvent.victim.idMain.GetComponent<StructureComponent>();
        //                var deployable = de.DamageEvent.victim.idMain.GetComponent<DeployableObject>();
        //                if (structure && structure._master != null)// && structure._master.ownerID != killeruser.userID)
        //                {
        //                    Logger.LogDebug("atacante: " + killeruser.userID.ToString() + "( " + killeruser.displayName + " ) strutura: " + structure.name.ToString() + " Dono: " + structure._master?.creatorID.ToString());
        //                }
        //                else if (deployable && deployable.creatorID != killeruser.userID)
        //                {
        //                    Logger.LogDebug("atacante: " + killeruser.userID.ToString() + "( " + killeruser.displayName + " ) strutura: " + deployable.name.ToString() + " Dono: " + deployable.creatorID.ToString());
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogDebug("[HookPlayerKilled] Erro no log de C4!" + ex);
        //        }
        //    }
        //}

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
                    shareCommand.Execute(Arguments, ChatArguments);
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
                    break;
                case "/rbban":
                    break;
                case "/rustbuster":
                    break;
                case "/rbunban":
                    break;
                default:
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(configServer.NameServer, configServer.WarnInvalidCommand);
                    break;
            }
        }

        void InitializeTable(Dictionary<string, LootSpawnList> tables)
        {
            displaynameToDataBlock.Clear();
            foreach (ItemDataBlock itemdef in DatablockDictionary.All)
            {
                displaynameToDataBlock.Add(itemdef.name.ToLower(), itemdef);
            }
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
            if (de.Entity.UOwnerID == p.UID || p.Admin)
            {
                de.Open = true;
            }
            else if (shareCommand.isShare(de.Entity.UOwnerID, p.UID))
            {
                de.Open = true;
            }
            else
            {
                de.Open = false;
            }
        }

        void EntityHurt(HurtEvent he)
        {
            try
            {
                if (he.AttackerIsPlayer && he.DamageEvent.attacker.client != null && he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>() != null)
                {
                    if (he.Entity.UOwnerID == he.DamageEvent.attacker.userID || he.DamageEvent.attacker.client.netUser.admin)
                    {
                        RemoveCommand.GiveItemRemove(he, he.Entity.Name, he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>());
                    }
                    else
                    {
                        if (shareCommand.isShare(he.Entity.UOwnerID, he.DamageEvent.attacker.userID))
                        {
                            RemoveCommand.GiveItemRemove(he, he.Entity.Name, he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>());
                        }
                        else
                        {
                            Fougerite.Server.Cache[he.DamageEvent.attacker.userID].MessageFrom(configServer.NameServer, "[color red] Você não tem permissão para remover.");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[HookOnHurtEntity] Some error showed up error 0. Report this. " + ex);
            }
        }

        void PlayerHurt(HurtEvent he)
        {
            try
            {
                if (he.AttackerIsPlayer && he.VictimIsPlayer)
                {

                    Fougerite.Player cachePlayer = (Fougerite.Player)he.Attacker;
                    Fougerite.Player cachePlayer2 = (Fougerite.Player)he.Victim;

                    if (cachePlayer.UID != cachePlayer2.UID)
                    {
                        HomeTeleport cachedHomeTeleport = cachePlayer2.PlayerClient.GetComponent<HomeTeleport>();
                        if (cachedHomeTeleport != null)
                            cachedHomeTeleport.OnCancel();

                        var cacheTpr = cachePlayer2.PlayerClient.GetComponent<TPRequest>();
                        if (cacheTpr != null)
                            cacheTpr.OnCancel();

                        if (FriendCommand.isFriend(cachePlayer.UID, cachePlayer2.UID))
                        {
                            he.DamageAmount = 0f;
                            cachePlayer.Notice("☠", "Pare é seu amigo " + cachePlayer2.Name, 3);
                        }
                        else
                        {
                            // cancela teleport
                            tpr.OnHurtPlayer(cachePlayer2.PlayerClient);

                            StoragePlayer cacheStorage = cachePlayer.PlayerClient.gameObject.GetComponent<StoragePlayer>();
                            if (cacheStorage.showDamange)
                            {
                                cachePlayer.Notice("☠", "Você acertou " + he.DamageEvent.victim.client.userName + " : " + (cachePlayer2.Health - he.DamageAmount).ToString("N0") + " | 100", 4);
                            }

                            StoragePlayer cacheStorage2 = cachePlayer2.PlayerClient.gameObject.GetComponent<StoragePlayer>();
                            if (cacheStorage2.showDamange)
                            {
                                cachePlayer2.Notice("☠", cachePlayer.Name + " Acertou você Dano: " + he.DamageAmount.ToString("N0"), 4);
                            }
                        }
                    }
                }
                else if (he.VictimIsPlayer && he.AttackerIsEntity && he.DamageEvent.attacker.idMain != null && he.DamageEvent.victim.client != null)
                {

                    Fougerite.Player cachePlayer2 = (Fougerite.Player)he.Victim;
                    if (cachePlayer2 != null)
                    {
                        if (FriendCommand.isFriend(he.DamageEvent.attacker.userID, cachePlayer2.UID))
                        {
                            he.DamageAmount = 0f;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[HookOnHurtPlayer] Some error showed up error 2. Report this. " + ex);
            }
        }

        void PlayerKilled(DeathEvent event2)
        {

            NetUser killeruser = event2.DamageEvent.attacker.client?.netUser ?? null;
            NetUser hurteduser = event2.DamageEvent.victim.client?.netUser ?? null;
            if (killeruser != null && killeruser.playerClient != null && hurteduser != null && hurteduser.playerClient != null && event2.DamageEvent.victim.idMain.GetComponent("HumanController"))
            {
                // cancela teleport
                tpr.OnHurtPlayer(hurteduser.playerClient);

                // pega todos os atributos
                double distanciaKill = Math.Floor(Vector3.Distance(event2.DamageEvent.attacker.id.transform.position, event2.DamageEvent.victim.id.transform.position));
                string armaOnKilled = "";
                string corpoOnKilled = event2.DamageEvent.bodyPart.GetNiceName();
                var vitima = event2.DamageEvent.victim.idMain.GetComponent<HumanBodyTakeDamage>();
                // validar arma
                if (vitima._bleedingLevel == event2.DamageEvent.amount)
                {
                    armaOnKilled = "Sangramento";
                }
                else if (!(event2.DamageEvent.extraData is WeaponImpact))
                {
                    if (event2.DamageEvent.attacker.id.GetComponent<TimedExplosive>())
                    {
                        armaOnKilled = "C4";
                    }
                    else if (event2.DamageEvent.attacker.id.GetComponent<TimedGrenade>())
                    {
                        armaOnKilled = "Granada";
                    }
                    else if (event2.DamageEvent.damageTypes == 0 && WaterLine.Height != 0f && vitima.transform.position.y <= WaterLine.Height)
                    {
                        armaOnKilled = "nulo"; //Agua";
                    }
                    else if (event2.DamageEvent.attacker.id.GetComponent<Radiation>() && event2.DamageEvent.attacker.id.GetComponent<Metabolism>().GetRadLevel() >= 500f)
                    {
                        armaOnKilled = "nulo"; //Radiação";
                    }
                    else if (event2.DamageEvent.attacker.id.GetComponent<Metabolism>())
                    {
                        if (event2.DamageEvent.damageTypes.ToString() == "damage_melee")
                        {
                            armaOnKilled = "Arco";
                        }
                        else
                        {
                            armaOnKilled = "nulo"; //Suicide"; //keda //fome //frio
                        }
                    }
                    else if (event2.DamageEvent.attacker.id.GetComponent<SpikeWall>())
                    {
                        armaOnKilled = "Spike Wall";
                    }
                    else
                    {
                        armaOnKilled = "Arco";
                    }
                }
                else
                {
                    WeaponImpact cachedWeapon = event2.DamageEvent.extraData as WeaponImpact;
                    armaOnKilled = cachedWeapon.dataBlock.name;
                }
                if (armaOnKilled != "nulo")
                {
                    if (armaOnKilled == "Sangramento")
                    {
                        BroadCast(configServer.NameServer, "[color #0dbf2f][color #ff0000]" + hurteduser.displayName + "[/color] morreu sangrando.");
                    }
                    else
                    {
                        BroadCast(configServer.NameServer, "[color #0dbf2f][color #ff0000]" + killeruser.displayName + "[/color]  matou [color #00fff5]" + hurteduser.displayName + "[/color] acertou " + corpoOnKilled + ",com ([color #ffffff]" + armaOnKilled + "[/color]) dist. [color #ff0000]" + distanciaKill + "mt[/color].[/color]");
                        if (armaOnKilled != "Spike Wall" && killeruser.userID != hurteduser.userID)
                        {
                            // Puts("[death]{ 'atacante':'" + killeruser.userID.ToString() + "', 'vitima':'" + hurteduser.userID.ToString() + "', 'arma':'" + armaOnKilled + "', 'distancia':'" + distanciaKill + "', 'local':'" + corpoOnKilled + "'}[death]");
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        void OnServerShutdown()
        {
            SaveFiles();
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

        //============================ Classes ==================================

        public class StoragePlayer : MonoBehaviour
        {
            public string oldUserPm;
            public bool showDamange = true;
        }

        public class Ingrediente
        {
            public string item;
            public string quantidade;
            public Dictionary<string, int> ingredientes = new Dictionary<string, int>();
            public string[] split;

            public Ingrediente(List<string> itens)
            {
                foreach (string value in itens)
                {
                    split = value.Split(new Char[] { '_' });
                    ingredientes.Add(split[0].ToLower(), Convert.ToInt32(split[1]));
                }
            }
        }

        public class ResourcesItens
        {
            public static Dictionary<string, Ingrediente> IngredientesItem;

            public static void Init()
            {
                IngredientesItem = new Dictionary<string, Ingrediente>();
                IngredientesItem.Add("Bed", new Ingrediente(new List<string>() { "Cloth_40", "Metal Fragments_100" }));
                IngredientesItem.Add("Camp Fire", new Ingrediente(new List<string>() { "Wood_5" }));
                IngredientesItem.Add("Furnace", new Ingrediente(new List<string>() { "Stones_15", "Wood_20", "Low Grade Fuel_10" }));
                IngredientesItem.Add("LargeSpikeWall", new Ingrediente(new List<string>() { "Wood_200" }));
                IngredientesItem.Add("LargeWoodStorage", new Ingrediente(new List<string>() { "Wood_60" }));
                IngredientesItem.Add("MetalCeiling", new Ingrediente(new List<string>() { "Low Quality Metal_6" }));
                IngredientesItem.Add("MetalDoor", new Ingrediente(new List<string>() { "Metal Fragments_200" }));
                IngredientesItem.Add("MetalDoorFrame", new Ingrediente(new List<string>() { "Low Quality Metal_4" }));
                IngredientesItem.Add("MetalFoundation", new Ingrediente(new List<string>() { "Low Quality Metal_8" }));
                IngredientesItem.Add("MetalPillar", new Ingrediente(new List<string>() { "Low Quality Metal_2" }));
                IngredientesItem.Add("MetalRamp", new Ingrediente(new List<string>() { "Low Quality Metal_5" }));
                IngredientesItem.Add("MetalStairs", new Ingrediente(new List<string>() { "Low Quality Metal_5" }));
                IngredientesItem.Add("MetalWall", new Ingrediente(new List<string>() { "Low Quality Metal_4" }));
                IngredientesItem.Add("MetalWindowFrame", new Ingrediente(new List<string>() { "Low Quality Metal_4" }));
                //IngredientesItem.Add("MetalWindowBars", new Ingrediente(new List<string>() { "Stones_1" }));
                IngredientesItem.Add("Repair Bench", new Ingrediente(new List<string>() { "Stones_12", "Wood_60", "Metal Fragments_50", "Low Grade Fuel_6" }));
                IngredientesItem.Add("Sleeping Bag", new Ingrediente(new List<string>() { "Cloth_15" }));
                IngredientesItem.Add("Small Stash", new Ingrediente(new List<string>() { "Leather_10" }));
                IngredientesItem.Add("Spike Wall", new Ingrediente(new List<string>() { "Wood_100" }));
                IngredientesItem.Add("WoodBarricade", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("Wood Barricade", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("WoodCeiling", new Ingrediente(new List<string>() { "Wood Planks_6" }));
                IngredientesItem.Add("WoodDoorFrame", new Ingrediente(new List<string>() { "Wood Planks_4" }));
                IngredientesItem.Add("WoodenDoorFrame", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("WoodenDoor", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("WoodFoundation", new Ingrediente(new List<string>() { "Wood Planks_8" }));
                IngredientesItem.Add("WoodGate", new Ingrediente(new List<string>() { "Wood_120" }));
                IngredientesItem.Add("WoodGateway", new Ingrediente(new List<string>() { "Wood_400" }));
                IngredientesItem.Add("WoodPillar", new Ingrediente(new List<string>() { "Wood Planks_2" }));
                IngredientesItem.Add("WoodRamp", new Ingrediente(new List<string>() { "Wood Planks_5" }));
                IngredientesItem.Add("WoodShelter", new Ingrediente(new List<string>() { "Wood_50" }));
                IngredientesItem.Add("WoodStairs", new Ingrediente(new List<string>() { "Wood Planks_5" }));
                IngredientesItem.Add("WoodStorageBox", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("WoodWall", new Ingrediente(new List<string>() { "Wood Planks_4" }));
                IngredientesItem.Add("WoodWindowFrame", new Ingrediente(new List<string>() { "Wood Planks_4" }));
                IngredientesItem.Add("Workbench", new Ingrediente(new List<string>() { "Stones_8", "Wood_50" }));
                IngredientesItem.Add("WoodBox", new Ingrediente(new List<string>() { "Wood_30" }));
                IngredientesItem.Add("WoodBoxLarge", new Ingrediente(new List<string>() { "Wood_60" }));
                IngredientesItem.Add("LargeWoodSpikeWall", new Ingrediente(new List<string>() { "Wood_200" }));
                IngredientesItem.Add("WoodSpikeWall", new Ingrediente(new List<string>() { "Wood_100" }));

            }
        }

        //=======================================================================

        //========================== ready e save files =========================

        public static void ReadyFile()
        {
            friendList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(friendList, "data/friendList.json");
            shareList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(shareList, "data/shareList.json");
            permList = ProjectX.ReadyConfigChecked<Dictionary<ulong, List<string>>>(permList, "data/permList.json");
            homeList = ProjectX.ReadyConfigChecked<Dictionary<ulong, Dictionary<string, string>>>(homeList, "data/homeList.json");
            userCache = ProjectX.ReadyConfigChecked<Dictionary<ulong, Dictionary<string, string>>>(userCache, "data/userCache.json");
            cacheKits = ProjectX.ReadyConfigChecked< Dictionary<ulong, Dictionary<string, double>>>(cacheKits, "data/cacheKits.json");
        }

        public static void SaveFiles()
        {
            if (friendList.Count != 0)
            {
                Logger.Log("Saving friendList.");
                JsonHelper.SaveFile(friendList, GetAbsoluteFilePath("data/friendList.json"));
            }

            if (homeList.Count != 0)
            {
                Logger.Log("Saving homeList.");
                JsonHelper.SaveFile(homeList, GetAbsoluteFilePath("data/homeList.json"));
            }

            if (userCache.Count != 0)
            {
                Logger.Log("Saving userCache.");
                JsonHelper.SaveFile(userCache, GetAbsoluteFilePath("data/userCache.json"));
            }

            if (permList.Count != 0)
            {
                Logger.Log("Saving permissoes.");
                JsonHelper.SaveFile(permList, GetAbsoluteFilePath("data/permList.json"));
            }

            if (shareList.Count != 0)
            {
                Logger.Log("Saving shares.");
                JsonHelper.SaveFile(shareList, GetAbsoluteFilePath("data/shareList.json"));
            }

            if (cacheKits.Count != 0)
            {
                Logger.Log("Saving cacheKits.");
                JsonHelper.SaveFile(cacheKits, GetAbsoluteFilePath("data/cacheKits.json"));
            }

        }

        //=======================================================================
    }
}
