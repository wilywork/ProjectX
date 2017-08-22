using System;
using System.Linq;
using UnityEngine;
using Fougerite;
using Fougerite.Events;
using static ProjectX.ProjectX;
using System.Threading;

namespace ProjectX.Plugins
{
    public class antiGlith
    {
        private static Vector3 Vector3Down = new Vector3(0f, -1f, 0f);
        private static Vector3 Vector3Down2 = new Vector3(0f, -15f, 0f);
        private static Vector3 Vector3Up = new Vector3(0f, 1f, 0f);
        public static Vector3 Vector3ABitUp = new Vector3(0f, 0.1f, 0f);
        public static Vector3 Vector3testUp = new Vector3(1f, 6f, 1f);

        public static RaycastHit cachedRaycast;
        public static Vector3 cachedPosition;
        public static Facepunch.MeshBatch.MeshBatchInstance cachedhitInstance;
        public static bool cachedBoolean;
        public static float cacheCalcPos;
        private static int terrainLayer = LayerMask.GetMask(new string[] { "Static", "Terrain" });

        public static int limitRamps = 2;

        public static int alturaPermitida = 40;
        public static string warnLimitAltura = "[color red]Está construção atingiu o limite máximo de altura.";

        public static int limiteFundacoes = 100;
        public static string warnLimitFundacoes = "[color red]Está construção atingiu o limite máximo de {0} fundações.";


