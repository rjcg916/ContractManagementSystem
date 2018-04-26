using System;
using System.Web.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using CMSCommon;

namespace Elan.MyCostCenterPermissions
{

    public partial class MyCostCenterPermissionsUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                //// store cost codes in dictionaries (eliminate duplicates)
                var LRFCostCodes = new StringDictionary();
                var AgreementCostCodes = new StringDictionary();


                // check all groups of which user is a member
                SPWeb web = SPContext.Current.Web;
                SPUser theUser = web.CurrentUser;

                System.Diagnostics.Trace.WriteLine("MyCostCenterPermissions for: " + theUser.Name);
                foreach (SPGroup group in theUser.Groups)
                {
                    System.Diagnostics.Trace.WriteLine("Checking Group: " + group.Name);

                    /*if (group.Name.StartsWith(Constants.COSTCENTERPROFILEPREFIX))
                    {
                        System.Diagnostics.Trace.WriteLine("Found Profile Group: " + group.Name);
                        string strProfile = group.Name.Substring((Constants.COSTCENTERPROFILEPREFIX.Length));
                        LRFCostCodes[strProfile] = strProfile;
                    }

                    if (group.Name.StartsWith(Constants.COSTCENTERUSERPREFIX))
                    {
                        System.Diagnostics.Trace.WriteLine("Found User Group: " + group.Name);
                        string strUser = group.Name.Substring((Constants.COSTCENTERUSERPREFIX.Length));
                        LRFCostCodes[strUser] = strUser;
                    }*/


                    if (group.Name.StartsWith(Constants.COSTCENTERSUPERUSERPREFIX))
                    {
                        System.Diagnostics.Trace.WriteLine("Found SuperUser Group: " + group.Name);
                        string strSuperUser = group.Name.Substring((Constants.COSTCENTERSUPERUSERPREFIX.Length));
                        AgreementCostCodes[strSuperUser] = strSuperUser;
                        LRFCostCodes[strSuperUser] = strSuperUser;
                    }

                }

                blLRFs.DataSource = LRFCostCodes;
                blLRFs.DataValueField = "key";
                blLRFs.DataTextField = "value";
                blLRFs.DataBind();

                pnlLRFs.Visible = false;
                if (LRFCostCodes.Count > 0)
                {
                    pnlLRFs.Visible = true;
                }


                blAgreements.DataSource = AgreementCostCodes;
                blAgreements.DataValueField = "key";
                blAgreements.DataTextField = "value";
                blAgreements.DataBind();

                pnlAgreements.Visible = false;
                if (AgreementCostCodes.Count > 0)
                {
                    pnlAgreements.Visible = true;
                }


            }
        }
    }
}
