using System.Collections.Generic;
using BTD_Mod_Helper.Api.Towers;

namespace PowersInShop
{
    public class Powers : ModTowerSet
    {
        public override bool AllowInRestrictedModes => Main.AllowInRestrictedModes;

        public override int GetTowerSetIndex(List<string> towerSets)
        {
            return towerSets.IndexOf("Support") + 1;
        }
    }
}