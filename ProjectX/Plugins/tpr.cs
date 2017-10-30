using Fougerite;
using System;
using UnityEngine;

namespace ProjectX.Plugins
{

    public class tpr
    {
        public static float timeEspired = 15;
        public static float timeCooldown = 30;
        public static float timeTeleport = 4;
        public static float distanceCheckDeploys = 35;

        public static string playerNotFound = "[color yellow] Player não encontrado.";
        public static string notFoundText = "[color yellow] Informe o nome do jogador.";
        public static string teleportCancelled = "[color yellow] Teleporte cancelado!";
        public static string cooldownTpr = "[color yellow] Você deve esperar [color #ffffff]{0}s[/color] antes de solicitar outro teletransporte.";
        public static string playerInTpr = "[color yellow] O jogador alvo já tem um pedido pendente.";
        public static string blockTprYourself = "[color yellow] Impossível teleportar para si mesmo.";
        public static string blockTprInHome = "[color yellow] Você não pode aceitar tpr enquanto da home.";
        public static string blockTprInHomeP = "[color yellow] Você não pode enviar tpr enquanto da home.";
        public static string cancel = "[color red]Você recusou o pedido de telepordo do [color white]{0}[/color].";
        public static string cancelAll = "[color green] Você cancelou todos os teletransportes atuais.";
        public static string warnCancel = "{0}[color red] recusou seu pedido de teleporte.";
        public static string sending = "[color green] Você enviou um pedido para [color white]{0}[/color].";
        public static string warnStruture = "[color red] Afaste-se das construções!!";
        public static string warnPlayerStruture = "{0} [color red]está muito perto das construções!!";
        public static string notTpr = "[color yellow] Você não tem solicitação de tpr.";
        public static string warnNewTpr = "[color green] Você recebeu um pedido de teletransporte {0}. [color white]/tpa[/color] para aceitar, [color white]/tpc[/color] para recusar.";
        public static string warnTprExist = "[color yellow] Você já solicitou um teletransporte , você deve aguardar.";
        public static string warnBlockTpr = "[color yellow] Você não tem permissão para usar tpr de onde você está.";
        public static string warnBlockRock = "[color yellow]Parece que você está perto de uma árvore ou rocha, afaste-se!";
        public static string warnBlockRockP = "[color yellow]Cancelado, seu amigo [color #ffffff]{0}[/color] está perto de uma árvore ou rocha!";

        private static Vector3 VectorDown = new Vector3(0f, -0.4f, 0f);
        private static RaycastHit cachedRaycast;
        private static int terrainLayer = LayerMask.GetMask(new string[] { "Static" });

