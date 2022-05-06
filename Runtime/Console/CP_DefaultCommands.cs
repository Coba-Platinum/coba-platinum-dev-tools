using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CobaPlatinum.TextUtilities;

namespace CobaPlatinum.DebugTools.Console.DefaultCommands
{
    public class CP_DefaultCommands : MonoBehaviour
    {
        [PlatinumCommand("Time-Scale")]
        [PlatinumCommandDescription("Set the current time scale within the game.")]
        public void GetTimeScale()
        {
            CP_DebugWindow.ConsoleLog(TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }

        [PlatinumCommand("Time-Scale")]
        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            CP_DebugWindow.ConsoleLog("Set time scale to: " + TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }
    }
}
