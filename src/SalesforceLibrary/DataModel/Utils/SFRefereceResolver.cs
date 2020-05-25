using SalesforceLibrary.DataModel.Abstraction;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Utils
{
    internal class SFRefereceResolver
    {
        private Dictionary<string, sObject> m_IdToObjectReference = new Dictionary<string, sObject>();

        internal void AddReference(sObject i_Object)
        {
            if (i_Object == null)
                throw new ArgumentNullException("i_Object");
            if (string.IsNullOrEmpty(i_Object.Id))
                throw new ArgumentNullException("i_Object.Id");

            if (!m_IdToObjectReference.TryAdd(i_Object.Id, i_Object))
            {
                m_IdToObjectReference[i_Object.Id].CopyNonNullValuesFromsObject(i_Object);
            }
        }

        internal void DeleteReference(sObject i_Object)
        {
            if (i_Object == null)
                throw new ArgumentNullException("i_Object");

            i_Object.removeReferencesBeforeDelete(this);

            if (!string.IsNullOrEmpty(i_Object.Id))
            {
                m_IdToObjectReference.Remove(i_Object.Id);
            }
        }

        internal bool TryGetObjectFromId<T>(string i_Id, out T o_Object) where T : sObject
        {
            o_Object = null;
            if (!string.IsNullOrWhiteSpace(i_Id))
            {
                sObject tempObjectReference;
                if (m_IdToObjectReference.TryGetValue(i_Id, out tempObjectReference))
                {
                    o_Object = tempObjectReference as T;
                }
            }
            return o_Object != null;
        }

        internal void UpdateStoredObjectsReferences()
        {
            sObject[] objects = new sObject[m_IdToObjectReference.Count];
            m_IdToObjectReference.Values.CopyTo(objects, 0);
            foreach (sObject sObjectInstance in objects)
            {
                sObjectInstance.updateReferencesAfterDeserialize(this);
            }
        }
    }
}
