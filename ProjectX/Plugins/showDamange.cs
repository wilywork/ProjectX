using Fougerite;
using Fougerite.Events;
using System;
using UnityEngine;
using static ProjectX.ProjectX;

namespace ProjectX.Plugins
{

    class ShowDamange
    {

        public class Config
        {
            public string warnNotPermissonRemove;
            public string warnYourFriend;
            public string warnHitDamange;
            public string warnHitYouDamange;
            public string broadcastDiedBleeding;
            public string broadcastKilledPlayer;
            public string activeShowDamange;
            public string disableShowDamange;
            public float timerPopup;

            public Config Default()
            {
                warnYourFriend = "Pare é seu amigo {0}";
                warnNotPermissonRemove = "Você não tem permissão para remover.";
                warnHitDamange = "Você acertou {0} : {1}";
                warnHitYouDamange = "{0} Acertou você Dano: {1}";
                broadcastDiedBleeding = "{0} morreu sangrando.";
                broadcastKilledPlayer = "{0}  matou {1} acertou {2},com ({3}) dist. {4}mt.";
                activeShowDamange = "Show Damange ativado!";
                disableShowDamange = "Show Damange desativado!";
                timerPopup = 1.5f;
                return this;
            }
        }

        public static Config configShowDamange = new Config();

        public static void Start()
        {
            configShowDamange = ProjectX.ReadyConfigChecked<Config>(configShowDamange.Default(), "config/showDamange.json");
        }

