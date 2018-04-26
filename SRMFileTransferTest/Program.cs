using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using SRMFileTransfer;
using CMSCommon;

namespace SRMFileTransferTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SPSite siteCollection = new SPSite("http://ecms-dev"))
            {

                SPWebApplication webApplication = siteCollection.WebApplication;


                SPList list  = siteCollection.RootWeb.Lists["Purchasing Request Forms"];

                //true
           //     SPListItem item = list.Items.GetItemById(77);
           //     bool amend = PurchasingForm.isAmendment(item);

                //false
          //      item = list.Items.GetItemById(75);
          //      amend = PurchasingForm.isAmendment(item);

              SRMFileTransfer.Utils.GenerateSRMFiles(webApplication);

            }
        }
    }
}
