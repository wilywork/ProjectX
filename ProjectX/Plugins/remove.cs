using System.Collections.Generic;
using UnityEngine;
using Fougerite.Events;
using static ProjectX.ProjectX;
using System;
using Fougerite;

namespace ProjectX.Plugins
{
    public class RemoveCommand
    {
        public static string removeActivated = "[color green]Remove ({0}) ativado por [color #ffffff]{1}[/color] segundos.";
        public static string removeDeactivated = "[color red]Remove desativado.";
        public static string noAccess = "[color red]Você não tem permissão de usar esse comando.";
        public static string wrongArguments = "[color red]Parametros errados.";
        public static float maxTimeremove = 600;

        private static StructureComponent cachedStructureRemove;
        public static RemoveHandler cachedRemoveHandler;
        public static float autoDeactivate = 30f;
        public static float cachedSeconds;
        public static string cachedType;

        private static Fougerite.Player player;

        public class RemoveHandler : MonoBehaviour
        {
            public PlayerClient playerclient;
            public Inventory inventory;
            public string removeType;
            public float deactivateTime;
            public string userid;

            public RemoveHandler() {
                playerclient = GetComponent<PlayerClient>();
                enabled = false;
                userid = playerclient.userID.ToString();
            }

            public void Activate()
            {
                enabled = true;
                inventory = playerclient.rootControllable.idMain.GetComponent<Inventory>();
                Fougerite.Server.Cache[playerclient.userID].MessageFrom(ProjectX.configServer.NameServer, string.Format(removeActivated, removeType, (deactivateTime - Time.realtimeSinceStartup).ToString()));
            }
            void OnDestroy()
            {
                if (playerclient != null && playerclient.netUser != null)
                    Fougerite.Server.Cache[playerclient.userID].MessageFrom(ProjectX.configServer.NameServer, removeDeactivated);
            }
            void FixedUpdate()
            {
                if (Time.realtimeSinceStartup > deactivateTime) DeactivateRemover(playerclient.netUser);
            }
        }

        static void ActivateDeactivateRemover(NetUser netuser, string ttype, float secs, int length)
        {
            cachedRemoveHandler = netuser.playerClient.GetComponent<RemoveHandler>();
            if (cachedRemoveHandler != null && length == 0) { DeactivateRemover(netuser); return; }
            if (cachedRemoveHandler == null) cachedRemoveHandler = netuser.playerClient.gameObject.AddComponent<RemoveHandler>();
            if (secs > maxTimeremove) {
                secs = maxTimeremove;
            }
            cachedRemoveHandler.deactivateTime = Time.realtimeSinceStartup + secs;
            cachedRemoveHandler.removeType = ttype;
            cachedRemoveHandler.Activate();
        }

        static void DeactivateRemover(NetUser netuser)
        {
            GameObject.Destroy(netuser.playerClient.GetComponent<RemoveHandler>());
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];

            cachedSeconds = autoDeactivate;
            cachedType = string.Empty;

            if (ChatArguments.Length == 0 || (ChatArguments.Length == 1 && float.TryParse(ChatArguments[0], out cachedSeconds)))
            {
                if (cachedSeconds == 0f) cachedSeconds = autoDeactivate;
                cachedType = "normal";
            }
            else if (ChatArguments.Length == 1 || (ChatArguments.Length == 2 && float.TryParse(ChatArguments[1], out cachedSeconds)))
            {
                if (cachedSeconds == 0f) cachedSeconds = autoDeactivate;

                if (player.Admin)
                {
                    cachedType = ChatArguments[0];
                }
                else {
                    player.MessageFrom(ProjectX.configServer.NameServer, noAccess);
                    return;
                }
            }
            else {
                player.MessageFrom(ProjectX.configServer.NameServer, wrongArguments);
                return;
            }

            ActivateDeactivateRemover(Arguments.argUser, cachedType, cachedSeconds, ChatArguments.Length);

        }

        public static void GiveItemRemove(HurtEvent he, string gameobjectname, RemoveCommand.RemoveHandler rplayer)
        {
            try
            {
                if (!ResourcesItens.IngredientesItem.ContainsKey(gameobjectname)) return;

                if (rplayer.removeType == "all")
                {
                    if (!he.Entity.IsDeployableObject() && he.Entity.GetLinkedStructs().Count > 0)
                    {
                        foreach (var item in he.Entity.GetLinkedStructs())
                        {
                            Destroyed(item);
                        }
                    }
                    Destroyed(he.Entity);
                    return;

                }
                else if (he.Entity.IsStructureMaster() || he.Entity.IsStructure() && rplayer.removeType != "admin")
                {
                    cachedStructureRemove = he.DamageEvent.victim.idMain.GetComponent<StructureComponent>();

                    if (cachedStructureRemove != null && !cachedStructureRemove._master.ComponentCarryingWeight(cachedStructureRemove))
                    {
                        Destroyed(he.Entity);
                    }
                    else
                    {
                        Fougerite.Server.Cache[he.DamageEvent.attacker.userID].MessageFrom(ProjectX.configServer.NameServer, "[color red] Estrutura carregada, remova o objeto que está ao redor ou em cima.");
                        return;
                    }
                }
                else
                {
                    Destroyed(he.Entity);
                }

                foreach (KeyValuePair<string, int> ingredients in ResourcesItens.IngredientesItem[gameobjectname])
                {
                    if (displaynameToDataBlock.ContainsKey(ingredients.Key.ToLower()))
                    {                        
                        ProjectX.AddItemInventory(Fougerite.Server.Cache[rplayer.playerclient.userID], displaynameToDataBlock[ingredients.Key], ingredients.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[Remove] Error GiveItemRemove! " + ex);
            }
        }

    }

}
