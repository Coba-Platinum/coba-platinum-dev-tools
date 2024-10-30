using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

using CobaPlatinum.DebugTools.PlatinumConsole;
using CobaPlatinum.TextUtilities;
using CobaPlatinum.DebugTools.ExposedFields;
using CobaPlatinum.DebugTools.PlatinumConsole.DefaultCommands;
using CobaPlatinum.DebugTools.PlatinumConsole.DefaultExposedFields;

namespace CobaPlatinum.DebugTools
{
    [RequireComponent(typeof(PlatinumConsole_DefaultCommands))]
    [RequireComponent(typeof(PlatinumConsole_DefaultExposedFields))]
    public class PlatinumConsole_DebugWindow : MonoBehaviour
    {
        #region Singleton

        public static PlatinumConsole_DebugWindow Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one instance of PlatinumConsole found!");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        #endregion

        [SerializeField] public bool debugModeEnabled = false;
        [SerializeField][ExposedField("Debug Window Showing")] public bool showDebugWindow = false;
        [SerializeField] private Rect windowRect = new Rect(20, 20, 800, 50);
        private Rect alignedWindowRect;
        [SerializeField][ExposedField] private int tabIndex = 0;
        [SerializeField] private DebugWindowAlignment alignment = DebugWindowAlignment.Right;

        [SerializeField] private int maxConsoleMessages = 200;
        [SerializeField] private Queue<string> consoleMessages = new Queue<string>();

        public static Color LOG_COLOR = Color.white;
        public static Color WARNING_COLOR = Color.yellow;
        public static Color ERROR_COLOR = Color.red;
        public static Color OBJECT_COLOR = Color.green;
        public static Color INPUT_COLOR = Color.cyan;

        public ConsoleTag UNITY_TAG = new ConsoleTag("UNITY", TextUtils.UnnormalizedColor(180, 0, 255));
        public ConsoleTag PLATINUM_CONSOLE_TAG = new ConsoleTag("CP CONSOLE", TextUtils.UnnormalizedColor(0, 165, 255));

        private string consoleInput;
        private Vector2 consoleScrollPosition = Vector2.zero;
        private Vector2 fieldsScrollPosition = Vector2.zero;
        private bool autoScroll = true;

        [SerializeField] private PlatinumConsole_ConsoleMethods platinumConsoleMethods;
        [SerializeField] private PlatinumConsole_ExposedFields exposedFields;

        [SerializeField] private List<SuggestedCommand> suggestedCommands;
        [SerializeField] private int selectedSuggestion = 0;

        //New Debug Window
        [SerializeField] private GameObject debugModeCanvas;
        [SerializeField] private GameObject debugWindowObject;
        [SerializeField] private TextMeshProUGUI debugConsoleOutput;
        [SerializeField] private ScrollRect debugConsoleScrollRect;
        [SerializeField] private TextMeshProUGUI exposedFieldsOutput;
        [SerializeField] private GameObject quickActionReferenceObject;
        [SerializeField] private GameObject quickActionObjectList;
        [SerializeField] private TMP_InputField commandInputField;
        [SerializeField] private GameObject suggestedCommandsBox;
        [SerializeField] private TextMeshProUGUI suggestedCommandsText;

        [SerializeField] private bool useCanvasMode = false;

        private Keyboard kb;

        public void Initialize()
        {
            DontDestroyOnLoad(this);

            platinumConsoleMethods = new PlatinumConsole_ConsoleMethods();
            exposedFields = new PlatinumConsole_ExposedFields();
        }

        private void Start()
        {
            InitializeDebugWindow();

            kb = InputSystem.GetDevice<Keyboard>();

            InvokeRepeating("SlowUpdate", 1f, 1f);
        }

