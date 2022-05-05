using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CobaPlatinum.DebugWindow;
using CobaPlatinum.DebugWindow.Console;

public class PlatinumConsoleMethods
{
    public static List<PlatinumCommand> Commands { get; private set; } = new List<PlatinumCommand>();
    private static List<string> methodNames = new List<string>();
    public static int cachedSignatures = 0;
    public static int cachedAliases = 0;
    public PlatinumConsoleMethods()
    {
        ReCacheMethods();
    }

    public string[] GetCommandsContaining(string input)
    {
        return methodNames.Where(k => k.Contains(input)).ToArray();
    }

    public void ReCacheMethods()
    {
        Commands = new List<PlatinumCommand>();
        methodNames = new List<string>();

        MonoBehaviour[] sceneActive = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour mono in sceneActive)
        {
            AddObjectMethodsToTerminal(mono);
        }

        PlatinumConsole.Instance.LogConsoleMessage("Platinum Console methods cached. Total commands cached: " + TextUtils.ColoredText(Commands.Count, Color.green), LogType.Log, PlatinumConsole.Instance.PLATINUM_CONSOLE_TAG);
        PlatinumConsole.Instance.LogConsoleMessage("Platinum Console method signatures cached. Total signatures cached: " + TextUtils.ColoredText(cachedSignatures, Color.green), LogType.Log, PlatinumConsole.Instance.PLATINUM_CONSOLE_TAG);
        PlatinumConsole.Instance.LogConsoleMessage("Platinum Console aliases cached. Total aliases cached: " + TextUtils.ColoredText(cachedAliases, Color.green), LogType.Log, PlatinumConsole.Instance.PLATINUM_CONSOLE_TAG);
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
            if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumCommandAttribute)) is PlatinumCommandAttribute attribute)
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

                if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumCommandDescriptionAttribute)) is PlatinumCommandDescriptionAttribute descAttribute)
                {
                    Commands[methodNames.IndexOf(attribute.commandName)].commandDescription = descAttribute.commandDescription;
                }

                if (Attribute.GetCustomAttribute(methodFields[i], typeof(PlatinumCommandAliasesAttribute)) is PlatinumCommandAliasesAttribute aliasAttribute)
                {
                    Commands[methodNames.IndexOf(attribute.commandName)].AddAliases(aliasAttribute.aliases);
                    cachedAliases += aliasAttribute.aliases.Length;
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

    public bool IsAnAlias(string _command)
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
