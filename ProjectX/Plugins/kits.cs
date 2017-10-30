using Fougerite;
using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{

    public class Kits
    {

        public class Config
        {
            public string kithelp;
            public string kithelp2;
            public string kitredeemed;
            public string noAccess;
            public string unknownKit;
            public string yourKits;
            public string kitView;
            public string kitTimeCooldown;
            public static string timerIntervalKit;
            public Dictionary<string, PropKits> listKits;

            public Config Default()
            {
                kithelp = "/kit => para ver todos os kit's";
                kithelp2 = "/kit name => para receber o kit";
                kitredeemed = "Você recebeu o kit.";
                noAccess = "Você não tem permissão para usar este comando.";
                unknownKit = "Esse kit não existe.";
                yourKits = "Seus Kit's[/color]:";
                kitView = "{0} - {1}";
                kitTimeCooldown = " Você deve esperar {0} para usar o kit {1} novamente.";
                timerIntervalKit = " Tempo de intervalo {0}.";
                listKits = new Dictionary<string, PropKits>() {
                    {"admin", new PropKits("admin", 5f, new Dictionary<string, int>() { { "uber hatchet", 1 }, { "invisible helmet", 1 }, { "invisible vest", 1 }, { "invisible pants", 1 }, { "invisible boots", 1 }, { "cooked chicken breast", 35 } })},
                    {"starter", new PropKits(null, 60f, new Dictionary<string, int>() { { "stone hatchet", 1 }, { "torch", 1 }, { "bandage", 5}, { "cooked chicken breast", 35} })}
                };
                return this;
            }
        }

        public static Config configKits = new Config();

        //inicia os kits
        public static void Start()
        {
            configKits = ProjectX.ReadyConfigChecked<Config>(configKits.Default(), "config/kits.json");
        }

        // givekits
        public static void GiveKit(Fougerite.Player player, PropKits kit)
        {
            if (player != null && player.PlayerClient != null) {
                try
                {
                    foreach (KeyValuePair<string, int> ingredients in kit.itens)
                    {
                        if (ProjectX.displaynameToDataBlock.ContainsKey(ingredients.Key.ToLower()))
                        {
                            ProjectX.AddItemInventory(player, ProjectX.displaynameToDataBlock[ingredients.Key], ingredients.Value);
                        }
                    }
                    player.MessageFrom(ProjectX.configServer.NameServer, configKits.kitredeemed);
                }
                catch (Exception ex)
                {
                    Logger.LogDebug("[Error] GiveKit: "+ ex);
                }
            }
            else
            {
                Logger.LogDebug("[Error] Player offline para GiveKit");
            }
        }

        //check permissoes e tempo de cooldown
        public static void HasKit(Fougerite.Player player, string kit)
        {

            if (configKits.listKits[kit].permission == null || Permission.HasPermission(player.UID, configKits.listKits[kit].permission))
            {
                if (ProjectX.cacheKits.ContainsKey(player.UID)) {
                    if (ProjectX.cacheKits[player.UID].ContainsKey(kit)) {

                        if (ProjectX.cacheKits[player.UID][kit] < ProjectX.TimeSeconds())
                        {
                            ProjectX.cacheKits[player.UID][kit] = ProjectX.TimeSeconds() + configKits.listKits[kit].cooldown;
                            GiveKit(player, configKits.listKits[kit]);
                        }
                        else
                        {
                            int cacheTimeUser = Convert.ToInt32((ProjectX.cacheKits[player.UID][kit] - ProjectX.TimeSeconds()));

                            if (cacheTimeUser > 60)
                            {
                                player.MessageFrom(ProjectX.configServer.NameServer, string.Format(configKits.kitTimeCooldown, (cacheTimeUser/60).ToString() + "min", kit));
                            }
                            else
                            {
                                player.MessageFrom(ProjectX.configServer.NameServer, string.Format(configKits.kitTimeCooldown, cacheTimeUser.ToString() + "s", kit));
                            }

                        }
                    }
                    else
                    {
                        ProjectX.cacheKits[player.UID].Add(kit, ProjectX.TimeSeconds() + configKits.listKits[kit].cooldown);
                        GiveKit(player, configKits.listKits[kit]);
                    }
                } else
                {
                    Fougerite.SerializableDictionary<string, double> cacheTimeCheck = new Fougerite.SerializableDictionary<string, double>();
                    cacheTimeCheck.Add(kit, ProjectX.TimeSeconds() + configKits.listKits[kit].cooldown);
                    ProjectX.cacheKits.Add(player.UID, cacheTimeCheck);
                    GiveKit(player, configKits.listKits[kit]);
                }

            }
            else
            {
                player.MessageFrom(ProjectX.configServer.NameServer, configKits.noAccess);
            }
        }

        // mykits
        public static void printYourKits(Fougerite.Player player)
        {
            player.MessageFrom(ProjectX.configServer.NameServer, configKits.yourKits);
            foreach (var kit in configKits.listKits)
            {
                if (kit.Value.permission == null || Permission.HasPermission(player.UID, kit.Value.permission))
                {
                    player.MessageFrom(ProjectX.configServer.NameServer, string.Format(configKits.kitView, kit.Key, kit.Value.desc));
                }
            }
        }
    }

    public class PropKits
    {
        public string permission;
        public string desc;
        public float cooldown;
        public Dictionary<string, int> itens;

        public PropKits(string _name, float _cooldown, Dictionary<string, int> _itens)
        {
            if (_name != null) {
                permission = _name;
            }
            
            cooldown = _cooldown;
            itens = _itens;

            string brev = _cooldown + "seg";

            if (_cooldown >= 60)
            {
                brev = Math.Round(_cooldown / 60) + "min";
            }

            desc = string.Format(Kits.Config.timerIntervalKit, brev);
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
                if (Kits.configKits.listKits.ContainsKey(ChatArguments[0]))
                {
                    Kits.HasKit(cachePlayer, ChatArguments[0]);
                }
                else
                {
                    cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.configKits.unknownKit);
                }
            }
            else
            {
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.configKits.kithelp);
                cachePlayer.MessageFrom(ProjectX.configServer.NameServer, Kits.configKits.kithelp2);
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
