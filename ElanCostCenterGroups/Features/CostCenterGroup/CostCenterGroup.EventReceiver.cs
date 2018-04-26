using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;
using Microsoft.Office.Server.UserProfiles;
using CMSCommon;


namespace Elan.Features.CostCenterGroup
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("fb7a2c16-3de9-4b32-b0db-832247d880d6")]
    public class CostCenterGroupEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        const string JOB_NAME = "ElanCostCenterGroups";
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {

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
                Trace.WriteLine("CostCenterGroup Event - Error with app/web/site");
                return;
            }

               
            // install the job
            Trace.WriteLine("CostCenterGroup: Creating Job");
            try {
            if (app.JobDefinitions.Count > 0)
            {
                foreach (SPJobDefinition job in app.JobDefinitions)
                {

                    if (job.Name == JOB_NAME)
                    {
                        Trace.WriteLine("CostCenterGroup: Deleting Job:" + job.Name);
                        job.Delete();
                        app.Update();
                    }
                }
            } }
            catch (Exception ex) {
                Trace.WriteLine("CostCenterGroup: " + ex.Message + ex.InnerException + ex.Source);
            }
            

            CostCenterGroupsJob Job = new CostCenterGroupsJob(JOB_NAME, app);

            SPDailySchedule schedule = new SPDailySchedule();

            schedule.BeginHour = 1;
            schedule.BeginMinute = 1;
            schedule.EndHour = 2;
            schedule.EndMinute = 1;

            Job.Schedule = schedule;

            Job.Update();

            Trace.WriteLine("CostCenterGroup: Created Job: " + Job.Name);

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
                Trace.WriteLine("CostCenterGroup Event - Error with app/web/site");
                return;
            }


            if (app.JobDefinitions.Count > 0)
            {
                foreach (SPJobDefinition job in app.JobDefinitions)
                {
                    if (job.Name == JOB_NAME)
                    {
                        Trace.WriteLine("CostCenterGroup: job deleted" + job.Name);
                        job.Delete();
                        app.Update();
                    }
                }
            }




        }

        //public override void FeatureActivated(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


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
