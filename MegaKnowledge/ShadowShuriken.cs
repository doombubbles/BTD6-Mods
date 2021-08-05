using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;

namespace MegaKnowledge
{
    public class ShadowShuriken : ModDisplay
    {
        public override string BaseDisplay =>
            Game.instance.model.GetTower(TowerType.NinjaMonkey).GetWeapon().projectile.display;
        
        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }

    }
}