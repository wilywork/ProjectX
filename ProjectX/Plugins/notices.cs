using System.Collections.Generic;
using TimerEdit;
using UnityEngine;

namespace ProjectX.Plugins
{
    class Notices
    {

        public class Config {
            public int timerBroadCast;
            public List<string> listNotices;

            public Config Default()
            {
                timerBroadCast = 60;
                listNotices = new List<string>() { "like fan page.." };
                return this;
            }
        }
        
        public static Config configNotices = new Config();

        private static int countBroadCastNotice = 0;

        public static void Start() {
            configNotices = ProjectX.ReadyConfigChecked<Config>( configNotices.Default(), "config/notices.json");

            //start timer repeat message
            TimerEvento.Repeat(configNotices.timerBroadCast, 0, () => {
                NextBroadCastNotice();
            });
        }
        
        public static void NextBroadCastNotice()
        {
            if ((configNotices.listNotices.Count - 1) >= countBroadCastNotice)
            {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, configNotices.listNotices[countBroadCastNotice]);
                countBroadCastNotice++;
            }
            else
            {
                ProjectX.BroadCast(ProjectX.configServer.NameServer, configNotices.listNotices[0]);
                countBroadCastNotice = 1;
            }
        }
    }
}
