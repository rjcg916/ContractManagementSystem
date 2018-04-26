using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Elan.SharePoint.LRFApproval.Properties;

namespace Elan.SharePoint.LRFApproval.LRF_Request_Status
{
    public partial class LRF_Request_StatusUserControl : UserControl
    {
        private string queryStringParameter;
        public string QueryStringParameter
        {
            get { return queryStringParameter; }
            set { queryStringParameter = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            pnlStatus.Visible = true;

            if (string.IsNullOrEmpty(QueryStringParameter))
            {
                lblMessage.Text = "Error: Missing Query string parameter name, configure the webpart to add the Query string name for the LRF form ID.";
                lblMessage.Visible = true;
                pnlStatus.Visible = false;
                pnlCancelled.Visible = false;
                return;
            }


            if (Request.QueryString[queryStringParameter] == null || string.IsNullOrEmpty(Request.QueryString[queryStringParameter]))
            {
                lblMessage.Text = "Error: Missing Query string parameter value";
                lblMessage.Visible = true;
                pnlStatus.Visible = false;
                pnlCancelled.Visible = false;
                return;
            }

            if (!Page.IsPostBack)
            {
                string formID = Request.QueryString[queryStringParameter].ToString();
                using (SPWeb web = SPContext.Current.Site.OpenWeb())
                {
                    SPList lstForms = web.Lists.TryGetList(Settings.Default.ListTitleLRF);
                    if (lstForms == null)
                    {
                        lblMessage.Text = "Error: Could not locate Legal Request Forms List in the Root Site";
                        lblMessage.Visible = true;
                        pnlStatus.Visible = false;
                        pnlCancelled.Visible = false;
                        return;
                    }

                    SPListItem item = lstForms.Items.GetItemById(Convert.ToInt32(formID));
                    if (item == null)
                    {
                        lblMessage.Text = "Error: Could not locate LRF with ID: " + formID;
                        lblMessage.Visible = true;
                        pnlStatus.Visible = true;
                        pnlCancelled.Visible = false;
                        return;
                    }

                    string currentStatus = string.Empty;

                    // if can't find a status, display "lrf created" as default
                    if (item[Settings.Default.FieldRequestStatus] == null || string.IsNullOrEmpty(item[Settings.Default.FieldRequestStatus].ToString()))
                    {
                        currentStatus = "lrf created";
                    }
                    else
                    {
                        currentStatus = item[Settings.Default.FieldRequestStatus].ToString();
                        if (currentStatus.IndexOf("#") > -1)
                            currentStatus = currentStatus.Substring(currentStatus.IndexOf("#") + 1);
                    }

                    if (!string.IsNullOrEmpty(currentStatus))
                        SetCurrentStatus(currentStatus.ToLower().Trim());
                }
            }
        }

        private void SetCurrentStatus(string currentStatus)
        {
            string onImageUrlAssigned = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeAssignedOn.png";
            string onImageUrlCreate = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeCreateOn.png";
            string onImageUrlDptApproval = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeDptApprovalOn.png";
            string onImageUrlDptApproved = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeDptApprovedOn.png";
            string onImageUrlExecuted = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeExecutedOn.png";
            string onImageUrlFinanceApproval = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeFinanceApprovalOn.png";
            string onImageUrlFinanceApproved = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeFinanceApprovedOn.png";
            string onImageUrlSubmit = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeSubmitOn.png";
            string onImageUrlCancelled = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeLrfCancelledOn.png";
            string onImageUrlRejected = "~/_layouts/images/Elan.SharePoint.LRFApproval/LifeLrfRejectedOn.png";
 
            pnlCancelled.Visible = false;
            switch (currentStatus)
            {
                // Legal Review
                case "lrf created":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    break;

                case "lrf submitted":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    break;

                case "lrf departmental approval":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    break;

                case "department approved":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    imgDeptApproved.ImageUrl = onImageUrlDptApproved;
                    break;

                case "lrf finance approval":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    imgDeptApproved.ImageUrl = onImageUrlDptApproved;
                    imgLrfFinanceApproval.ImageUrl = onImageUrlFinanceApproval;
                    break;

                case "finance approved":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    imgDeptApproved.ImageUrl = onImageUrlDptApproved;
                    imgLrfFinanceApproval.ImageUrl = onImageUrlFinanceApproval;
                    imgFinanceApproved.ImageUrl = onImageUrlFinanceApproved;

                    break;

                case "attorney/paralegal assigned":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    imgDeptApproved.ImageUrl = onImageUrlDptApproved;
                    imgLrfFinanceApproval.ImageUrl = onImageUrlFinanceApproval;
                    imgFinanceApproved.ImageUrl = onImageUrlFinanceApproved;
                    imgAttorneyAssigned.ImageUrl = onImageUrlAssigned;
                    break;

                case "fully executed contract":
                    imgLRFCreation.ImageUrl = onImageUrlCreate;
                    imgLRFSubmitted.ImageUrl = onImageUrlSubmit;
                    imgLRFDeptApproval.ImageUrl = onImageUrlDptApproval;
                    imgDeptApproved.ImageUrl = onImageUrlDptApproved;
                    imgLrfFinanceApproval.ImageUrl = onImageUrlFinanceApproval;
                    imgFinanceApproved.ImageUrl = onImageUrlFinanceApproved;
                    imgAttorneyAssigned.ImageUrl = onImageUrlAssigned;
                    imgFullyExecuted.ImageUrl = onImageUrlExecuted;
                    break;

                case "lrf cancelled":
                    pnlStatus.Visible = false;
                    imgLrfCancelled.ImageUrl = onImageUrlCancelled;
                    lblCancelledRejected.Text = "LRF Cancelled";
                    pnlCancelled.Visible = true;
                    break;

                case "lrf rejected":
                    pnlStatus.Visible = false;
                    imgLrfCancelled.ImageUrl = onImageUrlRejected;
                    lblCancelledRejected.Text = "LRF Rejected";
                    pnlCancelled.Visible = true;
                    break;
            }
        }
    }
}