        private void FixedUpdate()
        {
            if (autoScroll)
            {
                debugConsoleScrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void SlowUpdate()
        {
            PlatinumConsole_ExposedFields.UpdateCachedFieldValues();
        }

        void InitializeDebugWindow()
        {
            alignedWindowRect = windowRect;

            switch(alignment)
            {
                case DebugWindowAlignment.Left:
                    alignedWindowRect.height = Screen.height - 40;
                    break;
                case DebugWindowAlignment.Right:
                    alignedWindowRect.height = Screen.height - 40;
                    alignedWindowRect.x = Screen.width - alignedWindowRect.width - windowRect.x;
                    break;
                case DebugWindowAlignment.Top:
                    alignedWindowRect.width = Screen.width - 40;
                    break;
                case DebugWindowAlignment.Bottom:
                    alignedWindowRect.width = Screen.width - 40;
                    alignedWindowRect.y = Screen.height - alignedWindowRect.height - windowRect.y;
                    break;
                default:
                    alignedWindowRect.height = Screen.height - 40;
                    break;
            }
        }

        private void ToggleConsole()
        {
            setShowDebugWindow(!showDebugWindow);

            if (showDebugWindow)
            {
                toggleDebugMode(true);
                InitializeDebugWindow();
            }
        }

        public void setShowDebugWindow(bool _showDebugWindow)
        {
            this.showDebugWindow = _showDebugWindow;
        }

        public void toggleDebugMode(bool _debugMode)
        {
            this.debugModeEnabled = _debugMode;

            debugModeCanvas.SetActive(debugModeEnabled);
        }

        [PlatinumConsole_Command("ReCache-Commands")]
        [PlatinumConsole_CommandDescription("Re-cache and regenerate all commands and quick actions for the debug console.")]
        [PlatinumConsole_CommandQuickAction("Re-Cache Commands")]
        public void ReCacheCommands()
        {
            platinumConsoleMethods.ReCacheMethods();
        }

        [PlatinumConsole_Command("ReCache-Variables")]
        [PlatinumConsole_CommandDescription("Re-cache all exposed variables for the debug console.")]
        [PlatinumConsole_CommandQuickAction("Re-Cache Variables")]
        public void ReCacheExposedVariables()
        {
            exposedFields.ReCacheFields();
        }

        [PlatinumConsole_Command("ReCache-All")]
        [PlatinumConsole_CommandDescription("Re-cache all exposed variables for the debug console and re-cache and regenerate all commands and quick actions for the debug console.")]
        [PlatinumConsole_CommandQuickAction("Re-Cache All")]
        public void ReCacheAll()
        {
            platinumConsoleMethods.ReCacheMethods();
            exposedFields.ReCacheFields();
        }

        public void SendCommand()
        {
            if (showDebugWindow)
            {
                ExecuteCommand(consoleInput);
                consoleInput = "";
                commandInputField.text = "";
            }
        }

        public void Update()
        {
            if (kb.f12Key.wasPressedThisFrame)
            {
                toggleDebugMode(!debugModeEnabled);
            }

            if (kb.f8Key.wasPressedThisFrame)
            {
                ToggleConsole();
            }

            if (kb.upArrowKey.wasPressedThisFrame)
            {
                selectedSuggestion++;
                commandInputField.caretPosition = commandInputField.text.Length;
            }

            if (kb.downArrowKey.wasPressedThisFrame)
            {
                selectedSuggestion--;
                commandInputField.caretPosition = commandInputField.text.Length;
            }

            if (kb.enterKey.wasPressedThisFrame)
            {
                SendCommand();
            }

            if (kb.tabKey.wasPressedThisFrame)
            {
                consoleInput = suggestedCommands[selectedSuggestion].commandName;
                commandInputField.text = suggestedCommands[selectedSuggestion].commandName;
                selectedSuggestion = 0;
                commandInputField.caretPosition = commandInputField.text.Length;
            }
        }

        private void OnGUI()
        {
            if (showDebugWindow)
            {
                if (useCanvasMode)
                {
                    debugWindowObject.SetActive(true);
                    DrawDebugWindow(0);
                }
                else
                {
                    // Register the window. Notice the 3rd parameter
                    alignedWindowRect = GUI.Window(0, alignedWindowRect, DrawDebugWindow, "Coba Platinum Debug Window");
                }

            }
            else
            {
                debugWindowObject.SetActive(false);
            }
        }

        private void DrawDebugWindow(int windowID)
        {
            switch (tabIndex)
            {
                case 0:
                    DrawDebugConsole();
                    break;
                case 1:
                    DrawExposedVariables();
                    break;
                case 3:
                    DrawDebugSettings();
                    break;
                default:
                    //Do nothing
                    break;
            }
        }

        public void setTabIntex(int _index)
        {
            tabIndex = _index;

            switch (tabIndex)
            {
                case 0:
                    DrawDebugConsole();
                    break;
                case 1:
                    DrawExposedVariables();
                    break;
                case 2:
                    DrawQuickActions();
                    break;
                case 3:
                    DrawDebugSettings();
                    break;
                default:
                    //Do nothing
                    break;
            }
        }

        public void setAutoScroll(bool _autoScroll)
        {
            autoScroll = _autoScroll;
        }

        public void hideDebugWindow()
        {
            showDebugWindow = false;
        }

        public void DrawDebugSettings()
        {
            //GUI.Label(new Rect(10, 60, 800, 20), "Debug Window Settings:");
        }

        public void SetMaxConsoleMessages(int _maxMessages)
        {
            maxConsoleMessages = _maxMessages;
        }

        public void DrawDebugConsole()
        {
            int index = 0;
            debugConsoleOutput.text = "";
            foreach (string message in consoleMessages)
            {
                debugConsoleOutput.text += (message + "\n");

                index++;
            }

            consoleInput = commandInputField.text;

            if(consoleInput != null && !consoleInput.Equals(""))
                DrawCommandSuggestions();
            else
                suggestedCommandsBox.SetActive(false);
        }

        public void DrawCommandSuggestions()
        {
            suggestedCommands = new List<SuggestedCommand>();

            foreach (PlatinumCommand command in PlatinumConsole_ConsoleMethods.Commands)
            {
                if (command.commandName.ToLower().StartsWith(consoleInput.ToLower()) && !command.commandName.ToLower().Equals(consoleInput.ToLower()))
                {
                    for (int i = 0; i < command.commandSignatures.Count; i++)
                    {
                        suggestedCommands.Add(new SuggestedCommand(command.commandName, command.commandName + command.GetArguments(i).ToString()));
                    }
                }
            }

            if (suggestedCommands.Count > 0)
            {
                suggestedCommandsBox.SetActive(true);

                suggestedCommandsText.text = "";
                for (int i = suggestedCommands.Count - 1; i >= 0; i--)
                {
                    if (selectedSuggestion == i)
                    {
                        suggestedCommandsText.text += TextUtils.ColoredText(suggestedCommands[i].commandSignature, Color.green) + "\n";
                    }
                    else
                    {
                        suggestedCommandsText.text += suggestedCommands[i].commandSignature + "\n";
                    }
                }

                if (selectedSuggestion > suggestedCommands.Count - 1)
                {
                    selectedSuggestion = 0;
                }

                if (selectedSuggestion < 0)
                {
                    selectedSuggestion = suggestedCommands.Count - 1;
                }
            }
            else
            {
                suggestedCommandsBox.SetActive(false);
            }
        }

        public void DrawExposedVariables()
        {
            int index = 0;

            exposedFieldsOutput.text = "";
            foreach (string exposedObject in PlatinumConsole_ExposedFields.ExposedMemberObjects)
            {
                exposedFieldsOutput.text += ($"[{TextUtils.ColoredText("GameObject", Color.green)} - {exposedObject}]:" + "\n");

                index++;

                List<TrackedExposedField> trackedFields = PlatinumConsole_ExposedFields.GetTrackedFieldsFromObject(exposedObject);
                foreach (TrackedExposedField field in trackedFields)
                {
                    exposedFieldsOutput.text += ($" L [{ TextUtils.ColoredText(field.fieldName, Color.cyan)}:{ field.fieldType}]  Value: { TextUtils.ColoredText(field.fieldValue, Color.magenta)}" + "\n");

                    index++;
                }
            }
        }

        public void DrawQuickActions()
        {
            int index = 0;

            List<GameObject> oldQuickActionObjects = new List<GameObject>();
            foreach (Transform child in quickActionObjectList.transform) oldQuickActionObjects.Add(child.gameObject);
            foreach(GameObject child in oldQuickActionObjects)
            {
                if(!child.name.Contains("Reference"))
                    DestroyImmediate(child);
            }

            quickActionReferenceObject.SetActive(false);
            foreach (PlatinumQuickAction quickAction in PlatinumConsole_ConsoleMethods.QuickActions)
            {
                GameObject quickActionObject = Instantiate(quickActionReferenceObject, quickActionObjectList.transform);
                quickActionObject.name = quickAction.quickActionName;
                quickActionObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = quickAction.quickActionName;
                quickActionObject.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => ExecuteCommand(quickAction.quickActionCommand));
                quickActionObject.SetActive(true);

                index++;
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            LogConsoleMessage("Coba Platinum Console linked to Unity debug log", LogType.Log, PLATINUM_CONSOLE_TAG);
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            LogConsoleMessage(logString, type, UNITY_TAG);
        }

        public void LogConsoleMessage(string _message, LogType _messageType, ConsoleTag _consoleTag = null)
        {
            string newMessage = "";

            if (_consoleTag != null)
                newMessage += "[" + TextUtils.ColoredText(_consoleTag.tagLabel, _consoleTag.tagColor) + "]";

            if (_messageType == LogType.Log)
            {
                newMessage += "[" + TextUtils.ColoredText(_messageType.ToString().ToUpper(), LOG_COLOR) + "]: "
                    + TextUtils.ColoredText(_message, LOG_COLOR);
            }
            else if (_messageType == LogType.Warning)
            {
                newMessage += "[" + TextUtils.ColoredText(_messageType.ToString().ToUpper(), WARNING_COLOR) + "]: "
                    + TextUtils.ColoredText(_message, WARNING_COLOR);
            }
            else if (_messageType == LogType.Error)
            {
                newMessage += "[" + TextUtils.ColoredText(_messageType.ToString().ToUpper(), ERROR_COLOR) + "]: "
                    + TextUtils.ColoredText(_message, ERROR_COLOR);
            }
            else if (_messageType == LogType.Exception)
            {
                newMessage += "[" + TextUtils.ColoredText(_messageType.ToString().ToUpper(), ERROR_COLOR) + "]: "
                    + TextUtils.ColoredText(_message, ERROR_COLOR);
            }
            else
            {
                newMessage += "[" + TextUtils.ColoredText(_messageType.ToString().ToUpper(), LOG_COLOR) + "]: "
                    + TextUtils.ColoredText(_message, LOG_COLOR);
            }

            consoleMessages.Enqueue(newMessage);

            if (consoleMessages.Count > maxConsoleMessages)
                consoleMessages.Dequeue();
        }

        public static void ConsoleLog(string _message)
        {
            Instance.LogConsoleMessage(_message, LogType.Log, Instance.PLATINUM_CONSOLE_TAG);
        }

        public static void ConsoleWarning(string _message)
        {
            Instance.LogConsoleMessage(_message, LogType.Warning, Instance.PLATINUM_CONSOLE_TAG);
        }

        public static void ConsoleError(string _message)
        {
            Instance.LogConsoleMessage(_message, LogType.Error, Instance.PLATINUM_CONSOLE_TAG);
        }

        void LogTaglessConsoleMessage(string _message)
        {
            consoleMessages.Enqueue(_message);

            if (consoleMessages.Count > maxConsoleMessages)
                consoleMessages.Dequeue();
        }

        public static void Log(UnityEngine.Object _myObj, object _msg)
        {
            Debug.Log(TextUtils.ColoredText(_myObj.GetType() + ".cs:" + _myObj.name, OBJECT_COLOR)
                + TextUtils.ColoredText(" > ", Color.white) + _msg);
        }

        public static void LogWarning(UnityEngine.Object _myObj, object _msg)
        {
            Debug.LogWarning(TextUtils.ColoredText(_myObj.GetType() + ".cs:" + _myObj.name, OBJECT_COLOR)
                + TextUtils.ColoredText(" > ", Color.white) + _msg);
        }

        public static void LogError(UnityEngine.Object _myObj, object _msg)
        {
            Debug.LogError(TextUtils.ColoredText(_myObj.GetType() + ".cs:" + _myObj.name, OBJECT_COLOR)
                + TextUtils.ColoredText(" > ", Color.white) + _msg);
        }

        public void ExecuteCommand(string _command)
        {
            if (_command != "")
            {
                LogTaglessConsoleMessage("> " + TextUtils.ColoredText(_command, INPUT_COLOR));
                consoleInput = "";

                bool registered = false;
                string result = null;

                Regex regex = new Regex("\"(.*?)\"");
                var quotationArguments = regex.Matches(_command);

                List<string> args = new List<string>();
                string command;
                if (quotationArguments.Count > 0)
                {
                    for (int i = 0; i < quotationArguments.Count; i++)
                    {
                        string formatedQuotationArgument = quotationArguments[i].Value.Replace(" ", "_");
                        _command = _command.Replace(quotationArguments[i].Value, formatedQuotationArgument.Replace("\"", ""));
                    }
                }

                args = _command.Split(new char[] { ' ' }).ToList();
                command = args[0].ToLower();
                args.RemoveAt(0);
                foreach (PlatinumCommand method in PlatinumConsole_ConsoleMethods.Commands)
                {
                    if (method.HasAlias(command))
                    {
                        if (registered)
                            LogConsoleMessage("Multiple commands are defined with: " + command, LogType.Error, PLATINUM_CONSOLE_TAG);

                        foreach (var methodSignature in method.methods)
                        {
                            ParameterInfo[] methodParameters = methodSignature.GetParameters();

                            if (methodParameters.Length != args.Count)
                                continue;

                            Type type = (method.methods[0].DeclaringType);
                            List<object> argList = new List<object>();

                            // Cast Arguments if there is any
                            if (args.Count != 0)
                            {
                                if (methodParameters.Length == args.Count)
                                {
                                    // Cast string arguments to input objects types
                                    for (int i = 0; i < methodParameters.Length; i++)
                                    {
                                        try
                                        {
                                            var a = Convert.ChangeType(args[i].Replace("_", " "), methodParameters[i].ParameterType);
                                            argList.Add(a);
                                        }
                                        catch
                                        {
                                            result = string.Format("Counld not convert {0} to Type {1}", args[i], methodParameters[i].ParameterType);
                                            LogConsoleMessage(result, LogType.Error, PLATINUM_CONSOLE_TAG);
                                        }
                                    }
                                }
                            }
                            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                            {
                                var instance_classes = GameObject.FindObjectsOfType(type);
                                if (instance_classes != null)
                                {
                                    foreach (var instance_class in instance_classes)
                                    {
                                        result = (string)methodSignature.Invoke(instance_class, argList.ToArray());
                                    }
                                }
                            }
                            else
                            {
                                var instance_class = Activator.CreateInstance(type);
                                result = (string)methodSignature.Invoke(instance_class, argList.ToArray());
                            }
                            registered = true;
                            return;
                        }

                        result = string.Format("Invalid number of parameters!");
                        LogConsoleMessage(result, LogType.Error, PLATINUM_CONSOLE_TAG);
                    }
                }
                if (!string.IsNullOrEmpty(result))
                    LogConsoleMessage(result, LogType.Error, PLATINUM_CONSOLE_TAG);

                if (registered) return;
                LogConsoleMessage("Unknown command", LogType.Error, PLATINUM_CONSOLE_TAG);
            }
        }

        [PlatinumConsole_Command("Commands", "Show a list of all possible commands.")]
        public void PrintCommandList()
        {
            LogTaglessConsoleMessage("------ Commands ------");
            LogTaglessConsoleMessage("{COMMAND FORMAT} - {COMMAND DESCRIPTION}  - Use \"help {command}\" for more info.");

            foreach (var method in PlatinumConsole_ConsoleMethods.Commands)
            {
                LogTaglessConsoleMessage(TextUtils.ColoredText("\"" + method.commandSignatures[0] + "\"", INPUT_COLOR) + $" - {method.commandDescription}");
                string aliases = "";

                if (method.commandAliases.Count > 1)
                {
                    for (int i = 0; i < method.commandAliases.Count - 1; i++)
                    {
                        aliases += TextUtils.ColoredText(method.commandAliases[i], Color.cyan) + ", ";
                    }
                    aliases += TextUtils.ColoredText(method.commandAliases[method.commandAliases.Count - 1], Color.cyan);
                    LogTaglessConsoleMessage("  - Aliases: " + aliases);
                }
            }

            LogTaglessConsoleMessage("----------------------");
        }

        [PlatinumConsole_Command("Clear", "Clear the debug console.")]
        [PlatinumConsole_CommandQuickAction("Clear Console")]
        public void ClearConsole()
        {
            consoleMessages.Clear();
            LogConsoleMessage("Coba Platinum Console cleared!", LogType.Log, PLATINUM_CONSOLE_TAG);
        }

        [PlatinumConsole_Command()]
        public void Help()
        {
            LogTaglessConsoleMessage("----------------------");
            LogTaglessConsoleMessage("Welcome to the Coba Platinum Console!");
            LogTaglessConsoleMessage("This console can be used to debug the game by utilizing commands and viewing output logs from");
            LogTaglessConsoleMessage("the game! In order to see specific details about a command, please use the 'help {command}' command.");
            LogTaglessConsoleMessage("To see a full list of all the currently cached commands, use 'commands'.");
            LogTaglessConsoleMessage("----------------------");
        }

        [PlatinumConsole_Command("Help", "Learn more about a specific command.")]
        public void Help(string command)
        {
            foreach (var method in PlatinumConsole_ConsoleMethods.Commands)
            {
                if (method.HasAlias(command))
                {
                    LogTaglessConsoleMessage("----------------------");
                    LogTaglessConsoleMessage("Generated command help for " + TextUtils.ColoredText(command, Color.green));

                    LogTaglessConsoleMessage("Available command signatures:");
                    for (int i = 0; i < method.commandSignatures.Count; i++)
                    {
                        LogTaglessConsoleMessage("  - " + TextUtils.ColoredText(command + method.GetArguments(i), Color.cyan));
                    }

                    List<ParameterInfo> methodParameters = new List<ParameterInfo>();
                    foreach (var signature in method.methods)
                    {
                        foreach (var parameter in signature.GetParameters())
                        {
                            bool parameterExists = false;
                            for (int i = 0; i < methodParameters.Count; i++)
                            {
                                if (methodParameters[i].Name.Equals(parameter.Name))
                                {
                                    parameterExists = true;
                                }
                            }

                            if (!parameterExists)
                                methodParameters.Add(parameter);
                        }
                    }

                    if (methodParameters.Count > 0)
                    {
                        LogTaglessConsoleMessage("Parameter info:");
                        for (int i = 0; i < methodParameters.Count; i++)
                        {
                            LogTaglessConsoleMessage("  - " + TextUtils.ColoredText(methodParameters[i].Name, Color.cyan) + ": " + TextUtils.ColoredText(methodParameters[i].ParameterType.Name, Color.green));
                        }
                    }

                    if (method.commandAliases.Count > 1)
                    {
                        LogTaglessConsoleMessage("Available command aliases:");
                        string aliases = "";
                        for (int i = 0; i < method.commandAliases.Count - 1; i++)
                        {
                            aliases += TextUtils.ColoredText(method.commandAliases[i], Color.cyan) + ", ";
                        }
                        aliases += TextUtils.ColoredText(method.commandAliases[method.commandAliases.Count - 1], Color.cyan);
                        LogTaglessConsoleMessage("  - " + aliases);
                    }

                    LogTaglessConsoleMessage(" ");
                    LogTaglessConsoleMessage("Command description:");
                    LogTaglessConsoleMessage(method.commandDescription);
                    LogTaglessConsoleMessage("----------------------");

                    return;
                }
            }

            LogTaglessConsoleMessage(TextUtils.ColoredText("Command not found in cache!", Color.red));
        }
    }

    [System.Serializable]
    public class ConsoleTag
    {
        public string tagLabel;
        public Color tagColor;

        public ConsoleTag()
        {

        }

        public ConsoleTag(string _tag, Color _color)
        {
            tagLabel = _tag;
            tagColor = _color;
        }
    }

    public struct SuggestedCommand
    {
        public string commandName;
        public string commandSignature;

        public SuggestedCommand(string _commandName, string _commandSignature)
        {
            commandName = _commandName;
            commandSignature = _commandSignature;
        }
    }

    public enum DebugWindowAlignment
    {
        Left,
        Top,
        Right,
        Bottom
    }
}
