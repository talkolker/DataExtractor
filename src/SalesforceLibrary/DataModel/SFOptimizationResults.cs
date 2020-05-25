using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SalesforceLibrary.DataModel
{
    [JsonObject]
    public class SFOptimizationResults
    {
        public List<ServiceAppointment> Services { get; set; }

        public List<AssignedResource> AssignedResourcesToUpdate { get; set; }

        public List<AssignedResource> AssignedResourcesToCreate { get; set; }

        public List<AssignedResource> AssignedResourcesToDelete { get; set; }

        public List<ResourceAbsence> Absences { get; set; }

        public List<ResourceAbsence> BreaksToDelete { get; set; }

        public List<ResourceAbsence> BreaksToCreate { get; set; }

        [JsonIgnore]
        public OperationStatistics Statistics { get; set; }
        public List<Metric> Metrics { get; set; }

        [JsonIgnore]
        public int NumberOfObjects
        {
            get
            {
                int numberOfObjectsInLists = 0;
                numberOfObjectsInLists += getListSize(Services);
                numberOfObjectsInLists += getListSize(AssignedResourcesToUpdate);
                numberOfObjectsInLists += getListSize(AssignedResourcesToCreate);
                numberOfObjectsInLists += getListSize(AssignedResourcesToDelete);
                numberOfObjectsInLists += getListSize(Absences);
                numberOfObjectsInLists += getListSize(BreaksToDelete);
                numberOfObjectsInLists += getListSize(BreaksToCreate);

                return numberOfObjectsInLists;
            }
        }

        private int getListSize<T>(IList<T> i_ListOfObjects)
        {
            if (i_ListOfObjects == null)
                return 0;
            else
                return i_ListOfObjects.Count;
        }

        internal void postDeserialization(SFRefereceResolver i_refereceResolver = null)
        {
            if (i_refereceResolver == null)
            {
                i_refereceResolver = new SFRefereceResolver();
            }

            DeserializationUtils.RemoveListFromReferenceResolver(BreaksToDelete, i_refereceResolver);
            DeserializationUtils.RemoveListFromReferenceResolver(AssignedResourcesToDelete, i_refereceResolver);

            DeserializationUtils.AddListToReferenceResolver(Services, i_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Absences, i_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(AssignedResourcesToUpdate, i_refereceResolver);

            if (AssignedResourcesToCreate != null)
            {
                AssignedResourcesToCreate.ForEach(ar => ar.updateReferencesAfterDeserialize(i_refereceResolver));
            }
            if (BreaksToCreate != null)
            {
                BreaksToCreate.ForEach(ra => ra.updateReferencesAfterDeserialize(i_refereceResolver));
            }

            //i_refereceResolver.UpdateStoredObjectsReferences();

        }

        internal SFOptimizationResults getPartialResults(int i_StartingIndex, int i_RemainingObjectsCount)
        {
            SFOptimizationResults partialResults = new SFOptimizationResults();

            partialResults.AssignedResourcesToDelete = getPartialList(AssignedResourcesToDelete, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.BreaksToDelete = getPartialList(BreaksToDelete, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.Services = getPartialList(Services, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.Absences = getPartialList(Absences, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.AssignedResourcesToUpdate = getPartialList(AssignedResourcesToUpdate, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.AssignedResourcesToCreate = getPartialList(AssignedResourcesToCreate, ref i_StartingIndex, ref i_RemainingObjectsCount);
            partialResults.BreaksToCreate = getPartialList(BreaksToCreate, ref i_StartingIndex, ref i_RemainingObjectsCount);
            //partialResults.Statistics = Statistics;

            return partialResults;
        }


        private List<T> getPartialList<T>(List<T> i_List, ref int i_StartingIndex, ref int i_RemainingObjectsCount)
        {
            List<T> returnedListObjects = null;
            if (i_List != null && i_RemainingObjectsCount > 0)
            {
                int currentListSize = i_List.Count;
                if (currentListSize > i_StartingIndex)
                {
                    returnedListObjects = i_List.Skip(i_StartingIndex).Take(i_RemainingObjectsCount).ToList();
                    i_StartingIndex = 0;
                    i_RemainingObjectsCount -= returnedListObjects.Count;
                }
                else
                {
                    i_StartingIndex -= currentListSize;
                }
            }
            return returnedListObjects;
        }

        //public static SFOptimizationResults Parse(string i_StringToPrase, bool i_IsManaged = true)
        //{
        //    return Parse(i_StringToPrase, i_IsManaged, null);
        //}

        internal static SFOptimizationResults Parse(string i_StringToPrase, SFRefereceResolver i_ReferenceReslover)
        {

            if (string.IsNullOrWhiteSpace(i_StringToPrase))
                throw new ArgumentNullException("i_StringToPrase");

            SFOptimizationResults parsedOptimizationObjects = JsonConvert.DeserializeObject<SFOptimizationResults>(i_StringToPrase,
                DeserializationUtils.SFJsonSerializerSettings);
            parsedOptimizationObjects.postDeserialization(i_ReferenceReslover);
            return parsedOptimizationObjects;
        }

        public class OperationStatistics
        {
            public int NumberOfUnppinnedServices { get; set; }

            public int NumberOfScheduledServices { get; set; }

            public int NumberOfPinnedServices { get; set; }

            public double AverageTravelTimeBefore { get; set; }

            public double AverageTravelTimeAfter { get; set; }

            public string DynamicGanttResultsText { get; set; }
        }
    }
}
