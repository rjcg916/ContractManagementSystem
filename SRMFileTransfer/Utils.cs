using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Security.Principal;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using CMSCommon;

namespace SRMFileTransfer
{
    public class Utils
    {


        public static void GenerateSRMFiles(SPWebApplication webApplication)
        {
            string PURCHASINGSTATUSFIELD = "Purchasing_x0020_Status";
            string PURCHASINGTOSRM = "SentTOSRM";
            string ISSUBMITTEDFIELDNAME = "Is Submitted";

            using (SPSite site = webApplication.Sites[0])
            {

                SPList configList = null;
                SPList theLibrary = null;
                try
                {
                    SPWeb web = site.RootWeb;
                    configList = web.Lists[Constants.SITECONFIGLIST];
                    theLibrary = web.Lists[Constants.PURCHASINGFORMLIBRARYNAME];
                }
                catch (Exception ex) {
                    Trace.WriteLine("GenerateSRMFiles: ConfigList/PurchasingLibrary Error " + ex.ToString());
                }


                SPFieldBoolean boolIsSubmitted = null; 
                    
                try {
                    boolIsSubmitted = theLibrary.Fields.GetField(ISSUBMITTEDFIELDNAME) as SPFieldBoolean;
                } 
                catch (Exception ex)
                {
                    Trace.WriteLine("GenerateSRMFiles: IsSubmittedField Error " + ex.ToString());
                }

                // build the file
                StringBuilder SRMContents = new StringBuilder();

                //Trace.WriteLine("GenerateSRMFiles: Starting Search for Submittals" );

                foreach (SPListItem form in theLibrary.Items)
                {
                    // select file
                    if ( (form[ISSUBMITTEDFIELDNAME] != null) && (form[PURCHASINGSTATUSFIELD] != null) )
                    {
                        string status = String.Empty;
                        if (!String.IsNullOrEmpty(form[PURCHASINGSTATUSFIELD].ToString()))
                           status = form[PURCHASINGSTATUSFIELD].ToString();

                        bool isSubmitted = false;
                        if (!String.IsNullOrEmpty(form[ISSUBMITTEDFIELDNAME].ToString()))
                            isSubmitted = (bool)boolIsSubmitted.GetFieldValue(form[ISSUBMITTEDFIELDNAME].ToString());


                        //only update purchases submitted but not yet sent to SRM
                        if ((isSubmitted) && (String.Compare(status, PURCHASINGTOSRM) != 0) ) 
                        {

                             // new, in process, complete, other                            
                             //add contents of form to file buffer

                                string theFile = PurchasingForm.CreateSRMContents(form);

                                bool forTransfer = true;
                                forTransfer = !PurchasingForm.isAmendment(form);

                                if (!string.IsNullOrEmpty(theFile) && forTransfer)
                                {
                                    if (SRMContents.Length > 0)
                                        SRMContents.Append(System.Environment.NewLine);

                                    SRMContents.Append(theFile);

                                }

                                try
                                {
                                    //mark this form as complete, so SRM file will not be duplicated
                                    form[PURCHASINGSTATUSFIELD] = theLibrary.Fields.GetField(PURCHASINGSTATUSFIELD).GetFieldValue(PURCHASINGTOSRM);
                                    form.Update();
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine("Generate SRM File: UpdateStatus Error: " + ex.ToString());
                                }
                        }
                    }

                }

                //write the file containing all forms ready for submission
                if (SRMContents.Length > 0)
                {
                    //write the file using dedicated login
                    CMSCommon.Impersonate imp = new Impersonate();

                    string login = CMSCommon.Utils.GetConfigSetting(configList, Constants.SAPCMSLOGINTITLE);
                    string pw = CMSCommon.Utils.GetConfigSetting(configList, Constants.SAPCMSPASSWORDTITLE);

                    if (imp.impersonateValidUser(login, "ecorp", pw))
                    {                        
                        string SRMDirectory = CMSCommon.Utils.GetConfigSetting(configList, Constants.SRMFOLDERTITLE);

                        //generate SRMFileName name
                        string SRMFileName = "contract_" + //DateTime.Now.Year + DateTime.Now.Month.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                        DateTime.Now.ToString("yyyy MM dd", CultureInfo.InvariantCulture).Replace(" ", "") +
                        DateTime.Now.ToString("hh mm", CultureInfo.InvariantCulture).Replace(" ", "") + ".txt";

                        try
                        {
                            //create the file
                            File.WriteAllText(SRMDirectory + SRMFileName, SRMContents.ToString());
                            //given success: mark all included forms as submitted
                        }
                        catch (Exception ex) {
                            //if file found, delete it
                            Trace.WriteLine("WriteAllText: SRMDirectory/File: " + SRMDirectory + " " + SRMFileName + " " + ex.ToString());                           
                        }
                        finally
                        {
                            imp.undoImpersonation();
                        }
                    }

                }

            }

        }


    }
}
