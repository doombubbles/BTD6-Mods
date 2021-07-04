using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Displays.Tier5
{
    public class RoyalFlushDisplay : ModTowerDisplay<CardMonkey>
    {
        public override string BaseDisplay => GetDisplay(TowerType.Alchemist, 0, 0, 5);
        
        public override bool UseForTower(int[] tiers)
        {
            return tiers[0] == 5;
        }

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            // node.PrintInfo();
            
            // Remove the potion in hand
            node.RemoveBone("AlchemistRig:Propjectile_R");

            // PrintInfo() showed this has multiple SkinnedMeshRenderers, so have to specify the index
            SetMeshTexture(node, "RoyalFlushDisplay", 0);
            SetMeshTexture(node, "RoyalFlushDisplay", 1);
        }
    }
}