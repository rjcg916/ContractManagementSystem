using System;
using System.Security;
using System.Security.Principal;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;
using CMSCommon;

namespace SAPCodeUpdate
{
    class SAPCodeUpdateTimerJob : SPJobDefinition
    {

        public SAPCodeUpdateTimerJob()
            : base()
        {
        }

        public SAPCodeUpdateTimerJob(string jobName, SPService service, SPServer server, SPJobLockType targetType)
            : base(jobName, service, server, targetType)
        {
        }

        public SAPCodeUpdateTimerJob(string jobName, SPWebApplication webApplication)
            : base(jobName, webApplication, null, SPJobLockType.ContentDatabase)
        {
            this.Title = "SAP Code Update Timer Job";
        }


        public override void Execute(Guid contentDBId)
        {
            SPWebApplication webApplication = this.Parent as SPWebApplication;

            SPList configList = webApplication.Sites[0].RootWeb.Lists[Constants.SITECONFIGLIST];

            //fetch account/password for CMS SAP DB
            string login = CMSCommon.Utils.GetConfigSetting(configList, "SAPCMSLogin");
            string pw = CMSCommon.Utils.GetConfigSetting(configList, "SAPCMSPassword");

            CMSCommon.Impersonate imp = new Impersonate();

  
            if (imp.impersonateValidUser(login, "ecorp", pw))
            {

                WindowsIdentity  myIdent = WindowsIdentity.GetCurrent();

                Trace.WriteLine("Impersonating User: " + myIdent.Name);

                Utils.SAPCodeUpdate(webApplication);

                imp.undoImpersonation();

            }

        }

    }
}
