using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers
{
    public class CamoTrap : ModTrackPower
    {
        public override int Cost => Main.CamoTrapCost;
        protected override int Pierce => Main.CamoTrapPierce;
        public override int Order => 7;

        protected override ProjectileModel GetProjectile(PowerModel powerModel)
        {
            return powerModel.GetBehavior<CamoTrapModel>().projectileModel;
        }
    }
}