        public static void EntityHurt(HurtEvent he)
        {
            try
            {
                if (he.AttackerIsPlayer && he.DamageEvent.attacker.client != null)
                {
                    if (he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>() != null)
                    {

                        if (he.Entity.UOwnerID == he.DamageEvent.attacker.userID || he.DamageEvent.attacker.client.netUser.admin)
                        {
                            RemoveCommand.GiveItemRemove(he, he.Entity.Name, he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>());
                        }
                        else
                        {
                            if (ShareCommand.isShare(he.Entity.UOwnerID, he.DamageEvent.attacker.userID))
                            {
                                RemoveCommand.GiveItemRemove(he, he.Entity.Name, he.DamageEvent.attacker.client.GetComponent<RemoveCommand.RemoveHandler>());
                            }
                            else
                            {
                                Fougerite.Server.Cache[he.DamageEvent.attacker.userID].MessageFrom(configServer.NameServer, configShowDamange.warnNotPermissonRemove);
                            }

                        }
                    }
                    else
                    {
                        StoragePlayer cacheStorage = he.DamageEvent.attacker.client.gameObject.GetComponent<StoragePlayer>();
                        if (cacheStorage.showDamange)
                        {
                            float damangeCalc = he.Entity.Health - he.DamageAmount;
                            string entityName = he.Entity.Name.ToLower();
                            if(entityName.IndexOf("pillar") == -1 && entityName.IndexOf("ceiling") == -1 && entityName.IndexOf("foundation") == -1)
                            {
                                if (damangeCalc >= 0)
                                {
                                    if(damangeCalc != he.Entity.MaxHealth)
                                    {
                                        Fougerite.Server.Cache[he.DamageEvent.attacker.userID].Notice("☢", damangeCalc.ToString("N0") + " | " + he.Entity.MaxHealth, configShowDamange.timerPopup);
                                    }
                                    else
                                    {
                                        Fougerite.Server.Cache[he.DamageEvent.attacker.userID].Notice("☢", he.Entity.Health + " | " + he.Entity.MaxHealth, configShowDamange.timerPopup);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[HookOnHurtEntity] Some error showed up error 0. Report this. " + ex);
            }
        }

        public static void PlayerHurt(HurtEvent he)
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
                            cachePlayer.Notice("☠", string.Format(configShowDamange.warnYourFriend, cachePlayer2.Name), configShowDamange.timerPopup);
                        }
                        else
                        {
                            // cancela teleport
                            tpr.OnHurtPlayer(cachePlayer2.PlayerClient);

                            StoragePlayer cacheStorage = cachePlayer.PlayerClient.gameObject.GetComponent<StoragePlayer>();
                            if (cacheStorage.showDamange)
                            {
                                cachePlayer.Notice("☠", string.Format(configShowDamange.warnHitDamange, he.DamageEvent.victim.client.userName, (cachePlayer2.Health - he.DamageAmount).ToString("N0") + " | 100"), configShowDamange.timerPopup);
                            }

                            StoragePlayer cacheStorage2 = cachePlayer2.PlayerClient.gameObject.GetComponent<StoragePlayer>();
                            if (cacheStorage2.showDamange)
                            {
                                cachePlayer2.Notice("☠", string.Format(configShowDamange.warnHitYouDamange, cachePlayer.Name, he.DamageAmount.ToString("N0")), 2);
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

        public static void PlayerKilled(DeathEvent event2)
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
                        BroadCast(configServer.NameServer, string.Format(configShowDamange.broadcastDiedBleeding, hurteduser.displayName));
                    }
                    else
                    {
                        BroadCast(configServer.NameServer, string.Format(configShowDamange.broadcastKilledPlayer, killeruser.displayName, hurteduser.displayName, corpoOnKilled, armaOnKilled, distanciaKill));
                        if (armaOnKilled != "Spike Wall" && killeruser.userID != hurteduser.userID)
                        {
                            Logger.Log("[death]{ 'atacante':'" + killeruser.userID.ToString() + "', 'vitima':'" + hurteduser.userID.ToString() + "', 'arma':'" + armaOnKilled + "', 'distancia':'" + distanciaKill + "', 'local':'" + corpoOnKilled + "'}[death]");
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

        public static void NPCHurt(HurtEvent he)
        {
            try
            {
                if (he.AttackerIsPlayer && he.DamageEvent.attacker.client != null)
                {
                    StoragePlayer cacheStorage = he.DamageEvent.attacker.client.gameObject.GetComponent<StoragePlayer>();
                    if (cacheStorage.showDamange)
                    {
                        NPC animal = (NPC)he.Victim;
                        float damangeCalc = animal.Health - he.DamageAmount;
                        if (damangeCalc > 0 && damangeCalc != animal.Character.maxHealth)
                        {
                            Fougerite.Server.Cache[he.DamageEvent.attacker.userID].Notice("☢", damangeCalc.ToString("N0") + " | " + animal.Character.maxHealth, configShowDamange.timerPopup);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[NPCHurt] Some error showed up error 0. Report this. " + ex);
            }
        }

        public static void OnEntityDestroyed(DestroyEvent de)
        {
            NetUser killeruser = de.DamageEvent.attacker.client?.netUser ?? null;

            if (killeruser != null && killeruser.playerClient != null)
            {
                try
                {
                    if (de.DamageEvent.victim.idMain != null)
                    {
                        var structure = de.DamageEvent.victim.idMain.GetComponent<StructureComponent>();
                        var deployable = de.DamageEvent.victim.idMain.GetComponent<DeployableObject>();
                        if (structure && structure._master != null)// && structure._master.ownerID != killeruser.userID)
                        {
                            Logger.LogDebug("atacante: " + killeruser.userID.ToString() + "( " + killeruser.displayName + " ) strutura: " + structure.name.ToString() + " Dono: " + structure._master?.creatorID.ToString() + " location: " + structure.gameObject.transform.localPosition.ToString());
                        }
                        else if (deployable && deployable.creatorID != killeruser.userID)
                        {
                            Logger.LogDebug("atacante: " + killeruser.userID.ToString() + "( " + killeruser.displayName + " ) strutura: " + deployable.name.ToString() + " Dono: " + deployable.creatorID.ToString() + " location: " + deployable.gameObject.transform.localPosition.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogDebug("[HookPlayerKilled] Erro no log de C4!" + ex);
                }
            }
        }

    }

    public class DamangeCommand
    {
        public static void DammangeActiveCommand(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            ProjectX.StoragePlayer cacheStorage = Fougerite.Server.Cache[Arguments.argUser.userID].PlayerClient.gameObject.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage != null)
                cacheStorage.showDamange = true;

            Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ShowDamange.configShowDamange.activeShowDamange);
        }

        public static void DammangeDesactiveCommand(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            ProjectX.StoragePlayer cacheStorage = Fougerite.Server.Cache[Arguments.argUser.userID].PlayerClient.gameObject.GetComponent<ProjectX.StoragePlayer>();
            if (cacheStorage != null)
                cacheStorage.showDamange = false;

            Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ShowDamange.configShowDamange.disableShowDamange);
        }
    }
}
