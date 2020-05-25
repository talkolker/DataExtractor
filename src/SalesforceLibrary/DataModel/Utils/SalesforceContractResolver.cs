using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace SalesforceLibrary.DataModel.Utils
{
    internal class SalesforceContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "Name")
            {
                SFNameFieldAttribute classNameAttribute = member.DeclaringType.GetCustomAttribute<SFNameFieldAttribute>();
                if (classNameAttribute != null)
                    property.PropertyName = classNameAttribute.FieldName;
            }
            else
            {
                property.PropertyName = DeserializationUtils.GetPropertyName(member, property.PropertyName);
            }

            
            return property;
        }

    }
}
