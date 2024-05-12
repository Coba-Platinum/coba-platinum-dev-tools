using UnityEngine;
using System;


namespace CobaPlatinum.DebugTools.PlatinumConsole
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PlatinumConsole_CommandAttribute : Attribute
    {
        public string commandName;
        public string commandDescription = "No command description set.";

        public PlatinumConsole_CommandAttribute()
        {

        }

        public PlatinumConsole_CommandAttribute(string _alias)
        {
            commandName = _alias;
        }
        public PlatinumConsole_CommandAttribute(string _alias, string _description)
        {
            commandName = _alias;
            commandDescription = _description;
        }
    }

    public class PlatinumConsole_CommandDescriptionAttribute : Attribute
    {
        public string commandDescription = "No command description set.";

        public PlatinumConsole_CommandDescriptionAttribute(string _description)
        {
            commandDescription = _description;
        }
    }

    public class PlatinumConsole_CommandAliasesAttribute : Attribute
    {
        public string[] aliases;

        public PlatinumConsole_CommandAliasesAttribute(string[] _aliases)
        {
            aliases = _aliases;
        }
    }

    public class PlatinumConsole_CommandQuickActionAttribute : Attribute
    {
        public string quickActionName;
        public string quickActionCommand;

        public PlatinumConsole_CommandQuickActionAttribute(string _quickActionName, string _quickActionCommand)
        {
            quickActionName = _quickActionName;
            quickActionCommand = _quickActionCommand;
        }

        public PlatinumConsole_CommandQuickActionAttribute(string _quickActionName)
        {
            quickActionName = _quickActionName;
        }

        public PlatinumConsole_CommandQuickActionAttribute()
        {

        }
    }
}
