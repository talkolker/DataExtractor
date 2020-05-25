using System;

namespace SalesforceLibrary.DataModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class SFNameFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public SFNameFieldAttribute(string i_NameField)
        {
            FieldName = i_NameField;
        }
    }
}
