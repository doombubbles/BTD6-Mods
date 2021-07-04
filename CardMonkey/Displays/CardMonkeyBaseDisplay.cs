using System.Linq;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;

namespace CardMonkey.Displays
{
    public class CardMonkeyBaseDisplay : ModTowerDisplay<CardMonkey>
    {
        // Copy the Boomerang Monkey display
        public override string BaseDisplay => GetDisplay(TowerType.BoomerangMonkey);

        public override bool UseForTower(int[] tiers)
        {
            return tiers.Sum() == 0;
        }

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            // Print info about the node in order to edit it easier
            // node.PrintInfo();
            // node.SaveMeshTexture();
            
            
            // Set our custom texture
            SetMeshTexture(node, Name);

            // Make it not hold the Boomerang
            node.RemoveBone("SuperMonkeyRig:Dart");
        }
    }
}