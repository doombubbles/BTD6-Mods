using Assets.Scripts.Models.Towers;

namespace PowersInShop.Towers
{
    public class Pontoon : ModPowerTower
    {
        public override string BaseTower => TowerType.Pontoon;
        public override int Cost => Main.PontoonCost;
        public override int Order => 2;
    }
}