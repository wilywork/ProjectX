using Fougerite.Events;
using System.Collections.Generic;

namespace ProjectX.Plugins
{
    class CraftBlock
    {
        public class Config
        {
            public string msgBlockCraft;
            public List<string> listCraftBlocks;

            public Config Default()
            {
                msgBlockCraft = "Item Bloqueado!";
                listCraftBlocks = new List<string>() { "Sleeping Bag Blueprint", "Camp Fire Blueprint", "Bed Blueprint", "Wood Shelter Blueprint" };
                return this;
            }
        }

        public static Config configCraft = new Config();

        public static void Start()
        {
            configCraft = ProjectX.ReadyConfigChecked<Config>(configCraft.Default(), "config/craftBlock.json");
        }

        public static void HookOnCrafting(CraftingEvent craft)
        {
            if (configCraft.listCraftBlocks.Contains(craft.ItemName))
            {
                craft.Cancel();
                craft.Player.Notice("✘", configCraft.msgBlockCraft, 5);
            }
        }
    }
}
