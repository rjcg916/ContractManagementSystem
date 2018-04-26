using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace Elan.SharePoint.LRFApproval.LRFWizard
{
    [ToolboxItemAttribute(false)]
    public class LRFWizard : WebPart
    {

        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            // display audience trim links here
            HyperLink hlRequest = new HyperLink();
            hlRequest.Text = "Elan Request Form";
            hlRequest.NavigateUrl = "http://ecms-val/_layouts/FormServer.aspx?XsnLocation=http://ecms-val/FormServerTemplates/Elan%20LRF.xsn&SaveLocation=http://ecms-val/Legal%20Request%20Forms&Source=http://ecms-val/&Company=Elan&DefaultItemOpen=1";
            this.Controls.Add(hlRequest);

            this.Controls.Add(new LiteralControl("<br/>"));


            // display all previous request

            
            Label label1 = new Label();
            label1.Text = "Previous Requests";
            this.Controls.Add(label1);
            
            this.Controls.Add(new LiteralControl("<br/>"));

            DropDownList ddlLRFs = new DropDownList();
            
            ListItem anLRF1 = new ListItem("LRF 1");
            ddlLRFs.Items.Add(anLRF1);

            ListItem anLRF2 = new ListItem("LRF 2");
            ddlLRFs.Items.Add(anLRF2);

            this.Controls.Add(ddlLRFs);

            this.Controls.Add(new LiteralControl("<br/>"));

            //display all executed agreements
            Label label2 = new Label();
            label2.Text = "Executed Agreements";
            this.Controls.Add(label2);

            this.Controls.Add(new LiteralControl("<br/>"));


            DropDownList ddlAgreements = new DropDownList();

            ListItem anAgreement1 = new ListItem("Agreement 1");
            ddlAgreements.Items.Add(anAgreement1);

            ListItem anAgreement2 = new ListItem("Agreement 2");
            ddlAgreements.Items.Add(anAgreement2);
            this.Controls.Add(ddlAgreements);


        }
    }
}
