using System;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.Popups;
using MelonLoader;
using Assets.Scripts.Utils;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using UnityEngine;

[assembly: MelonInfo(typeof(FasterForward.Main), "Faster Forward", "1.0.2", "doombubbles")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace FasterForward
{
    public class Main : BloonsTD6Mod
    {
        public override string MelonInfoCsURL =>
            "https://raw.githubusercontent.com/doombubbles/BTD6-Mods/main/FasterForward/Main.cs";

        public override string LatestURL =>
            "https://github.com/doombubbles/BTD6-Mods/blob/main/FasterForward/FasterForward.dll?raw=true";

        public static int speed = 3;

        public override void OnUpdate()
        {
            int lastSpeed = speed;
            if (Input.GetKeyDown(KeyCode.F1))
            {
                speed = 3;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                speed = 5;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                speed = 10;
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                speed = 25;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                PopupScreen.instance.ShowSetValuePopup("Custom Fast Forward Speed",
                    "Sets the Fast Forward speed to the specified value",
                    new Action<int>(i =>
                    {
                        speed = i;
                        Game.instance.ShowMessage(
                            "Fast Forward Speed is now " + speed + "x" + (speed == 3 ? " (Default)" : ""), 1f);
                    }), speed);
            }

            if (speed != lastSpeed)
            {
                Game.instance.ShowMessage("Fast Forward Speed is now " + speed + "x" + (speed == 3 ? " (Default)" : ""),
                    1f);
            }


            if (TimeManager.FastForwardActive)
            {
                TimeManager.timeScaleWithoutNetwork = speed;
                TimeManager.networkScale = speed;
            }
            else
            {
                TimeManager.timeScaleWithoutNetwork = 1;
                TimeManager.networkScale = 1;
            }

            TimeManager.maxSimulationStepsPerUpdate = speed;
        }
    }
}