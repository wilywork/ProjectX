using Fougerite;
using System;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    class Chat
    {
        public class Tag
        {
            public string prefix;
            public string color;

            public Tag(string a, string b) {
                prefix = a;
                color = b;
            }
        }

        public class Config
        {
            public Dictionary<string, Tag> tags;

            public List<object> blockChat;

            public int timerMuteSeconds;

            public Config Defaut()
            {
                tags = new Dictionary<string, Tag>() {
                    { "admin", new Tag("[admin] ","[color red]") }
                };
                blockChat = new List<object>() { "lix", "caralh", "pu.t", "pu,t", "ho da p", "mae ", " mae", "li.x", "caralho", "skyrust", "servegame.com", "28015", "net.connect", "uranium", "abor.to", "abor-to", "resto de ab", "aborto", "fo.da", "da.se", "se fud", "cù", "c u", "cú", " cu", "seu cu", "fode", "siririca", "por4", "l.i.x.o", "pora", "buceta", "penis", "porra", "f d p", "fd.p", "li.xo", "mãe", "fdm", "f d m", "f-d-m", "f-d-p", "cabacinho", "cabaçinho", "fodase", "foda-se", "[color", "merda", "puta", "fdp", "c*", "vagabundo", "merdinha", "merd", "tifode", "vsf", "lixo", "abuser", "abuse", "filhodaputa", "bosta", "c.u", "m.e.r.d.a" };
                timerMuteSeconds = 300;
                return this;
            }
        }

        private static string playerFlood = "";
        private static string TimeFlood = "";
        private static int playerFloodCont = 0;

        public static Config configChat = new Config();

        public static void Start()
        {
            configChat = ProjectX.ReadyConfigChecked<Config>(configChat.Defaut(), "config/chat.json");
        }

        public static void HookChat(Fougerite.Player p, ref ChatString text)
        {

            if (String.IsNullOrEmpty(text.NewText) || text.NewText == "\"\"")
            {
                text.NewText = "";
                return;
            }

            if (ProjectX.muteList.Contains(p.UID))
            {
                text.NewText = "";
                p.MessageFrom(ProjectX.configServer.NameServer, "[color red]Você está multado.");

            }
            else
            {
                if (!p.Admin)
                {
                    //reduz string
                    if (text.NewText.Length > 82)
                    {
                        text.NewText = text.NewText.Substring(0, 82);
                    }

                    //tolower
                    text.NewText = text.NewText.ToLower();

                    foreach (string value in configChat.blockChat)
                    {
                        if (text.NewText.IndexOf(value) != -1)
                        {
                            p.MessageFrom(ProjectX.configServer.NameServer, "[color red]Palavra proibida no chat!");
                            text.NewText = "";
                            return;
                        }
                    }


                    DateTime tempo = DateTime.Now;
                    if (playerFlood != p.SteamID)
                    {
                        playerFlood = p.SteamID;
                        playerFloodCont = 0;
                        TimeFlood = tempo.ToString("mm");
                    }
                    else
                    {
                        if (TimeFlood == tempo.ToString("mm"))
                        {
                            playerFloodCont++;
                        }
                        else
                        {
                            TimeFlood = tempo.ToString("mm");
                            playerFloodCont = 0;
                        }
                    }

                    if (playerFloodCont >= 4)
                    {

                        mute.Add(p, configChat.timerMuteSeconds, null);
                        text.NewText = "";
                        return;
                    }
                }
                else
                {
                    if (playerFlood != p.SteamID)
                    {
                        playerFlood = p.SteamID;
                    }
                }

                foreach(string perm in configChat.tags.Keys)
                {
                    if (Permission.HasPermission(p.UID, perm))
                    {
                        ConsoleNetworker.Broadcast($"chat.add {ProjectX.QuoteSafe(configChat.tags[perm].prefix + p.Name)} { ProjectX.QuoteSafe(configChat.tags[perm].color + text.NewText.Substring(1, text.NewText.Length - 2)).Replace("\\\"", "\"")}");
                        text.NewText = "";
                        break;
                    }
                }

                return;

            }
        }

    }
}