        //valida os objectos adicionados no mapa pelo usuario
        public static void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity, Fougerite.Player actualplacer) {
            try
            {
                if (Entity != null)
                {

                    //valida se estes itens nao estao sobre outros
                    if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp")
                    || Entity.Name.Contains("Pillar") || Entity.Name == "WoodDoor" || Entity.Name == "MetalDoor")
                    {

                        string name = Entity.Name;
                        var location = Entity.Location;


                        bool isdoor = false;
                        float d = 4.5f;
                        if (name.Contains("Pillar"))
                        {
                            d = 0.40f;
                        }
                        else if (name.Contains("Door"))
                        {
                            isdoor = true;
                            d = 0.40f;
                        }
                        else if (name.ToLower().Contains("smallstash"))
                        {
                            d = 0.40f;
                        }
                        else if (name.Contains("Foundation"))
                        {
                            d = 4.5f;
                        }
                        else if (name.Contains("Ramp"))
                        {
                            d = 3.5f;
                        }
                        Collider[] x = Physics.OverlapSphere(location, d);
                        if (
                            x.Any(
                                l =>
                                    l.name.ToLower().Contains("woodbox") || l.name.ToLower().Contains("smallstash") ||
                                    (l.name.ToLower().Contains("door") && !isdoor)))
                        {
                            TimerEdit.TimerEvento.Once(1, () => {
                                Destroyed(Entity);
                            });
                            giveItemRemove(name, actualplacer, 1);
                            return;
                        }

                    }

                    //valida quantidade de rampas no mesmo local
                    if (Entity.Name.Contains("Ramp"))
                    {
                        RaycastHit cachedRaycast;
                        bool cachedBoolean;
                        Facepunch.MeshBatch.MeshBatchInstance cachedhitInstance;

                        if (Facepunch.MeshBatch.MeshBatchPhysics.Raycast(Entity.Location + new Vector3(0f, 0.1f, 0f), Vector3Down, out cachedRaycast, out cachedBoolean, out cachedhitInstance))
                        {
                            if (cachedhitInstance != null && cachedhitInstance.physicalColliderReferenceOnly != null)
                            {
                                var cachedComponent = cachedhitInstance.physicalColliderReferenceOnly.GetComponent<StructureComponent>();
                                if (cachedComponent != null && cachedComponent.type == StructureComponent.StructureComponentType.Foundation || cachedComponent.type == StructureComponent.StructureComponentType.Ceiling)
                                {
                                    var weight = cachedComponent._master._weightOnMe;
                                    int ramps = 0;
                                    if (weight != null && weight.ContainsKey(cachedComponent))
                                    {
                                        ramps += weight[cachedComponent].Count(structure => structure.type == StructureComponent.StructureComponentType.Ramp);
                                    }
                                    if (ramps > limitRamps)
                                    {
                                        TimerEdit.TimerEvento.Once(2, () => {
                                            Destroyed(Entity);
                                        });
                                        giveItemRemove(Entity.Name, actualplacer, 1);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    //valida se o objecto esta dentro de uma rocha
                    if (Player != null && IsIntoRock(Entity.Location)) {
                        giveItemRemove(Entity.Name, actualplacer, 1, "[color red]Proibido construir dentro de uma rocha.");
                        TimerEdit.TimerEvento.Once(1, () => {
                            Destroyed(Entity);
                        });
                        TeleportPlayerOutRock(Player);
                    }

                    //valida se ha barricada em baixo do pilar
                    if (Entity.Name.Contains("Pillar"))
                    {
                        if (Physics.OverlapSphere(Entity.Location, 0.34f).Where(collider => collider.GetComponent<DeployableObject>() != null).Any(collider => collider.GetComponent<DeployableObject>().name.Contains("Barricade_Fence")))
                        {
                            TimerEdit.TimerEvento.Once(2, () => {
                                Destroyed(Entity);
                            });
                            giveItemRemove(Entity.Name, actualplacer, 1, "[color red]Isto não é permitido!");
                        }
                    }

                    //valida objetos debaixo de construcoes
                    if(CheckObjectInBuilds(Entity, Player)){
                        return;
                    }

                    //valida se ha players perto da barricada
                    if(PlayerInBarricade(Entity, Player)){
                        return;
                    }

                    //valida se este objecto foi construindo em area proibida
                    if(RestrictMap(Entity, Player)){
                        return;
                    }
                    

                    //valida limite de construcoes
                    try
                    {
                        new Thread(new ParameterizedThreadStart(buildRestricao)).Start(new parametrosBuild(actualplacer, Entity));
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Error Thread:" + ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[GlitchFix] Some error showed up error 0. Report this. " + ex);
            }
        }
        
        
        //valida objetos debaixo de construcoes
        public static bool CheckObjectInBuilds(Entity _Entity, Fougerite.Player _Player){
            try
            {
                if (_Entity.Name == "WoodBoxLarge" || _Entity.Name == "SmallStash" || _Entity.Name == "WoodBox" || _Entity.Name == "Furnace")
                {
                    for (int b = -1; b < 1; b++)
                    {
                        for (int c = -4; c < 4; c++)
                        {
                            MeshBatchPhysics.Raycast(_Entity.Location + new Vector3(0, c, b), Vector3testUp, out cachedRaycast, out cachedBoolean, out cachedhitInstance);
                            if (cachedhitInstance != null && cachedhitInstance.physicalColliderReferenceOnly != null)
                            {
                                if (cachedhitInstance.physicalColliderReferenceOnly.gameObject.name.Contains("Foundation"))
                                {
                                    cacheCalcPos = _Entity.Location.y - cachedhitInstance.physicalColliderReferenceOnly.gameObject.transform.position.y;
                                    if (cacheCalcPos < 3.9f)
                                    {
                                        giveItemRemove(_Entity.Name, _Player, 1, "[color red]Isso é proibido.");
                                        TimerEdit.TimerEvento.Once(2, () => {
                                            Destroyed(_Entity);
                                        });
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[GlitchFix] Some error showed up error 2. Report this. " + ex);
            }
            return false;
        }

        //valida se há players perto da barricada
        public static bool PlayerInBarricade(Entity _Entity, Fougerite.Player _Player){
            try
            {
                if (_Entity.Name == "Wood Barricade")
                {
                    foreach (Collider collider in Physics.OverlapSphere(_Entity.Location + Vector3Up, 2.5f))
                    {
                        if (collider != null && collider.gameObject != null && collider.gameObject.name.IndexOf("Player") != -1)
                        {
                            _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]A players muito perto da barricada.");
                            Destroyed(_Entity);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[GlitchFix] Some error showed up error 3. Report this. " + ex);
            }
            return false;
        }

        //verifica se esta em baixo ou do lado de uma rocha
        public static bool IsIntoRock(Vector3 position) {
            RaycastHit hitinfo;
            Vector3 origin = position + new Vector3(0f, 20f, 0f);
            if (Physics.Raycast(origin, Vector3.down, out hitinfo))
            {
                if (Physics.Raycast(cachedPosition, Vector3Down, out cachedRaycast, terrainLayer))
                {
                    return true;
                }
                else if (Physics.Raycast(cachedPosition, Vector3Down2, out cachedRaycast, terrainLayer))
                {
                    return true;
                }
            }
            return false;
        }
        
        //valida se o objecto foi construindo em area proibida
        public static bool RestrictMap(Entity _Entity, Fougerite.Player _Player){
            int cache_x = Convert.ToInt32(_Entity.Location.x);
            int cache_z = Convert.ToInt32(_Entity.Location.z);

            try
            {
                bool destroyItem = false;
                if (_Player == null || !_Player.Admin)
                {

                    if (cache_x <= 6490 && cache_x >= 5660 && cache_z >= -3920 && cache_z <= -3090)
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]Perto da Small, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].");
                        destroyItem = true;
                    }
                    else if (cache_x <= 5192 && cache_x >= 4430 && cache_z >= -4141 && cache_z <= -3623)
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]Perto do Vale dos Ursos, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].");
                        destroyItem = true;
                        //casinhas e celeiro
                    }
                    else if ((cache_x <= 6249 && cache_x >= 6232 && cache_z >= -4811 && cache_z <= -4790) ||
                      (cache_x <= 6297 && cache_x >= 6278 && cache_z >= -4878 && cache_z <= -4860) ||
                      (cache_x <= 6334 && cache_x >= 6301 && cache_z >= -4783 && cache_z <= -4753) ||
                      (cache_x <= 6041 && cache_x >= 6024 && cache_z >= -4448 && cache_z <= -4434) ||
                      (cache_x <= 5771 && cache_x >= 5748 && cache_z >= -4302 && cache_z <= -4287) ||
                      (cache_x <= 5716 && cache_x >= 5694 && cache_z >= -4254 && cache_z <= -4233) ||
                      (cache_x <= 6624 && cache_x >= 6604 && cache_z >= -3388 && cache_z <= -3407))
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]Perto da casa, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].");
                        destroyItem = true;
                        //galpoes
                    }
                    else if ((cache_x <= 6357 && cache_x >= 6334 && cache_z >= -4691 && cache_z <= -4672) ||
                          (cache_x <= 6165 && cache_x >= 6085 && cache_z >= -4428 && cache_z <= -4339) ||
                          (cache_x <= 6448 && cache_x >= 6378 && cache_z >= -3955 && cache_z <= -3834) ||
                          (cache_x <= 6780 && cache_x >= 6607 && cache_z >= -4427 && cache_z <= -4094))
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]Proibido construir aqui, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].");
                        destroyItem = true;
                    }

                    if (destroyItem)
                    {

                        RemoveTime objetoRemover = null;
                        if (_Entity.IsDeployableObject())
                        {
                            DeployableObject deploy = (DeployableObject)_Entity.Object;
                            if (deploy != null && deploy.gameObject != null)
                            {
                                objetoRemover = deploy.gameObject.AddComponent<RemoveTime>();
                                objetoRemover.componentEntity = _Entity;
                            }
                            else
                            {
                                TimerEdit.TimerEvento.Once(2, () => {
                                    Destroyed(_Entity);
                                });
                                Logger.LogDebug("[Error] GlithFIX ================== falta deploy.gameObject");
                            }
                        }
                        else if (_Entity.IsStructure())
                        {
                            StructureComponent struture = (StructureComponent)_Entity.Object;
                            if (struture != null && struture.gameObject != null)
                            {
                                objetoRemover = struture.gameObject.AddComponent<RemoveTime>();
                                objetoRemover.componentEntity = _Entity;
                            }
                            else
                            {
                                TimerEdit.TimerEvento.Once(2, () => {
                                    Destroyed(_Entity);
                                });
                                Logger.LogDebug("[Error] GlithFIX ================== falta struture.gameObject");
                            }
                        }
                        else
                        {
                            Logger.LogDebug("GlithFIX =========================================");
                        }
                        
                        return true;
                    }
                    else if (_Player != null && _Player.PlayerClient != null && _Player.PlayerClient.lastKnownPosition != null && IsIntoRock(_Player.PlayerClient.lastKnownPosition))
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, "[color red]Proibido construir dentro de uma rocha.");
                        TimerEdit.TimerEvento.Once(2, () => {
                            Destroyed(_Entity);
                        });
                        TeleportPlayerOutRock(_Player);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[GlitchFix] Some error showed up error 4. Report this. " + ex);
            }
            
            return false;
        }
        
        //valida o local onde o player nasceu
        public static void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se) {
            var loc = player.Location;
            Vector3 cachedPositionSpawn = loc;
            RaycastHit cachedRaycast;
            cachedPositionSpawn.y += 100f;
            try
            {
                if (Physics.Raycast(loc, Vector3Up, out cachedRaycast, terrainLayer))
                {
                    cachedPositionSpawn = cachedRaycast.point;
                }
                if (!Physics.Raycast(cachedPositionSpawn, Vector3Down, out cachedRaycast, terrainLayer)) return;
            }
            catch
            {
                return;
            }
            if (cachedRaycast.collider.gameObject.name != "") return;
            if (cachedRaycast.point.y < player.Y) return;
            Logger.LogDebug(player.Name + " tried to rock glitch at " + player.Location);
            player.MessageFrom(ProjectX.configServer.NameServer, "[color red] Não tente mais bugar na rocha.");
            TeleportPlayerOutRock(player);

        }


