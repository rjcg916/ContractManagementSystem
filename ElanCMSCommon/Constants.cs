using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace CMSCommon
{
    public class Constants
    {

 
        public const string ALLOWEDREQUESTORSLIST = "AllowedRequestorsForUser";
        public const string ALLOWEDREQUESTORLISTUSER = "User";
        public const string ALLOWEDREQUESTORLISTREQUESTOR = "Requestor";

        //site configuration parameters
        public const string SITECONFIGLIST = "SiteConfig";
        public const string SAPMASTERDBTITLE = "SAPMasterDB";
        public const string SAPMASTERDBSERVERTITLE = "SAPMasterDBServer";
        public const string SAPMASTERFOLDERTITLE = "SAPMasterFolder";
        public const string SAPCMSLOGINTITLE = "SAPCMSLogin";
        public const string SAPCMSPASSWORDTITLE = "SAPCMSPassword";

        public const string SRMFOLDERTITLE = "SRMFolder";
    
        //user profile values
        public const string SAPUSERNAMEFIELD = "SapUserName";
        public const string CMSPROFILEDEPTNUM = "DeptNum";
        public const string CMSPROFILECOSTCENTER = "CostCenter";

        //list and libraries
        public const string PURCHASINGFORMLIBRARYNAME = "Purchasing Request Forms";
        public const string LEGALREQUESTFORMSLIBRARYNAME = "Legal Request Forms";
        public const string CMSUSERPROFILELIST = "CmsUserProfile";

        //cost center 

        public const int COSTCENTERLENGTH = 5;
        public const int MINCOSTCENTER = 10000;
        public const int MAXCOSTCENTER = 99999;

        public const string COSTCENTERPREFIX = "CostCenter";
        public const string COSTCENTERPROFILEPREFIX = "CostCenterProfile_";
        public const string COSTCENTERPROFILEDESC = "Cost Center Profile Group: This group's membership is managed by a timer job.";
        public const string COSTCENTERUSERPREFIX = "CostCenterUser_";
        public const string COSTCENTERUSERDESC = "Cost Center User Group: Add users to this group who are NOT in the cost center.";
        public const string COSTCENTERSUPERUSERPREFIX = "CostCenterSuperUser_";
        public const string COSTCENTERSUPERUSERDESC = "Cost Center Super User Group: Add users to this group who should have Super User access for the cost center.";

        //CMS Group Names
        public const string CMSFINANCEGROUP = "Finance Team";
        public const string CMSOWNERGROUP = "Contract Management System Owners";
        
       
        //URLS 
        public const string CMSDEVURL = "http://ecms-dev/";
        public const string CMSVALURL = "http://ecms-val/";
        public const string CMSPRDURL = "http://ecms/";
    }
}
