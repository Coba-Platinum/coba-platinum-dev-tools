using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CobaPlatinum.TextUtilities;
using System;
using System.IO;

namespace CobaPlatinum.DebugTools.PlatinumConsole.DefaultCommands
{
    public class PlatinumConsole_DefaultCommands : MonoBehaviour
    {
        [PlatinumConsole_Command("Time-Scale")]
        [PlatinumConsole_CommandDescription("Set the current time scale within the game.")]
        [PlatinumConsole_CommandQuickAction("Print Time-Scale")]
        public void GetTimeScale()
        {
            PlatinumConsole_DebugWindow.ConsoleLog(TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }

        [PlatinumConsole_Command("Time-Scale")]
        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            PlatinumConsole_DebugWindow.ConsoleLog("Set time scale to: " + TextUtils.ColoredText(Time.timeScale.ToString(), Color.green));
        }

        [PlatinumConsole_Command("Debug-Screenshot")]
        [PlatinumConsole_CommandDescription("Takes a screenshot of the game and stored it in the project's persistent data directory.")]
        [PlatinumConsole_CommandQuickAction("Take Debug Screenshot")]
        public void DebugScreenshot()
        {
            StartCoroutine(TakeDebugScreenShot());
        }

        IEnumerator TakeDebugScreenShot()
        {
            yield return new WaitForEndOfFrame();
            string screenshotName = $"{Application.productName}-{DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo)}";

            Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots");

            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Screenshots/" + screenshotName + ".png");
        }
    }
}
