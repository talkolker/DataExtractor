using System;
using System.Collections.Generic;

namespace Processor
{
    public class Measurments
    {
        Dictionary<String, long> measurments = new Dictionary<string, long>();
        private long measureToSubtract;

        public long MeasureToSubtract => measureToSubtract;

        public void addMeasureToSubstarct(long syncWatchElapsedMilliseconds)
        {
            measureToSubtract = syncWatchElapsedMilliseconds;
        }

        public Dictionary<string, long> getMeasurments => measurments;

        public void updateMesurments()
        {
            measurments[Measures.RULES_PROCESSING] -= measurments[Measures.RULES_QUERY];
            measurments[Measures.OBJECTIVES_PROCESSING] -= measurments[Measures.OBJECTIVES_QUERY];
            measurments[Measures.SA_PROCESSING] -= measurments[Measures.SA_QUERY];
            measurments[Measures.DEPENDENCIES_PROCESSING] -= measurments[Measures.DEPENDENCIES_QUERY];
            measurments[Measures.MST_PROCESSING] -= measurments.ContainsKey(Measures.MST_QUERY) ? measurments[Measures.MST_QUERY] : 0;
            measurments[Measures.STM_PROCESSING] -= measurments[Measures.STM_QUERY];
            measurments[Measures.PARENT_PROCESSING] -= measurments[Measures.PARENT_QUERY];
            measurments[Measures.VISITING_HOURS_PROCESSING] -= measurments.ContainsKey(Measures.VISITING_HOURS_QUERY) ? measurments[Measures.VISITING_HOURS_QUERY] : 0;
            measurments[Measures.RESOURCES_PROCESSING] -= measurments[Measures.RESOURCES_QUERY];
            measurments[Measures.UNLICENSED_USERS_PROCESSING] -= measurments[Measures.UNLICENSED_USERS_QUERY];
            measurments[Measures.SAS_PROCESSING] -= measurments[Measures.SAS_QUERY];
            measurments[Measures.ABSENCES_SHIFTS_PROCESSING] -= measurments[Measures.ABSENCES_SHIFTS_QUERY];
            measurments[Measures.ADITTIONAL_DATA_STM_PROCESSING] -= measurments[Measures.ADITTIONAL_DATA_STM_QUERY];
            measurments[Measures.CAPACITIES_PROCESSING] -= measurments[Measures.CAPACITIES_QUERY];
            measurments[Measures.CALENDARS_PROCESSING] -= measurments[Measures.CALENDARS_QUERY];
        }

        public void addMeasure(string measureType, long watchElapsedMilliseconds)
        {
            if (!measurments.ContainsKey(measureType))
            {
                measurments.Add(measureType, watchElapsedMilliseconds);
            }
            else
            {
                measurments[measureType] = measurments[measureType] + watchElapsedMilliseconds;
            }
        }
    }
    
    public static class Measures
    {
        public const string RULES_QUERY = "Get rules Query";
        public const string OBJECTIVES_QUERY = "Get objectives Query";
        public const string SA_QUERY = "Get invoked service Query";
        public const string DEPENDENCIES_QUERY = "Get dependencies Query";
        public const string MST_QUERY = "Get MST services query";
        public const string STM_QUERY = "Get STMs query";
        public const string PARENT_QUERY = "Get parent query";
        public const string VISITING_HOURS_QUERY = "Get parent visiting hours query";
        public const string RESOURCES_QUERY = "Get resources query";
        public const string UNLICENSED_USERS_QUERY = "Get unlicensed users query";
        public const string SAS_QUERY = "Get additional services Query";
        public const string ABSENCES_SHIFTS_QUERY = "Get absences and shifts Query";
        public const string ADITTIONAL_DATA_STM_QUERY = "Get additional stms Query";
        public const string CAPACITIES_QUERY = "Get capacities Query";
        public const string CALENDARS_QUERY = "Get calendars Query";


        public const string RULES_PROCESSING = "Get rules processing time (w/o DB operation)";
        public const string OBJECTIVES_PROCESSING = "Get objectives processing time (w/o DB operation)";
        public const string SA_PROCESSING = "Get invoked service processing time (w/o DB operation)";
        public const string DEPENDENCIES_PROCESSING = "Get dependencies processing time (w/o DB operation)";
        public const string MST_PROCESSING = "Get MST services processing time (w/o DB operation)";
        public const string STM_PROCESSING = "Get STMs processing time (w/o DB operation)";
        public const string PARENT_PROCESSING = "Get parent processing time (w/o DB operation)";
        public const string VISITING_HOURS_PROCESSING = "Get parent visiting hours processing time (w/o DB operation)";
        public const string RESOURCES_PROCESSING = "Get resources processing time (w/o DB operation)";
        public const string UNLICENSED_USERS_PROCESSING = "Get unlicensed users processing time (w/o DB operation)";
        public const string SAS_PROCESSING = "Get additional services processing time (w/o DB operation)";
        public const string ABSENCES_SHIFTS_PROCESSING = "Get absences and shifts processing time (w/o DB operation)";
        public const string ADITTIONAL_DATA_STM_PROCESSING = "Get additional stms processing time (w/o DB operation)";
        public const string CAPACITIES_PROCESSING = "Get capacities processing time (w/o DB operation)";
        public const string CALENDARS_PROCESSING = "Get calendars processing time (w/o DB operation)";
        

        public const string OBJECTIVES_RULES_PARALLEL = "Get objectives and rules total time done parallel (with queries)";
        public const string ADITTIONAL_DATA_PARALLEL = "Get capcities, resource terrs, breaks, shifts, services total time done parallel (with queries)";
    }

    public class RestAPIMeasurments : Measurments
    {
    }
}