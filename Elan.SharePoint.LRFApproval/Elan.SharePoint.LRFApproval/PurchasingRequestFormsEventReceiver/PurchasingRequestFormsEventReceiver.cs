using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Elan.SharePoint.LRFApproval.Properties;
using Elan.SharePoint.LRFApproval.Common;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace Elan.SharePoint.LRFApproval.PurchasingRequestFormsEventReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class PurchasingRequestFormsEventReceiver : SPItemEventReceiver
    {
        SPListItem currentItem;
        public SPListItem CurrentItem
        {
            get { return currentItem; }
            set { currentItem = value; }
        }

        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
        }

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
        }

        #region comments
        //string currentItemID = string.Empty;
        //if (properties != null && properties.List != null)
        //{
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //    {
        //        if (properties.List.Title == Settings.Default.ListTitlePurchasingRequests)
        //        {
        //            using (SPWeb web = properties.OpenWeb())
        //            {

        //                //this.EventFiringEnabled = false;
        //                //CurrentItem = properties.ListItem;

        //                //int requestAmount = 0;
        //                //string costCenterName = string.Empty;
        //                //string costCenterNumber = string.Empty;

        //                //SPUser requestor = null;

        //                //if (CurrentItem[Settings.Default.FieldLRFFormRequestAmount] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRFFormRequestAmount].ToString()))
        //                //    requestAmount = CommonEventReceiver.MakeInt(CurrentItem[Settings.Default.FieldLRFFormRequestAmount].ToString());

        //                //if (CurrentItem[Settings.Default.FieldLRRequestor] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRRequestor].ToString()))
        //                //{
        //                //    string requestorusername = CurrentItem[Settings.Default.FieldLRRequestor].ToString();
        //                //    requestor = web.EnsureUser(requestorusername);
        //                //}
        //                //if (requestor == null)
        //                //{
        //                //    CommonEventReceiver.WriteLogEntry(currentItem, "Error: Invalid or Missing Required field: Requestor", "Missing Required field: Requestor.");
        //                //    return;
        //                //}
        //                //costCenterNumber = CommonEventReceiver.GetRequestorsCostCenterNumber(web, requestor);
        //                //if (string.IsNullOrEmpty(costCenterNumber))
        //                //{
        //                //    CommonEventReceiver.WriteLogEntry(currentItem, "Error: Could not locate Requestor's Cost Center Number", "Missing Requestor's Cost Center Number.");
        //                //    return;
        //                //}
        //                //else
        //                //{
        //                //    costCenterName = CommonEventReceiver.GetCostCenterName(web, costCenterNumber, currentItem);
        //                //    if (string.IsNullOrEmpty(costCenterName)) return;
        //                //}

        //                //StringBuilder sb = new StringBuilder();
        //                //sb.AppendLine(CreateHeaderLine(costCenterNumber, web));

        //            }
        //        }
        //    });
        //}
        #endregion
        //private bool WriteSRMTextFile()
        //{
        //    bool executed = false;

        //    string[] lines = { "First line", "Second line", "Third line" };
        //    string fileName = "contract_" + //DateTime.Now.Year + DateTime.Now.Month.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
        //    DateTime.Now.ToString("yyyy MM dd", CultureInfo.InvariantCulture).Replace(" ", "") +
        //    DateTime.Now.ToString("hh mm", CultureInfo.InvariantCulture).Replace(" ", "") + ".txt";

        //    // Example #2: Write one string to a text file.
        //    string text = "A class is the most powerful data type in C#. Like structures, " +
        //                   "a class defines the data and behavior of the data type. ";
        //    System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", text);

        //    //Output (to WriteText.txt):
        //    //   A class is the most powerful data type in C#. Like structures, a class defines the data and behavior of the data type.

        //    // Example #3: Write only some strings in an array to a file.
        //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\WriteLines2.txt"))
        //    {
        //        foreach (string line in lines)
        //        {
        //            if (line.Contains("Second") == false)
        //            {
        //                file.WriteLine(line);
        //            }
        //        }
        //        // Output to WriteLines2.txt after Example #3:
        //        //First line
        //        //Third line
        //    }

        //    // Example #4: Append new text to an existing file
        //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\WriteLines2.txt", true))
        //    {
        //        file.WriteLine("Fourth line");
        //        //  Output to WriteLines2.txt after Example #4:
        //        //First line
        //        //Third line
        //        //Fourth line
        //    }

        //    return executed;
        //    /* 

        //     */
        //}

        //private string CreateHeaderLine(string costCenterNumber, SPWeb web)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    int requestAmount = -1;
        //    SPUser requestor = null;
        //    string contractNumber = string.Empty;
        //    string lrfTitle = string.Empty;
        //    string supplier = string.Empty;
        //    string expirationDate = string.Empty;
        //    string effectiveDate = string.Empty;
        //    string totalAmount = string.Empty;
        //    string currency = "USD";
        //    string costCenter = string.Empty;


        //    if (CurrentItem[Settings.Default.FieldLRFFormRequestAmount] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRFFormRequestAmount].ToString()))
        //        requestAmount = CommonEventReceiver.MakeInt(CurrentItem[Settings.Default.FieldLRFFormRequestAmount].ToString());

        //    if (CurrentItem[Settings.Default.FieldLRRequestor] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRRequestor].ToString()))
        //    {
        //        string requestorusername = CurrentItem[Settings.Default.FieldLRRequestor].ToString();
        //        requestor = web.EnsureUser(requestorusername);
        //    }
        //    if (requestor == null)
        //    {
        //        CommonEventReceiver.WriteLogEntry(currentItem, "Error: Invalid or Missing Required field: Requestor", "Missing Required field: Requestor.");
        //        //return "";
        //    }
        //    else
        //    {
        //        //costCenterNumber = CommonEventReceiver.GetRequestorsCostCenterNumber(web, requestor);
        //        //if (!string.IsNullOrEmpty(costCenterNumber))
        //        //    costCenterNumber = costCenterNumber.Substring(0, 2) + "00";
        //    }



        //    if (CurrentItem[Settings.Default.FieldAgreementContractNumber] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRRequestor].ToString()))
        //    {
        //        contractNumber = CurrentItem[Settings.Default.FieldAgreementContractNumber].ToString();
        //    }

        //    if (!string.IsNullOrEmpty(costCenterNumber) && costCenterNumber.Length > 1)
        //        costCenterNumber = costCenterNumber.Substring(0, 2) + "00";
        //    else
        //        costCenterNumber = "0000";

        //    if (CurrentItem[Settings.Default.FieldAgreementContractNumber] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldLRRequestor].ToString()))
        //    {
        //        contractNumber = CurrentItem[Settings.Default.FieldAgreementContractNumber].ToString();
        //    }

        //    if (CurrentItem[Settings.Default.FieldAgreementLRFTitle] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldAgreementLRFTitle].ToString()))
        //    {
        //        lrfTitle = CurrentItem[Settings.Default.FieldAgreementLRFTitle].ToString();
        //    }


        //    if (CurrentItem[Settings.Default.FieldAgreementSupplier] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldAgreementSupplier].ToString()))
        //    {
        //        supplier = CurrentItem[Settings.Default.FieldAgreementSupplier].ToString();
        //    }

        //    if (CurrentItem[Settings.Default.FieldAgreementEffectiveDate] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldAgreementEffectiveDate].ToString()))
        //    {
        //        effectiveDate = DateTime.Parse(CurrentItem[Settings.Default.FieldAgreementEffectiveDate].ToString()).ToShortDateString();
        //    }
        //    if (CurrentItem[Settings.Default.FieldAgreementExpirationDate] != null && !string.IsNullOrEmpty(CurrentItem[Settings.Default.FieldAgreementExpirationDate].ToString()))
        //    {
        //        expirationDate = DateTime.Parse(CurrentItem[Settings.Default.FieldAgreementExpirationDate].ToString()).ToShortDateString();
        //    }


        //    sb.Append(System.Environment.NewLine);
        //    // Header Line start
        //    sb.Append("H");
        //    sb.Append("\t");
        //    sb.Append(costCenterNumber);
        //    sb.Append("\t");
        //    sb.Append(contractNumber);
        //    sb.Append("\t");
        //    sb.Append(lrfTitle);
        //    sb.Append("\t");
        //    sb.Append(requestor.LoginName.Substring(requestor.LoginName.IndexOf("\\")));
        //    sb.Append("\t");
        //    sb.Append(requestor.Email);
        //    sb.Append("\t");
        //    sb.Append(web.CurrentUser.LoginName.Substring(requestor.LoginName.IndexOf("\\")));
        //    sb.Append("\t");
        //    sb.Append(web.CurrentUser.Email);
        //    sb.Append("\t");
        //    sb.Append(supplier);
        //    sb.Append("\t");
        //    sb.Append(effectiveDate);
        //    sb.Append("\t");
        //    sb.Append(expirationDate);
        //    sb.Append("\t");

        //    //Iterate through cost centers and create a header line for each


        //    //sb.Append(costCenterNumber);
        //    //sb.Append("\t");
        //    //sb.AppendFormat("\t\t");
        //    //sb.Append("More Text");
        //    //sb.AppendFormat("\r\n");


        //    return sb.ToString();
        //}

        //private void ReadForm(SPFile myForm)
        //{

        //    XmlTextReader textReader = new XmlTextReader(myForm.OpenBinaryStream());
        //    textReader.WhitespaceHandling = WhitespaceHandling.None;
        //    textReader.Read();
        //    // If the node has value

        //    while (textReader.Read())
        //    {

        //    }


        //    XmlDocument myDoc = new XmlDocument();
        //    myDoc.Load(@"C:\RequestForInvestment.xml");
        //    XmlNodeList nl = myDoc.SelectNodes("//FileAttachment");
        //    foreach (XmlNode n in nl)
        //    {
        //        string s = n.InnerText;

        //        //Now that we got the content of the attachment node, it will be decoded using Convert.FromBase64String():

        //        byte[] b = Convert.FromBase64String(s);

        //        //Before the real filecontent starts there is a header in the Base64-string, 
        //        //including the original filename, 
        //        //length of the file and so on. So lets start with parsing he header:

        //        //At position 20 there is a DWORD containing the length of the filename buffer.
        //        //Due to the filename is stored as Unicode the length is multiplied by 2. 
        //        //At position 24 in the header the filename starts, which is read in the for-loop.

        //        int namebufferlen = b[20] * 2;
        //        byte[] filenameBuffer = new byte[namebufferlen];
        //        for (int i = 0; i <= namebufferlen; i++)
        //        {
        //            filenameBuffer[i] = b[24 + i];
        //        }

        //        //The byte[] gets converted to a string via an Unicode char[]. 
        //        //In this sample I get one \0 char at the end of the string so that is why using the
        //        //Substring() on the filename (Sorry, just to get it running quickly...).

        //        char[] asciiChars = UnicodeEncoding.Unicode.GetChars(filenameBuffer);
        //        string filename = new string(asciiChars);
        //        filename = filename.Substring(0, filename.Length - 1);


        //        //Now we can read the real file content from the buffer and save it as file:

        //        byte[] filecontent = new byte[b.Length - (24 + namebufferlen)];
        //        for (int i = 0; i < filecontent.Length; i++)
        //        {
        //            filecontent[i] = b[24 + namebufferlen + i];
        //        }
        //        FileStream fs = new FileStream(@"C:\" + filename, FileMode.Create);
        //        fs.Write(filecontent, 0, filecontent.Length);
        //        fs.Close();
        //    }


        //    //            // Get a reference to the root node of the InfoPath form.
        //    // XPathNavigator domNav = MainDataSource.CreateNavigator();

        //    // // Read the entire XML of the InfoPath form into an XMLReader 
        //    //// object.
        //    // XmlReader reader = domNav.ReadSubtree();
        //    // reader.MoveToContent();

        //    //// Deserialize the InfoPath form into an expenseReport object.
        //    // XmlSerializer xser = new XmlSerializer(typeof(expenseReport));
        //    // expenseReport expRep = (expenseReport)xser.Deserialize(reader);


        //}

    }
}
