using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace Elan.SharePoint.LRFApproval.Common
{

    public class Util
    {

        public static SPListItem InsertItemUnique(SPList list, string column, string value)
        {
            
            if (list.Items != null)
            {
                foreach (SPListItem item in list.Items)
                {
                    string curValue = item[column].ToString();
                    if (curValue.ToLower().Equals(value.ToLower()))
                        return item;
                }

            }

		    SPListItem listItem = list.Items.Add();
		    listItem[column] = value.ToLower();
	        listItem.Update();
            return listItem;
        }

        public static int MakeInt(string input)
        {
            int outInt = 0;
            string strOut = input;
            if (input.Length > 0)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (!char.IsNumber(c))
                        strOut = strOut.Replace(c.ToString(), "");
                }
            }

            if (!string.IsNullOrEmpty(strOut))
                outInt = Convert.ToInt32(strOut.Trim());

            return outInt;
        }



    }
}
