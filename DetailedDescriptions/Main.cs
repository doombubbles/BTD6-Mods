using System.Collections.Generic;
using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.Upgrade;
using Harmony;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(DetailedDescriptions.Main), "Detailed Descriptions", "1.0.0", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace DetailedDescriptions
{
    public class Main : MelonMod
    {
        public static string tooltip;
        public static UpgradePopup popup;
        public static string baseTooltip;
        public static string baseDescription;
        
        public static string GetTooltip(string upgradeName)
        {
            return UPGRADE_TOOLTIPS[upgradeName];
        }
        
        public static string GetDescription(string towerName)
        {
            return TOWER_DESCRIPTIONS[towerName.ToUpper()];
        }

        [HarmonyPatch(typeof(UpgradeDetails), nameof(UpgradeDetails.OnPointerEnter))]
        internal class UpgradeDetails_OnPointerEnter
        {
            [HarmonyPostfix]
            public static void PostFix(UpgradeDetails __instance)
            {
                tooltip = GetTooltip(__instance.upgrade.name);
            }
        }
        
        [HarmonyPatch(typeof(UpgradeDetails), nameof(UpgradeDetails.OnPointerExit))]
        internal class UpgradeDetails_OnPointerExit
        {
            [HarmonyPostfix]
            public static void PostFix(UpgradeDetails __instance)
            {
                tooltip = null;
            }
        }
        
        [HarmonyPatch(typeof(UpgradePopup), nameof(UpgradePopup.Show))]
        internal class UpgradePopup_Show
        {
            [HarmonyPrefix]
            public static void PreFix(UpgradePopup __instance, ref string description)
            {
                popup = __instance;
                if (baseTooltip == null)
                {
                    baseTooltip = description;
                }
                if (Input.GetKey(KeyCode.LeftShift) && tooltip != null)
                {
                    description = tooltip;
                }
            }
        }
        
        [HarmonyPatch(typeof(UpgradePopup), nameof(UpgradePopup.Hide))]
        internal class UpgradePopup_Hide
        {
            [HarmonyPrefix]
            public static void PreFix()
            {
                popup = null;
                baseTooltip = null;
            }
        }
        
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
        internal class Input_GetKeyDown
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result, KeyCode key)
            {
                if (key == KeyCode.LeftShift && __result)
                {
                    if (popup != null)
                    {
                        popup.Show(tooltip);
                    }

                    var upgradeScreen = MenuManager.instance.GetCurrentMenu().TryCast<UpgradeScreen>();
                    if (upgradeScreen != null)
                    {
                        baseDescription = upgradeScreen.towerDescription.text;
                        upgradeScreen.towerDescription.SetText(GetDescription(upgradeScreen.towerTitle.text));
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(KeyCode))]
        internal class Input_GetKeyUp
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result, KeyCode key)
            {
                if (key == KeyCode.LeftShift && __result)
                {
                    if (popup != null)
                    {
                        popup.Show(baseTooltip);
                    }
                    
                    var upgradeScreen = MenuManager.instance.GetCurrentMenu().TryCast<UpgradeScreen>();
                    if (upgradeScreen != null)
                    {
                        upgradeScreen.towerDescription.SetText(baseDescription);
                    }
                }
            }
        }

        private static readonly Dictionary<string, string> TOWER_DESCRIPTIONS = new Dictionary<string, string>
        {
            {"DART MONKEY", "<u>Dart</u> attack (1d, 2p, 0.95s, 32r, <i>Sharp</i>)  "},
            {"BOOMERANG MONKEY", "<u>Boomerang</u> attack (1d, 4p, 43r, 1.42s, <i>Sharp</i>)"},
            {"BOMB SHOOTER", "<u>Bomb</u> attack (1.5s, 40r), creates on-hit <u>Explosion</u> (1d, 14p, <i>Explosion</i>)"},
            {"TACK SHOOTER", "<u>Tacks</u> attack (1d, 1p, 1.4s, 23r, 8j, <i>Sharp</i>)"},
            {"ICE MONKEY", "<u>Freeze</u> attack (1d, 40p, 2.4s, 20r, <i>Cold</i>) that applies <u>Frozen</u> status for 1.5s"},
            {"GLUE GUNNER", "<u>Glue</u> attack (0d, 1p, 1.0s, 46r, <i>Acid</i>) that applies <u>Glued</u> (11s duration, 50% slow, 3 layers)"},
            {"SNIPER MONKEY", "<u>Bullet</u> attack (2d, 1p, 1.59s, ∞r, <i>Sharp</i>, impact)"},
            {"MONKEY SUB", "<u>Dart</u> attack (1d, 2p, 0.75s, 42r, <i>Sharp</i>)"},
            {"MONKEY BUCCANEER", "<u>Dart</u> attack (1d, 4p, 1.0s, 60r, <i>Sharp</i>) (Also shoots behind Boat if there are targets there)."},
            {"MONKEY ACE", "<u>Radial Darts</u> attack (1d, 5p, 8j, 2.1s, <i>Sharp</i>, passive). Flies on a circular path with radius 80, or a figure 8 or figure infinite with radii 40."},
            {"HELI PILOT", "<u>Darts</u> attack (1d, 3p, 40r, 0.56s, 2j, <i>Sharp</i>)."},
            {"MORTAR MONKEY", "<u>Shell</u> attack (2.0s, ∞r) creates <u>Explosion</u> effect (1d, 40p, <i>Explosion</i> type, ~34 blast radius)."},
            {"DARTLING GUNNER", "<u>Dart</u> attack (1d, 1p, .2s, ∞r, <i>Sharp</i>, 23° spread)"},
            {"WIZARD MONKEY", "<u>Bolt</u> attack (1d, 3p, 40r, 1.1s, <i>Energy</i>)"},
            {"SUPER MONKEY", "<u>Dart</u> attack (1d, 1p, 0.06s, 50r, <i>Sharp</i>)."},
            {"NINJA MONKEY", "<u>Shuriken</u> attack (1d, 2p, 40r, 0.7s, <i>Sharp</i>, Camo)."},
            {"ALCHEMIST", "<u>Potion</u> attack (1d, 15p, 2.0s, 45r, <i>Acid</i>) that applies <u>Acid</u> status (1d/2.0s, 4s duration, <i>Acid</i>)."},
            {"DRUID", "<u>Thorn</u> attack (1d, 1p, 1.1s, 35r, 5j, <i>Sharp</i>)."},
            {"BANANA FARM", "<u>Banana</u> effect (40r, $80 income, split over 4 projectiles throughout the round, 15s lifetime)."},
            {"SPIKE FACTORY", "<u>Spikes</u> attack (1d, 5p, 2.2s, 34r, targets track, 50s or end of round lifespan)."},
            {"MONKEY VILLAGE", "<u>Buff</u> effect (40r; grants: +10%r)."},
            {"ENGINEER MONKEY", "<u>Nail</u> attack (1d, 3p, 0.7s, 40r, <i>Sharp</i>)."}
        };

        private static readonly Dictionary<string, string> UPGRADE_TOOLTIPS = new Dictionary<string, string>
        {
            #region Dart Monkey
            {"Sharp Shots", "+1p (3)"},
            {"Razor Sharp Shots", "+2p (5)"},
            {"Spike-o-pult", "<u>Dart</u> replaced by <u>Spikeball</u> (1d, 1cd (2), 22p, 1.15s, <i>Shatter</i>). <u>Spikeball</u> can rebound off walls."},
            {"Juggernaut", "<u>Spikeball</u> replaced by <u>Juggernaut</u> (1d, 2cd (3), 100p, 36.5r, 1.0s, <i>Normal</i>). <u>Juggernaut</u> can rebound off walls."},
            {"Ultra-Juggernaut", "<u>Juggernaut</u> replaced by <u>Ultrajugg</u> (4d, 8cd (12), 200p, 1.0s, <i>Normal</i>). <u>Ultrajugg</u> can re-hit Bloons after rebounding, " +
                                 "and emits 6 <u>Juggernauts</u> at 100p remaining and when it expires."},
            
            {"Quick Shots", "85%s (0.8075)"},
            {"Very Quick Shots", "78.43%s (0.6333)"},
            {"Triple Shot", "+2j (3)"},
            {"Super Monkey Fan Club", "75%s (0.475), <b>Ability</b> (50s cooldown, 15s duration): Transforms 10 nearest Dart Monkeys (upgraded up to 242) into Superfans " +
                                      "that have a <u>Dart</u> attack (1d, 2p, 0.06s, 40r, <i>Sharp</i>), inheriting buffs to the transformed tower."},
            {"Plasma Monkey Fan Club", "<b>Ability</b> (50s cooldown, 15s duration): Transforms 20 nearest dart monkeys (upgraded up to 252) into Plasmafans " +
                                       "that have a <u>Plasma</u> attack (2d, 5p, 0.03s, 40r, <i>Plasma</i>), inheriting buffs to the transformed tower."},
            
            {"Long Range Darts", "+8r (40)"},
            {"Enhanced Eyesight", "+8r (48)\n" +
                                  "Gains Camo detection"},
            {"Crossbow", "<u>Dart</u> replaced by <u>Bolt</u> (3d, 3p, 0.95s, 56r, <i>Sharp</i>)"},
            {"Sharp Shooter", "+3d (6), 0.85s\n" +
                              "50d Crit every 8-12 shots"},
            {"Crossbow Master", "+7p (10) (16p w/ 105, 23p w/ 205), +20r (76), 0.16s, <i>Normal</i>\n" +
                                "Crit now occurs every 4-8 shots"},
            #endregion
            
            #region Boomerang Monkey
            {"Improved Rangs", "+4p (8)"},
            {"Glaives", "+5p (13), 85%s (1.207)"},
            {"Glaive Ricochet", "<u>Boomerang</u> replaced by <u>Glaive</u> (1d, 50p, 1.2s, 43r, <i>Sharp</i>)"},
            {"M.O.A.R Glaives", "+50p (100), 50%s (0.6)"},
            {"Glaive Lord", "Gains <u>Orbital Glaive</u> attack (2d, 5cd (7), 5md (7), ∞p, 0.1s, 40r zone, <i>Normal</i>, camo). <u>Glaive</u> buffed: first hit applies shred effect (10d/1.0s, 15s duration, stackable)."},
            
            {"Faster Throwing", "75%s (1.065)"},
            {"Faster Rangs", "75%s (0.8), faster projectile speed"},
            {"Bionc Boomerang", "+1md (2), 0.28s"},
            {"Turbo Charge", "<b>Ability</b> (45s cooldown): +1d and 7× faster (0.04s) for 10s."},
            {"Perma Charge", "+3d (4, 5md) (8, 9md w/ 052), 0.04s\n<b>Ability</b> (40s cooldown): +8d (12) for 15s."},
            
            {"Long Range Rangs", "+6.5r (49.5)"},
            {"Red Hot Rangs", "+1d, <i>Normal</i>"},
            {"Kylie Boomerang", "<u>Boomerang</u> replaced by <u>Kylie</u> (2d, 18p, 1.42s, 49.5r, <i>Normal</i>) that travels in a straight line and can re-hit Bloons every .3s"},
            {"MOAB Press", "Gains <u>Heavy Kylie</u> attack (1d, 4md (5), 200p (300p w/ 104, 320p w/ 204), 10.0s, <i>Normal</i>) that targets and knocks back Blimps below BAD and can re-hit every 0.1s."},
            {"MOAB Domination", "<u>Kylie</u> buffed: +10d (12), 50%s (0.71). " +
                                "<u>Heavy Kylie</u> buffed: +15md (20), 50%s (5.0s), 100r, 0.25s stun, can target BADs, creates explosion effect instead of returning: " +
                                "100d, 20p, <i>Normal</i>, 50 blast radius, applies burn status (50d/1s, 4s duration)"},
            #endregion
            
            #region Bomb Shooter
            {"Bigger Bombs", "+6p (20)"},
            {"Heavy Bombs", "+1d (2), +10p (30)"},
            {"Really Big Bombs", "+1d (3), +20p (48)"},
            {"Bloon Impact", "+3r (43), <i>Normal</i>. Applies stun status for 1s (cannot affect Blimps)."},
            {"Bloon Crush", "+9d (12)\nStun buffed: 2s duration, can affect Blimps"},
            
            {"Faster Reload", "75%s (1.125)"},
            {"Missile Launcher", "80%s (0.9), +4r (44), faster projectile speed"},
            {"MOAB Mauler", "+4cd (5), +18md (19), +5r (49)"},
            {"MOAB Assassin", "+12md (31), +5r (54). <b>Ability</b> (30s cooldown): Targets the strongest blimp: removes its top layer (up to 750d), and creates a (3d, 100p) <i>Explosion</i>."},
            {"MOAB Eliminator", "99md (100), <i>Normal</i>. <b>Ability</b> (10s cooldown): Targets the strongest blimp: removes its top layer (up to 4500d), and creates a (3d, 100p) <i>Explosion</i>."},
            
            {"Extra Range", "+7r (47)"},
            {"Frag Bombs", "+2r (49), On-hit: <u>Frags</u> (1d (2d w/ 302, 12 w/ 502), 1p (2p w/ 202, 3p w/ 502), 8j, <i>Sharp</i>)"},
            {"Cluster Bombs", "<u>Frags</u> replaced by <u>Clusters</u> (1d, 10p, 8j, <i>Explosion</i>)"},
            {"Recursive Cluster", "Every other shot does a <u>Supercluster</u>, which also makes a <u>Subcluster</u> (1d, 60p (120p w/ 104), <i>Explosion</i>)."},
            {"Bomb Blitz", "+4d (5), <i>Normal</i>. Every shot does a <u>Super Cluster</u>. <b>Passive</b> (40s cooldown): When something leaks, destroys all Bloons (including camo) and MOABs, and deals 2000 damage to all other Blimps."},
            #endregion
            
            #region Tack Shooter
            {"Faster Shooting", "75%s (1.05)"},
            {"Even Faster Shooting", "60%s (0.63)"},
            {"Hot Shots", "+1d (2), <i>Normal</i>"},
            {"Ring of Fire", "<u>Tacks</u> replaced by <u>Firezone</u> (3d (4d w/ 401, 5d w/ 402), 60p (70p w/ 410, 80p w/ 420), 0.535s, 23r zone, <i>Fire</i>)."},
            {"Inferno Ring", "+1d (4), +6md (10), 0.1s, +11.5r (34.5). Gains <u>Meteor</u> attack (700d, 1p, 4.0s, ∞r, <i>Fire</i>, Camo, Strong targeting) " +
                             "that creates <u>Explosion</u> (50d, 10p, 18 blast radius) and applies <u>Burn</u> status (50d/1.0s, 4s duration, <i>Fire</i>)."},
            
            {"Long Range Tacks", "+4r (27)"},
            {"Super Range Tacks", "+4r (31), +1p (2)"},
            {"Blade Shooter", "<u>Tacks</u> replaced by <u>Blades</u> (1d, 6p, 1.19s, 31r, <i>Sharp</i>)"},
            {"Blade Maelstrom", "<b>Ability</b> (20s cooldown): Emits blades (1d, ∞p, 0.0333s, 2j, <i>Sharp</i>) for 3s."},
            {"Super Maelstrom", "+1d (2), <i>Normal</i>. <b>Ability</b> (20s cooldown): Emits blades (2d, ∞p, 0.0333s, 4j, <i>Normal</i>) for 9s"},
            
            {"More Tacks", "+2j (10)"},
            {"Even More Tacks", "+2j (12)"},
            {"Tack Sprayer", "75%s (1.05), +4j (16)"},
            {"Overdrive", "+1p (2), 33.33%s (0.35)"},
            {"The Tack Zone", "+7p (9), 75%s (0.2625), +23r (46), +16j (32), <i>Normal</i>"},
            #endregion
            
            #region Ice Monkey
            {"Permafrost", "Applies <u>Permafrost</u> status (50% slow; 25% for Blimps, if targetable)."},
            {"Metal Freeze", "Can pop Lead (but not necessarily Frozen)."},
            {"Ice Shards", "+5r (25), Frozen Bloons emit <u>Shards</u> (1d, 3p, 3j, <i>Shatter</i>) when hit."},
            {"Embrittlement", "+1d (2), Camo, On-hit effects: De-camo, De-regrow. Applies <u>Brittle</u> status (take +1d, 2s duration)."},
            {"Super Brittle", "+3d (5), 90%s (2.16), <i>Normal</i>. <u>Brittle</u> status buffed: take +4d, 3s duration. Blimps can receive <u>Permafrost</u> and <u>Brittle</u>."},
            
            {"Enhanced Freeze", "75%s (1.8s), <u>Frozen</u> status now lasts 2.2s."},
            {"Deep Freeze", "<u>Frozen</u> status is passed down 1 layer."},
            {"Arctic Wind", "+60p (100), gains <u>Slow</u> aura (<i>Cold</i>, 50% slow). Land towers are placeable on any water in range."},
            {"Snowstorm", "+10r (30), <b>Ability</b> (60s cooldown): 1 damage to everything (non-white) on screen; " +
                          "applies <u>Frozen</u> status to remaining Bloons (including white) for 4s and to Blimps for 2s."},
            {"Absolute Zero", "+200p (300), +10r (40), <u>Slow</u> effect is now 60%, <u>Frozen</u> status is passed down 3 layers. " +
                              "<b>Ability</b> (40s cooldown): 1 damage and 10s <u>Frozen</u> status to everything; all Ice Monkeys gain 66.66%s buff for 10s."},
            
            {"Larger Radius", "+7r (27)"},
            {"Re-Freeze", "<u>Freeze</u>: <i>Glacier</i>"},
            {"Cryo Cannon", "Replaces <u>Freeze</u> attack with <u>Ice-Bomb</u> (1.0s, 46r, <i>Color</i>) that applies <u>Freeze</u> (2d, 30p, 20r, <i>Glacier</i>, <u>Frozen</u> for 1.5s)"},
            {"Icicles", "<u>Frozen</u> Bloons have <u>Icicles</u> (2d, 3p, <i>Sharp</i>)."},
            {"Icicle Impale", "75%s (0.75), +49md (50), <i>Shatter</i>. Blimps can be targeted: ZOMGSs are 50% slower, other Blimps move at ZOMGs' original speed."},
            #endregion
            
            #region Glue Gunner
            {"Glue Soak", "<u>Glued</u> status soaks through all non-Blimp layers."},
            {"Corrosive Glue", "<u>Glued</u> status buffed: 1d/2.3s and can overwrites weaker effects. <u>Glue</u> can affect Blimps, but lasts half as long and doesn't slow."},
            {"Bloon Dissolver", "<u>Glued</u> status buffed: 1d/.575s"},
            {"Bloon Liquefier", "<u>Glued</u> status buffed: 1d/.1s"},
            {"The Bloon Solver", "2j, 4p, 25%s (0.25), impact. <u>Glued</u> status buffed: 3cd/0.1s and 3md/0.1s."},
            
            {"Bigger Globs", "+1p (2)"},
            {"Glue Splatter", "+4p (6), impact"},
            {"Glue Hose", "33.33%s (0.333)"},
            {"Glue Strike", "<b>Ability</b> (30s cooldown): Applies weakening-glue (take +1d) to all Bloons, (Blimps w/ 240). Soaks 6 layers. Will refresh duration of stronger glue instead of replacing."},
            {"Glue Storm", "<b>Ability</b> (30s cooldown): Every 2s for the next 15s, applies weakening-glue to all valid targets with twice the duration and slowing power of standard attack. Soaks 9 layers."},
            
            {"Stickier Glue", "<u>Glued</u> status buffed: +13s duration (24s)."},
            {"Stronger Glue", "<u>Glued</u> status buffed: 75% slow."},
            {"MOAB Glue", "Can now target Blimps, but they are only slowed half as much as Bloons (37.5%) and for half the duration (12s)."},
            {"Relentless Glue", "<u>Glued</u> Bloons drop the glue when popped, to be picked up by the next target, lasts 5s."},
            {"Super Glue", "+50md (50), +5p (6), <u>Glued</u> status buffed: 1d/2.3s (2.0s with 205); Bloons are slowed 100%; MOABs and DDTs are slowed 100% for the first 5s; BFBs are slowed 95% for the first 2.5s; ZOMGs are slowed 90% for the first 0.75s."},
            #endregion 

            #region Sniper Monkey
            {"Full Metal Jacket", "+2d (4), <i>Normal</i>"},
            {"Large Calibre", "+3d (7)"},
            {"Deadly Precision", "+13d (20), +15cd (35)"},
            {"Maim MOAB", "+10d (30, 45cd), Stuns blimps (MOABs 3s, BFBs 1.5s, ZOMGs and DDTs 0.75s)."},
            {"Cripple MOAB", "+30d (60, 75cd), Stun duration buffed: MOAB 7s, BFB 6s, ZOMG 3s, DDT 4s and applies <u>Crippled</u> status (take +5d) (0.75s for BADs)."},
            
            {"Night Vision Goggles", "Gains Camo detection\n<u>Bullet</u> buffed: +2 Camo damage"},
            {"Shrapnel Shot", "On-hit effect: emits <u>Shrapnel</u> (1d (2d, 4d, 6d, 12d w/ 220 through 520), 3p (4p w/ 023), 5j, <u>Sharp</u>, 45° spread centred on the direction fired)"},
            {"Bouncing Bullet", "4p (no longer impact), jumps to targets within ~55r until pierce runs out."},
            {"Supply Drop", "<u>Bullet</u> buffed: <i>Normal</i>. <b>Ability</b> (60s cooldown): Drops a crate worth $500-1000."},
            {"Elite Sniper", "40%s (0.636s), buffs all other Snipers 75%s and provides Elite targeting (Near exit > Ceramics > Strong). <b>Ability</b> (60s cooldown): Drops a crate worth $1500-2500."},
            
            {"Fast Firing", "70%s (1.113s)"},
            {"Even Faster Firing", "70%s (0.779s)"},
            {"Semi-Automatic", "33.33%s (0.2597s)"},
            {"Full Auto Rifle", "50%s (0.12985s), <u>Bullet</u> buffed: <i>Normal</i>"},
            {"Elite Defender", "50%s (0.064925s), 1% faster per percent along track target is. <b>Passive</b> (10s cooldown): Shoots 4× faster for 7s after leaking."},
            #endregion

            #region Monkey Sub
            {"Longer Range", "+10r (52)"},
            {"Advanced Intel", "Can target anything in the primary attack range of any tower, including Camo detection."},
            {"Submerge and Support", "Gains Submerge targeting option, replacing <u>Dart</u> with <u>Sonar</u> (0d, ∞p, 1.75s (1.275s w/ 301, 0.956s w/ 302), de-camo)."},
            {"Bloontonium Reactor", "<u>Sonar</u> buffed: 0.3s. While submerged, gains <u>Radioactive</u> attack (1d (2ld w/ 420), 70p (84p w/ 410), 0.3s (0.255s w/ 410, 0.191s w/ 420), <i>Normal</i>). " +
                                    "Water towers in range have 85% ability cooldowns."},
            {"Energizer", "<u>Radioactive</u> buffed: 3d, 1000p/ Water towers in range have 50% ability cooldowns, all others 80%. Heroes in range get +50% XP."},
            
            {"Barbed Darts", "+3p (5)"},
            {"Heat-tipped Darts", "<u>Dart</u>: <i>Normal</i>"},
            {"Ballistic Missile", "+8r (50), gains <u>Missile</u> attack (1d, 5cd (6), 5md (6), 100p, 1.3s (1.14s w/ 031, 0.85s w/ 032), <i>Explosion</i>). Infinite range with 230 crosspath."},
            {"First Strike Capability", "<b>Ability</b> (60s cooldown): 10000 <i>Normal</i> damage to strongest Bloon, which can pierce blimp layers; additionally creates a (350d, ∞p, 75r, <i>Normal</i>) explosion."},
            {"Pre-emptive Strike", "<u>Missile</u> buffed: +6cd (11), +6md (11), 33.33%s (0.5s). <b>Ability</b> cooldown is now 45s. <b>Passive</b>: Sends an assassin (up to 750d) at any Blimp that enters the map."},
            
            {"Twin Guns", "50%s (0.375)"},
            {"Airburst Darts", "<u>Dart</u> is now an impact projectile, emits <u>Airbursts</u> on hit (1d, 2p, 3j, <i>Sharp</i>)"},
            {"Triple Guns", "66.66%s (0.25)"},
            {"Armor Piercing Darts", "<u>Dart</u> buffed: +1d (2), +2md (4).\n<u>Airbursts</u> buffed: +1md (2), +3p (5)."},
            {"Sub Commander", "50%s (0.125), <u>Command</u> buff for all subs in range including self: <u>Dart</u> +1d, <u>Dart</u> +4p, all other attacks double damage."},
            
            #endregion
            
            #region Monkey Buccaneer
            {"Buccaneer-Faster Shooting", "75%s (0.75)"},
            {"Buccaneer-Double Shot", "<u>Dart</u> buffed: +1j (2)"},
            {"Buccaneer-Destroyer", "20%s (0.15)"},
            {"Buccaneer-Aircraft Carrier", "Spawns three <u>Planes</u>: <u>Forward Darts</u> (1d, 9p, 0.15s, 2j, <i>Sharp</i>), " +
                                           "<u>Radial-Darts</u> (1d, 9p, 1.0s, 8j, <i>Sharp</i>), <u>Moab-Missile</u> (15md, 3p, 3.0s, <i>Explosion</i>)"},
            {"Buccaneer-Carrier Flagship", "<u>Dart</u> buffed: <i>Normal</i>. <u>Plane Darts</u> buffed: +1d (2), +3cd (5), +5p (14), <i>Normal</i>. " +
                                           "<u>Moab Missile</u> buffed: +15md (30), +1p (4), 50%s (1.5), <i>Normal</i>. Buffs all water towers and Aces 85%s."},
            
            {"Buccaneer-Grape Shot", "Gains <u>Grapes</u> attack (1d, 1p, 1.3s, 5j (10j w/ 210), <i>Sharp</i>)"},
            {"Buccaneer-Hot Shot", "<u>Grapes</u> buffed: <i>Fire</i>, applies a burn status (1d/1.5s, <i>Fire</i>, 3s duration)"},
            {"Buccaneer-Cannon Ship", "Gains <u>Cannonball</u> attack (1d, 1.2s, impact) that creates an explosion on hit (1d, 28p, <i>Explosion</i>) and <u>Frags</u> (1d, 8j, <i>Sharp</i>, impact)"},
            {"Buccaneer-Monkey Pirates", "<u>Cannonball</u> buffed: +1d (2), +2j (3). <b>Ability</b> (50s cooldown): Immediately removes the strongest non-ZOMG/BAD Blimp, gaining full cash."},
            {"Buccaneer-Pirate Lord", "50%s. <u>Grapes</u> buffed: +4d (5), +5cd (10). <b>Ability</b> (30s cooldown): now has three hooks and gains double cash; can use two hooks at once to take down a ZOMG."},
            
            {"Buccaneer-Long Range", "+11r (71), projectile speeds increased."},
            {"Buccaneer-Crow's Nest", "Camo"},
            {"Buccaneer-Merchantman", "$200 end of round income."},
            {"Buccaneer-Favored Trades", "50%s (0.5), +$300 end of round income ($500). Gains a buff: +10% sell price to anything in range; stackable up to 95% cap."},
            {"Buccaneer-Trade Empire", "+1d (2), +1cd (3), +1md (3), +$300 end of round income ($800). Gains a buff: +1d, +1cd, +1md, +$20n income to up to 20 xx3 or xx4 Buccaneers."},
            #endregion
            
            #region Monkey Ace
            {"Rapid Fire", "60%s (1.26)"},
            {"Lots More Darts", "<u>Radial Darts</u> buffed: +4j (12)"},
            {"Fighter Plane", "Flies 20% faster, gains <u>Moab Missile</u> attack (18md, 3p, 4.0s, 2j, <i>Explosion</i>, homing, Blimps only)"},
            {"Operation: Dart Storm", "<u>Radial Darts</u> buffed: 50%s (0.63), +4j (16). <u>Moab Missile</u> buffed: 50%s (2.0), +1p (4). All crosspath attacks twice as fast."},
            {"Sky Shredder", "<u>Radial Dart</u> buffed: +2d (3), +2cd (5), +3p (8), 50%s (0.315), +16j (32), <i>Normal</i>. <u>Moab Missile</u> buffed: 150md, +1p (5), <i>Normal</i>."},
            
            {"Exploding Pineapple", "Gains <u>Pineapple</u> attack (1d, 20p (32p w/ 011), 3s, <i>Explosion</i>)"},
            {"Spy Plane", "Camo"},
            {"Bomber Ace", "<u>Pineapple</u> replaced by <u>Bombing Run</u> (3d, 20p (32p w/ 031), 1.7s, 4j, <i>Explosion</i>) that's dropped on path"},
            {"Ground Zero", "<u>Bombing Run</u> buffed: +7d (10), +20p (40). <b>Ability</b> (45s cooldown): 700d to everything."},
            {"Tsar Bomba", "<u>Bombing Run</u> buffed: normal type. <b>Ability</b> (40s cooldown): 3000d to everything, with an 8.2s stun to anything that survives."},
            
            {"Sharper Darts", "<u>Radial Darts</u> buffed: +3p (8)"},
            {"Centered Path", "Gains 'Centred Path' targeting (fly on a circular path in the center of the screen with radius 90)."},
            {"Neva-Miss Targeting", "<u>Radial Darts</u> buffed: faster projectile speed and homing."},
            {"Spectre", "<u>Radial Darts</u> replaced by <u>Barrage</u> (0.06s, ∞r) which alternately fires <u>Darts</u> (2d, 30p (37 w/ 204), <i>Sharp</i>) and <u>Bombs</u> (3d, 60p, <i>Explosion</i>). "},
            {"Flying Fortress", "<u>Barrage</u> buffed: +2d, +2j, 66.66%s (0.04), <i>Normal</i>. The 3 projectiles have separate targeting (first, last, and close)."},
            #endregion
            
            #region Heli Pilot
            {"Quad Darts", "+2j (4)"},
            {"Pursuit", "Gains 'Pursuit' targeting (always flies towards a point slightly ahead of the first Bloon)."},
            {"Razor Rotors", "Gains <u>Rotor</u> attack (1d, 10p, 35r zone, 0.75s, <i>Normal</i>)."},
            {"Apache Dartship", "Gains <u>Machine Gun</u> attack (1d, 5p, 0.05s, <i>Sharp</i>) and <u>Rocket</u> attack (2d, 40p, 1.0s, ∞r, 4j, <i>Explosion</i>)."},
            {"Apache Prime", "<u>Dart</u> buffed: +5d (6), +20p (23), <i>Normal</i>. <u>Machine Gun</u> buffed: +4d (5), +6p (11), <i>Plasma</i>. <u>Rocket</u> buffed: +15md (17)."},
            
            {"Bigger Jets", "Flies faster."},
            {"IFR", "Camo."},
            {"Downdraft", "Gains <u>Downdraft</u> attack (0d, 0.15s (.12s w/ 032)1p, ) that sends Bloon 50-300 units back."},
            {"Support Chinook", "<b>Ability</b> (60s cooldown): Move a tower, except for: Aces, Helis, Farms, Villages, Aircraft Carriers, and Temples.) <b>Ability</b> (60s cooldown): Alternates between dropping $1000-$2000 and 50-75 lives."},
            {"Special Poperations", "<b>Ability</b> (60s cooldown): Alternates between dropping $2000-$4000 and 100-150 lives. <b>Ability</b> (40s cooldown): deploy a Marine (30s lifetime) with a <u>Bullet</u> attack (6d, 30p, 0.05s, 50r, <i>Normal</i>)."},
            
            {"Faster Darts", "Darts travel faster."},
            {"Faster Firing", "<u>Darts</u> buffed: 80%s."},
            {"MOAB Shove", "Gains <u>Shove</u> attack (0d, 1p) that pushes back MOABs and BFBs, or slows DDTs and ZOMGs."},
            {"Comanche Defense", "<u>Passive</u>: Summons 1, 2, 3 Mini-Comanches for 12s when a Bloon crosses 25%, 50%, 75% of the track" +
                                 "with <u>Darts</u> attack (2d, 3p, 0.3s, 40r, 3j, <i>Sharp</i>) and <u>Rocket</u> attack (1d, 2cd (3), 2md (3), 100p, 3.0s, <i>Explosion</i>)."},
            {"Comanche Commander", "+1d (including mini comanche attacks). Comanches are permanent."},
            #endregion
            
            #region Mortar Monkey
            {"Bigger Blast", "+5p (45)"},
            {"Bloon Buster", "+1d (2)"},
            {"Shockwave", "<u>Shell</u> also creates: <u>Shockwave</u> (45p, <i>Explosion</i>) that stuns in small radius for 0.5s, then deals 1d to anything in a larger radius that was not stunned."},
            {"The Big One", "+40p (85), <i>Normal</i>. <u>Explosion</u> buffed: +3d (5)."},
            {"The Biggest One", "+115p (200). <u>Explosion</u> buffed: +15d (20), +20cd (40), +20md (40), ~80 blast radius. <u>Shockwave</u> buffed: +10cd (11) outside stun, ~120 blast radius."},
            
            {"Mortar Faster Reload", "75%s (1.5)"},
            {"Mortar Rapid Reload", "72%s (1.08)"},
            {"Heavy Shells", "+3cd (4), +1md (2), +1fd, +1 lead damage, +2 stunned damage, <i>Normal</i>."},
            {"Artillery Battery", "25%s (0.27)"},
            {"Pop and Awe", "+8 stunned damage (+10), +9cd (12), +3ld (4), +3fd (4), +3md (4). <b>Ability</b> (60s cooldown): Every second for the next 8 seconds, hits everything on screen for 20d and a 1s stun."},
            
            {"Increased Accuracy", "Less variance in shell target."},
            {"Burny Stuff", "On-damage effect: apply <u>Burn</u> status (1d/1.5s (2d, 3d, 25d for 302 through 502), <i>Fire</i>, 3s duration)."},
            {"Signal Flare", "Camo. On-damage effect: De-camo (black, zebra, and DDT not affected)."},
            {"Shattering Shells", "On-hit effect: De-regrow, De-fortify, De-camo, but still not DDTs. "},
            {"Blooncineration", "<i>Normal</i>, <u>Burn</u> status buffed: 5d/1s, 100md/1s. De-camo and De-fortify now affect DDTs. " +
                                "<u>Shell</u> gains a <u>Firewall</u> effect (1d, 20p, 0.1s, <i>Normal</i>, Camo)."},
            #endregion
            
            #region Dartling Gun
            {"Focused Firing", "<u>Dart</u> buffed: -14° spread (9°)"},
            {"Laser Shock", "On-hit effect: <u>Laser Shock</u> (1d/1s, takes +1d from other Laser Shock Dartling Guns)."},
            {"Laser Cannon", "<u>Dart</u> attack replaced with <u>Laser</u> (2d, 4p, .2s, ∞r, <i>Energy</i>). <u>Laser Shock</u> now lasts 2s."},
            {"Plasma Accelerator", "<u>Laser</u> replaced with <u>Beam</u> (1d, 50p (75 w/ 402), .25s, ∞r, <i>Plasma</i>) " +
                                   "that at the tip does (2d +10md, 50p, .25s, 4r) applying <u>Laser Shock</u> for 5s."},
            {"Ray of Doom", "No tip effect, but <u>Beam</u> buffed +3d (5), +13md (25), +950p (1000). <u>Laser Shock</u> buffed: 15d/.1s, 5s duration"},
            
            {"Advanced Targeting", "Camo."},
            {"Faster Barrel Spin", "0.66%s (.132)"},
            {"Hydra Rocket Pods", "<u>Dart</u> attack replaced with <u>Rocket</u> (0d, 6p (2 used up per Bloon), .132s, ∞r) that create <u>Blasts</u> on each hit (1d, 6p, 8r, <i>Normal</i>)."},
            {"Rocket Storm", "<u>Rockets</u> and <u>Blasts</u> buffed: +2p. <b>Ability</b> (30s): Produces 19 waves of 9 <u>Missiles</u> (6d, 8p, ∞r, <i>Normal</i>, 120°) that briefly stun Bloons."},
            {"M.A.D", "<u>Rockets</u> now have (3d +750md, 8p, .4s). <b>Ability</b> <u>Missiles</u> are larger and have +8p (16)."},
            
            {"Faster Swivel", "2x faster turn speed."},
            {"Powerful Darts", "<u>Dart</u> attack buffed: +2p (3), and increased projectile speed."},
            {"Buckshot", "<u>Dart</u> attack replaced with <u>Buckshot</u> (4d, 4p, 1.8s (1.2s w/ 023), 6j, 130r)."},
            {"Bloon Area Denial System", "<u>Buckshot</u> attack buffed: 25%s (0.45s) (0.3s w/ 024). Gains 'Independent Targeting' option for barrels to shoot First, Last, Close Strong."},
            {"Bloon Exclusion Zone", "<u>Buckshot</u> attack buffed: 66%s (0.3s) (0.2s w/ 025), +6j (12), +4d (8). Top two barrels completely ignore Line of Sight."},
            #endregion
            
            #region Wizard Monkey
            {"Guided Magic", "<u>Bolt</u> buffed: homing, ignores walls. Wall of Fire placed according to target priority."},
            {"Arcane Blast", "<u>Bolt</u> buffed: +1d (2)."},
            {"Arcane Mastery", "+20r (60), <u>Bolt</u> buffed: +1d (3), +4p (6), 50%s (0.55)."},
            {"Arcane Spike", "+2d (5), +11md (16), 50%s (0.275), <u>Plasma</u>"},
            {"Archmage", "<u>Bolt</u> buffed: +2d (7), +6md (24), +4p (10), 50%s (0.1375). Gains Dragon's Breath and Shimmer attacks."},
            
            {"Fireball", "Gains <u>Fireball</u> attack (1d, 3.0s, <i>Fire</i>) that creates an explosion (1d, 15p, <i>Explosion</i>) on damage."},
            {"Wall of Fire", "Ever 5.5s creates <u>Wall of Fire</u> effect (1d, 15p (20p w/ 021), 20r, 0.1s, <i>Fire</i>, 4.5s lifetime)."},
            {"Dragon's Breath", "Gains <u>Flame</u> attack (1d, 1cd (2), 4p (6p w/ 031), 0.1s, 50r, <i>Fire</i>) that applies <u>Burn</u> status (1d/1.5s, <i>Fire</i>, 3s duration). " +
                                "Wall of Fire now happens every 4.5s."},
            {"Summon Phoenix", "<b>Ability</b> (60s cooldown, 20s duration): Summons a <u>Phoenix</u> with <u>Flame</u> attack (4d, 6p, ∞r, 0.1s, <i>Fire</i>, Camo)."},
            {"Wizard Lord Phoenix", "<u>Flame</u> buffed: +46p (50). <u>Phoenix</u> is permanent. <b>Ability</b> (50s cooldown, 20s duration): Transforms into <u>Phoenix Lord</u> " +
                                    "with <u>Flame</u> attack (20d, 50p, ∞r, 0.1s, <i>Normal</i>, Camo) and <u>Meteor</u> attack (50d, 500p, ∞r, 1.0s, 8j, Normal, Camo)."},
            
            {"Intense Magic", "<u>Bolt</u> buffed: +5p (7), faster projectile speed."},
            {"Monkey Sense", "Camo."},
            {"Shimmer", "Gains Shimmer attack (0d, 200p, 2.5s, 70r, De-camo)."},
            {"Necromancer: Unpopped Army", "Stores up to 500 pops within 70r in <u>Graveyard</u> for 2 rounds (pops worth 7-13 after round 80). " +
                                           "Gains <u>Reanimate</u> attack (1.5s (-10% per 100 current <u>Graveyard</u>), spawns 1-4 <u>Zombloons</u>, costs 1-10 <u>Graveyard</u> pops.). " +
                                           "<u>Zombloon</u>: 2d (+1 per 200 current <u>Graveyard</u>), 1p (+1 per pop used), <i>Normal</i>, 10s."},
            {"Soulbind", "+40r (80). <u>Bolt</u> buffed: 25%s (0.275). <u>Shimmer</u> buffed: 50%s (1.25). <u>Graveyard</u> capacity: 3000. " +
                                   "Gains <u>Reanimate Blimp</u> attack (3s; 20 pops for <u>ZMoab</u> (40d (+1 per 200 current <u>Graveyard</u>), 20p, <i>Normal</i>, 20s), " +
                                   "or 50 pops for <u>ZBfb</u> (100d, 50p, <i>Normal</i>, 13.3s.) if Graveyard > 2000). ALL zombies have +1d and +50% lifetime."},
            #endregion
            
            #region Super Monkey
            {"Laser Blasts", "+1p (2), <i>Energy</i>."},
            {"Plasma Blasts", "+1p (3), 50%s, <i>Plasma</i>."},
            {"Sun Avatar", "+3p (6), +2j (3)."},
            {"Sun Temple", "<u>Sun Beams</u> replaced by <u>Sunblast</u> (5d, 20p, 0.06s, 65r, <i>Normal</i>) (Look online for Sacrifice stuff lol)."},
            {"True Sun God", "+10d (15) (Look online for Sacrifice stuff lol)."},
            
            {"Super Range", "+10r (60)"},
            {"Epic Range", "+1p (2), +12r (72), faster projectile speed."},
            {"Robo Monkey", "+4p (6) and gains a second attack that's a copy of the first, with independent choice of targeting priority."},
            {"Tech Terror", "Attack replaced by <u>Plasma</u> (1d, 8p, 0.048s, 72r, <i>Plasma</i>). <b>Ability</b> (45s cooldown): 1000d, ∞p, 60r."},
            {"The Anti-Bloon", "+4d (5), +5p (13), +10r (82), <i>Normal</i>. <b>Ability</b> (45s cooldown): 3500d, ∞p, 100r."},
            
            {"Knockback", "Applies <u>Knockback</u> status (~0.5s duration, 125% slow for regular Bloons, 60% for Leads/Ceramics, and 30% for Blimps)."},
            {"Ultravision", "+3r (53), Camo."},
            {"Dark Knight", "<u>Dart</u> replaced by <u>Monkeyrang</u> (1d, 2md (3), 5p, 0.06s, 53r, <i>Sharp</i>). <u>Knockback</u> buffed: 100% slow to Leads/Ceramics. " +
                            "<b>Ability</b> (20s cooldown): Teleport to a chosen point within range."},
            {"Dark Champion", "+1d (2), +1md (5), +2p (7), 50%s (0.03), <i>Normal</i>. <b>Ability</b> buffed: Can teleport anywhere."},
            {"Legend of the Night", "+3d (5, 8md), +15p (22), +4r (57). <b>Passive</b> (120s cooldown): Before something leaks, delete it, and anything else that would leak in the next 8s."},
            #endregion
            
            #region Ninja Monkey
            {"Ninja Discipline", "+7r (47), 62%s (0.433)"},
            {"Sharp Shurikens", "<u>Shuriken</u> buffed: +2p (4)."},
            {"Double Shot", "<u>Shuriken</u> buffed: +1j (2)."},
            {"Bloonjitsu", "<u>Shuriken</u> buffed: +3j (5)."},
            {"Grandmaster Ninja", "+10r (57). <u>Shuriken</u> buffed: +1d (2), 0.217s, +3j (8)."},
            
            {"Distraction", "<u>Shuriken</u> gains on-damage effect: Bloons have a 15% chance to be sent back 10-300 units."},
            {"Counter-Espionage", "All attacks gain De-camo on-damage effect."},
            {"Shinobi Tactics", "92%s and +8%p (multiplicative) to all Ninjas in range, stacking up to 20 times. "},
            {"Bloon Sabotage", "<b>Ability</b> (60s cooldown, 15s duration): All Bloons and Blimps move at half speed, including new spawns, but not children of Blimps."},
            {"Grand Saboteur", "<b>Ability</b> (60s cooldown, 30s duration): All Bloons and blimps move at half speed, including new spawns, but not children of blimps; deals 25%d to new Blimps entering the map."},
            
            {"Seeking Shuriken", "<u>Shurikens</u> can seek targets."},
            {"Caltrops", "Every 4.4s, place a <u>Caltrop</u>: 1d, 6p, <i>Sharp</i>."},
            {"Flash Bomb", "Every 4th Shuriken is replaced by <u>Flash-Bomb</u> attack (1d, 60p, <i>Normal</i>, 1s stun to Bloons)."},
            {"Sticky Bomb", "Gains <u>Sticky Bomb</u> attack (5.0s, 60r) that targets the strongest Blimp and applies <u>Bombed</u> status, dealing 500d on expiration."},
            {"Master Bomber", "<u>Flash Bomb</u> buffed: +4d (5), stun can now affect Moabs for 0.25s. <u>Sticky Bomb</u> buffed: ∞r, 40%s (2.0), also stuns for 1s. " +
                              "<u>Bombed</u> status buffed: 1000d."},
            #endregion
            
            #region Alchemist
            {"Larger Potions", "<u>Potion</u> buffed: +5p (20), larger blast radius."},
            {"Acidic Mixture Dip", "Every 10s, applies <u>Acidified</u> (+1cd, +1md, lead, lasts 10 shots) to random non-Alchemists in range, prioritizing those not already buffed."},
            {"Berserker Brew", "Every 8s applies <u>Berserk</u> (+1d, +2p, 90%s, +10%r, lasts 5.0s (6.0s w/ 320) or 25 (35 w/ 320) shots, cannot be reapplied for 5.0s (4.0s w/ 320)) " +
                               "to closest non-Alchemist tower in range that's buffable."},
            {"Stronger Stimulant", "<u>Berserk</u> buffed: +1p (+3p), now 85%s, +5%r (+15%r), lasts +6.0s (12.0s) or +15 (40) shots."},
            {"Permanent Brew", "<u>Acidified</u> and <u>Berserk</u> buffs are permanent (unless this Alchemist is sold)."},
            
            {"Stronger Acid", "<u>Acid</u> status buffed: 1d/1.5s, 4.5s duration."},
            {"Perishing Potions", "<u>Potion</u> buffed: +4md (5), removes fortified from Bloons, or does an extra 15d to fortified Blimps (20 total)."},
            {"Unstable Concoction", "Gains <u>Unstable Potion</u> attack (3p, 6.0s, 67.5r, Blimps only) that applies <u>Unstable</u> (on pop, 1%d, 10%md, 50p (Blimps use 2p), <i>Explosion</i>)."},
            {"Transforming Tonic", "<b>Ability</b> (60s cooldown, 20s duration): Transforms into a <u>Monster</u> with <u>Beam</u> attack (2d, 6p, 72r, 0.03s, <i>Plasma</i>)."},
            {"Total Transformation", "Ability (40s cooldown, 20s duration): Transforms self and 5 nearby towers (tier 3 or lower) into <u>Monsters</u>. Other <u>Monsters</u> have +4p (10)."},
            
            {"Alchemist Faster Throwing", "80%s (1.6)"},
            {"Acid Pool", "If no target for <u>Potion</u>, instead creates <u>Acid Pool</u> (7s lifetime, 1d, 5p) that applies <u>Acid</u> status."},
            {"Lead to Gold", "<u>Potion</u> buffed: applies <b>Golden Lead</b> status (gives $50 when the Lead layer is popped)."},
            {"Rubber to Gold", "Gains <u>Gold Potion</u> attack (15p, 5.0s) that applies <u>Golden</u> status (+2 cash modifier), which soaks through Bloons but not Blimps, and doesn't affect BADs."},
            {"Bloon Master Alchemist", "Gains <u>Red Potion</u> attack (200p (MOABs use 20p, BFBs/DDTs 50p, ZOMGs 100p), 10.0s, ∞r) that transforms target into a Red Bloon."},
            #endregion
            
            #region Druid
            {"Hard Thorns", "<u>Thorn</u> buffed: +1p (2), <i>Shatter</i>"},
            {"Heart of Thunder", "Gains <u>Lightning</u> attack (1d, 31p, 2.3s, <i>Plasma</i>)."},
            {"Druid of the Storm", "Gains <u>Tornado</u> attack (0d, 30p, 2.5s) that pushes back Bloons ~30-300 units."},
            {"Ball Lightning", "Gains <b>Ball Lightning</b> attack (2d, 30p, 6.0s creation, 0.35s damaging, <i>Plasma</i>)."},
            {"Superstorm", "Camo. <u>Lightning</u> buffed: +2d (3). <u>Ball Lightning</u> buffed: +3d (5). " +
                           "Gains <u>Superstorm</u> attack (12d, 200p (MOABs use 20p, BFBs and DDTs 50p, ZOMGs 200p), 4.0s) that pushes back ~30-300 units and spawns <u>Ball Lightnings</u>."},
            
            {"Thorn Swarm", "<u>Thorns</u> buffed: +3j (8)."},
            {"Heart of Oak", "All attacks gain on-hit effect: De-regrow."},
            {"Druid of the Jungle", "Gains <u>Vine</u> attack (1.4s) that targets strongest Bloon and destroys it."},
            {"Jungle's Bounty", "+20r (55). <u>Ability</u> (40s cooldown): Gains $200, plus $100 for every Banana Farm in range."},
            {"Spirit of the Forest", "+$1000 end of round income. <u>Vine</u> buffed: 0.3s. " +
                                     "Gains <u>Brambles</u> attack (2d, 8cd (10), 8md (10), ∞p, 0.5s, ∞r, <i>Sharp</i> type, Camo, cannot be buffed). " +
                                     "<b>Ability</b> buffed: +25 lives."},
            
            {"Druidic Reach", "+10r (45)."},
            {"Heart of Vengeance", "+x% speed, up to +100%, where x is 10 + lives below amount when upgrade was purchased, ignoring lives above the starting amount."},
            {"Druid of Wrath", "+y% speed (multiplicative), up to +100%, where y is half the damage dealt by this druid since it was last idle for more than 2s."},
            {"Poplust", "Gains a buff: +15% speed and pierce to other Druids in range; can stack (additively) up to 5 times."},
            {"Avatar of Wrath", "+3d (4), 50%s (0.55), +5r (50), +1d for every 3000 rbe on screen, up to +30."},
            #endregion
            
            #region Banana Farm
            {"Increased Production", "<u>Bananas</u> buffed: +2j (6), +$40 income ($120)."},
            {"Greater Production", "<u>Bananas</u> buffed: +2j (8), +$40 income ($160)."},
            {"Banana Plantation", "<u>Bananas</u> buffed: +8j (16), +$160 income ($320)."},
            {"Banana Research Facility", "<u>Bananas</u> become <u>Banana Crates</u>: 5j, $1500 income."},
            {"Banana Central", "+$4500 income ($6000). Gains a buff: +25% income to all 4xx farms (stacks multiplicatively with x2x)."},
            
            {"Long Life Bananas", "<u>Bananas</u> buffed: +15s lifetime (30)."},
            {"Valuable Bananas", "+25% income."},
            {"Monkey Bank", "+$150 income ($230), held in the bank instead bananas. Balance increases by 15% at end of round, up to a capacity of $7000. 031 shows glowing $ sign, 032 autocollects."},
            {"IMF loan", "+$3000 capacity ($10000). <b>Ability</b> (90s cooldown): Loan $10k, to be repaid by taking 50% from any future income."},
            {"Monkey-Nomics", "Ability (60s cooldown): Gain $10k."},
            
            {"EZ Collect", "<u>Bananas</u> can be collected from further away (about 50% more)."},
            {"Banana Salvage", "Expired <u>Bananas</u> give half their usual amount."},
            {"Marketplace", "+12j (16), +$240 income ($320). <u>Bananas</u> are automatically collected immediately."},
            {"Central Market", "+$800 income ($1120). Gains a buff: +10% end of round income to all xx3+ Buccaneers, stacks additively up to 10 times (+100%), but not with Trade Empire."},
            {"Monkey Wall Street", "$4000 end of round income."},
            #endregion
            
            #region Spike Factory
            {"Bigger Stacks", "+5p (10)"},
            {"White Hot Spikes", "Spikes become <i>Normal</i>."},
            {"Spiked Balls", "+1d (2), +3cd (5), +1fd, +7p (17)."},
            {"Spiked Mines", "+3cd (8). Creates an <u>Explosion</u> (10d, 2cd (12), 1fd, 40p, <i>Explosion</i>) that applies <u>Burn</u> status (1d/2s, <i>Fire</i>, 6s duration) when spikes expire."},
            {"Super Mines", "Speed becomes 4.4s. <u>Explosion</u> buffed: +990d (1000), +20p (60), <i>Normal</i>. " +
                            "Each Spike of each mine makes a smaller <u>Explosion</u> (10d, 10p, 20r, <i>Explosion</i>)."},
            
            {"Faster Production", "60%s (1.32)"},
            {"Even Faster Production", "75%s (0.99)"},
            {"MOAB SHREDR", "+4md (5)"},
            {"Spike Storm", "<b>Ability</b> (40s cooldown): For 1s, places <u>Spikes</u> (1d, 4md (5), 5p, 10s (15s w/ 041) lifetime) every .005s randomly on track."},
            {"Carpet of Spikes", "Default and <b>Ability</b> spikes gain +2d (3, 7md). <b>Passive</b> (15s cooldown): Same as activated ability."},
            
            {"Long Reach", "+8r (42), 100s lifespan."},
            {"Directed Spikes", "Gains extra targeting options: close, far, and smart (earliest point Bloons haven't reached yet). <b>Passive</b> (1 round cooldown): 4× faster (0.55s) for 2.5s."},
            {"Long Life Spikes", "140s or end of 2 rounds lifespan."},
            {"Deadly Spikes", "+1d (2), 3 round lifespan."},
            {"Perma-Spike", "+8d (10), +45p (50) (70p w/ 105), 6.0s, 300s lifespan."},
            #endregion
            
            #region Monkey Village
            {"Bigger Radius", "+8r (48)"},
            {"Jungle Drums", "<u>Buff</u> improved: now also gives 85%s."},
            {"Primary Training", "Gains <u>Primary Buff</u> (targets: primary towers; grants: +1p, +10%r, increased projectile speed)."},
            {"Primary Mentoring", "<u>Primary Buff</u> improved: free tier 1 upgrades, +5r, 90% ability cooldowns."},
            {"Primary Expertise", "<u>Primary Buff</u> improved: +2p (so +3 overall), free tier 2 upgrades. " +
                                  "Gains <u>Catapult</u> attack (10d, 190cd (200), 190md (200), 100p but Blimps use 10p, ∞r, 3.0s, <i>Normal</i>, Camo)."},
            
            {"Grow Blocker", "Applies Anti-regrow status to Bloons in range."},
            {"Radar Scanner", "Buff improved: Grants Camo."},
            {"Monkey Intelligence Bureau", "Buff improved: grants <i>Normal</i> damage."},
            {"Call to Arms", "<b>Ability</b> (45s cooldown): Provides another buff for 10s (+50%p, 66.67%s)."},
            {"Homeland Defense", "<b>Ability</b> (60s cooldown): Applies over an infinite range for 20s, and improved to (+100%p, 50%s)."},
            
            {"Monkey Business", "<u>Buff</u> improved: +10% discount to base towers and upgrades up to tier 3."},
            {"Monkey Commerce", "<i>Buff</i> improved: +5% discount to base towers and upgrades up to tier 3, stackable (additively) up to 3 times total."},
            {"Monkey Town", "<u>Buff</u> improved: +50% cash modifier (additive with other cash modifiers)."},
            {"Monkey City", "+10r (50). Provides a free Dart Monkey each round. Gains <u>Support Buff</u> (targets: income towers; grants: +10% income)."},
            {"Monkeyopolis", "Actual cost is $5000 * number of non-tier-5 Farms in range. Sacrifices those Farms to gain income of $300 * floor(sacrifice $ / 2000)."},
            #endregion
            
            #region Engineer Monkey
            {"Sentry Gun", "Every 10s, place a <u>Sentry</u> that lasts 25s and has <u>Sentry Nail</u> attack (1d, 2p (3p w/ 101), 0.95s, 45r (49r w/ 110), <i>Sharp</i> (<i>Shatter</i> w/ 101))."},
            {"Faster Engineering", "Sentries built every 6s."},
            {"Sprockets", "<u>Nail</u> and <u>Sentry Nail</u> buffed: 60%s (0.42 and 0.57)."},
            {"Sentry Expert", "<u>Crushing Sentry</u> has <u>Spiked-ball</u> attack (1d, 1cd (2), 22p, 50r, 1.0s, <i>Shatter</i>). " +
                              "<u>Boom Sentry</u> has a <u>Bomb</u> attack (2d, 30p, 50r, 1.3s, 18 blast radius, <i>Explosion</i>). " +
                              "<u>Cold Sentry</u> has a <u>Ice-ball</u> attack (1d, 15p, 50r, 1.5s, <i>Cold</i>) that applies Frozen/Permafrost status for 1.5s. " +
                              "<u>Energy Sentry</u> has a <u>Laser</u> attack (2d, 4p, 50r, 0.57s, <i>Energy</i>)."},
            {"Sentry Paragon", "Places <u>Paragon Sentries</u> with <u>Plasma</u> attack (2d, 5p, 50r, 0.06s, <i>Plasma</i>) that self-destruct for (100d, 50p, <i>Plasma</i>)."},
            
            {"Larger Service Area", "+20r (60)"},
            {"Deconstruction", "<u>Nail</u> buffed: +1md (2), +1fd."},
            {"Cleansing Foam", "Every 2s (1.2s w/ 230), places <u>Foam</u> (0d, 1 ld, 10p (15p w/ 230), <i>Normal</i>, 8.5s lifetime, De-camo, De-grow)."},
            {"Overclock", "<b>Ability</b> (45s cooldown): Chosen tower attacks 1.667× faster for the next (105 - 15 tier)s. " +
                          "Heroes have 'tier' (level - 1)/4. Overclocked farms give more income; Villages have +25%r."},
            {"Ultraboost", "<b>Ability</b> (45s cooldown): When Overclock is applied, the tower gains an additional permanent 4% reload (2.5%r for Villages) buff, which stacks additively up to 10 times."},
            
            {"Oversize Nails", "<u>Nail</u> buffed: +5p (8), <i>Shatter</i>."},
            {"Pin", "<u>Nail</u> buffed: Applies <u>Pinned</u> status (0.95s duration, 100% slow, Ceramic and higher are immune)."},
            {"Double Gun", "50%s (0.35)."},
            {"Bloon Trap", "Places <u>Traps</u> (500p, +1 cash modifier, 100r collection for extra money). Places new <u>Traps</u> 2.9s after collection of previous, 20s (16s with 204) cooldown."},
            {"XXXL Trap", "<u>Trap</u> buffed: 10000p, can trap blimps below BAD, and cools-down at 1/6th rate."},
            #endregion
            
        };
    }

     
    
}