using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.Practices.SharePoint.Common.Configuration;
using Microsoft.SharePoint.Administration;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;

namespace ElanCostCenterGroups
{
    public class ConfigureLogging
    {
        DiagnosticsAreaCollection _myAreas = null;
        DiagnosticsAreaCollection MyAreas
        {
            get
            {
                if (_myAreas == null)
                {
                    _myAreas = new DiagnosticsAreaCollection();
                    DiagnosticsArea newArea = new DiagnosticsArea("CMS");
                    newArea.DiagnosticsCategories.Add(new DiagnosticsCategory(
                        "CostCenterGroupJob", EventSeverity.None, TraceSeverity.Verbose));
                    _myAreas.Add(newArea);
                }
                return _myAreas;
            }
        }

        public void AddAreasToConfiguration()
        {
            IConfigManager configMgr =
                SharePointServiceLocator.GetCurrent().GetInstance<IConfigManager>();

            DiagnosticsAreaCollection configuredAreas = new
               DiagnosticsAreaCollection(configMgr);

            foreach (DiagnosticsArea newArea in MyAreas)
            {
                var existingArea = configuredAreas[newArea.Name];

                if (existingArea == null)
                {
                    configuredAreas.Add(newArea);
                }
                else
                {
                    foreach (DiagnosticsCategory c in newArea.DiagnosticsCategories)
                    {
                        var existingCategory = existingArea.DiagnosticsCategories[c.Name];
                        if (existingCategory == null)
                        {
                            existingArea.DiagnosticsCategories.Add(c);
                        }
                    }
                }
            }

            configuredAreas.SaveConfiguration();


        }
    }

}
