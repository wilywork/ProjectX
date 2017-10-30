using System;
using System.Linq;
using UnityEngine;
using Fougerite;
using Fougerite.Events;
using static ProjectX.ProjectX;

namespace ProjectX.Plugins
{
    public class AntiGlith
    {
        public class Config
        {
            public bool restrictedMap;
            public bool controlBuildLimit;
            public int limitRamps;
            public int heightAllowed;
            public string warnHeightAllowed;
            public int limitFoundations;
            public string warnLimitFoundations;
            public string warnNotAllow;
            public string warnBuildRock;
            public string warnNearSmall;
            public string warnNearValeBear;
            public string warnNearBuild;
            public string warnNearHere;
            public string warnTryBugRock;
            public string warnNearPlayerBarricade;

            public Config Default()
            {
                restrictedMap = true;
                controlBuildLimit = true;
                limitRamps = 2;
                heightAllowed = 40;
                warnHeightAllowed = "[color red]Está construção atingiu o limite máximo de altura.";
                limitFoundations = 100;
                warnLimitFoundations = "[color red]Está construção atingiu o limite máximo de {0} fundações.";
                warnNotAllow = "[color red]Isto não é permitido!";
                warnBuildRock = "[color red]Proibido construir dentro de uma rocha.";
                warnNearSmall = "[color red]Perto da Small, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].";
                warnNearValeBear = "[color red]Perto do Vale dos Ursos, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].";
                warnNearBuild = "[color red]Perto da construção, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].";
                warnNearHere = "[color red]Proibido construir aqui, será removido automaticamente pelo sistema em [color #ffffff] 60 Segundos[/color].";
                warnTryBugRock = "[color red] Não tente mais bugar na rocha.";
                warnNearPlayerBarricade = "[color red]A players muito perto da barricada.";
                return this;
            }
        }

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

        public static Config configGlith = new Config();

