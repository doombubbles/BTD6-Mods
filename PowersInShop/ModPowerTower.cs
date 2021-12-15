using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Utils;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop
{
    public abstract class ModPowerTower : ModTower<Powers>
    {
        public sealed override int TopPathUpgrades => 0;
        public sealed override int MiddlePathUpgrades => 0;
        public sealed override int BottomPathUpgrades => 0;
        public override bool DontAddToShop => Cost < 0;
        
        public override string DisplayName => 
            Game.instance.GetLocalizationManager().GetText(Name);
        public sealed override string Description =>
            Game.instance.GetLocalizationManager().GetText(Name + " Description");
        
        public sealed override SpriteReference IconReference => PortraitReference;
        public sealed override SpriteReference PortraitReference
        {
            get
            {
                var powerWithName = Game.instance.model.GetPowerWithName(Name);
                return powerWithName.tower?.portrait ?? powerWithName.icon;
            }
        }
        
        public override void ModifyBaseTowerModel(TowerModel towerModel)
        {
            towerModel.powerName = Name;
        }
    }
}