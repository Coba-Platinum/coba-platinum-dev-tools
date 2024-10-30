using CobaPlatinum.DebugTools.ErrorLog;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorLogPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorMessageText;
    public string ErrorMessage { get { return errorMessageText.text; } }
    [SerializeField] private TextMeshProUGUI occurrencesText;

    private int occurrences = 0;

    public void SetErrorMessage(string  errorMessage)
    {
        errorMessageText.text = errorMessage;
        IncrementOccurrences();

        LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.GetComponent<RectTransform>());
    }

    public void IncrementOccurrences()
    {
        occurrences++;

        occurrencesText.text = $"x{occurrences}";
    }

    public void DeletePanel()
    {
        List<ErrorLogPanel> errorLogs = PlatinumConsole_ErrorLog.Instance.ErrorLogs;
        errorLogs.Remove(this);
        Destroy(gameObject);
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = errorMessageText.text;
    }
}
