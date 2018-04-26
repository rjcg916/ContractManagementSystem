using System;
using System.IO;
using System.Diagnostics;
using Microsoft.SharePoint;


namespace CMSCommon
{
    public class Utils
    {

        public static void MoveToDir(string sourceDirectory, string prefix, string destDirectory)
        {
            try
            {
                string[] files = Directory.GetFiles(sourceDirectory, prefix);

                foreach (string file in files)
                {
                    MoveToDir(file, destDirectory);
                }
            }
            catch (Exception ex) {
                Trace.WriteLine("catch MoveFilesToDir: sourceDirectory: " + sourceDirectory + " " + ex.ToString() );    
            }

        }

        public static void MoveToDir(string sourceFileName, string destDirectory)
        {
            try
            {
                Trace.WriteLine("MoveToDir: sourceFileName: " + sourceFileName + " destDirectory: " + destDirectory);

                string fileOnly = sourceFileName.Substring(sourceFileName.LastIndexOf(@"\") + 1);

                string destFile = destDirectory + fileOnly;

                //if file already exists in destination, delete it
                if (File.Exists(destFile))
                {
                    Trace.WriteLine("File.Exists(destFile): " + destFile);
                    File.Delete(destFile);
                }

                Trace.WriteLine("MoveToDir: source: " + sourceFileName + " destFile: " + destFile);

                //move file
                File.Move(sourceFileName, destFile);

            }
            catch (Exception ex)
            {
                Trace.WriteLine("catch MoveFileToDir: sourceFileName: " + sourceFileName + " " + ex.ToString());
            }

        }

        public static string GetLatestFile(string path, string prefix)
        {
            string fileLatest = String.Empty;

            DateTime dtLatest = new DateTime(2000, 11, 22); // arbitrary date in past

            try
            {
                string[] files = Directory.GetFiles(path, prefix);

                foreach (string file in files)
                {
                    DateTime dtFile = File.GetCreationTime(file);

                    if (dtFile > dtLatest)
                    {
                        dtLatest = dtFile;
                        fileLatest = file;
                    }
                }

                return fileLatest;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("catch: GetLatestFile: path/prefix: " + path + " " + prefix + " " + ex.ToString());
                return String.Empty;
            }

        }

        public static string GetConfigSetting(SPList configList, string index)
        {
            try
            {
                string value = null;

                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Title'/>" +
                      "<Value Type='Text'>" + index + "</Value></Eq></Where>";
                SPListItemCollection spItems = configList.GetItems(query);

                SPListItem entry = spItems[0];
                value = (string)entry["Value"];

               // Trace.WriteLine(String.Format("GetConfigSetting: list: {0} index: {1} value: {2}", configList, index, value));

                return value;

            }
            catch
            {
                Trace.WriteLine(String.Format("catch GetConfigSetting: list: {0} index: {1}", configList, index));
                return "";
            }
        }
    }
}