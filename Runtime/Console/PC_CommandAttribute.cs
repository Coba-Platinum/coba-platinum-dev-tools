using UnityEngine;
using System;


namespace CobaPlatinum.DebugTools.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PC_CommandAttribute : Attribute
    {
        public string commandName;
        public string commandDescription = "No command description set.";

        public PC_CommandAttribute()
        {

        }

        public PC_CommandAttribute(string _alias)
        {
            commandName = _alias;
        }
        public PC_CommandAttribute(string _alias, string _description)
        {
            commandName = _alias;
            commandDescription = _description;
        }
    }

    public class PC_CommandDescriptionAttribute : Attribute
    {
        public string commandDescription = "No command description set.";

        public PC_CommandDescriptionAttribute(string _description)
        {
            commandDescription = _description;
        }
    }

    public class PC_CommandAliasesAttribute : Attribute
    {
        public string[] aliases;

        public PC_CommandAliasesAttribute(string[] _aliases)
        {
            aliases = _aliases;
        }
    }

    public class PC_CommandQuickActionAttribute : Attribute
    {
        public string quickActionName;
        public string quickActionCommand;

        public PC_CommandQuickActionAttribute(string _quickActionName, string _quickActionCommand)
        {
            quickActionName = _quickActionName;
            quickActionCommand = _quickActionCommand;
        }

        public PC_CommandQuickActionAttribute(string _quickActionName)
        {
            quickActionName = _quickActionName;
        }

        public PC_CommandQuickActionAttribute()
        {

        }
    }
}
