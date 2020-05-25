using SalesforceLibrary.DataModel.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public interface IResourcePreferenceParent
    {
        List<ResourcePreference> ResourcePreferences { get; set; }
    }
}
