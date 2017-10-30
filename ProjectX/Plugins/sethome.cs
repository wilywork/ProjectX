using Fougerite;
using UnityEngine;
using System;

namespace ProjectX.Plugins
{
    public class Home
    {
        public static string teleportCancelled = "[color red]Teleporte cancelado!";
        public static string teleportRestricted = "[color yellow]Você não pode se teleporta para sua casa neste local!";
        public static string onlyBuildingsMessage = "[color yellow]Só é possível dar sethome em cima da fundação com 1 pilar.";
        public static string onlyFoundationsMessage = "[color yellow]Só é possível dar sethome em cima da fundação com 1 pilar.";
        public static string warnSethomeRock = "[color yellow]Parece que você esta perto de uma árvore ou rocha, afaste-se!";
        public static string onlySelfOrFriend = "[color yellow]Para dar sethome na casa de seu amigo, é preciso ter share.";
        public static string onlySelfMessage = "On buildings, you are only allowed to sethome on your home.";
        public static string nohomesSet = "[color yellow]Você não tem nenhuma casa.";
        public static string homeDoesntExist = "[color yellow]Está casa não existe!";
        public static string notAllowedHere = "[color red]Você não tem permissão para dar sethome aqui.";
        public static string alreadyWaiting = "[color red]Você já está a espera de um teletransporte casa.";
        public static string cooldownMessage = "[color yellow]Você deve esperar {0} segundos antes de solicitar outro teletransporte casa.";
        public static string teleportationMessage = "[color #00efca]Você será teleportado para sua casa em {0} segundos.";
        public static string homelistMessage = "Lista de casas:";
        public static string newhome = "[color green]Você definiu um novo lar chamado[/color] {0} @ {1}";
        public static string maxhome = "[color yellow]Você atingiu o limite máximo de casas.";
        public static string homeErased = "[color green]Sua casa( [color #ffffff]{0}[/color] ) foi apagada com sucesso!";
        public static string sethomeHelp1 = "/sethome XXX => to set home where you stand";
        public static string sethomeHelp2 = "/sethome remove XXX => to remove a home";
        public static string sethomeHelp3 = "[color yellow]Uso de espaço no nome da home é proibido.";


        public static bool sethomeOnlyBuildings = true;
        public static bool sethomeOnlyFoundation = true;
        public static bool sethomeOnlySelf = true;
        public static bool useShare = true;

        public static string notAllowed = "[color red]Você não tem permissão para usar este comando.";
        public static bool cancelOnHurt = true;
        public static int maxAllowed = 4;
        public static int timerTeleport = 7;
        public static int timeinterval = 60;

        public static Vector3 Vector3Down = new Vector3(0f, -1f, 0f);
        public static Vector3 VectorDown = new Vector3(0f, -0.4f, 0f);
        public static Vector3 UnderPlayerAdjustement = new Vector3(0f, -1.15f, 0f);
        public static Facepunch.MeshBatch.MeshBatchInstance cachedhitInstance;
        public static bool cachedBoolean;
        public static RaycastHit cachedRaycast;
        public static int terrainLayer = LayerMask.GetMask(new string[] { "Static" });
        public static String[] splitText;
        public static Char[] delimitador = new Char[] { ' ' };

    }

    public class HomeTeleport : MonoBehaviour
    {
        public Fougerite.Player playerclient;
        public float activeTime = 0;
        public string LocHome;
        public Vector3 location;
        public bool ativado = false;


        public void Ativar(Fougerite.Player pl)
        {
            playerclient = pl;
            this.activeTime = Time.realtimeSinceStartup + Home.timerTeleport;
            Home.splitText = LocHome.Split(Home.delimitador);
            this.location = new Vector3(float.Parse(Home.splitText[0]), float.Parse(Home.splitText[1]), float.Parse(Home.splitText[2]));
        }

        public void Activate()
        {
            if (playerclient != null && playerclient.PlayerClient != null){
                ProjectX.TeleportPlayer(playerclient.PlayerClient.netUser, location);
            }

            this.OnDestroy();
        }

        public void OnDestroy()
        {
            GameObject.Destroy(this);
        }

        public void OnCancel()
        {
            if (playerclient != null && playerclient.PlayerClient != null)
            {
                Fougerite.Server.Cache[playerclient.UID].MessageFrom(ProjectX.configServer.NameServer, Home.teleportCancelled);
                this.OnDestroy();
            }

        }

        void FixedUpdate()
        {
            if (Time.realtimeSinceStartup > activeTime && !ativado)
            {
                ativado = true;
                this.Activate();
            }
        }
    }

    public class sethomeCommand
    {

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string cacheLocRegex;

