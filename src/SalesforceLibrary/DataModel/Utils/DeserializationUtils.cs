using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using TimeZoneConverter;

public static class DeserializationUtils
{
    internal static readonly ISet<string> NamespacesToIgnore;
    public static readonly JsonSerializerSettings SFJsonSerializerSettings;

    internal const string k_NamespaceFieldFormat = "{0}__{1}";

    static DeserializationUtils()
    {
        NamespacesToIgnore = new HashSet<string>();

        SFJsonSerializerSettings = new JsonSerializerSettings
        {
#if DEBUG
            Formatting = Formatting.Indented,
#else
                Formatting = Formatting.None,
#endif
            ContractResolver = new SalesforceContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        };
    }

    internal static void ProcessRelatedObjectsCollection<T>(RelatedObjectCollection<T> i_ObjectCollection,
        SFRefereceResolver i_ReferenceResolver) where T : sObject
    {
        if (i_ObjectCollection != null)
        {
            foreach (T innerRecord in i_ObjectCollection.records)
            {
                i_ReferenceResolver.AddReference(innerRecord);
                innerRecord.updateReferencesAfterDeserialize(i_ReferenceResolver);
            }
        }
    }

    internal static T GetSingleObjectReference<T>(string i_ObjectId, SFRefereceResolver i_ReferenceResolver)
        where T : sObject
    {
        return GetSingleObjectReference<T>(i_ObjectId, null, i_ReferenceResolver);
    }

    internal static T GetSingleObjectReference<T>(string i_ObjectId, JToken i_ObjectJson,
        SFRefereceResolver i_ReferenceResolver) where T : sObject
    {
        T objectReference;
        if (!i_ReferenceResolver.TryGetObjectFromId(i_ObjectId, out objectReference))
        {
            if (i_ObjectJson != null && i_ObjectJson.Type == JTokenType.Object)
            {
                objectReference = i_ObjectJson.ToObject<T>(JsonSerializer.Create(SFJsonSerializerSettings));
                i_ReferenceResolver.AddReference(objectReference);
                objectReference.updateReferencesAfterDeserialize(i_ReferenceResolver);
            }
        }

        return objectReference;
    }

    internal static void AddListToReferenceResolver<T>(List<T> i_ObjectList, SFRefereceResolver i_RefereceResolver)
        where T : sObject
    {
        if (i_ObjectList != null)
        {
            i_ObjectList.ForEach(i_RefereceResolver.AddReference);
        }
    }

    internal static void RemoveListFromReferenceResolver<T>(List<T> i_ObjectList, SFRefereceResolver i_RefereceResolver)
        where T : sObject
    {
        if (i_ObjectList != null)
        {
            i_ObjectList.ForEach(i_RefereceResolver.DeleteReference);
        }
    }

    internal static string GetPropertyName(MemberInfo i_PropertyInfo, string i_PropertyName)
    {
        string returnValue = i_PropertyName;
        PackageNamespaceAttribute namespaceAttribute = i_PropertyInfo.GetCustomAttribute<PackageNamespaceAttribute>();
        if (namespaceAttribute != null && !NamespacesToIgnore.Contains(namespaceAttribute.Namespace))
        {
            returnValue = string.Format(k_NamespaceFieldFormat, namespaceAttribute.Namespace, i_PropertyName);
        }

        return returnValue;
    }

    internal static T CreateEmptyReferenceFromId<T>(string i_ObjectId, T i_Property,
        SFRefereceResolver i_RefereceResolver) where T : sObject
    {
        if (i_Property == null)
        {
            if (!string.IsNullOrEmpty(i_ObjectId))
            {
                i_Property = (T) Activator.CreateInstance(typeof(T), i_ObjectId);
                i_RefereceResolver.AddReference(i_Property);
            }
        }

        return i_Property;
    }

    internal static TimeZoneInfo GetTimeZoneFromSFString(string i_SFTimeZone)
    {
        return TZConvert.GetTimeZoneInfo(i_SFTimeZone);
    }

    internal static bool ShouldCopyPropertyValue(PropertyInfo i_Property, object i_Value)
    {
        if (i_Value == null)
        {
            return false;
        }

        if (i_Property.PropertyType == typeof(string))
        {
            if (string.IsNullOrEmpty((string) i_Value))
            {
                return false;
            }
        }
        else
        {
            // Skip collections
            if (typeof(IEnumerable).IsAssignableFrom(i_Property.PropertyType))
            {
                return false;
            }
        }

        if (i_Property.PropertyType.IsValueType)
        {
            var typeDefault = Activator.CreateInstance(i_Property.PropertyType);
            if (i_Value.Equals(typeDefault))
            {
                return false;
            }
        }

        return i_Property.CanWrite;
    }
}