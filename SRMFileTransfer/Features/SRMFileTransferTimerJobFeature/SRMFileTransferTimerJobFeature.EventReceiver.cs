using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;

namespace SRMFileTransfer.Features.SRMFileTransferTimerJobFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("bb5812af-5334-4b78-86b6-0088e7e8e125")]
    public class SRMFileTransferTimerJobFeatureEventReceiver : SPFeatureReceiver
    {
        const string JOB_NAME = "SRMFileTransfer";
        
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {

            Trace.WriteLine("SRMFileTransfer Event - FeatureActivated"); 
            
            SPWeb web = null;
            SPWebApplication app = null;
            SPSite site = properties.Feature.Parent as SPSite;

            if (site != null)
            {
                app = site.WebApplication;
                web = site.RootWeb;
            }
            else
            {
                Trace.WriteLine("SRMFileTransfer Event - Error with app/web/site");
                return;
            }


           
            // install the job
            Trace.WriteLine("SRMFileTransfer: Creating Job");
            try {
            if (app.JobDefinitions.Count > 0)
            {
                foreach (SPJobDefinition job in app.JobDefinitions)
                {

                    if (job.Name == JOB_NAME)
                    {
                        Trace.WriteLine("SRMFileTransfer: Deleting Job:" + job.Name);
                        job.Delete();
                        app.Update();
                    }
                }
            } }
            catch (Exception ex) {
                Trace.WriteLine("catch SRMFileTransfer: " + ex.Message + ex.InnerException + ex.Source);
            }
            

            SRMFileTransferTimerJob Job = new SRMFileTransferTimerJob(JOB_NAME, app);

            SPMinuteSchedule schedule = new SPMinuteSchedule();

            schedule.Interval = 5;
            schedule.BeginSecond = 1;

            Job.Schedule = schedule;

            Job.Update();

            Trace.WriteLine("SRMFileTransfer: Created Job: " + Job.Name);

        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPWebApplication app = null;
            SPWeb web = null;
            SPSite site = properties.Feature.Parent as SPSite;
            if (site != null)
            {
                app = site.WebApplication;
                web = site.RootWeb;
            }
            else
            {
                Trace.WriteLine("SRMFileTransfer Event - Error with app/web/site");
                return;
            }



            if (app.JobDefinitions.Count > 0)
            {
                foreach (SPJobDefinition job in app.JobDefinitions)
                {
                    if (job.Name == JOB_NAME)
                    {
                        Trace.WriteLine("SRMFileTransfer: job deleted" + job.Name);
                        job.Delete();
                        app.Update();
                    }
                }
            }

            
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
