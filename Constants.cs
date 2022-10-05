using System.Text.RegularExpressions;
using Sitecore.Data;

namespace ExtendedCHIntegration.Foundation.DAM
{
    public static class Constants
    {
        public const string CONTENT_HUB_CONTENT = "stylelabs-content-id";
        public const string EXISTING_ITEM = "ExistingItem";
        public static readonly ID LocalDataSourceFolderId = new ID("{1C82E550-EBCD-4E5D-8ABD-D50D0809541E}");
        public static readonly ID DataSourceTemplateID = new ID("{C6115437-F9B8-401D-8A76-56C58E202F81}");
        public static readonly ID RelatedItemPathFieldID = new ID("{7ADBCD12-324B-432F-9281-51B4025DC699}");
        public static readonly ID LinkedDataItemTemplateID = new ID("{760A35C5-D2B4-4A7E-B24B-5403468FA76D}");
        public static readonly ID ImageImportTemplateID = new ID("{795AD6AA-8E12-4748-A4C2-B1B4A0C7E813}");
        public static readonly ID ContentHubEntryTemplateID = new ID("{0D17BE5B-EC2A-4296-8D3F-930EB60DFE7C}");
        public static readonly ID EntityMappingEntityTypeSchemaFieldId = new ID("{9E009C2F-AD3C-4108-AA48-15689C1D4E1B}");
        public static readonly ID RelationFieldMappingCmpRelationFieldNameFieldId = new ID("{C3101AF2-0121-4011-A9E9-FB727BB40999}");
        public static readonly ID EntityMappingItemNamePropertyField = new ID("{E82AF1D8-7521-47E2-9E1B-898E5686D1C2}");
        public static readonly ID LocalDsId = new ID("{1C82E550-EBCD-4E5D-8ABD-D50D0809541E}");
        public static readonly ID CreatedByField = new ID("{5DD74568-4D4B-44C1-B513-0AF5F4CDA34F}");
        public static readonly ID ItemOwnerField = new ID("{52807595-0F8F-4B20-8D2A-CB71D28C6103}");
        public static readonly ID LocalDataItemImageMappingTemplateID = new ID("{1922A0D1-203B-4AE8-9BBD-233039175239}");
        public static readonly Regex UriParser = new Regex("(?<=URI=)[^;]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly ID EntityIdentifierField = new ID("{9B0343A9-9F69-4E0F-A059-9215BC8FE422}");
        public static readonly ID VersionEntityIdentifierField = new ID("{A6C0DED3-5165-4005-AC59-E7629D3225B0}");

        public const string LocalDS = "local:";
        public const string DefaultString = "DEFAULT";
        public const string DefaultNumber = "1";
        public const string DefaultID = "{11111111-1111-1111-1111-111111111111}";
        public const string DbName = "master";
        public const string WorkflowRelationshipName = "MContentToActiveState";
        public const string EntityIdentifier = "EntityIdentifier";

        public static class IDToCHField
        {
            public static readonly ID TemplateID = new ID("{5CA0CE2C-E19A-486B-8AEE-77A472CCDDBE}");
        }

        public static class MappingWithBackup
        {
            public static readonly ID TemplateID = new ID("{3B32F054-4E4A-4A83-9657-0BF97E226E4E}");
            public static class Fields
            {
                public static readonly ID BackupFieldID = new ID("{8EA1B7C3-F7A8-4E36-BD00-4887161D134A}");
            }
        }

        public static class EntityMapping
        {
            public static class Fields
            {
                public static readonly ID CmpItemWorkflow = new ID("{BEFB908F-60BE-4078-9DE9-AA6D249327F4}");
                public static readonly ID ImportedStartingState = new ID("{88DE1205-C391-4620-BE6C-4EBDB1300402}");
                public static readonly ID CreateNewVersionIfNotInInitialState =
                    new ID("{D84C1FC7-8EE8-4824-9A22-C8C51DBCE7E8}");

                public static readonly ID ActiveStateField = new ID("{A1DC6727-D4ED-4860-9C03-39F8A5DBDF99}");
                public static readonly ID LockFieldsField = new ID("{1FB3D00F-A854-4BC9-9711-2AF16B103D68}");
            }
        }

        public static class Workflows
        {
            public static readonly ID WorkflowFieldID = new ID("{A4F985D9-98B3-4B52-AAAF-4344F6E747C6}");
            public static readonly ID WorkflowStateFieldID = new ID("{3E431DE1-525E-47A3-B6B0-1CCBEC3A8C98}");
            public static readonly ID WorkflowCommentsFieldName = new ID("{9A87FD2B-964D-4F03-8FA8-65296ABE0D99}");
            public static class WorkflowAction
            {
                public static readonly ID StateIdentifier = new ID("{29755DF2-1AA8-4AD8-95CA-E9527C971140}");
            }
        }
        
        public static class ContentHubEntity
        {
            public static class Fields
            {
                public static readonly ID CmpAuthorFieldId = new ID("{8209D8F1-ECB3-49D9-B44A-09896A80027C}");
                public static readonly ID VersionSpecificIdField = new ID("{A6C0DED3-5165-4005-AC59-E7629D3225B0}");
            }
        }

        public static class ExportTemplate
        {
            public static readonly ID TemplateID = new ID("{7C0B70D3-6641-4FBA-88D5-5A701C39A8B4}");

            public static class Fields
            {
                public static readonly ID SitecoreTemplate = new ID("{66AE623D-9140-4FD8-920D-90623609ACDE}");
                public static readonly ID ContentHubSchema = new ID("{FBF5C2E6-1CB8-40FC-9429-B33F9B068BC4}");
                public static readonly ID ProcessDeletes = new ID("{47C3E018-BF03-4BDE-8FCD-927589139998}");
                public static readonly ID ProcessUpdates = new ID("{1582FC73-06CE-4DA8-8DC5-61F59FD36437}");
                public static readonly ID NameField = new ID("{DEE1E23C-05DB-401A-BF76-4F02030F0134}");
                public static readonly ID NameInDamField = new ID("{5A4E11C4-2F8E-47F2-8CC6-ECF8E0F61B54}");
                public static readonly ID LangVersionForName = new ID("{9AC81339-EA55-42F6-986C-2D1531CA2E50}");
                public static readonly ID LanguageMapping = new ID("{49F0C787-4E06-413E-BE61-B551EABD6669}");
                public static readonly ID LanguageToExport = new ID("{6C9A5333-CE62-4E40-9D59-533FAF5E6CFF}");
            }
        }

        public static class ExportFieldTemplate
        {
            public static readonly ID TemplateID = new ID("{FB735871-3870-46F8-8D73-5973B95ACAD1}");

            public static class Fields
            {
                public static readonly ID Required = new ID("{FB735871-3870-46F8-8D73-5973B95ACAD1}");
                public static readonly ID SitecoreField = new ID("{2CC6FE2B-0298-4000-B578-1A595B75FB95}");
                public static readonly ID DAMField = new ID("{F858F6F7-74FB-4B54-873A-7B2C6A409464}");
                public static readonly ID DefaultIfEmpty = new ID("{972CD499-F883-46CC-8338-B0335E4F85EA}");
                public static readonly ID IsLocalisable = new ID("{36ABF114-603B-4575-8DAA-0EB02177BB3B}");
            }
        }

        public static class PublicLink
        {
            public const string RelativeUrl = "RelativeUrl";
            public const string VersionHash = "VersionHash";
            public const string ConversionConfiguration = "ConversionConfiguration";
            public const string PublicLinkField = "public_link";
        }

        public static class Asset
        {
            public const string MainFile = "MainFile";
            public const string Height = "height";
            public const string Width = "width";
            public const string Properties = "properties";
        }
    }
}
