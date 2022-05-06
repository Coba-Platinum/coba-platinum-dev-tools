using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using CobaPlatinum.DebugTools.ExposedFields;
using CobaPlatinum.DebugTools;
using CobaPlatinum.TextUtilities;

public class CP_ExposedFields
{
    public static List<ExposedFieldInfo> exposedMembers { get; private set; } = new List<ExposedFieldInfo>();
    public static List<TrackedExposedField> trackedFields { get; private set; } = new List<TrackedExposedField>();
    public static List<string> exposedMemberObjects { get; private set; } = new List<string>();
    public int cachedExposedFields;

    public CP_ExposedFields()
    {
        ReCacheFields();
    }

    private void ReCacheFields()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                MemberInfo[] members = type.GetMembers(flags);

                foreach (MemberInfo member in members)
                {
                    if (Attribute.GetCustomAttribute(member, typeof(ExposedFieldAttribute)) is ExposedFieldAttribute attribute)
                    {
                        if (attribute != null)
                        {
                            exposedMembers.Add(new ExposedFieldInfo(member, attribute));
                            cachedExposedFields++;
                        }
                    }
                }
            }
        }

        CP_DebugWindow.Instance.LogConsoleMessage("CP Console exposed fields cached. Total exposed fields cached: " + TextUtils.ColoredText(cachedExposedFields, Color.green), LogType.Log, CP_DebugWindow.Instance.PLATINUM_CONSOLE_TAG);
    }

    public static void UpdateCachedFieldValues()
    {
        trackedFields.Clear();

        foreach (ExposedFieldInfo member in CP_ExposedFields.exposedMembers)
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

                            trackedFields.Add(new TrackedExposedField(instance_class.name, member.exposedFieldAttribute.displayName, value, obj.GetType().Name));
                        }
                    }
                }
            }
            else
            {
                trackedFields.Add(new TrackedExposedField("Unknown", member.exposedFieldAttribute.displayName, "", ""));
            }
        }
    }

    public static List<TrackedExposedField> GetTrackedFieldsFromObject(string _object)
    {
        List<TrackedExposedField> trackedFields = new List<TrackedExposedField>();
        foreach (TrackedExposedField field in CP_ExposedFields.trackedFields)
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
        if(!exposedMemberObjects.Contains(_objectName))
            exposedMemberObjects.Add(_objectName);
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

        CP_ExposedFields.AddExposedMemberObject(_fieldObject);
    }
}
