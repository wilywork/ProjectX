// Reference: Facepunch.ID
// Reference: Facepunch.MeshBatch
// Reference: Facepunch.HitBox

namespace ProjectX.Plugins
{
    using UnityEngine;

    public class OwnerCommand
    {
        /* owner */
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            RaycastHit cachedRaycast;
            string nameOwner = null;
            bool cachedBoolean;
            Facepunch.MeshBatch.MeshBatchInstance cachedhitInstance;

            if (Arguments.argUser.playerClient != null && Arguments.argUser.playerClient.rootControllable != null)
            {

                Fougerite.Player player = Fougerite.Server.Cache[Arguments.argUser.playerClient.userID];

                Character cachedCharacter = Arguments.argUser.playerClient.rootControllable.idMain.GetComponent<Character>();

                if (!MeshBatchPhysics.Raycast(cachedCharacter.eyesRay, out cachedRaycast, out cachedBoolean, out cachedhitInstance)) { player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Isso não é uma construção!"); return; }

                if (cachedhitInstance != null)
                {
                    Collider cachedCollider = cachedhitInstance.physicalColliderReferenceOnly;

                    StructureComponent cachedStructure = cachedCollider.GetComponent<StructureComponent>();

                    if (cachedStructure != null && cachedStructure._master != null)
                    {
                        StructureMaster cachedMaster = cachedStructure._master;

                        if (ProjectX.userCache.ContainsKey(cachedMaster.ownerID))
                        {
                            nameOwner = ProjectX.userCache[cachedMaster.ownerID]["name"];
                        }

                        if (Arguments.argUser.CanAdmin())
                        {
                            player.MessageFrom(ProjectX.configServer.NameServer, string.Format("Dono construção: [color #fc8815] {0} [/color] ID: [color #fc8815] {1}[/color]", nameOwner == null ? "UnknownPlayer" : nameOwner.ToString(), cachedMaster.ownerID.ToString()));
                        }
                        else
                        {
                            player.MessageFrom(ProjectX.configServer.NameServer, string.Format("Dono desta construção - [color #fc8815] {0}", nameOwner == null ? "UnknownPlayer" : nameOwner.ToString()));
                        }
                        return;
                    }
                    else
                    {
                        player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Isso não é uma construção!");
                        return;
                    }
                }
                else
                {
                    DeployableObject cachedDeployable = cachedRaycast.collider.GetComponent<DeployableObject>();
                    if (cachedDeployable != null)
                    {

                        if (ProjectX.userCache.ContainsKey(cachedDeployable.ownerID))
                        {
                            nameOwner = ProjectX.userCache[cachedDeployable.ownerID]["name"];
                        }

                        string tipo = "objeto";
                        if (cachedDeployable.gameObject != null && cachedDeployable.gameObject.name != null && cachedDeployable.gameObject.name.Contains("MaleSleeper"))
                        {
                            tipo = "corpo";
                        }
                        if (Arguments.argUser.CanAdmin())
                        {
                            player.MessageFrom(ProjectX.configServer.NameServer, string.Format("Dono deste {2} - [color #fc8815] {0} [/color] SteamID: [color #fc8815] {1}", nameOwner == null ? "UnknownPlayer" : nameOwner.ToString(), cachedDeployable.ownerID.ToString(), tipo));
                        }
                        else
                        {
                            player.MessageFrom(ProjectX.configServer.NameServer, string.Format("Dono deste {1} - [color #fc8815] {0}", nameOwner == null ? "UnknownPlayer" : nameOwner.ToString(), tipo));
                        }
                        return;
                    }
                    else
                    {
                        player.MessageFrom(ProjectX.configServer.NameServer, "[color yellow]Isso não é uma construção!");
                        return;
                    }
                }
            }
        }
    }
}
