using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CobaPlatinum.DebugTools;
using CobaPlatinum.DebugTools.PlatinumConsole;
using CobaPlatinum.TextUtilities;
using System.Threading.Tasks;

public class PlatinumConsole_ConsoleMethods
{
    public static List<PlatinumCommand> Commands { get; private set; } = new List<PlatinumCommand>();
    public static List<PlatinumQuickAction> QuickActions { get; private set; } = new List<PlatinumQuickAction>();
    private static List<string> methodNames = new List<string>();
    public static int cachedSignatures = 0;
    public static int cachedAliases = 0;
    public static int cachedQuickActions = 0;

    public PlatinumConsole_ConsoleMethods()
    {
        ReCacheMethods();
    }

    public string[] GetCommandsContaining(string input)
    {
        return methodNames.Where(k => k.Contains(input)).ToArray();
    }

    public async void ReCacheMethods()
    {
        MonoBehaviour[] sceneActive = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

        await Task.Run(() => ReCacheMethodsAsync(sceneActive));
    }

    public void ReCacheMethodsAsync(MonoBehaviour[] _sceneActive)
    {
        Commands = new List<PlatinumCommand>();
        QuickActions = new List<PlatinumQuickAction>();
        methodNames = new List<string>();

        cachedSignatures = 0;
        cachedAliases = 0;
        cachedQuickActions = 0;


        foreach (MonoBehaviour mono in _sceneActive)
        {
            AddObjectMethodsToTerminal(mono);
        }

        PlatinumConsole_DebugWindow.Instance.LogConsoleMessage("CP Console methods cached. Total commands cached: " + TextUtils.ColoredText(Commands.Count, Color.green), LogType.Log, PlatinumConsole_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
        PlatinumConsole_DebugWindow.Instance.LogConsoleMessage("CP Console method signatures cached. Total signatures cached: " + TextUtils.ColoredText(cachedSignatures, Color.green), LogType.Log, PlatinumConsole_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
        PlatinumConsole_DebugWindow.Instance.LogConsoleMessage("CP Console aliases cached. Total aliases cached: " + TextUtils.ColoredText(cachedAliases, Color.green), LogType.Log, PlatinumConsole_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
        PlatinumConsole_DebugWindow.Instance.LogConsoleMessage("CP Console quick actions cached. Total quick actions cached: " + TextUtils.ColoredText(cachedQuickActions, Color.green), LogType.Log, PlatinumConsole_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
    }

    public static void AddObjectMethodsToTerminal(MonoBehaviour mono)
    {
        Type monoType = mono.GetType();

        // Retreive the fields from the mono instance
        MethodInfo[] methodFields = monoType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

        // search all fields and find the attribute
        for (int i = 0; i < methodFields.Length; i++)
        {
            // if we detect any attribute print out the data.
            if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumConsole_CommandAttribute)) is PlatinumConsole_CommandAttribute attribute)
            {
                if(attribute.commandName == null)
                    attribute.commandName = methodFields[i].Name;

                if (methodNames.Contains(attribute.commandName) == false)
                {
                    methodNames.Add(attribute.commandName);
                    PlatinumCommand newCommand = new PlatinumCommand(attribute.commandName, attribute.commandDescription);
                    Commands.Add(newCommand);
                    newCommand.AddMethodSignature(PlatinumCommand.FormatSignature(newCommand, methodFields[i]), methodFields[i]);
                    cachedSignatures++;
                    cachedAliases++;
                }
                else
                {
                    Commands[methodNames.IndexOf(attribute.commandName)].AddMethodSignature(PlatinumCommand.FormatSignature(Commands[methodNames.IndexOf(attribute.commandName)], methodFields[i]), methodFields[i]);
                    cachedSignatures++;
                }

                if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumConsole_CommandDescriptionAttribute)) is PlatinumConsole_CommandDescriptionAttribute descAttribute)
                {
                    Commands[methodNames.IndexOf(attribute.commandName)].commandDescription = descAttribute.commandDescription;
                }

                if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumConsole_CommandAliasesAttribute)) is PlatinumConsole_CommandAliasesAttribute aliasAttribute)
                {
                    Commands[methodNames.IndexOf(attribute.commandName)].AddAliases(aliasAttribute.aliases);
                    cachedAliases += aliasAttribute.aliases.Length;
                }

                //Add a quick action
                if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumConsole_CommandQuickActionAttribute)) is PlatinumConsole_CommandQuickActionAttribute quickActionAttribute)
                {
                    if (quickActionAttribute.quickActionName == null)
                        quickActionAttribute.quickActionName = methodFields[i].Name;

                    if (quickActionAttribute.quickActionCommand == null)
                        quickActionAttribute.quickActionCommand = attribute.commandName;

                    PlatinumQuickAction newQuickAction = new PlatinumQuickAction(quickActionAttribute.quickActionName, quickActionAttribute.quickActionCommand);
                    QuickActions.Add(newQuickAction);
                    cachedQuickActions++;
                }
            }
        }
    }
}

public class PlatinumCommand
{
    public string commandName;
    public List<string> commandSignatures;
    public List<string> commandAliases;
    public string commandDescription;
    public List<MethodInfo> methods;

    public PlatinumCommand(string _commandName, string _commandDescription)
    {
        commandName = _commandName;
        commandDescription = _commandDescription;
        commandSignatures = new List<string>();
        commandAliases = new List<string>();
        methods = new List<MethodInfo>();
        commandAliases.Add(commandName);
    }

    public void AddMethodSignature(string _signature, MethodInfo _methodInfo)
    {
        commandSignatures.Add(_signature);
        methods.Add(_methodInfo);
    }

    public static string FormatSignature(PlatinumCommand command, MethodInfo signature)
    {
        string commandFormat = $"{command.commandName}";
        for (int i = 0; i < signature.GetParameters().Length; i++)
        {
            commandFormat += " {" + signature.GetParameters()[i].Name + "}";
        }

        return commandFormat;
    }

    public void AddAliases(string[] _aliases)
    {
        for (int i = 0; i < _aliases.Length; i++)
        {
            if(!commandAliases.Contains(_aliases[i]))
                commandAliases.Add(_aliases[i]);
        }
    }

    public bool HasAlias(string _command)
    {
        for (int i = 0; i < commandAliases.Count; i++)
        {
            if (_command.ToLower().Equals(commandAliases[i].ToLower()))
                return true;
        }

        return false;
    }

    public string GetArguments(int _signatureIndex)
    {
        string commandFormat = "";
        for (int i = 0; i < methods[_signatureIndex].GetParameters().Length; i++)
        {
            commandFormat += " {" + methods[_signatureIndex].GetParameters()[i].Name + "}";
        }

        return commandFormat;
    }
}

public struct PlatinumQuickAction
{
    public string quickActionName;
    public string quickActionCommand;

    public PlatinumQuickAction(string _quickActionName, string _quickActionCommand)
    {
        quickActionName = _quickActionName;
        quickActionCommand = _quickActionCommand;
    }
}
