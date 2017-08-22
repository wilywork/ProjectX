using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    public class Kits
    {

        //estancia
        public static string cantUseKit = "Você não tem permissão para usar este kit.";
        public static string kithelp = "/kit => para ver todos os kit's";
        public static string kithelp2 = "/kit name => para receber o kit";
        public static string kitredeemed = "Você recebeu o kit.";
        public static string noAccess = "Você não tem permissão para usar este comando.";
        public static string unknownKit = "Esse kit não existe.";
        public static string yourKits = "Seus Kit's:";
        public static string kitView = "{0} - {1}";
        public static string kitTimeCooldown = "Você deve esperar {0} para usar o kit {1} novamente.";

        public static Inventory inventario;

        // kits
        public static Dictionary<string, PropKits> listKits;

        //inicia os kits
        public static void Start()
        {
            listKits = new Dictionary<string, PropKits>();

            listKits.Add("starter", new PropKits("starter", " Tempo de intervalo 1 minuto.", 60f, false, new ProjectX.Ingrediente(new List<string>() { "Stone Hatchet_1", "Torch_1", "Bandage_5", "Cooked Chicken Breast_35" })));
        }

        // givekits
        public static void GiveKit(Fougerite.Player player, PropKits kit)
        {
            inventario = player.PlayerClient.rootControllable.GetComponent<Inventory>();

            foreach (KeyValuePair<string, int> ingredients in kit.itens)
            {
                if (ProjectX.displaynameToDataBlock.ContainsKey(ingredients.Key.ToLower()))
                {
                    ProjectX.AddItemInventory(player, ProjectX.displaynameToDataBlock[ingredients.Key].name, ingredients.Value);
                }
            }
            player.MessageFrom(ProjectX.configServer.NameServer, kitredeemed);
        }

        //check permissoes e tempo de cooldown
        public static void HasKit(Fougerite.Player player, string kit)
        {

            if (!listKits[kit].perm || Permission.HasPermission(player.UID, listKits[kit].namePerm))
            {
                if (ProjectX.cacheKits.ContainsKey(player.UID)) {
                    if (ProjectX.cacheKits[player.UID].ContainsKey(kit)) {

                        if (ProjectX.cacheKits[player.UID][kit] < ProjectX.TimeSeconds())
                        {
                            ProjectX.cacheKits[player.UID][kit] = ProjectX.TimeSeconds() + listKits[kit].cooldown;
                            GiveKit(player, listKits[kit]);
                        }
                        else
                        {
                            int cacheTimeUser = Convert.ToInt32((ProjectX.cacheKits[player.UID][kit] - ProjectX.TimeSeconds()));

                            if (cacheTimeUser > 60)
                            {
                                player.MessageFrom(ProjectX.configServer.NameServer, string.Format(kitTimeCooldown, (cacheTimeUser/60).ToString() + "min", kit));
                            }
                            else
                            {
                                player.MessageFrom(ProjectX.configServer.NameServer, string.Format(kitTimeCooldown, cacheTimeUser.ToString() + "s", kit));
                            }

                        }
                    }
                    else
                    {
                        ProjectX.cacheKits[player.UID].Add(kit, ProjectX.TimeSeconds() + listKits[kit].cooldown);
                        GiveKit(player, listKits[kit]);
                    }
                } else
                {
                    Fougerite.SerializableDictionary<string, double> cacheTimeCheck = new Fougerite.SerializableDictionary<string, double>();
                    cacheTimeCheck.Add(kit, ProjectX.TimeSeconds() + listKits[kit].cooldown);
                    ProjectX.cacheKits.Add(player.UID, cacheTimeCheck);
                    GiveKit(player, listKits[kit]);
                }

            }
            else
            {
                player.MessageFrom(ProjectX.configServer.NameServer, noAccess);
            }
        }

        // mykits
        public static void printYourKits(Fougerite.Player player)
        {
            player.MessageFrom(ProjectX.configServer.NameServer, Kits.yourKits);
            foreach (var kit in listKits)
            {
                if (!kit.Value.perm || Permission.HasPermission(player.UID, kit.Value.namePerm))
                {
                    player.MessageFrom(ProjectX.configServer.NameServer, string.Format(kitView, kit.Key, kit.Value.desc));
                }
            }
        }
    }

    public class PropKits
    {

        public bool perm;
        public string namePerm;
        public string desc;
        public float cooldown;
        public Dictionary<string, int> itens;

        public PropKits(string _name, string _desc, float _cooldown, bool _perm, ProjectX.Ingrediente _itens)
        {
            perm = _perm;
            namePerm = _name;
            desc = _desc;
            cooldown = _cooldown;
            itens = _itens.ingredientes;
        }

    }

    public class KitCommand
    {

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Fougerite.Player cachePlayer = Fougerite.Server.Cache[Arguments.argUser.userID];

            if (ChatArguments.Length == 0)
            {
                // exibir lista de kits disponiveis para este player
                Kits.printYourKits(cachePlayer);
            }
            else if (ChatArguments.Length == 1)
            {
                if (Kits.listKits.ContainsKey(ChatArguments[0]))
                {
                    Kits.HasKit(cachePlayer, ChatArguments[0]);
                }
                else
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.unknownKit);
                }
            }
            else
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.kithelp);
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.kithelp2);
            }

        }
    }

    public class KitsCommand
    {
        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            // exibir lista de kits disponiveis para este player
            Kits.printYourKits(Fougerite.Server.Cache[Arguments.argUser.userID]);
        }
    }
}
