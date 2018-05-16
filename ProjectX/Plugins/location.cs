using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectX.Plugins
{
    public class LocCommand
    {

        public class Config
        {
            public string warnYourLocation;

            public Config Default()
            {
                warnYourLocation = "[color #00ff45]{3} localização [/color]( X {0} [color #00ff45]|[/color] Y {1} [color #00ff45]|[/color] Z {2} )[color #00ff45] perto de [/color]{4}";
                return this;
            }
        }

        public static Config configLocation = new Config();

        public static void Start()
        {
            configLocation = ProjectX.ReadyConfigChecked<Config>(configLocation.Default(), "config/location.json");
        }

        public static float nearest;
        public static Vector2 nearestVector;
        public static float cachedDistance;

        public static Dictionary<Vector2, string> locationsList = GetLocList();

        static Dictionary<Vector2, string> GetLocList()
        {
            var locationslist = new Dictionary<Vector2, string>();
            locationslist.Add(new Vector3(Convert.ToSingle(5907), Convert.ToSingle(-1848)), "Hacker Valley South");
            locationslist.Add(new Vector3(Convert.ToSingle(5268), Convert.ToSingle(-1961)), "Hacker Mountain South");
            locationslist.Add(new Vector3(Convert.ToSingle(5268), Convert.ToSingle(-2700)), "Hacker Valley Middle");
            locationslist.Add(new Vector3(Convert.ToSingle(4529), Convert.ToSingle(-2274)), "Hacker Mountain North");
            locationslist.Add(new Vector3(Convert.ToSingle(4416), Convert.ToSingle(-2813)), "Hacker Valley North");
            locationslist.Add(new Vector3(Convert.ToSingle(3208), Convert.ToSingle(-4191)), "Wasteland North");
            locationslist.Add(new Vector3(Convert.ToSingle(6433), Convert.ToSingle(-2374)), "Wasteland South");
            locationslist.Add(new Vector3(Convert.ToSingle(4942), Convert.ToSingle(-2061)), "Wasteland East");
            locationslist.Add(new Vector3(Convert.ToSingle(3827), Convert.ToSingle(-5682)), "Wasteland West");
            locationslist.Add(new Vector3(Convert.ToSingle(3677), Convert.ToSingle(-4617)), "Sweden");
            locationslist.Add(new Vector3(Convert.ToSingle(5005), Convert.ToSingle(-3226)), "Everust Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(4316), Convert.ToSingle(-3439)), "North Everust Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(5907), Convert.ToSingle(-2700)), "South Everust Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(6825), Convert.ToSingle(-3038)), "Metal Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(7185), Convert.ToSingle(-3339)), "Metal Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(5055), Convert.ToSingle(-5256)), "Metal Hill");
            locationslist.Add(new Vector3(Convert.ToSingle(5268), Convert.ToSingle(-3665)), "Resource Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(5531), Convert.ToSingle(-3552)), "Resource Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(6942), Convert.ToSingle(-3502)), "Resource Hole");
            locationslist.Add(new Vector3(Convert.ToSingle(6659), Convert.ToSingle(-3527)), "Resource Road");
            locationslist.Add(new Vector3(Convert.ToSingle(5494), Convert.ToSingle(-5770)), "Beach");
            locationslist.Add(new Vector3(Convert.ToSingle(5108), Convert.ToSingle(-5875)), "Beach Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(5501), Convert.ToSingle(-5286)), "Coast Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(5750), Convert.ToSingle(-4677)), "Coast Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(6120), Convert.ToSingle(-4930)), "Coast Resource");
            locationslist.Add(new Vector3(Convert.ToSingle(6709), Convert.ToSingle(-4730)), "Secret Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(7085), Convert.ToSingle(-4617)), "Secret Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(6446), Convert.ToSingle(-4667)), "Factory Radtown");
            locationslist.Add(new Vector3(Convert.ToSingle(6120), Convert.ToSingle(-3452)), "Small Radtown");
            locationslist.Add(new Vector3(Convert.ToSingle(5218), Convert.ToSingle(-4800)), "Big Radtown");
            locationslist.Add(new Vector3(Convert.ToSingle(6809), Convert.ToSingle(-4304)), "Hangar");
            locationslist.Add(new Vector3(Convert.ToSingle(6859), Convert.ToSingle(-3865)), "Tanks");
            locationslist.Add(new Vector3(Convert.ToSingle(6659), Convert.ToSingle(-4028)), "Civilian Forest");
            locationslist.Add(new Vector3(Convert.ToSingle(6346), Convert.ToSingle(-4028)), "Civilian Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(6120), Convert.ToSingle(-4404)), "Civilian Road");
            locationslist.Add(new Vector3(Convert.ToSingle(4316), Convert.ToSingle(-5682)), "Ballzack Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(4720), Convert.ToSingle(-5660)), "Ballzack Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(4742), Convert.ToSingle(-5143)), "Spain Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(4203), Convert.ToSingle(-4570)), "Portugal Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(4579), Convert.ToSingle(-4637)), "Portugal");
            locationslist.Add(new Vector3(Convert.ToSingle(4842), Convert.ToSingle(-4354)), "Lone Tree Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(5368), Convert.ToSingle(-4434)), "Forest");
            locationslist.Add(new Vector3(Convert.ToSingle(5907), Convert.ToSingle(-3400)), "Rad-Town Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(4955), Convert.ToSingle(-3900)), "Next Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(5674), Convert.ToSingle(-4048)), "Silk Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(5995), Convert.ToSingle(-3978)), "French Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(7085), Convert.ToSingle(-3815)), "Ecko Valley");
            locationslist.Add(new Vector3(Convert.ToSingle(7348), Convert.ToSingle(-4100)), "Ecko Mountain");
            locationslist.Add(new Vector3(Convert.ToSingle(6396), Convert.ToSingle(-3428)), "Zombie Hill");
            return locationslist;
        }

        public static void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            string reply;
            if (GetLocationString(ref Arguments.argUser, pl, out reply))
            {
                Arguments.ReplyWith(reply);
                pl.MessageFrom(ProjectX.configServer.NameServer, reply);
            }
        }

        public static bool GetLocationString(ref NetUser source, Fougerite.Player location, out string reply)
        {
            bool flag = false;
            try
            {

                string[] v3 = location.Location.ToString("F").Trim(new char[] { '(', ')', ' ' }).Split(new char[] { ',' });
                reply = string.Format(configLocation.warnYourLocation, v3[0], v3[1], v3[2], (location.PlayerClient.netUser == source ? "Sua" : location.PlayerClient.userName), FindLocationName(location.Location));
                flag = true;
            }
            catch (Exception)
            {
                reply = string.Empty;
            }
            return flag;
        }

        public static string FindLocationName(Vector3 position)
        {

            nearest = 999999999f;
            Vector2 currentPos = new Vector2(position.x, position.z);
            foreach (KeyValuePair<Vector2, string> pair in locationsList)
            {
                cachedDistance = Vector2.Distance(currentPos, pair.Key);
                if (cachedDistance < nearest)
                {
                    nearestVector = pair.Key;
                    nearest = cachedDistance;
                }
            }
            return locationsList[nearestVector];

        }
    }
}
