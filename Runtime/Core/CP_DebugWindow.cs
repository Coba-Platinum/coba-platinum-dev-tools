using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

using CobaPlatinum.DebugTools.Console;
using CobaPlatinum.TextUtilities;
using CobaPlatinum.DebugTools.ExposedFields;

namespace CobaPlatinum.DebugTools
{
    public class CP_DebugWindow : MonoBehaviour
    {
        private string version = "0.1.6";

        #region Singleton

        public static CP_DebugWindow Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one instance of PlatinumConsole found!");
                return;
            }

            Instance = this;

            Initialize();
        }

        #endregion

        [SerializeField][ExposedField("Debug Window Showing")] public bool showDebugWindow = false;
        [SerializeField] private Rect windowRect = new Rect(20, 20, 800, 50);
        [SerializeField][ExposedField] private int tabIndex = 0;

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

        [SerializeField] private CP_ConsoleMethods platinumConsoleMethods;
        [SerializeField] private CP_ExposedFields exposedFields;

        public void Initialize()
        {
            DontDestroyOnLoad(this);

            platinumConsoleMethods = new CP_ConsoleMethods();
            exposedFields = new CP_ExposedFields();
        }

        private void Start()
        {
            InitializeDebugWindow();
        }

        private void FixedUpdate()
        {
            CP_ExposedFields.UpdateCachedFieldValues();
        }

        void InitializeDebugWindow()
        {
            windowRect.height = Screen.height - 40;
        }

        private void ToggleConsole()
        {
            showDebugWindow = !showDebugWindow;

            if (showDebugWindow)
                InitializeDebugWindow();
        }

        public void SendCommand()
        {
            if (showDebugWindow && GUI.GetNameOfFocusedControl().Equals("DebugCommandField"))
            {
                ExecuteCommand(consoleInput);
                consoleInput = "";
            }
        }

        private void OnGUI()
        {
            if (showDebugWindow)
            {
                // Register the window. Notice the 3rd parameter
                windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Coba Platinum Debug Window v" + version);
            }

            if (Event.current.Equals(Event.KeyboardEvent("None")))
            {
                SendCommand();
            }

            if (Event.current.Equals(Event.KeyboardEvent("f12")))
            {
                ToggleConsole();
            }
        }

        private void DrawDebugWindow(int windowID)
        {
            if (GUI.Button(new Rect(windowRect.width - 24, 5, 18, 18), ""))
            {
                showDebugWindow = false;
            }

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Debug Console"))
            {
                tabIndex = 0;
            }
            if (GUILayout.Button("Exposed Variables"))
            {
                tabIndex = 1;
            }
            if (GUILayout.Button("Settings"))
            {
                tabIndex = 2;
            }
            GUILayout.EndHorizontal();

            switch (tabIndex)
            {
                case 0:
                    DrawDebugConsole();
                    break;
                case 1:
                    DrawExposedVariables();
                    break;
                case 2:
                    DrawDebugSettings();
                    break;
                default:
                    DrawDebugConsole();
                    break;
            }
        }

        public void DrawDebugSettings()
        {
            GUI.Label(new Rect(10, 60, 800, 20), "Debug Window Settings:");
        }

        public void SetMaxConsoleMessages(int _maxMessages)
        {
            maxConsoleMessages = _maxMessages;
        }

        public void DrawDebugConsole()
        {
            GUI.Label(new Rect(10, 60, 800, 20), "Debug Console: (Type \"commands\" to view a list of all added commands)");
            autoScroll = GUI.Toggle(new Rect(windowRect.width - 110, 60, 100, 20), autoScroll, "Auto scroll");

            Rect consoleRect = new Rect(10, 80, windowRect.width - 20, windowRect.height - 120);
            Rect consoleViewRect = new Rect(consoleRect.x, consoleRect.y, consoleRect.width, 20 * consoleMessages.Count);

            GUI.Box(new Rect(consoleRect.x, consoleRect.y, consoleRect.width - 20, consoleRect.height), "");

            if (autoScroll)
                consoleScrollPosition = new Vector2(0, consoleViewRect.height);

            consoleScrollPosition = GUI.BeginScrollView(consoleRect, consoleScrollPosition, consoleViewRect, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

            int index = 0;
            foreach (string message in consoleMessages)
            {
                Rect labelRect = new Rect(15, consoleViewRect.y + (20 * index), consoleViewRect.width, 20);
                GUI.Label(labelRect, message);
                index++;
            }

            GUI.EndScrollView();

            GUI.SetNextControlName("DebugCommandField");
            consoleInput = GUI.TextField(new Rect(10, Screen.height - 71, windowRect.width - 190, 21), consoleInput);

            if (GUI.Button(new Rect(windowRect.width - 170, Screen.height - 71, 160, 21), "Send Command"))
            {
                SendCommand();
            }
        }

        public void DrawExposedVariables()
        {
            GUI.Label(new Rect(10, 60, 800, 20), "Exposed Variables:");

            Rect scrollViewRect = new Rect(10, 80, windowRect.width - 20, windowRect.height - 120);
            Rect scrollViewContentRect = new Rect(scrollViewRect.x, scrollViewRect.y, scrollViewRect.width, 20 * consoleMessages.Count);

            GUI.Box(new Rect(scrollViewRect.x, scrollViewRect.y, scrollViewRect.width - 20, scrollViewRect.height), "");

            fieldsScrollPosition = GUI.BeginScrollView(scrollViewRect, fieldsScrollPosition, scrollViewRect, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

            int index = 0;

            foreach(string exposedObject in CP_ExposedFields.exposedMemberObjects)
            {
                Rect labelRect = new Rect(15, scrollViewContentRect.y + (20 * index), scrollViewContentRect.width, 20);
                GUI.Label(labelRect, $"[{TextUtils.ColoredText("GameObject", Color.green)} - {exposedObject}]:");
                index++;

                List<TrackedExposedField> trackedFields = CP_ExposedFields.GetTrackedFieldsFromObject(exposedObject);
                foreach (TrackedExposedField field in trackedFields)
                {
                    Rect labelFieldRect = new Rect(15, scrollViewContentRect.y + (20 * index), scrollViewContentRect.width, 20);
                    GUI.Label(labelFieldRect, $"    - [{TextUtils.ColoredText(field.fieldName, Color.cyan)}:{field.fieldType}]  Value: {TextUtils.ColoredText(field.fieldValue, Color.magenta)}");
                    index++;
                }
            }

            GUI.EndScrollView();
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
                foreach (var method in CP_ConsoleMethods.Commands)
                {
                    if (method.IsAnAlias(command))
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

        [PlatinumCommand("Commands", "Show a list of all possible commands.")]
        public void PrintCommandList()
        {
            LogTaglessConsoleMessage("------ Commands ------");
            LogTaglessConsoleMessage("{COMMAND FORMAT} - {COMMAND DESCRIPTION}  - Use \"help {command}\" for more info.");

            foreach (var method in CP_ConsoleMethods.Commands)
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

        [PlatinumCommand("Clear", "Clear the debug console.")]
        public void ClearConsole()
        {
            consoleMessages.Clear();
            LogConsoleMessage("Coba Platinum Console cleared!", LogType.Log, PLATINUM_CONSOLE_TAG);
        }

        [PlatinumCommand()]
        public void Help()
        {
            LogTaglessConsoleMessage("----------------------");
            LogTaglessConsoleMessage("Welcome to the Coba Platinum Console!");
            LogTaglessConsoleMessage("This console can be used to debug the game by utilizing commands and viewing output logs from");
            LogTaglessConsoleMessage("the game! In order to see specific details about a command, please use the 'help {command}' command.");
            LogTaglessConsoleMessage("To see a full list of all the currently cached commands, use 'commands'.");
            LogTaglessConsoleMessage("----------------------");
        }

        [PlatinumCommand("Help", "Learn more about a specific command.")]
        public void Help(string command)
        {
            foreach (var method in CP_ConsoleMethods.Commands)
            {
                if (method.IsAnAlias(command))
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
}
