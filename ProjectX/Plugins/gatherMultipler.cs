using Fougerite.Events;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    class GatherMultipler
    {

        private static Dictionary<string, int> gatherMulti = new Dictionary<string, int>() {
            {"Wood", 1},
            {"Sulfur Ore", 1},
            {"Metal Ore", 1},
            {"Stones", 1},
            {"Leather", 1},
            {"Cloth", 1},
            {"Raw Chicken Breast", 1},
            {"Animal Fat", 1},
            {"Blood", 1}
        };

        public static void Start()
        {
            gatherMulti = ProjectX.ReadyConfigChecked<Dictionary<string, int>>(gatherMulti, "config/gatherMultipler.json");
        }

        public static void HookOnPlayerGathering(Fougerite.Player player, GatherEvent ge)
        {
            if (player.Inventory.FreeSlots > 0)
            {
                if (ge.Type != "Tree")
                {
                    if (ge.Quantity > 0)
                    {
                        ProjectX.AddItemInventory(player, ge.Item, ge.Quantity * gatherMulti[ge.Item]);
                    }
                }
            } else
            {
                ProjectX.SendPopup(player.PlayerClient.netUser, "5", "☢", ProjectX.configServer.InventoryFull);
            }
        }
    }
}
