using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
namespace Elan.SharePoint.LRFApproval.LRF_Request_Status
{
    [ToolboxItemAttribute(false)]
    public class LRF_Request_Status : WebPart
    {
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/Elan.SharePoint.LRFApproval/LRF Request Status/LRF Request StatusUserControl.ascx";
        private String queryStringParameterName = String.Empty;

        protected override void CreateChildControls()
        {
            LRF_Request_StatusUserControl control = (LRF_Request_StatusUserControl)Page.LoadControl(_ascxPath);
            control.QueryStringParameter = queryStringParameterName;
            Controls.Add(control);
        }

        [
         WebBrowsable(true),
         WebDisplayName("Query String Name"),
         WebDescription(""),
         Personalizable(
         PersonalizationScope.Shared),
         Category("Settings"),
         DefaultValue("")
        ]
        public string QueryStringParameter
        {
            get { return queryStringParameterName; }
            set { queryStringParameterName = value; }
        }


        //[ConnectionProvider("Provider for string from TextBox", "TextBoxStringProvider")]
        //public ITextBoxString TextBoxStringProvider()
        //{
        //    return this;
        //}
    }
}
