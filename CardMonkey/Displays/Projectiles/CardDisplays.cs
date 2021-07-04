using Assets.Scripts.Simulation.SMath;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;

namespace CardMonkey.Displays.Projectiles
{
    // All the Card Projectile displays are so similar, I just kept them in one .cs file
    // I would've used the multiple instance loading like in CardMonkeyMultiDisplay,
    // but I wanted to be able to directly reference the different classes themselves

    public class RedCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class BlueCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class GoldCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class RedWildCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class GoldWildCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class BlueWildCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class WildAceCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class AceInTheHoleAbilityDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override float PixelsPerUnit => 5f;
        
        public override Vector3 PositionOffset => new Vector3(0, 5f, 0);

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, nameof(WildAceCardDisplay));
        }
    }
    
    public class AceOfSpadesCardDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, Name);
        }
    }
    
    public class AceOfSpadesAbilityDisplay : ModDisplay
    {
        public override string BaseDisplay => Generic2dDisplay;

        public override float PixelsPerUnit => 5f;

        public override Vector3 PositionOffset => new Vector3(0, 5f, 0);

        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            Set2DTexture(node, nameof(AceOfSpadesCardDisplay));
        }
    }
}