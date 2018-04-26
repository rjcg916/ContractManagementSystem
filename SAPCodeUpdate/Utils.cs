using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using CMSCommon;

namespace SAPCodeUpdate
{
    public class Utils
    {

        public static void SAPCodeUpdate(SPWebApplication webApplication)
        {


            using (SPSite site = webApplication.Sites[0])
            {

                SPList configList = null;
                try
                {
                    configList = site.RootWeb.Lists[Constants.SITECONFIGLIST];
                }
                catch
                {
                    Trace.WriteLine("SAPCodeUpdate: SiteConfig List Missing: " + Constants.SITECONFIGLIST);
                    return;
                }

                // fetch DB Server Name
                string sapMasterDBServer = CMSCommon.Utils.GetConfigSetting(configList, Constants.SAPMASTERDBSERVERTITLE);

                // fetch DB Name
                string sapMasterDB = CMSCommon.Utils.GetConfigSetting(configList, Constants.SAPMASTERDBTITLE);

                SqlConnection sqlConnection = null;
                string connectionString = String.Format("Data Source={0};Initial Catalog={1};Integrated Security=True;", sapMasterDBServer, sapMasterDB);

                try
                {

                    sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();

                    // fetch SAP Master Folder Path

                    string sourceDir = CMSCommon.Utils.GetConfigSetting(configList, Constants.SAPMASTERFOLDERTITLE);
                    string archiveDir = sourceDir + @"archive\";
                    Trace.WriteLine("SAPCodeUpdate: sourceDir: " + sourceDir);

                    // run updates for each code table and move file to archive

                    string sourceFileCC = CMSCommon.Utils.GetLatestFile(sourceDir, "costcenter*");
                    if (!string.IsNullOrEmpty(sourceFileCC))
                    {
                        ExecuteSAPUpdateCommand(sqlConnection, sourceFileCC, "UpdateCostCenters");
                        CMSCommon.Utils.MoveToDir(sourceDir, "costcenter*", archiveDir);
                    }


                    string sourceFilePRJ = CMSCommon.Utils.GetLatestFile(sourceDir, "projectcode*");
                    if (!string.IsNullOrEmpty(sourceFilePRJ))
                    {
                        ExecuteSAPUpdateCommand(sqlConnection, sourceFilePRJ, "UpdateProjectCodes");
                        CMSCommon.Utils.MoveToDir(sourceDir, "projectcode*", archiveDir);
                    }

                    string sourceFilePRD = CMSCommon.Utils.GetLatestFile(sourceDir, "product_category*");
                    if (!string.IsNullOrEmpty(sourceFilePRD))
                    {
                        ExecuteSAPUpdateCommand(sqlConnection, sourceFilePRD, "UpdateProductCategories");
                        CMSCommon.Utils.MoveToDir(sourceDir, "product_category*", archiveDir);
                    }

                }
                catch (Exception ex)
                {
                    Trace.WriteLine("SAPCodeUpdate: Error Connecting to SQL: connection string: " + connectionString + " " + ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        sqlConnection.Close();
                }

            }

        }


        private static void ExecuteSAPUpdateCommand(SqlConnection sqlConnection, string sourceFile, string spName)
        {
            Trace.WriteLine("ExecuteSAPUpdateCommand: spName: " + spName + " sourceFile: " + sourceFile);

            SqlCommand cmd = null;
            try
            {
                cmd = new SqlCommand();
                Int32 rowsAffected;

                cmd.CommandText = spName;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@fn", System.Data.SqlDbType.VarChar)).Value = sourceFile;
                cmd.Connection = sqlConnection;

                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Trace.WriteLine("catch ExecuteSAPUpdateCommand: command " + cmd.CommandText + " " + ex.ToString() + " " + ex.StackTrace);
            }
        }
    }
}
