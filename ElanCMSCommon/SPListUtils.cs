namespace CMSCommon
{
    using System;
    using System.Collections.Specialized;
    using Microsoft.SharePoint;
    using System.Data;
    using System.Diagnostics;

    public class SPListUtils
    {

        public static void DeleteList(SPWeb web, string listName)
        {
            Trace.WriteLine("Attempting to delete list " + listName);

            SPList list = null;
            try
            {
                list = web.Lists[listName];
            }
            catch
            {
                Trace.WriteLine("list not found");
            }

            try
            {
                if (list != null)
                {
                    list.Delete();
                    Trace.WriteLine("Deleted List: " + listName);
                }
            }
            catch
            {
                Trace.WriteLine("error deleting list");
            }

        }


        /// <summary>
        /// Creates the library.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listName">Name of the library.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static SPList CreateLibrary(SPWeb web, string listName, string description, bool onQuickLaunch, bool allowContentTypes, bool enableVersioning)
        {

            SPList list = null;
            try
            {
                list = web.Lists[listName];
            }
            catch (Exception)
            {
            }

            if (list == null)
            {
                // web.AllowUnsafeUpdates = true;
                Guid guid = web.Lists.Add(listName, description, SPListTemplateType.DocumentLibrary);
                web.Lists[guid].OnQuickLaunch = onQuickLaunch;
                web.Update();

                list = web.Lists[guid];
                list.ContentTypesEnabled = allowContentTypes;
                list.EnableAttachments = false;
                list.OnQuickLaunch = onQuickLaunch;
                list.EnableVersioning = enableVersioning;
                list.NoCrawl = true;
                list.Update();

            }
            return list;
        }



        /// <summary>
        /// Creates the list.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="description">The description.</param>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static SPList CreateList(SPWeb web, string listName, string description, bool onQuickLaunch, bool allowContentTypes, bool enableVersioning)
        {


            SPList list = null;
            try
            {
                list = web.Lists[listName];
            }
            catch (Exception)
            {
            }

            if (list == null)
            {

                //web.AllowUnsafeUpdates = true;
                Guid guid = web.Lists.Add(listName, description, SPListTemplateType.GenericList);
                web.Lists[guid].OnQuickLaunch = onQuickLaunch;
                web.Update();

                list = web.Lists[guid];

                list.ContentTypesEnabled = allowContentTypes;
                list.EnableAttachments = false;
                list.OnQuickLaunch = onQuickLaunch;
                list.NoCrawl = true;
                list.EnableVersioning = enableVersioning;
                list.Update();

            }
            return list;
        }



        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columnType">Type of the column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">The required.</param>
        /// <returns></returns>
        public static SPField CreateColumn(SPList list, SPFieldType columnType, string columnName, string defaultValue, bool required)
        {
            if (list.Fields.ContainsField(columnName))
            {

                return list.Fields[columnName];
            }
            SPField newField = new SPField(list.Fields, columnType.ToString(), columnName);
            newField.Required = required;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                newField.DefaultValue = defaultValue;
            }


            list.Fields.Add(newField);
            list.Update();
            list.ParentWeb.Update();

            return newField;
        }


        /// <summary>
        /// Creates the date column.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">The required.</param>
        /// <returns></returns>
        public static SPField CreateDateColumn(SPList list, string columnName, string defaultValue, bool required)
        {

            if (list.Fields.ContainsField(columnName))
            {

                return list.Fields[columnName];
            }
            SPFieldDateTime newField = new SPFieldDateTime(list.Fields, SPFieldType.DateTime.ToString(), columnName);
            newField.Required = required;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                newField.DefaultValue = defaultValue;
            }

            list.Fields.Add(newField);
            list.Update();
            list.ParentWeb.Update();

            return newField;
        }


        /// <summary>
        /// Creates the text column.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static SPField CreateTextColumn(SPList list, string columnName, string displaySize, string defaultValue, bool required)
        {

            if (list.Fields.ContainsField(columnName))
            {
                return list.Fields[columnName];
            }
            list.Fields.Add(columnName, SPFieldType.Text, required);
            list.Update();

            SPField spField = list.Fields[columnName];
            if (string.IsNullOrEmpty(defaultValue))
            {
                spField.DefaultValue = defaultValue;
            }

            spField.DisplaySize = displaySize;

            spField.Update();

            return list.Fields[columnName];
        }


        public static SPField CreateTextColumnMultiLine(SPList list, string columnName, string defaultValue, int rowsCount, bool required)
        {

            if (list.Fields.ContainsField(columnName))
            {
                return list.Fields[columnName];
            }

            list.Fields.Add(columnName, SPFieldType.Note, false);
            list.Update();

            SPFieldMultiLineText spField = (SPFieldMultiLineText)list.Fields[columnName];
            spField.NumberOfLines = rowsCount;
            spField.Required = required;
            spField.UnlimitedLengthInDocumentLibrary = true;
            if (string.IsNullOrEmpty(defaultValue))
            {
                spField.DefaultValue = defaultValue;
            }
            spField.Update();

            return list.Fields[columnName];
        }



        public static SPFieldChoice CreateChoiceColumn(SPList list, string columnName, bool required)
        {

            if (list.Fields.ContainsField(columnName))
            {

                return (SPFieldChoice)list.Fields[columnName];
            }

            SPFieldChoice newField = (SPFieldChoice)list.Fields.CreateNewField(SPFieldType.Choice.ToString(), columnName);
            newField.Required = required;
            list.Fields.Add(newField);
            list.Update();

            return newField;
        }



        /// <summary>
        /// Creates the lookup column.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="remoteLookupFieldName">Name of field in lookup list</param>
        /// <param name="lookupListId"></param>
        /// <param name="lookupWebId"></param>
        /// <param name="allowMultipleValues">
        ///   if set to
        ///   <c>true</c>
        ///   makes the field a multiple value field.
        /// </param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static SPField CreateLookupColumn(SPList list, string columnName, string remoteLookupFieldName, Guid lookupListId, Guid lookupWebId, bool allowMultipleValues, string defaultValue)
        {

            if (list.Fields.ContainsField(columnName))
            {
                return list.Fields[columnName];
            }

            //     Guid lookupListId = list.ParentWeb.Lists[lookupList].ID;
            //     string lookupFieldName = list.Fields.AddLookup(columnName, lookupListId, true).Replace("_x0020_", " ");
            string lookupFieldName = list.Fields.AddLookup(columnName, lookupListId, lookupWebId, true);



            //   SPFieldLookup field = (SPFieldLookup)list.Fields[lookupFieldName];
            SPFieldLookup field = (SPFieldLookup)list.Fields.GetFieldByInternalName(lookupFieldName);
            //            field.LookupField =  list.Fields["Title"].InternalName;
            field.LookupField = remoteLookupFieldName;

            field.AllowMultipleValues = allowMultipleValues;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                field.DefaultValue = defaultValue;
            }
            field.Update();


            return field;
        }



        public static SPField CreateUserColumn(SPList list, string columnName, bool allowMultipleValues, bool required)
        {

            if (list.Fields.ContainsField(columnName))
            {

                return list.Fields[columnName];
            }

            list.Fields.Add(columnName, SPFieldType.User, required);
            // list.Fields.AddFieldAsXml(string.Format("<Field Type=\"User\" List=\"{0}\" ShowField=\"Title\" DisplayName=\"{1}\" Name=\"{1}\" />", list.Title, columnName));
            list.Update();

            SPFieldUser userField = (SPFieldUser)list.Fields[columnName];
            userField.AllowMultipleValues = allowMultipleValues;
            userField.Required = required;
            userField.Update(true);


            return userField;
        }

        /// <summary>
        /// Gets the item id by title.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        public static int GetItemIdByTitle(SPList list, string title)
        {

            int itemId = 0;

            SPQuery query = new SPQuery();

            query.Query = string.Format("<Where><Eq><FieldRef Name='Title'/><Value Type='Text'>{0}</Value></Eq></Where>", title);

            SPListItemCollection items = list.GetItems(query);

            if (items.Count == 1)
            {
                itemId = items[0].ID;
            }

            return itemId;

        }



        /// <summary>
        /// Inserts the text item to list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static SPListItem InsertTextItemToList(SPList list, string[] columns, string[] values)
        {
            SPListItem item = list.Items.Add();
            for (int i = 0; i < columns.Length; i++)
            {
                if ((values[i] != "") && (!String.IsNullOrEmpty(values[i])))
                {
                    string column = columns[i];
                    item[column] = values[i];
                }
            }


            try
            {
                item.Update();
            }
            catch
            {

            }
            return item;
        }

        /// <summary>
        /// Inserts the text values to list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="columns">The column.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static void InsertTextItemsToList(SPList list, string column, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] != "") && (!String.IsNullOrEmpty(values[i])))
                {
                    SPListItem item = list.Items.Add();
                    item[column] = values[i];
                    item.Update();
                }
            }
        }


        public static void ClearList(SPList list)
        {
            while (list.Items.Count > 0)
            {
                list.Items.Delete(list.Items.Count - 1);
                list.Update();
            }
        }


        public static int GetRandomItemId(SPList list, Random random)
        {
            int next = random.Next(list.Items.Count);
            SPListItem item = list.Items[next];
            return item.ID;
        }



        public static SPField MakeRequired(SPField field)
        {
            field.Required = true;
            field.Update();
            return field;
        }



        public static string[] GetColumnAsArray(SPList list, string columnName)
        {
            StringCollection collection = new StringCollection();
            foreach (SPListItem item in list.Items)
            {
                object columnValue = item[columnName];
                if (columnValue != null)
                {
                    collection.Add(columnValue.ToString());
                }
            }

            string[] items = new string[collection.Count];
            collection.CopyTo(items, 0);

            return items;
        }



        public static void ViewAddNonReadOnlyColumnsToDefaultView(SPList list)
        {
            SPView view = list.DefaultView;
            SPViewFieldCollection coll = view.ViewFields;

            foreach (SPField field in list.Fields)
            {
                if (!field.ReadOnlyField)
                {
                    if (!coll.Exists(field.InternalName))
                    {
                        coll.Add(field);
                        view.Update();
                    }
                }
            }
        }


        public static void ViewAddIdColumnToDefaultView(SPSite site, string listName)
        {
            SPList list = site.RootWeb.Lists[listName];

            list.DefaultView.ViewFields.Add("ID");

            list.DefaultView.Update();
        }


        public const string LipsumString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean sed odio orci, non tristique lorem. Aenean non nibh id metus auctor convallis. Vestibulum quis quam porta orci gravida semper. Morbi sed risus ante, id tempus massa. Fusce a ullamcorper felis. In nec nibh in dui mattis adipiscing non ut tortor. Vestibulum libero elit, semper non consectetur id, faucibus eu risus. Sed nec leo nunc, sit amet convallis nisl. Nunc congue condimentum nulla a pretium. Aenean quis massa neque, feugiat viverra est. Quisque sit amet neque ligula. Aliquam erat volutpat. Aenean dignissim volutpat sapien, a mollis felis posuere eget. Sed at justo fermentum eros lobortis posuere. Maecenas malesuada libero eget lacus feugiat rutrum. Donec massa urna, eleifend et luctus sit amet, hendrerit nec mi";
        static Random random = new Random();
        public static string GetRandomLengthLipsumString(int minLength, int maxLength)
        {
            string[] strings = LipsumString.Split(new string[] { " " }, StringSplitOptions.None);
            int next = random.Next(minLength, maxLength);
            string lipsumString = string.Empty;
            for (int i = 0; i < next; i++)
            {
                lipsumString += strings[i] + " ";
            }

            return lipsumString.Trim();
        }
        public static DataSet GetRequestItems(SPList list, string fieldName, string fieldValue)
        {
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='" + fieldName + "'/>" +
                          "<Value Type='Text'>" + fieldValue + "</Value></Eq></Where>";


            SPListItemCollection spItems = list.GetItems(query);

            DataSet dsItems = new DataSet();

            if (spItems.Count > 0)
                dsItems.Merge(spItems.GetDataTable());

            return dsItems;
        }

        public static DataSet GetRequestItems(SPList list, string filterFieldName, string filterFieldValue, string sortFieldName, string sortDirection)
        {

            bool Ascending;
            if (sortDirection.ToString() == "Ascending")
                Ascending = true;
            else
            {
                Ascending = false;
            }

            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='" + filterFieldName + "'/>" +
                          "<Value Type='Text'>" + filterFieldValue + "</Value></Eq></Where>";
            query.Query += "  <OrderBy> " +
                            "<FieldRef Name='" + sortFieldName + "' Ascending='" + Ascending.ToString().ToUpper() + "'  /> " +
                                                      "</OrderBy>";

            SPListItemCollection spItems = list.GetItems(query);

            DataSet dsItems = new DataSet();

            if (spItems.Count > 0)
                dsItems.Merge(spItems.GetDataTable());

            return dsItems;
        }

    }
}