using System;

namespace SalesforceLibrary.DataModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class PackageNamespaceAttribute : Attribute
    {
        public string Namespace { get; }

        public PackageNamespaceAttribute(string i_Namespace)
        {
            Namespace = i_Namespace;
        }
    }
}
