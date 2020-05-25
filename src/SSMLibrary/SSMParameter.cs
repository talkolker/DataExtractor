using System;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace SSMLibrary
{
    public class SSMParameter
    {
        public string Name { get; private set; }

        public string Value { get; private set; }

        public ParameterType Type { get; private set; }

        internal DateTime LastFetched { get; private set; }

        internal SSMParameter(Parameter i_Parameter)
        {
            Name = i_Parameter.Name;
            Value = i_Parameter.Value;
            Type = i_Parameter.Type;
            LastFetched = DateTime.Now;
        }
    }
}
