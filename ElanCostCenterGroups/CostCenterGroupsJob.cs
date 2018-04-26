using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using System.Diagnostics;
using CMSCommon;


namespace Elan
{
   
    class CostCenterGroupsJob : SPJobDefinition
    {

        public CostCenterGroupsJob()

            : base()
        {

        }

        public CostCenterGroupsJob(string jobName, SPService service, SPServer server, SPJobLockType targetType)

            : base(jobName, service, server, targetType)
        {

        }

        public CostCenterGroupsJob(string jobName, SPWebApplication webApplication)

            : base(jobName, webApplication, null, SPJobLockType.ContentDatabase)
        {

            this.Title = "Elan Cost Center Groups Job";

        }


        public override void Execute(Guid contentDbId)
        {

            SPWebApplication webApplication = this.Parent as SPWebApplication;

            using (SPSite site = webApplication.Sites[0])
            {
                ElanCostCenterGroups.Utils.UpdateCostCenterGroups(site, this.UpdateProgress); 
            }
        }
    }
}