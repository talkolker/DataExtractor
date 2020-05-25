using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public class PropertyBag<T>
    {
        private readonly Dictionary<string, T> m_Properties;

        internal PropertyBag(bool i_IsCaseSensitive = false)
        {
            StringComparer keyComparer = i_IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            m_Properties = new Dictionary<string, T>(keyComparer);
        }

        public T this[string i_PropertyName]
        {
            get
            {
                T returnValue;
                if (!m_Properties.TryGetValue(i_PropertyName, out returnValue))
                {
                    if (!TryGetValueWithNamespaces(i_PropertyName, out returnValue))
                    {
                        returnValue = default(T);
                    }
                }
                return returnValue;
            }
            set
            {
                if (value == null)
                {
                    m_Properties.Remove(i_PropertyName);
                }
                else
                {
                    if (m_Properties.ContainsKey(i_PropertyName))
                    {
                        m_Properties[i_PropertyName] = value;
                    }
                    else
                    {
                        m_Properties.Add(i_PropertyName, value);
                    }
                }
            }
        }

        private bool TryGetValueWithNamespaces(string i_PropertyName, out T o_ReturnValue)
        {
            bool foundNamespacesValue = false;
            o_ReturnValue = default(T);
            if (DeserializationUtils.NamespacesToIgnore != null)
            {
                foreach (string namespaceValue in DeserializationUtils.NamespacesToIgnore)
                {
                    string fieldWithNamespace = string.Format(DeserializationUtils.k_NamespaceFieldFormat, namespaceValue, i_PropertyName);
                    if (m_Properties.TryGetValue(fieldWithNamespace, out o_ReturnValue))
                    {
                        foundNamespacesValue = true;
                        break;
                    }
                }
            }
            return foundNamespacesValue;
        }

        public void Clear()
        {
            m_Properties.Clear();
        }

        public bool IsEmpty
        {
            get
            {
                return m_Properties.Count == 0;
            }
        }

        public Dictionary<string, T>.KeyCollection Keys => m_Properties.Keys;
    }
}