        public static bool CheckRadius(Vector3 position, float radius, bool all)
        {
            foreach (Collider collider in Physics.OverlapSphere(position, radius))
            {
                if (collider.gameObject.name == "WoodBoxLarge(Clone)" || collider.gameObject.name == "WoodBox(Clone)" || collider.gameObject.name == "Furnace(Clone)")
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckRock(PlayerClient playerClient)
        {                                // valida rocha perto
            foreach (Collider collider in UnityEngine.Physics.OverlapSphere(playerClient.lastKnownPosition, 2f, terrainLayer))
            {
                if (Physics.Raycast(playerClient.lastKnownPosition, VectorDown, out cachedRaycast, 1f, terrainLayer))
                {
                    if (cachedRaycast.collider == collider)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public static void OnHurtPlayer(PlayerClient playerClient)
        {
            TPRequest checkTPRequest = playerClient.GetComponent<TPRequest>();
            if (checkTPRequest != null)
            {
                checkTPRequest.Canceled();
            }
            else
            {
                TPIncoming checkTPIncoming = playerClient.GetComponent<TPIncoming>();
                if (checkTPIncoming != null)
                {
                    checkTPIncoming.Canceled();
                }
            }
        }
    }

    public class TPIncoming : MonoBehaviour
    {
        public Fougerite.Player playerclient;
        public Fougerite.Player playerDestinaty;

        public void OnDestroy()
        {
            GameObject.Destroy(this);
        }

        public void Canceled()
        {
            if (playerDestinaty != null && playerDestinaty.PlayerClient != null)
            {
                var cacheTpr = playerDestinaty.PlayerClient.GetComponent<TPRequest>();
                if (cacheTpr != null)
                {
                    playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                    cacheTpr.OnDestroy();
                }
            }
            this.OnDestroy();
        }
    }

    public class TPCooldown : MonoBehaviour
    {
        public Fougerite.Player playerclient;
        public float activeTime = Time.realtimeSinceStartup + tpr.timeCooldown;

        void FixedUpdate()
        {
            if (Time.realtimeSinceStartup > activeTime)
            {
                GameObject.Destroy(this);
            }
        }
    }

    public class TPRequest : MonoBehaviour
    {
        public Fougerite.Player playerclient;
        public Fougerite.Player playerDestinaty;
        public float activeTime = 0;
        public float timerCreate = Time.realtimeSinceStartup + tpr.timeEspired;
        public Vector3 location;
        public bool ativado = false;

        public void Teleported()
        {
            ativado = false;
            if (playerclient != null && playerclient.NetworkPlayer != null)
            {
                if (playerDestinaty != null && playerDestinaty.PlayerClient != null)
                {
                    if (tpr.CheckRock(playerDestinaty.PlayerClient)) {
                        playerclient.MessageFrom(ProjectX.configServer.NameServer, string.Format(tpr.warnBlockRockP, playerDestinaty.Name));
                        playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.warnBlockRock);
                    } else if (tpr.CheckRadius(playerDestinaty.PlayerClient.lastKnownPosition, tpr.distanceCheckDeploys, false))
                    {
                        ProjectX.TeleportPlayer(playerclient.PlayerClient.netUser, playerDestinaty.PlayerClient.lastKnownPosition);
                        var cacheCooldown = playerclient.PlayerClient.gameObject.AddComponent<TPCooldown>();
                        cacheCooldown.playerclient = playerclient;
                    } else
                    {
                        playerclient.MessageFrom(ProjectX.configServer.NameServer, string.Format(tpr.warnPlayerStruture, playerDestinaty.Name));
                        playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.warnStruture);
                    }
                }
                else {
                    playerclient.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                }
            }
            if (playerDestinaty != null && playerDestinaty.PlayerClient != null)
            {
                var cacheTpr = playerDestinaty.PlayerClient.GetComponent<TPIncoming>();
                if (cacheTpr != null) { cacheTpr.OnDestroy(); }
            }
            this.OnDestroy();
        }

        public void Active() {
            activeTime = Time.realtimeSinceStartup + tpr.timeTeleport;
            ativado = true;
        }

        public void OnDestroy()
        {
            GameObject.Destroy(this);
        }

        public void OnCancel()
        {
            if (playerDestinaty != null && playerDestinaty.PlayerClient != null)
            {
                var cacheTpr = playerDestinaty.PlayerClient.GetComponent<TPIncoming>();
                if (cacheTpr != null) {
                    playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                    cacheTpr.OnDestroy();
                }
            }
            if (playerclient != null && playerclient.PlayerClient != null)
            {
                playerclient.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                this.OnDestroy();
            }

        }

        public void Canceled()
        {
            if (playerDestinaty != null && playerDestinaty.PlayerClient != null)
            {
                var cacheTpr = playerDestinaty.PlayerClient.GetComponent<TPIncoming>();
                if (cacheTpr != null)
                {
                    playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                    cacheTpr.OnDestroy();
                }
            }
            this.OnDestroy();
        }

        public void Expired() {
            playerclient.MessageFrom(ProjectX.configServer.NameServer, "[color red] Usuário de destino não respondeu ao seu pedido.");
            if (playerDestinaty != null && playerDestinaty.PlayerClient != null) {
                var cacheTPIncoming2 = playerDestinaty.PlayerClient.GetComponent<TPIncoming>();
                if (cacheTPIncoming2 != null) { cacheTPIncoming2.OnDestroy(); }
            }
            this.OnDestroy();
        }

        void FixedUpdate()
        {
            if (Time.realtimeSinceStartup > activeTime && ativado)
            {
                this.Teleported();
            } else if (Time.realtimeSinceStartup > timerCreate) {
                this.Expired();
            }
        }
    }

    public class TprCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {

            if (ChatArguments.Length > 0)
            {
                Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.userID];
                Fougerite.Player cachePlayer = Fougerite.Server.GetServer().FindPlayer(ChatArguments[0]);

                if (cachePlayer != null)
                {
                    //clear olds tpr's
                    var checkTPRequest = player.PlayerClient.GetComponent<TPRequest>();
                    if (checkTPRequest != null) {
                        if (checkTPRequest.playerDestinaty != null && checkTPRequest.playerDestinaty.PlayerClient != null) {
                            var checkTPIncoming = checkTPRequest.playerDestinaty.PlayerClient.GetComponent<TPIncoming>();
                            checkTPIncoming.OnDestroy();
                            checkTPRequest.playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                        }
                        checkTPRequest.OnDestroy();
                    }
                    //check if player exist
                    if (cachePlayer.PlayerClient == null) { player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] O jogador alvo não está disponivel para teleporte."); return; }
                    if (cachePlayer.UID == player.UID) { player.MessageFrom(ProjectX.configServer.NameServer, tpr.blockTprYourself); return; };//si mesmo
                    //valida se o mesmo tem pedido de teleport
                    var cacheTPIncoming = player.PlayerClient.GetComponent<TPIncoming>();
                    if (cacheTPIncoming != null) { player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Você tem um pedido de teletransporte de entrada , você deve responder."); return; }
                    //valida se o outro player ja tem tpr
                    var cacheTPIncoming2 = cachePlayer.PlayerClient.GetComponent<TPIncoming>();
                    if(cacheTPIncoming2 != null) { player.MessageFrom(ProjectX.configServer.NameServer, tpr.playerInTpr); return; }
                    //valida cooldown de uso
                    var cacheTPCooldown = player.PlayerClient.GetComponent<TPCooldown>();
                    if (cacheTPCooldown != null) { player.MessageFrom(ProjectX.configServer.NameServer, string.Format( tpr.cooldownTpr, Convert.ToInt64(cacheTPCooldown.activeTime-Time.realtimeSinceStartup).ToString())); return; }
                    //valida se o player destino esta perto de construcao
                    if (!tpr.CheckRadius(cachePlayer.PlayerClient.lastKnownPosition, tpr.distanceCheckDeploys, false)){
                        player.MessageFrom(ProjectX.configServer.NameServer, string.Format(tpr.warnPlayerStruture, cachePlayer.Name));
                        return;
                    }
                    //valida se esta em modo home
                    var cacheCheckHome = player.PlayerClient.GetComponent<HomeTeleport>();
                    if (cacheCheckHome != null) { player.MessageFrom(ProjectX.configServer.NameServer, tpr.blockTprInHomeP); return; };
                    //criar pedido de teleport, salvar config no boneco e boneco receptor(class recept)
                    cacheTPIncoming2 = cachePlayer.PlayerClient.gameObject.AddComponent<TPIncoming>();
                    cacheTPIncoming2.playerclient = cachePlayer;
                    cacheTPIncoming2.playerDestinaty = player;
                    //criar pedido de teleport, salvar config no boneco e boneco receptor(class recept)
                    var cacheTPRequest = player.PlayerClient.gameObject.AddComponent<TPRequest>();
                    cacheTPRequest.playerclient = player;
                    cacheTPRequest.playerDestinaty = cachePlayer;
                    //SendReply(targetPlayer, string.Format("[color green]Você recebeu um pedido de teletransporte {0}. [color white]/tpa[/color] para aceitar, [color white]/tpc[/color] para recusar.", netuser.displayName));
                    player.MessageFrom(ProjectX.configServer.NameServer, string.Format(tpr.sending, cachePlayer.Name));
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, string.Format(tpr.warnNewTpr, player.Name));
                }
                else
                {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, tpr.playerNotFound);
                }
            }
            else
            {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, tpr.playerNotFound);
            }

        }
    }

    public class TpaCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            //valida se tem tpr pra aceitar
            Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.userID];
            var cacheTprI = player.PlayerClient.GetComponent<TPIncoming>();
            if (cacheTprI == null) { player.MessageFrom(ProjectX.configServer.NameServer, tpr.notTpr); return; };
            //valida se esta em modo home
            var cacheCheckHome = player.PlayerClient.GetComponent<HomeTeleport>();
            if (cacheCheckHome != null) { player.MessageFrom(ProjectX.configServer.NameServer, tpr.blockTprInHome); return; };
            //valida se pode aceitar tpr

            //valida se o player do tpr esta disponivel
            if (cacheTprI.playerDestinaty.PlayerClient == null) {
                cacheTprI.OnDestroy();
                player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow] Falha ao aceitar tpr, player do envio não esta disponível.");
                return;
            }

            if (tpr.CheckRadius(player.PlayerClient.lastKnownPosition, tpr.distanceCheckDeploys, false))
            {
                var cacheTprR = cacheTprI.playerDestinaty.PlayerClient.GetComponent<TPRequest>();
                cacheTprR.Active();
                player.MessageFrom(ProjectX.configServer.NameServer, string.Format("[color green]Você aceitou o pedido de teletransporte de [color white]{0}[/color].", cacheTprI.playerDestinaty.Name));
                cacheTprI.playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, string.Format("{0} [color green] aceitou seu pedido de teletransporte.", player.Name));
            }
            else {
                player.MessageFrom(ProjectX.configServer.NameServer,"[color red]Afaste-se das construções!!");
                cacheTprI.playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, string.Format("{0} [color red]está muito perto das construções!!", player.Name));
            }
        }
    }

    public class TpcCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            try
            {
                //valida se tem tpr, se tiver avisar o remetente
                //limpar tpr's
                Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.userID];
                if (player != null && player.PlayerClient != null)
                {
                    try
                    {
                        var cacheTprI = player.PlayerClient.GetComponent<TPIncoming>();
                        if (cacheTprI != null)
                        {
                            player.MessageFrom(ProjectX.configServer.NameServer, string.Format("[color red]Você recusou o pedido de telepordo do [color white]{0}[/color].", cacheTprI.playerDestinaty.Name));
                            cacheTprI.playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, string.Format("{0}[color red] recusou seu pedido de teleporte.", player.Name));

                            var cacheTprR = cacheTprI.playerDestinaty.PlayerClient.GetComponent<TPRequest>();
                            if (cacheTprR != null)
                            {
                                cacheTprR.OnDestroy();
                            }
                            cacheTprI.OnDestroy();
                            return;
                        }

                        var cacheTprR_ = player.PlayerClient.GetComponent<TPRequest>();
                        if (cacheTprR_ != null)
                        {
                            if (cacheTprR_.playerDestinaty.PlayerClient != null)
                            {
                                var cacheTprI_ = cacheTprR_.playerDestinaty.PlayerClient.GetComponent<TPIncoming>();

                                if (cacheTprI_ != null)
                                {
                                    cacheTprR_.playerDestinaty.MessageFrom(ProjectX.configServer.NameServer, tpr.teleportCancelled);
                                    cacheTprI_.OnDestroy();
                                }
                            }

                            cacheTprR_.OnDestroy();
                        }

                        player.MessageFrom(ProjectX.configServer.NameServer, "[color green] Você cancelou todos os teletransportes atuais.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug("[Error] TpcCommand: " + ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[Error] TpcCommand 2: " + ex);
            }
   
        }
    }

}