            if (ChatArguments.Length == 0)
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Home.sethomeHelp1);
                pl.MessageFrom(ProjectX.configServer.NameServer, Home.sethomeHelp2);
                return;
            }

            if (ChatArguments.Length == 2 && ChatArguments[0] == "remove")
            {
                //consulta home para apagar
                if (ProjectX.homeList.ContainsKey(pl.UID))
                {
                    if (ProjectX.homeList[pl.UID].ContainsKey(ChatArguments[1].ToString().ToLower()))
                    {
                        ProjectX.homeList[pl.UID].Remove(ChatArguments[1].ToString().ToLower());
                        pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Home.homeErased, ChatArguments[1]));
                    }
                    else
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Home.homeDoesntExist);
                    }
                }
                else
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.homeDoesntExist);
                }

                return;
            }

            if ( String.IsNullOrEmpty(ChatArguments[0]) || ChatArguments[0].IndexOf(" ") != -1 )
            {
                pl.MessageFrom(ProjectX.configServer.NameServer, Home.sethomeHelp3);
                return;
            }

            //validar se esta em modo tpr
            if (!pl.Admin)
            {

                // validar se pode usar sethome
                MeshBatchPhysics.Raycast(pl.PlayerClient.lastKnownPosition + Home.UnderPlayerAdjustement, Home.Vector3Down, out Home.cachedRaycast, out Home.cachedBoolean, out Home.cachedhitInstance);
                if (Home.sethomeOnlyBuildings && Home.cachedhitInstance == null)
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.onlyBuildingsMessage);
                    return;
                }
                // valida fundacao no chao com pilar
                if (Home.sethomeOnlyFoundation && Home.cachedhitInstance != null)
                {
                    if (!Home.cachedhitInstance.physicalColliderReferenceOnly.gameObject.name.Contains("Foundation"))
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Home.onlyFoundationsMessage);
                        return;
                    }
                    //correcao de bug no telhado
                    if ((pl.PlayerClient.lastKnownPosition.y - Home.cachedhitInstance.physicalColliderReferenceOnly.gameObject.transform.position.y) > 6)
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Home.onlyFoundationsMessage);
                        return;
                    }
                }
                //verifica se tem permisao ou é dele
                if (Home.sethomeOnlySelf && Home.cachedhitInstance != null)
                {
                    ulong ownerid = Home.cachedhitInstance.physicalColliderReferenceOnly.GetComponent<StructureComponent>()._master.ownerID;
                    if (ownerid != pl.PlayerClient.userID)
                    {
                        if (!ShareCommand.isShare(ownerid, pl.PlayerClient.userID))
                        {
                            pl.MessageFrom(ProjectX.configServer.NameServer, Home.onlySelfOrFriend);
                            return;
                        }
                    }
                }
                // valida rocha perto
                if (tpr.CheckRock(pl.PlayerClient))
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.warnSethomeRock);
                    return;
                }

            }

            cacheLocRegex = Math.Ceiling(pl.PlayerClient.lastKnownPosition.x).ToString() + " " + Math.Ceiling(pl.PlayerClient.lastKnownPosition.y).ToString() + " " + Math.Ceiling(pl.PlayerClient.lastKnownPosition.z).ToString(); //Regex.Replace(Home.pl.PlayerClient.lastKnownPosition.ToString(), "[(|)|,]", "");

            //verifica se tem home se nao cria
            if (!ProjectX.homeList.ContainsKey(pl.UID))
            {
                SerializableDictionary<string, string> HomeDefault = new SerializableDictionary<string, string>();
                HomeDefault.Add(ChatArguments[0].ToString().ToLower(), cacheLocRegex);
                ProjectX.homeList.Add(pl.UID, HomeDefault);
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Home.newhome, ChatArguments[0], pl.PlayerClient.lastKnownPosition.ToString()));
                return;
            }

            //verificar se existe com mesmo nome
            if (ProjectX.homeList[pl.UID].ContainsKey(ChatArguments[0].ToString().ToLower()) && ProjectX.homeList[pl.UID][ChatArguments[0].ToString().ToLower()] != null)
            {
                ProjectX.homeList[pl.UID][ChatArguments[0].ToString().ToLower()] = cacheLocRegex;
                pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Home.newhome, ChatArguments[0], pl.PlayerClient.lastKnownPosition.ToString()));
                return;
            }

            //se passou do limite avisar
            if (!pl.Admin && ProjectX.homeList.ContainsKey(pl.UID))
            {
                if (ProjectX.homeList[pl.UID].Count >= Home.maxAllowed)
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.maxhome);
                    return;
                }
            }

            //salvar home
            ProjectX.homeList[pl.UID].Add(ChatArguments[0].ToString().ToLower(), cacheLocRegex);
            pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Home.newhome, ChatArguments[0], pl.PlayerClient.lastKnownPosition.ToString()));

        }
    }

    public class HomeCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player pl = Fougerite.Server.Cache[Arguments.argUser.userID];

            if (ChatArguments.Length != 0)
            {
                if (ProjectX.homeList.ContainsKey(pl.UID))
                {
                    if (ProjectX.homeList[pl.UID].ContainsKey(ChatArguments[0].ToString().ToLower()))
                    {
                        //iniciar teleporte
                        HomeTeleport cachedHomeTeleport = pl.PlayerClient.GetComponent<HomeTeleport>();
                        if (cachedHomeTeleport == null)
                        {
                            cachedHomeTeleport = pl.PlayerClient.gameObject.AddComponent<HomeTeleport>();
                            cachedHomeTeleport.LocHome = ProjectX.homeList[pl.UID][ChatArguments[0].ToString().ToLower()];
                            pl.MessageFrom(ProjectX.configServer.NameServer, string.Format(Home.teleportationMessage, Home.timerTeleport.ToString()));
                            cachedHomeTeleport.Ativar(pl);
                        }
                        else
                        {
                            pl.MessageFrom(ProjectX.configServer.NameServer, Home.alreadyWaiting);
                        }

                    }
                    else
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, Home.homeDoesntExist);
                    }
                }
                else
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.nohomesSet);
                }
            }
            else
            {
                if (ProjectX.homeList.ContainsKey(pl.UID))
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.homelistMessage);
                    foreach (var home in ProjectX.homeList[pl.UID])
                    {
                        pl.MessageFrom(ProjectX.configServer.NameServer, home.Key + " - (" + home.Value + ")");
                    }
                }
                else
                {
                    pl.MessageFrom(ProjectX.configServer.NameServer, Home.nohomesSet);
                }
            }

        }
    }
}

