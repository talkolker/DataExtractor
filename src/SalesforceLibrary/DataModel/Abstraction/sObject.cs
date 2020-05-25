using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using SalesforceLibrary.DataModel.Standard;

namespace SalesforceLibrary.DataModel.Abstraction
{
    [JsonObject]
    public class sObject
    {
        [JsonProperty]
        internal SFAttributes attributes { get; set; }

        [JsonIgnore]
        public string ObjectType { get { return attributes?.type; } }

        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        public PropertyBag<bool?> BooleanFields { get; }

        [JsonIgnore]
        public PropertyBag<double?> NumericFields { get; }

        [JsonIgnore]
        public PropertyBag<List<sObject>> JunctionFields { get; }

        [JsonIgnore]
        public PropertyBag<DateTime?> DateFields { get; }

        [JsonIgnore]
        public PropertyBag<string> StringFields { get; }

        [JsonExtensionData(ReadData = true, WriteData = false)]
        protected IDictionary<string, JToken> m_JSONAdditionalData;

        [JsonIgnore]
        public object TranslatedObject { get; set; }

        public sObject() :
            this(null)
        { }

        public sObject(string i_ObjectId)
        {
            Id = i_ObjectId;

            BooleanFields = new PropertyBag<bool?>();
            NumericFields = new PropertyBag<double?>();
            JunctionFields = new PropertyBag<List<sObject>>();
            DateFields = new PropertyBag<DateTime?>();
            StringFields = new PropertyBag<string>();
        }

        internal virtual void removeReferencesBeforeDelete(SFRefereceResolver i_ReferenceResolver) { }


        internal virtual void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            if (m_JSONAdditionalData != null)
            {
                foreach (KeyValuePair<string, JToken> nonSchemaProperty in m_JSONAdditionalData)
                {
                    switch (nonSchemaProperty.Value.Type)
                    {
                        case JTokenType.Boolean:
                            BooleanFields[nonSchemaProperty.Key] = nonSchemaProperty.Value.Value<bool?>();
                            break;
                        case JTokenType.Float:
                        case JTokenType.Integer:
                            NumericFields[nonSchemaProperty.Key] = nonSchemaProperty.Value.Value<double?>();
                            break;
                        case JTokenType.Object:
                            JToken junctionToken = nonSchemaProperty.Value;
                            RelatedObjectCollection<sObject> deserializedJunctions = junctionToken.ToObject<RelatedObjectCollection<sObject>>();
                            DeserializationUtils.ProcessRelatedObjectsCollection(deserializedJunctions, i_ReferenceResolver);
                            JunctionFields[nonSchemaProperty.Key] = deserializedJunctions.records;
                            break;
                        case JTokenType.Date:
                            DateFields[nonSchemaProperty.Key] = nonSchemaProperty.Value.Value<DateTime?>();
                            break;
                        case JTokenType.String:
                            StringFields[nonSchemaProperty.Key] = nonSchemaProperty.Value.Value<string>();
                            break;
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            sObject objectToCompare = obj as sObject;
            if (objectToCompare == null)
                return false;

            if (Id != objectToCompare.Id)
                return false;

            if (string.IsNullOrEmpty(Id))
                return base.Equals(obj);

            return true;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return base.GetHashCode();
            }

            return Id.GetHashCode();
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        protected JToken removeObjectJTokenFromAdditionalDictionary(string i_ObjectAttribute)
        {
            JToken objectJson = null;
            if (m_JSONAdditionalData != null)
            {
                m_JSONAdditionalData.TryGetValue(i_ObjectAttribute, out objectJson);
                m_JSONAdditionalData.Remove(i_ObjectAttribute);
            }
            return objectJson;
        }

        internal void CopyNonNullValuesFromsObject(sObject i_Src)
        {
            if (i_Src == null)
                return;

            // Check myself and i_src are same type
            Type myDerivedType = GetType();
            if (myDerivedType != i_Src.GetType())
                return;

            //TODO: Check ids?

            BindingFlags propertiesSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            IEnumerable<PropertyInfo> nonEmptyProperties = myDerivedType.GetProperties(propertiesSearchFlags);
            foreach (PropertyInfo propertyToCopy in nonEmptyProperties)
            {
                object valueToCopy = propertyToCopy.GetValue(i_Src);
                if (DeserializationUtils.ShouldCopyPropertyValue(propertyToCopy, valueToCopy))
                {
                    propertyToCopy.SetValue(this, valueToCopy);
                }
            }
        }

        [JsonObject]
        internal class SFAttributes
        {
            [JsonProperty]
            internal string type { get; set; }
        }
    }
}
