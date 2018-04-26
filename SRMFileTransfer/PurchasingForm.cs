using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microsoft.SharePoint;

namespace SRMFileTransfer
{
    public class PurchasingForm
    {

        public static bool isAmendment(SPListItem theForm)
        {
            const string contractNumberField = "Contract Number";
            const string AMENDELIM = "-";
            const int    AMENDLEN = 12;

            bool isAmendment = false;

            //Amendment has last delimiter occurence after len

            if ( (theForm[contractNumberField] != null) &&
                 (!String.IsNullOrEmpty(theForm[contractNumberField].ToString() )) )
                isAmendment = theForm[contractNumberField].ToString().LastIndexOf(AMENDELIM) == AMENDLEN;

            return isAmendment;
        }
       
        public static string CreateSRMContents(SPListItem theForm)
        {

           // Trace.WriteLine("CreateSRMContents: Starting");
            //get the form data into an XMLDocument

            MemoryStream oMemoryStream = new MemoryStream(theForm.File.OpenBinary());
            XmlTextReader oReader = new XmlTextReader(oMemoryStream);

            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(oReader);

            oReader.Close();
            oMemoryStream.Close();

            // set namespace for InfoPath
            XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(oDoc.NameTable);
            NamespaceManager.AddNamespace("my", "http://schemas.microsoft.com/office/infopath/2003/myXSD/2011-10-05T00:10:10");

            //get header values
            //         string totalAmount = oDoc.DocumentElement.SelectSingleNode("my:TotalAmount", NamespaceManager).InnerText;
            string reviewCostAssignment = oDoc.DocumentElement.SelectSingleNode("my:ReviewCostAssignment", NamespaceManager).InnerText;
            string lrfNumber = oDoc.DocumentElement.SelectSingleNode("my:LRFNumber", NamespaceManager).InnerText;
            string lrfTitle = oDoc.DocumentElement.SelectSingleNode("my:LRFTitle", NamespaceManager).InnerText;
            string contractNumber = oDoc.DocumentElement.SelectSingleNode("my:ContractNumber", NamespaceManager).InnerText;
            string supplier = oDoc.DocumentElement.SelectSingleNode("my:SAPSupplierNumber", NamespaceManager).InnerText;
            string currency = oDoc.DocumentElement.SelectSingleNode("my:Currency", NamespaceManager).InnerText;

            string effectiveDate = oDoc.DocumentElement.SelectSingleNode("my:EffectiveDate", NamespaceManager).InnerText;
            if (!string.IsNullOrEmpty(effectiveDate))
                effectiveDate = DateTime.Parse(effectiveDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string expirationDate = oDoc.DocumentElement.SelectSingleNode("my:ExpirationDate", NamespaceManager).InnerText;
            if (!string.IsNullOrEmpty(expirationDate))
                expirationDate = DateTime.Parse(expirationDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);


            string requestor = oDoc.DocumentElement.SelectSingleNode("my:RequestorSAPUserNameHidden", NamespaceManager).InnerText.Replace("\t", "");
            //string requestor = oDoc.DocumentElement.SelectSingleNode("my:Requestor/pc:Person/pc:DisplayName", NamespaceManager).Value.Replace("\t", "");
            string creator = oDoc.DocumentElement.SelectSingleNode("my:creatorSAPUserNameHidden", NamespaceManager).InnerText.Replace("\t", "");
            string requestorEmail = oDoc.DocumentElement.SelectSingleNode("my:RequestorEmailHidden", NamespaceManager).InnerText;
            string creatorEmail = oDoc.DocumentElement.SelectSingleNode("my:CurrentUserEmailHidden", NamespaceManager).InnerText;

            string strNamespace = NamespaceManager.LookupNamespace("my");

            StringBuilder sb = new StringBuilder();
            int numGroupsEmitted = 0;

            // Loop through cost centers

            XmlNodeList selectedNodes = oDoc.DocumentElement.SelectNodes("my:group11/my:CostCenterCharge", NamespaceManager);

            foreach (XmlNode selectedNode in selectedNodes)
            {

                StringBuilder sbLineItem = new StringBuilder();
                decimal subTotal = 0; //keep track of sub-total of line items within cost center

                string costCenterHeaderNumber = string.Empty;
                string costCenter = string.Empty;
                XmlNodeList costcenteriterator = selectedNode.ChildNodes; // ("CostCenter", strNamespace);

                foreach (XmlNode node in costcenteriterator)
                {
                    if (node.Name == "my:CostCenter")
                        costCenter = node.InnerText;
                }

                if (!string.IsNullOrEmpty(costCenter))
                {
                    if (costCenter.Length > 1)
                        costCenterHeaderNumber = costCenter.Substring(0, 2) + "00";
                    else if (costCenter.Length == 1)
                        costCenterHeaderNumber = costCenter.Substring(0, 1) + "000";
                    else
                        costCenterHeaderNumber = "0000";
                }

                if (creator.IndexOf("|") > -1)
                    creator = creator.Substring(creator.IndexOf("|") + 1);


                if (requestor.IndexOf("|") > -1)
                    requestor = requestor.Substring(requestor.IndexOf("|") + 1);


                //fetch line item details
                string amount = string.Empty;
                string productCategory = string.Empty;
                string descrip = string.Empty;

                XmlNodeList CCChargeDetails = selectedNode.SelectNodes("my:CostCenterEntry/my:CostCenterDetails", NamespaceManager);
                foreach (XmlNode node in CCChargeDetails)
                {
                    XmlNodeList nodedetails = node.ChildNodes;
                    foreach (XmlNode nx in nodedetails)
                    {
                        XPathNavigator n = nx.CreateNavigator();

                        if (n.LocalName == "")
                            continue;
                        if (n.LocalName == "Amount")
                            amount = n.Value;
                        else if (n.LocalName == "ProductCategory")
                            productCategory = n.Value;
                        else if (n.LocalName == "Description")
                            descrip = n.Value;
                    }

                    sbLineItem.Append(System.Environment.NewLine);
                    sbLineItem.Append("L");
                    sbLineItem.Append("\t");
                    sbLineItem.Append(amount);
                    sbLineItem.Append("\t");
                    sbLineItem.Append(currency);
                    sbLineItem.Append("\t");
                    sbLineItem.Append(descrip);
                    sbLineItem.Append("\t");
                    sbLineItem.Append(productCategory);

                    //keep a running total for cost center line items
                    try
                    {
                        subTotal += Decimal.Parse(amount); //tally the line item sub-total
                    }
                    catch
                    {
                    }

                }

                // Header Line start

                StringBuilder sbHeader = new StringBuilder();
                sbHeader.Append("H");
                sbHeader.Append("\t");
                sbHeader.Append(costCenterHeaderNumber);
                sbHeader.Append("\t");
                sbHeader.Append(contractNumber);
                sbHeader.Append("\t");
                sbHeader.Append(lrfTitle);
                sbHeader.Append("\t");
                sbHeader.Append(requestor);
                sbHeader.Append("\t");
                sbHeader.Append(requestorEmail);
                sbHeader.Append("\t");
                sbHeader.Append(creator);
                sbHeader.Append("\t");
                sbHeader.Append(creatorEmail);
                sbHeader.Append("\t");
                sbHeader.Append(supplier);
                sbHeader.Append("\t");
                sbHeader.Append(effectiveDate);
                sbHeader.Append("\t");
                sbHeader.Append(expirationDate);
                sbHeader.Append("\t");
                sbHeader.Append(subTotal.ToString("G")); //decimal notation for cost center line item total
                sbHeader.Append("\t");
                sbHeader.Append(currency);
                sbHeader.Append("\t");
                sbHeader.Append(costCenter);

                //append header and current line item details
                //NOTE: header contains total derived from line items, so header and line item stored in different strings

                if (numGroupsEmitted > 0) //new line in between cost center headers
                    sb.Append(System.Environment.NewLine);
                sb.Append(sbHeader.ToString());
                sb.Append(sbLineItem.ToString());

                numGroupsEmitted += 1;
            }

          //  Trace.WriteLine("CreateSRMContents Done for " + theForm.Name);

            return sb.ToString();
        }

    }



}
