using UnityEngine;
using System;


namespace CobaPlatinum.DebugWindow.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PlatinumCommandAttribute : Attribute
    {
        public string commandName;
        public string commandDescription = "No command description set.";

        public PlatinumCommandAttribute()
        {

        }

        public PlatinumCommandAttribute(string _alias)
        {
            commandName = _alias;
        }
        public PlatinumCommandAttribute(string _alias, string _description)
        {
            commandName = _alias;
            commandDescription = _description;
        }
    }

    public class PlatinumCommandDescriptionAttribute : Attribute
    {
        public string commandDescription = "No command description set.";

        public PlatinumCommandDescriptionAttribute(string _description)
        {
            commandDescription = _description;
        }
    }

    public class PlatinumCommandAliasesAttribute : Attribute
    {
        public string[] aliases;

        public PlatinumCommandAliasesAttribute(string[] _aliases)
        {
            aliases = _aliases;
        }
    }
}
