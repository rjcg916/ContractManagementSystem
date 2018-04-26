using System;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using CMSCommon;

namespace CMSCommonTest
{
    class Program
    {
        static void Main(string[] args)
        {

            using (SPSite site = new SPSite("http://ecms-dev/"))
            {
                SPWebCollection sites = site.AllWebs;

                SPWeb web = sites[0];



          //      CMSCommon.SecurityUtils.CreateSiteGroup(site, "Finance Team", "ATest", "New Test of GroupOwner");

          //      CMSCommon.SecurityUtils.CreateSiteGroup(site, "Contract Management System Owners", "BTest", "Test of GroupOwner");

                //  Console.WriteLine("MoveFileToDir: ");
                //  Utils.MoveToDir(@"C:\file share\", "Fred*",  @"C:\file share\archive\");

                //Console.WriteLine("GetLatestFile: " +  Utils.GetLatestFile(@"c:\file share\", "Bob*"));

                //Console.WriteLine("MoveFileToDir: ");
                //Utils.MoveToDir(@"C:\file share\thefile.txt", @"C:\file share\archive\");

                //  SPList configList = web.Lists[Constants.SITECONFIGLIST];
                //  Console.WriteLine("Fetch: " + Constants.SAPMASTERDBSERVERTITLE  + " value: " + Utils.GetConfigSetting(configList, Constants.SAPMASTERDBSERVERTITLE));
                //  Console.WriteLine("Fetch: " + Constants.SAPMASTERDBSERVERTITLE + " value: " + Utils.GetConfigSetting(configList, Constants.SAPMASTERDBTITLE));
                //  Console.WriteLine("Fetch: " + Constants.SAPMASTERDBSERVERTITLE + " value: " + Utils.GetConfigSetting(configList, Constants.SAPMASTERFOLDERTITLE));


                SPList configList = web.Lists[Constants.SITECONFIGLIST];

                //fetch account/password for CMS SAP DB
                string login = CMSCommon.Utils.GetConfigSetting(configList, "SAPCMSLogin");
                string pw = CMSCommon.Utils.GetConfigSetting(configList, "SAPCMSPassword");

                CMSCommon.Impersonate imp = new Impersonate();

                WindowsIdentity myIdent = WindowsIdentity.GetCurrent();

                if (imp.impersonateValidUser(login, "ecorp", pw))
                {
                    myIdent = WindowsIdentity.GetCurrent();

                    Console.WriteLine("Impersonating: " + myIdent.Name);

                    //Insert your code that runs under the security context of a specific user here.
                    Console.WriteLine("MoveFileToDir: ");

                    Utils.MoveToDir(@"\\sapr3-dn11\cms\", "costcenter*",  @"\\sapr3-dn11\cms\archive\");

                    imp.undoImpersonation();
                }
                //else
                //{
                //    //Your impersonation failed. Therefore, include a fail-safe mechanism here.
                //}
                myIdent = WindowsIdentity.GetCurrent();
                Console.WriteLine("Current User: " + myIdent.Name);

            }

            Console.Write("Press ENTER to continue");
            Console.ReadLine();

        }
    }
}
