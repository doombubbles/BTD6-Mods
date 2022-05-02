using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers
{
    public class GlueTrap : ModTrackPower
    {
        public override int Cost => Main.GlueTrapCost;
        protected override int Pierce => Main.GlueTrapPierce;
        public override int Order => 6;

        protected override ProjectileModel GetProjectile(PowerModel powerModel)
        {
            return powerModel.GetBehavior<GlueTrapModel>().projectileModel;
        }
    }
}