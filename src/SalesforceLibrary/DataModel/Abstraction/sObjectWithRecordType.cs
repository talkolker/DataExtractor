using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public class sObjectWithRecordType : sObject
    {
        public string RecordTypeId { get; set; }

        [JsonIgnore]
        public RecordType RecordType { get; set; }

        public sObjectWithRecordType() :
            this(null)
        { }

        public sObjectWithRecordType(string i_ObjectId) :
            base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("RecordType");
            RecordType = DeserializationUtils.GetSingleObjectReference<RecordType>(RecordTypeId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
