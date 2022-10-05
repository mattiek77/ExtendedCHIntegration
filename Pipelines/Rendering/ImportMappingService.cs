using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Events;
using Sitecore.Shell.Applications.ContentManager;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Rendering
{
    public class ImportMappingService
    {
        private static readonly ID PageBase = new ID("{3F8A6A5D-7B1A-4566-8CD4-0A50F3030BD8}");
        private static Dictionary<Item, IList<Item>> ConfigItems;
        private static Item _configItem;
        private static readonly object SyncRoot = new object();

        private static readonly HashSet<ID> standardCMPFields = new HashSet<ID>()
        {
                Constants.CreatedByField,
                Constants.ItemOwnerField,
                Constants.ContentHubEntity.Fields.CmpAuthorFieldId,
                Constants.EntityIdentifierField,
                Constants.VersionEntityIdentifierField
        };

        private static Item ConfigItem()
        {
            if (_configItem != null)
                return _configItem;
            lock (SyncRoot)
            {
                Thread.MemoryBarrier();
                if (_configItem != null)
                    return _configItem;
                _configItem = Factory.GetDatabase("master").GetItem(Sitecore.Connector.CMP.Constants.ConfigItemId);
            }
            return _configItem;
        }

        public void OnItemSaved(object o, EventArgs ea)
        {
            var item = Event.ExtractParameter<Item>(ea, 0);
            CheckSaveItem(item);
        }

        public void OnItemSavedRemote(object o, EventArgs ea)
        {
            if (ea is ItemSavedRemoteEventArgs savedArgs)
            {
                CheckSaveItem(savedArgs.Item);
            }
        }

        private static void CheckSaveItem(Item item)
        {
            var cfig = ConfigItem();
            if (cfig != null && item != null && item.Paths.FullPath.StartsWith(cfig.Paths.FullPath))
            {
                lock (SyncRoot)
                {
                    ConfigItems = null;
                }
            }
        }

        private static Item GetNearestPageItem(Item item)
        {
            var i = item ?? Sitecore.Context.Item;
            while (!IsPage(i))
            {
                i = i.Parent;
                if (i == null)
                    return null;
            }

            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPage(Item i = null)
        {
            i = i ?? Sitecore.Context.Item;
            return TemplateManager.GetTemplate(i).DescendsFromOrEquals(PageBase);
        }

        private static Item GetMappingItem(Item item = null)
        {
            var nearestPage = GetNearestPageItem(item);
            if (nearestPage == null)
                return null;

           
            IList<Item> configChildren;
            Thread.MemoryBarrier();
            var localConfig = ConfigItems;
            if (localConfig == null)
            {
                lock (SyncRoot)
                {
                    configChildren = ConfigItem()?.Children
                        .Where(i => i.TemplateID == Sitecore.Connector.CMP.Constants.EntityMappingTemplateId).Where(i =>
                            ID.TryParse(i[Sitecore.Connector.CMP.Constants.EntityMappingTemplateFieldId],
                                out var id)).Where(i=> ((CheckboxField)i.Fields[Constants.EntityMapping.Fields.ActiveStateField]).Checked)
                        .Where(i=> ((CheckboxField)i.Fields[Constants.EntityMapping.Fields.LockFieldsField]).Checked).ToList();
                    if (configChildren != null && configChildren.Any())
                    {
                        localConfig = new Dictionary<Item, IList<Item>>();
                        foreach (var configRoot in configChildren)
                        {
                            localConfig.Add(configRoot, null);
                        }
                        Thread.MemoryBarrier();
                        ConfigItems = localConfig;
                    }
                }
            }
            else
            {
                configChildren = localConfig.Select(x => x.Key).ToList();
            }

            return configChildren?.FirstOrDefault(i =>
                    (ID.Parse(i[Sitecore.Connector.CMP.Constants.EntityMappingTemplateFieldId]) ==
                     nearestPage.TemplateID ||
                     ID.Parse(i[Sitecore.Connector.CMP.Constants.EntityMappingTemplateFieldId]) ==
                     nearestPage.BranchId) && i[Constants.EntityMapping.Fields.CmpItemWorkflow] ==
                    nearestPage[Constants.Workflows.WorkflowFieldID]);
        }

        private IList<Item> GetChildMapping(Item i)
        {
            Thread.MemoryBarrier();
            var localConfig = ConfigItems;
            if (localConfig != null && localConfig.TryGetValue(i, out var configs))
            {
                if (configs == null)
                {
                    configs = i.Children.Where(it =>
                        ID.TryParse(it[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId],
                            out var t)).ToList();
                    lock (SyncRoot)
                    {
                        localConfig[i] = configs;
                    }
                }
                return configs;
            }
            return i.Children.ToList();
        }

        public bool IsReadOnly(string fieldName, Item item)
        {
            var fieldId = item.Fields[fieldName].ID;
            return IsReadOnly(fieldId, item);
        }
     
        public bool IsReadOnly(Editor.Field field, Item item)
        {
            return IsReadOnly(field.ItemField.ID, item);
        }

        private bool IsReadOnly(ID fieldId, Item item)
        {
            var mappingItem = GetMappingItem(item);
            if (mappingItem == null)
            {
                return false;
            }
            if (standardCMPFields.Contains(fieldId))
            {
                return true;
            }

            var childrenWithValidIds = GetChildMapping(mappingItem);

            var mappingForField = childrenWithValidIds.Where(i => i.TemplateID != Constants.DataSourceTemplateID).Any(i =>
                ID.Parse(i[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId]) == fieldId);

            var mappingLocalDS = childrenWithValidIds.Where(i => i.TemplateID == Constants.DataSourceTemplateID || i.TemplateID == Constants.LocalDataItemImageMappingTemplateID).Any(
                i => item.Name.Equals(i[Constants.RelatedItemPathFieldID].Split('/').LastOrDefault(), StringComparison.OrdinalIgnoreCase) &&
                     ID.Parse(i[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId]) == fieldId);

            var isPage = IsPage(item);
            return isPage && mappingForField || !isPage && mappingLocalDS;
        }
    }
}