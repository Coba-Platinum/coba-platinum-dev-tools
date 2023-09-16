using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CobaPlatinum.TextUtilities;
using System;
using System.IO;

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

        [PC_Command("Debug-Screenshot")]
        [PC_CommandDescription("Takes a screenshot of the game and stored it in the project's persistent data directory.")]
        [PC_CommandQuickAction("Take Debug Screenshot")]
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
