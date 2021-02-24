namespace MegaKnowledge
{
    public class MegaKnowledge
    {
        public string name;
        public string description;
        public string set;
        public string cloneFrom;
        public string URL;
        public int offsetX;
        public int offsetY;
        public int required;
        public string GUID;
        public bool enabled = false;
        public string tower;
        public bool targetChanging;

        public MegaKnowledge(string name, string description, string set, string cloneFrom, int offsetX, int offsetY,
            int required, string tower, string url, bool targetChanging = false)
        {
            this.name = name;
            this.description = description;
            this.set = set;
            this.cloneFrom = cloneFrom;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.required = required;
            URL = url;
            this.targetChanging = targetChanging;
            this.tower = tower;
        }


        public static string SplodeyDarts = "SplodeyDarts";
        public static string DoubleRanga = "DoubleRanga";
        public static string BombVoyage = "BombVoyage";
        public static string TackAttack = "TackAttack";
        public static string IceFortress = "IceFortress";
        public static string GorillaGlue = "GorillaGlue";
        
        public static string RifleRange = "RifleRange";
        public static string AttackAndSupport = "AttackAndSupport";
        public static string Dreadnought = "Dreadnought";
        public static string AceHardware = "AceHardware";
        public static string AllPowerToThrusters = "AllPowerToThrusters";
        public static string MortarEmpowerment = "MortarEmpowerment";
        public static string DartlingEmpowerment = "DartlingEmpowerment";
        
        public static string CrystalBall = "CrystalBall";
        public static string XrayVision = "XrayVision";
        public static string ShadowDouble = "ShadowDouble";
        public static string Oktoberfest = "Oktoberfest";
        public static string BloonAreNotPrepared = "BloonAreNotPrepared";
        
        public static string RealHealthyBananas = "RealHealthyBananas";
        public static string SpikeEmpowerment = "SpikeEmpowerment";
        public static string DigitalAmplification = "DigitalAmplification";
        public static string Overtime = "Overtime";
    }
}