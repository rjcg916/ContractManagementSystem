using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.MetadataModel.Collections;
using Microsoft.SharePoint.BusinessData.Runtime;
using Microsoft.BusinessData.Runtime;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.Common
{
    class SAP
    {
        public static string GetCostCenterName(SPWeb web, string costCenterNumber)
        {
            string strFinalCostCenterName = string.Empty;
            bool processCompleted = false;
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    using (SPSite site = new SPSite(web.Site.RootWeb.Url))
                    {
                        site.AllowUnsafeUpdates = true;

                        using (SPServiceContextScope scope = new Microsoft.SharePoint.SPServiceContextScope(SPServiceContext.GetContext(site)))
                        {
                            string nameSpace = Settings.Default.BDCEntityNamespace;
                            //string entityName = "CostCenterSAP";
                            //string finderView = "Read List";
                            string strFinderView = Properties.Settings.Default.BDCFinderView;//"Read List";
                            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
                            IMetadataCatalog catalog = service.GetDatabaseBackedMetadataCatalog(SPServiceContext.Current);
                            //IEntity entity = catalog.GetEntity(Properties.Settings.Default.BDCEntityNamespace, Properties.Settings.Default.BDCEntityName);
                            IEntity entity = catalog.GetEntity(nameSpace, Properties.Settings.Default.BDCEntityName);
                            ILobSystemInstance LobSysteminstance = entity.GetLobSystem().GetLobSystemInstances()[0].Value;

                            // Get fields in the Entity.
                            IFieldCollection fieldCollection = entity.GetFinderView(strFinderView).Fields;

                            // Retrieve all the records in the Entity, and populate XML string (with choices)
                            IMethodInstance methodInstance = entity.GetMethodInstance(strFinderView, MethodInstanceType.Finder);
                            IFilterCollection filters = methodInstance.GetFilters();

                            var filter = (from f in filters where f is ComparisonFilter select f as ComparisonFilter).Single();
                            filter.Value = costCenterNumber;

                            IEntityInstanceEnumerator ientityInstanceEnumerator = entity.FindFiltered(filters, LobSysteminstance);

                            while (ientityInstanceEnumerator.MoveNext())
                            {
                                strFinalCostCenterName = ientityInstanceEnumerator.Current["description"].ToString();
                            }
                        }
                        site.AllowUnsafeUpdates = false;

                    }
                    processCompleted = true;
                });

            }
            catch (Exception ex)
            {
                Log.WriteOnlyLogEntry(web, "Error: Could not find cost center name for cost center number: " + costCenterNumber, ex.ToString());
            }

            if (string.IsNullOrEmpty(strFinalCostCenterName) && processCompleted)
                Log.WriteOnlyLogEntry(web, "Warning: Could not find cost center name for cost center number: " + costCenterNumber, "Process completed with no errors but no cost center name was found");

            return strFinalCostCenterName;
        }

    }
}
