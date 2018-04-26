using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;

namespace SAPCodeUpdate.Features.SAPCodeUpdateJobFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("9e11a329-833e-4736-9594-215c2f2a1745")]
    public class SAPCodeUpdateJobFeatureEventReceiver : SPFeatureReceiver
    {

        // Uncomment the method below to handle the event raised after a feature has been activated.
        const string JOB_NAME = "SAPCodeUpdate";

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            Trace.WriteLine("SAPCodeUpdate Event - FeatureActivated");

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
                Trace.WriteLine("SAPCodeUpdate Event - Error with app/web/site");
                return;
            }



            // install the job
            Trace.WriteLine("SAPCodeUpdate: Creating Job");
            try
            {
                if (app.JobDefinitions.Count > 0)
                {
                    foreach (SPJobDefinition job in app.JobDefinitions)
                    {

                        if (job.Name == JOB_NAME)
                        {
                            Trace.WriteLine("SAPCodeUpdate: Deleting Job:" + job.Name);
                            job.Delete();
                            app.Update();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("catch SAPCodeUpdate: " + ex.Message + ex.InnerException + ex.Source);
            }


            SAPCodeUpdateTimerJob Job = new SAPCodeUpdateTimerJob(JOB_NAME, app);

            SPDailySchedule schedule = new SPDailySchedule();

            schedule.BeginHour = 20;
            schedule.BeginMinute = 0;

            Job.Schedule = schedule;

            Job.Update();

            Trace.WriteLine("SAPCodeUpdate: Created Job: " + Job.Name);


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
                Trace.WriteLine("SAPCodeUpdate Event - Error with app/web/site");
                return;
            }



            if (app.JobDefinitions.Count > 0)
            {
                foreach (SPJobDefinition job in app.JobDefinitions)
                {
                    if (job.Name == JOB_NAME)
                    {
                        Trace.WriteLine("SAPCodeUpdate: job deleted" + job.Name);
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
