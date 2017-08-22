using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX.Plugins
{
    public class clearSystem
    {
        public static int Barricades() {
            int totalclear = 0;
            foreach (var deploy in Resources.FindObjectsOfTypeAll(typeof(DeployableObject)))
            {
                if ((deploy as DeployableObject).gameObject.name == "Barricade_Fence_Deployable(Clone)" && (deploy as DeployableObject)._carrier == null && shouldDestroy((deploy as DeployableObject).transform.position, 1.5f))
                {
                    TakeDamage.KillSelf((deploy as DeployableObject).GetComponent<IDMain>());
                    totalclear++;
                }
            }
            return totalclear;
        }

        public static void playersInativos() {

            ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color #ff9d00]Iniciando Limpeza, poderá dar lag.");

            //*******************************buscar jogadores*****************************
            var dataAtual = ProjectX.DataAtual();
            var inativos = new List<ulong>();
            int totalObjExcluido = 0;
            int totalObj = 0;

            foreach (var pair in ProjectX.userCache)
            {
                var keyUser = pair.Key.ToString();
                if (keyUser != "76561197961480181") { //donos arena
                    if (pair.Value["date"] != null)
                    {
                        DateTime ultimoAcesso = Convert.ToDateTime(pair.Value["date"]);
                        int totaldias = Convert.ToInt32((dataAtual.Date - ultimoAcesso.Date).TotalDays);
                        if (totaldias >= ProjectX.configServer.totalDeDiasSemlogar)
                        {
                            inativos.Add(pair.Key);
                        }
                    }
                    else
                    {
                        inativos.Add(pair.Key);
                    }
                }
            }

            //****************************** elementos ***********************************
            foreach (var strucMaster in Resources.FindObjectsOfTypeAll(typeof(StructureMaster)))
            {

                if (inativos.Contains((strucMaster as StructureMaster).ownerID))
                {
                    GameObject.Destroy((strucMaster as StructureMaster).GetComponent<IDMain>().gameObject);
                    totalObjExcluido++;
                }
                totalObj++;

            }

            foreach (var struc in Resources.FindObjectsOfTypeAll(typeof(StructureComponent)))
            {

                if ((struc as StructureComponent)._master)
                {
                    if (inativos.Contains((struc as StructureComponent)._master.ownerID))
                    {
                        GameObject.Destroy((struc as StructureComponent).GetComponent<IDMain>().gameObject);
                        totalObjExcluido++;
                    }
                }
                totalObj++;

            }

            foreach (var deploy in Resources.FindObjectsOfTypeAll(typeof(DeployableObject)))
            {

                if (inativos.Contains((deploy as DeployableObject).ownerID))
                {
                    GameObject.Destroy((deploy as DeployableObject).GetComponent<IDMain>().gameObject);
                    totalObjExcluido++;
                }
                else if ((deploy as DeployableObject).gameObject.name == "Barricade_Fence_Deployable(Clone)" && (deploy as DeployableObject)._carrier == null && shouldDestroy((deploy as DeployableObject).transform.position, 2f))
                {
                    GameObject.Destroy((deploy as DeployableObject).GetComponent<IDMain>().gameObject);
                    totalObjExcluido++;
                }
                totalObj++;

            }

            ProjectX.BroadCast(ProjectX.configServer.NameServer, "[color green]Limpeza concluída, limpou [color #ffffff]" + totalObjExcluido.ToString() + "[/color] objetos de um total de [color #ffffff]" + totalObj.ToString() + "[/color] objetos.");

        }

        public static bool shouldDestroy(Vector3 position, float dist)
        {
            foreach (Collider collider in Physics.OverlapSphere(position, dist))
            {
                if (collider.GetComponent<DeployableObject>() != null) continue;
                if (collider.gameObject.layer == 0)
                {
                    return false;
                }
            }
            return true;
        }

    }

    public class ClearCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (Arguments.argUser.admin || Permission.HasPermission(Arguments.argUser.userID, "admin")) {

                if (ChatArguments != null && ChatArguments.Length > 0 && (ChatArguments[0] == "barricada" || ChatArguments[0] == "barricadas")) {
                    Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, "[color green] Total de Barricadas apagadas: [/color]" + clearSystem.Barricades());
                } else {
                    clearSystem.playersInativos();
                }

            } else {
                Fougerite.Server.Cache[Arguments.argUser.userID].MessageFrom(ProjectX.configServer.NameServer, ProjectX.configServer.WarnNotPermission);
            }
        }
    }
}
