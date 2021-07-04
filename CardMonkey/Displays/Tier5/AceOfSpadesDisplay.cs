using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Displays.Tier5
{
    public class AceOfSpadesDisplay : ModTowerDisplay<CardMonkey>
    {
        public override string BaseDisplay => GetDisplay(TowerType.BoomerangMonkey, 5, 0, 0);
        
        public override bool UseForTower(int[] tiers)
        {
            return tiers[1] == 5;
        }

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            //node.SaveMeshTexture();
            //node.PrintInfo();
            
            node.RemoveBone("SuperMonkeyRig:Dart");

            // PrintInfo() showed this has multiple SkinnedMeshRenderers, so have to specify the index
            SetMeshTexture(node, "AceOfSpadesCape", 0);
            SetMeshTexture(node, "CardMonkeyBaseDisplay", 1);
        }
    }
}