        // teleporta o player para fora da rocha
        public static void TeleportPlayerOutRock(Fougerite.Player netuser) {
            if (netuser.PlayerClient != null && netuser.PlayerClient.lastKnownPosition != null) {
                Vector3 currentposition = netuser.PlayerClient.lastKnownPosition;
                Vector3 origin = currentposition + new Vector3(0f, 50f, 0f);
                RaycastHit hitinfo;

                if (Physics.Raycast(origin, Vector3.down, out hitinfo))
                {
                    if (hitinfo.transform.gameObject.layer == 10)
                    {
                        var y = float.Parse(origin.y.ToString()) - hitinfo.distance + 0.5f;
                        currentposition = new Vector3(currentposition.x, y, currentposition.z);
                    }
                }

                if (currentposition == netuser.PlayerClient.lastKnownPosition) return;
                ProjectX.TeleportPlayer(netuser.PlayerClient.netUser, currentposition);
            }
        }

        //valida limite das construcoes
        public static void buildRestricao(object props) {

            Fougerite.Player actualplacer = (Fougerite.Player)props.GetProperty("actualplacer");
            Fougerite.Entity Entity = (Entity)props.GetProperty("Entity");

            int cache_x = Convert.ToInt32(Entity.Location.x);
            int cache_z = Convert.ToInt32(Entity.Location.z);
            int ajustVar = 0;

            if (cache_x <= 7729 && cache_x >= 4129 && cache_z >= -6074 && cache_z <= -2574)
            {
                if (Entity.Name.IndexOf("Pillar") != -1)
                {
                    ajustVar = alturaPermitida - 20;
                } else
                {
                    ajustVar = limiteFundacoes - 75;
                }
            } else
            {
                if (Entity.Name.IndexOf("Pillar") != -1)
                {
                    ajustVar = alturaPermitida;
                }
                else
                {
                    ajustVar = limiteFundacoes;
                }
            }

            if (Entity.Name.IndexOf("Pillar") != -1)
            {
                foreach (var item in Entity.GetLinkedStructs())
                {
                    if (item.Name == "WoodFoundation" || item.Name == "MetalFoundation") {
                        if (ajustVar < Entity.Y - item.Y)
                        {
                            TimerEdit.TimerEvento.Once(2, () => {
                                Destroyed(Entity);
                            });
                            giveItemRemove(Entity.Name, actualplacer, 1, warnLimitAltura);
                        }
                        break;
                    }
                }
            } else if (Entity.Name == "WoodFoundation")
            {
                int countFundation = 1;
                foreach (var item in Entity.GetLinkedStructs())
                {
                    if (item.Name == "WoodFoundation")
                    {
                        if(countFundation >= ajustVar)
                        {
                            TimerEdit.TimerEvento.Once(2, () => {
                                Destroyed(Entity);
                            });
                            giveItemRemove(Entity.Name, actualplacer, 1, string.Format(warnLimitFundacoes, ajustVar));
                            break;
                        } else
                        {
                            countFundation++;
                        }
                    }
                }
            }
            else if (Entity.Name == "MetalFoundation")
            {
                int countFundation = 1;
                foreach (var item in Entity.GetLinkedStructs())
                {
                    if (item.Name == "MetalFoundation")
                    {
                        if (countFundation >= limiteFundacoes)
                        {
                            TimerEdit.TimerEvento.Once(2, () => {
                                Destroyed(Entity);
                            });
                            giveItemRemove(Entity.Name, actualplacer, 1, string.Format(warnLimitFundacoes, ajustVar));
                            break;
                        }
                        else
                        {
                            countFundation++;
                        }
                    }
                }
            }
        }

