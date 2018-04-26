using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;
using CMSCommon;

namespace SRMFileTransfer
{


    class SRMFileTransferTimerJob : SPJobDefinition
    {

        public SRMFileTransferTimerJob()
            : base()
        {
        }

        public SRMFileTransferTimerJob(string jobName, SPService service, SPServer server, SPJobLockType targetType)
            : base(jobName, service, server, targetType)
        {
        }

        public SRMFileTransferTimerJob(string jobName, SPWebApplication webApplication)
            : base(jobName, webApplication, null, SPJobLockType.ContentDatabase)
        {
            this.Title = "SRM File Transfer Job";
        }

        public override void Execute(Guid contentDBId)
        {
            SPWebApplication webApplication = this.Parent as SPWebApplication;

            Trace.WriteLine("Entering SRM File Transfer Job for " + webApplication.Name);

            Utils.GenerateSRMFiles(webApplication);
        }
    }

}