        public static void Start()
        {
            configGlith = ProjectX.ReadyConfigChecked<Config>(configGlith.Default(), "config/antiGlith.json");
        }

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
                            try {
                                Entity.Destroy();
                            } catch (Exception ex) {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 1 " + Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 1 Error " + ex);
                            }
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
                                    if (ramps > configGlith.limitRamps)
                                    {
                                        try {
                                            Entity.Destroy();
                                        } catch (Exception ex)
                                        {
                                            try { Logger.LogDebug("[Error] Entity.Destroy name 2 " + Entity.Name); } catch { }
                                            Logger.LogDebug("[GlitchFix] Entity.Destroy 2 Error " + ex);
                                        }
                                        giveItemRemove(Entity.Name, actualplacer, 1);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    //valida se o objecto esta dentro de uma rocha
                    if (Player != null && IsIntoRock(Entity.Location)) {
                        giveItemRemove(Entity.Name, actualplacer, 1, configGlith.warnBuildRock);
                        try {
                            Entity.Destroy();
                        }
                        catch (Exception ex)
                        {
                            try { Logger.LogDebug("[Error] Entity.Destroy name 3 " + Entity.Name); } catch { }
                            Logger.LogDebug("[GlitchFix] Entity.Destroy 3 Error " + ex);
                        }
                        TeleportPlayerOutRock(Player);
                        return;
                    }

                    //valida se ha barricada em baixo do pilar
                    if (Entity.Name.Contains("Pillar"))
                    {
                        if (Physics.OverlapSphere(Entity.Location, 0.34f).Where(collider => collider.GetComponent<DeployableObject>() != null).Any(collider => collider.GetComponent<DeployableObject>().name.Contains("Barricade_Fence")))
                        {
                            try {
                                Entity.Destroy();
                            }
                            catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 4 " + Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 4 Error " + ex);
                            }
                            giveItemRemove(Entity.Name, actualplacer, 1, configGlith.warnNotAllow);
                            return;
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
                    if(configGlith.restrictedMap && RestrictMap(Entity, Player)){
                        return;
                    }

                    //valida limite de construcoes
                    if (configGlith.controlBuildLimit) {
                        buildRestricao(actualplacer, Entity);
                    }
                    
                    //try
                    //{
                    //    new Thread(new ParameterizedThreadStart(buildRestricao)).Start(new parametrosBuild(actualplacer, Entity));
                    //}
                    //catch (Exception ex)
                    //{
                    //    Debug.Log("Error Thread:" + ex);
                    //}

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
                                        giveItemRemove(_Entity.Name, _Player, 1, configGlith.warnNotAllow);
                                        try {
                                            _Entity.Destroy();
                                        }
                                        catch (Exception ex)
                                        {
                                            try { Logger.LogDebug("[Error] Entity.Destroy name 5 " + _Entity.Name); } catch { }
                                            Logger.LogDebug("[GlitchFix] Entity.Destroy 5 Error " + ex);
                                        }
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
                            _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnNearPlayerBarricade);
                            try {
                                _Entity.Destroy();
                            }
                            catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 6 " + _Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 6 Error " + ex);
                            }
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
            try
            {

                int cache_x = Convert.ToInt32(_Entity.Location.x);
                int cache_z = Convert.ToInt32(_Entity.Location.z);

                bool destroyItem = false;
                if (_Player != null && !_Player.Admin)
                {

                    if (cache_x <= 6490 && cache_x >= 5660 && cache_z >= -3920 && cache_z <= -3090)
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnNearSmall);
                        destroyItem = true;
                    }
                    else if (cache_x <= 5192 && cache_x >= 4430 && cache_z >= -4141 && cache_z <= -3623)
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnNearValeBear);
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
                        _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnNearBuild);
                        destroyItem = true;
                        //galpoes
                    }
                    else if ((cache_x <= 6357 && cache_x >= 6334 && cache_z >= -4691 && cache_z <= -4672) ||
                          (cache_x <= 6165 && cache_x >= 6085 && cache_z >= -4428 && cache_z <= -4339) ||
                          (cache_x <= 6448 && cache_x >= 6378 && cache_z >= -3955 && cache_z <= -3834) ||
                          (cache_x <= 6780 && cache_x >= 6607 && cache_z >= -4427 && cache_z <= -4094))
                    {
                        _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnNearHere);
                        destroyItem = true;
                    }

                    if (destroyItem)
                    {

                        RemoveTime objetoRemover = null;
                        if (_Entity.IsDeployableObject())
                        {
                            DeployableObject deploy = _Entity.GetObject<DeployableObject>();
                            if (deploy != null && deploy.gameObject != null)
                            {
                                objetoRemover = deploy.gameObject.AddComponent<RemoveTime>();
                                objetoRemover.componentEntity = _Entity;
                            }
                            else
                            {
                                try {
                                    _Entity.Destroy();
                                }
                                catch (Exception ex)
                                {
                                    try { Logger.LogDebug("[Error] Entity.Destroy name 7 " + _Entity.Name); } catch { }
                                    Logger.LogDebug("[GlitchFix] Entity.Destroy 7 Error " + ex);
                                }
                                Logger.LogDebug("[Error] GlithFIX ================== falta deploy.gameObject");
                            }
                        }
                        else if (_Entity.IsStructure())
                        {
                            StructureComponent struture = _Entity.GetObject<StructureComponent>();
                            if (struture != null && struture.gameObject != null)
                            {
                                objetoRemover = struture.gameObject.AddComponent<RemoveTime>();
                                objetoRemover.componentEntity = _Entity;
                            }
                            else
                            {
                                try {
                                    _Entity.Destroy();
                                }
                                catch (Exception ex)
                                {
                                    try { Logger.LogDebug("[Error] Entity.Destroy name 8 " + _Entity.Name); } catch { }
                                    Logger.LogDebug("[GlitchFix] Entity.Destroy 8 Error " + ex);
                                }
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
                        _Player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnBuildRock);
                        try {
                            _Entity.Destroy();
                        }
                        catch (Exception ex)
                        {
                            try { Logger.LogDebug("[Error] Entity.Destroy name 9 " + _Entity.Name); } catch { }
                            Logger.LogDebug("[GlitchFix] Entity.Destroy 9 Error " + ex);
                        }
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
            player.MessageFrom(ProjectX.configServer.NameServer, configGlith.warnTryBugRock);
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
        //public static void buildRestricao(object props) {
        public static void buildRestricao(Fougerite.Player actualplacer, Fougerite.Entity Entity) {

            //Fougerite.Player actualplacer = (Fougerite.Player)props.GetProperty("actualplacer");
            //Fougerite.Entity Entity = (Entity)props.GetProperty("Entity");

            int cache_x = Convert.ToInt32(Entity.Location.x);
            int cache_z = Convert.ToInt32(Entity.Location.z);
            int ajustVar = 0;

            if (cache_x <= 7729 && cache_x >= 4129 && cache_z >= -6074 && cache_z <= -2574)
            {
                if (Entity.Name.IndexOf("Pillar") != -1)
                {
                    ajustVar = configGlith.heightAllowed - 20;
                } else
                {
                    ajustVar = configGlith.limitFoundations - 75;
                }
            } else
            {
                if (Entity.Name.IndexOf("Pillar") != -1)
                {
                    ajustVar = configGlith.heightAllowed;
                }
                else
                {
                    ajustVar = configGlith.limitFoundations;
                }
            }

            if (Entity.Name.IndexOf("Pillar") != -1)
            {
                foreach (var item in Entity.GetLinkedStructs())
                {
                    if (item.Name == "WoodFoundation" || item.Name == "MetalFoundation") {
                        if (ajustVar < Entity.Y - item.Y)
                        {
                            try {
                                Entity.Destroy();
                            } catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 10 " + Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 10 Error " + ex);
                            }
                        giveItemRemove(Entity.Name, actualplacer, 1, configGlith.warnHeightAllowed);
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
                            try {
                                Entity.Destroy();
                            }
                            catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 11 " + Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 11 Error " + ex);
                            }
                            giveItemRemove(Entity.Name, actualplacer, 1, string.Format(configGlith.warnLimitFoundations, ajustVar));
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
                        if (countFundation >= configGlith.limitFoundations)
                        {
                            try {
                                Entity.Destroy();
                            }
                            catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 12 " + Entity.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 12 Error " + ex);
                            }
                            giveItemRemove(Entity.Name, actualplacer, 1, string.Format(configGlith.warnLimitFoundations, ajustVar));
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
                if (name != null)
                {
                    try
                    {
                        switch (name)
                        {
                            case "WoodFoundation":
                                name = "wood foundation";
                                break;
                            case "MetalFoundation":
                                name = "metal foundation";
                                break;
                            case "WoodRamp":
                                name = "wood ramp";
                                break;
                            case "MetalRamp":
                                name = "metal ramp";
                                break;
                            case "WoodPillar":
                                name = "wood pillar";
                                break;
                            case "MetalPillar":
                                name = "metal pillar";
                                break;
                            case "WoodDoor":
                                name = "wood door";
                                break;
                            case "MetalDoor":
                                name = "metal door";
                                break;
                            case "WoodBoxLarge":
                                name = "woodbox large";
                                break;
                            default:
                                name = name.ToLower();
                                break;
                        }

                        if (name != null && displaynameToDataBlock.ContainsKey(name))
                        {
                            ProjectX.AddItemInventory(actualplacer, displaynameToDataBlock[name], 1);
                            if (message != null)
                            {
                                actualplacer.MessageFrom(ProjectX.configServer.NameServer, message);
                            }
                        } else
                        {
                            Logger.LogDebug("[Error] antiglith giveItemRemove item not find: " + name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug("[Error] antiglith giveItemRemove : " + ex);
                    }
                }
                else
                {
                    Logger.LogDebug("[Error] antiglith giveItemRemove 2: " + message);
                }
            }
        }
        
        //Objeto com os parametros necessarios para validar limites em segundo plano
        public class ParametrosBuild {
            public Fougerite.Player actualplacer;
            public Fougerite.Entity Entity;

            public ParametrosBuild(Fougerite.Player player, Fougerite.Entity item)
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
                            try {
                                item.Destroy();
                            }
                            catch (Exception ex)
                            {
                                try { Logger.LogDebug("[Error] Entity.Destroy name 13 " + item.Name); } catch { }
                                Logger.LogDebug("[GlitchFix] Entity.Destroy 13 Error " + ex);
                            }
                        }
                    }
                    try {
                        componentEntity.Destroy();
                    }
                    catch (Exception ex)
                    {
                        try { Logger.LogDebug("[Error] Entity.Destroy name 14 " + componentEntity.Name); } catch { }
                        Logger.LogDebug("[GlitchFix] Entity.Destroy 14 Error " + ex);
                    }
                    Destroy(this);
                }
            }
        }
        
    }

}
