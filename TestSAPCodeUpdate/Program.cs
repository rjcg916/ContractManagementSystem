using System;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.SharePoint;
using SAPCodeUpdate;
using CMSCommon;

namespace SAPCodeUpdateTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SPSite siteCollection = new SPSite(Constants.CMSDEVURL))
            {
                SAPCodeUpdate.Utils.SAPCodeUpdate(siteCollection.WebApplication);
            }

            Console.Write("Press ENTER to continue");
            Console.ReadLine();

        }
    }
}
