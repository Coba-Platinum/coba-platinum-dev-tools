using CobaPlatinum.DebugTools.PlatinumConsole;
using CobaPlatinum.TextUtilities;
using System.Collections.Generic;
using UnityEngine;

namespace CobaPlatinum.DebugTools.ErrorLog
{
    public class PlatinumConsole_ErrorLog : MonoBehaviour
    {
        #region Singleton

        public static PlatinumConsole_ErrorLog Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one instance of PlatinumConsole_ErrorLog found!");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        [SerializeField] private ErrorLogPanel errorLogPanelTemplate;
        [SerializeField] private Transform errorLogListParent;
        [SerializeField] private List<ErrorLogPanel> errorLogs = new List<ErrorLogPanel>();
        public List<ErrorLogPanel> ErrorLogs { get { return errorLogs; } }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        [PlatinumConsole_Command("test-error")]
        public void TestError(string message)
        {
            Debug.LogError(message);
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                CreateErrorLog(logString);
            }
        }

        private void CreateErrorLog(string message)
        {
            foreach (ErrorLogPanel errorLog in errorLogs) 
            {
                if(errorLog.ErrorMessage.Equals(message))
                {
                    errorLog.IncrementOccurrences();
                    return;
                }
            }

            ErrorLogPanel newErrorLog = Instantiate(errorLogPanelTemplate.gameObject, errorLogListParent).GetComponent<ErrorLogPanel>();
            newErrorLog.gameObject.SetActive(true);
            newErrorLog.SetErrorMessage(message);
            errorLogs.Add(newErrorLog);
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}