        //giva o item removido
        public static void giveItemRemove(string name, Fougerite.Player actualplacer, int qtd, string message = null) {
            if (actualplacer != null && actualplacer.IsOnline)
            {
                if (ResourcesItens.IngredientesItem.ContainsKey(name))
                {
                    ProjectX.AddItemInventory(actualplacer, ResourcesItens.IngredientesItem[name].item, 1);
                    if (message != null)
                    {
                        actualplacer.MessageFrom(ProjectX.configServer.NameServer, message);
                    }
                } else
                {
                    Logger.LogDebug("[Error] antiglith giveItemRemove: " + name);
                }
            }
        }
        
        
        //Objeto com os parametros necessarios para validar limites em segundo plano
        public class parametrosBuild {
            public Fougerite.Player actualplacer;
            public Fougerite.Entity Entity;

            public parametrosBuild(Fougerite.Player player, Fougerite.Entity item)
            {
                actualplacer = player;
                Entity = item;
            }
        }

        //classe que é ativada no objeto para remover em 1 min
        public class RemoveTime : MonoBehaviour {
            public Entity componentEntity;
            public float timeStop = Time.realtimeSinceStartup + 60; //daqui 1 min

            void FixedUpdate()
            {
                if (timeStop <= Time.realtimeSinceStartup)
                {
                    if (!componentEntity.IsDeployableObject() && componentEntity.GetLinkedStructs().Count > 0)
                    {
                        foreach (var item in componentEntity.GetLinkedStructs())
                        {
                            Destroyed(item);
                        }
                    }
                    Destroyed(componentEntity);
                    Destroy(this);
                }
            }
        }
        
    }

}
