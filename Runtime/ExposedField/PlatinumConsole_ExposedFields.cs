using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using CobaPlatinum.DebugTools.ExposedFields;
using CobaPlatinum.DebugTools;
using CobaPlatinum.TextUtilities;
using System.Threading.Tasks;

public class PlatinumConsole_ExposedFields
{
    public static List<ExposedFieldInfo> ExposedMembers { get; private set; } = new List<ExposedFieldInfo>();
    public static List<TrackedExposedField> TrackedFields { get; private set; } = new List<TrackedExposedField>();
    public static List<string> ExposedMemberObjects { get; private set; } = new List<string>();
    public int cachedExposedFields;

    public PlatinumConsole_ExposedFields()
    {
        ReCacheFields();
    }

    public async void ReCacheFields()
    {
        await Task.Run(() => RecacheFieldsAsync());
    }

    public void RecacheFieldsAsync()
    {
        ExposedMembers = new List<ExposedFieldInfo>();
        TrackedFields = new List<TrackedExposedField>();
        ExposedMemberObjects = new List<string>();

        cachedExposedFields = 0;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                MemberInfo[] members = type.GetMembers(flags);

                foreach (MemberInfo member in members)
                {
                    if (Attribute.GetCustomAttribute(member, typeof(ExposedFieldAttribute)) is ExposedFieldAttribute attribute)
                    {
                        if (attribute != null)
                        {
                            ExposedMembers.Add(new ExposedFieldInfo(member, attribute));
                            cachedExposedFields++;
                        }
                    }
                }
            }
        }

        PlatinumConsole_DebugWindow.Instance.LogConsoleMessage("Platinum Console exposed fields cached. Total exposed fields cached: " + TextUtils.ColoredText(cachedExposedFields, Color.green), LogType.Log, PlatinumConsole_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
    }

    public static void UpdateCachedFieldValues()
    {
        TrackedFields.Clear();

        foreach (ExposedFieldInfo member in PlatinumConsole_ExposedFields.ExposedMembers)
        {
            MemberInfo memberInfo = member.memberInfo;
            ExposedFieldAttribute attribute = member.exposedFieldAttribute;

            if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo field = (FieldInfo)memberInfo;

                object obj = null;
                string value = "N/A";

                if (memberInfo.ReflectedType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    var instance_classes = GameObject.FindObjectsOfType(memberInfo.ReflectedType);
                    if (instance_classes != null)
                    {
                        foreach (var instance_class in instance_classes)
                        {
                            obj = field.GetValue(instance_class);

                            if (obj != null)
                            {
                                value = obj.ToString();
                            }

                            TrackedFields.Add(new TrackedExposedField(instance_class.name, member.exposedFieldAttribute.displayName, value, obj.GetType().Name));
                        }
                    }
                }
            }
            else
            {
                TrackedFields.Add(new TrackedExposedField("Unknown", member.exposedFieldAttribute.displayName, "", ""));
            }
        }
    }

    public static List<TrackedExposedField> GetTrackedFieldsFromObject(string _object)
    {
        List<TrackedExposedField> trackedFields = new List<TrackedExposedField>();
        foreach (TrackedExposedField field in PlatinumConsole_ExposedFields.TrackedFields)
        {
            if (field.fieldObject.Equals(_object))
            {
                trackedFields.Add(field);
            }
        }

        return trackedFields;
    }

    public static void AddExposedMemberObject(string _objectName)
    {
        if(!ExposedMemberObjects.Contains(_objectName))
            ExposedMemberObjects.Add(_objectName);
    }
}

public struct ExposedFieldInfo
{
    public MemberInfo memberInfo;
    public ExposedFieldAttribute exposedFieldAttribute;

    public ExposedFieldInfo(MemberInfo _memberInfo, ExposedFieldAttribute _exposedFieldAttribute)
    {
        memberInfo = _memberInfo;
        exposedFieldAttribute = _exposedFieldAttribute;

        if(exposedFieldAttribute.displayName == null)
        {
            exposedFieldAttribute.displayName = _memberInfo.Name;
        }
    }
}

public struct TrackedExposedField
{
    public string fieldObject;
    public string fieldName;
    public string fieldValue;
    public string fieldType;

    public TrackedExposedField(string _fieldObject, string _fieldName, string _fieldValue, string _fieldType)
    {
        fieldObject = _fieldObject;
        fieldName = _fieldName;
        fieldValue = _fieldValue;
        fieldType = _fieldType;

        PlatinumConsole_ExposedFields.AddExposedMemberObject(_fieldObject);
    }
}
