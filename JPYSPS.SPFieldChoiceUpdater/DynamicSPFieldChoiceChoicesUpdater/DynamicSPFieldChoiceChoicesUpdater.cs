using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Linq;
using System.Collections.Generic;

namespace JPYSPS.SPFieldChoiceUpdater.DynamicSPFieldChoiceChoicesUpdater
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class DynamicSPFieldChoiceChoicesUpdater : SPItemEventReceiver
    {
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            UpdateChoices(properties.ListItem);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            UpdateChoices(properties.ListItem);
        }

        public void UpdateChoices(SPListItem item)
        {
            SPList list = item.ParentList;
            List<SPFieldChoice> fields = list.Fields.Cast<SPField>().Where(f => f.Type == SPFieldType.Choice).Select(f1 => (SPFieldChoice)f1).Where(f2 => f2.FillInChoice == true).ToList();
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                fields.ForEach(f =>
                {
                    bool bNeedUpdate = false;
                    var r = list.GetItems(new SPQuery { ViewFields = "<FieldRef Name='" + f.InternalName + "' />" });
                    List<string> choices = new List<string>();
                    foreach (SPListItem i in r)
                    {
                        choices.Add((i[f.InternalName] ?? "").ToString().Trim());
                    }
                    choices.AddRange(f.Choices.Cast<string>().ToList());
                    choices.RemoveAll(str => String.IsNullOrEmpty(str));
                    choices = choices.Distinct().ToList();
                    //System.IO.File.AppendAllText(@"c:\ppy\DSPFCU." + list.Title + ".txt", f.InternalName + " :::: " + string.Join(",", choices.ToArray()) + Environment.NewLine);
                    choices.ForEach(c =>
                    {
                        if (!(f.Choices.Contains(c)))
                        {
                            f.Choices.Add(c);
                            bNeedUpdate = true;
                        }
                    });
                    if (bNeedUpdate) f.Update();
                });
            });
        }


    }
}