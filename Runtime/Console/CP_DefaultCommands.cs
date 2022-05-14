using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CobaPlatinum.TextUtilities;

namespace CobaPlatinum.DebugTools.Console.DefaultCommands
{
    public class CP_DefaultCommands : MonoBehaviour
    {
        [PC_Command("Time-Scale")]
        [PC_CommandDescription("Set the current time scale within the game.")]
        [PC_CommandQuickAction("Print Time-Scale")]
        public void GetTimeScale()
        {
            CP_DebugWindow.ConsoleLog(TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }

        [PC_Command("Time-Scale")]
        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            CP_DebugWindow.ConsoleLog("Set time scale to: " + TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }
    }
}
