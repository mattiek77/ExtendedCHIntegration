using Sitecore.Abstractions;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;
using System;
using ExtendedCHIntegration.Foundation.DAM.Helpers;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Version = System.Version;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Import
{
    public class FetchEntity : ImportEntityProcessor
    {
        private readonly WebClientFactory _webClientFactory;
        private readonly CmpSettings _settings;
        private readonly BaseFactory _factory;
        private IWebMClient Client => _webClientFactory.GetClient;
        private readonly CmpHelper _cmpHelper;
        private static APIVersionHelper _apiVersionHelper;
        private static Version apiVersion;

        private static readonly object SyncRoot = new object();

        public FetchEntity(
          BaseFactory factory,
          BaseLog logger,
          WebClientFactory mClientFactory,
          CmpSettings settings,
          CmpHelper cmpHelper,
          APIVersionHelper versionHelper)
          : base(logger, settings)
        {
            this._factory = factory;
            this._webClientFactory = mClientFactory;
            _settings = settings;
            this._cmpHelper = cmpHelper;
            _apiVersionHelper = versionHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            long result1;
            if (!args.BusMessage.UserProperties.ContainsKey("target_id") || !long.TryParse(args.BusMessage.UserProperties["target_id"].ToString(), out result1))
                throw new ArgumentException("The message contains no valid target id.", nameof(args));
            args.ConfigItem = BaseHelper.GetConfigItem(this._factory, _settings);
            IEntity entity = this._cmpHelper.GetEntity(result1, logger);
            args.Entity = entity;
            args.EntityDefinition = this.Client.EntityDefinitions.GetAsync(args.Entity.DefinitionName).ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.IsNotNull((object)args.EntityDefinition, string.Format("The entity definition '{0}' of M-Entity (id: {1}) was not found in M.", (object)args.Entity.DefinitionName, (object)result1));
            args.EntityIdentifier = GetEntityIdentifier(args.Entity);
            Assert.IsNotNullOrEmpty(args.EntityIdentifier, "Failed to get the M-Entity's identifier.");
            args.Language = FetchEntity.GetLanguage(args.Entity, this.Client);
            Language result2;
            if (args.Language == (Language)null && Language.TryParse(args.ConfigItem[Sitecore.Connector.CMP.Constants.DefaultLanguage], out result2))
                args.Language = result2;
            Assert.IsNotNull((object)args.Language, "Failed to get the language.");
            args.ContentTypeIdentifier = this._cmpHelper.GetEntityDefinitionType(args.Entity);
        }

        private static Version GetApiVersion()
        {
            if (apiVersion == null)
            {
                lock (SyncRoot)
                {
                    if (apiVersion != null)
                    {
                        return apiVersion;
                    }

                    apiVersion = Version.Parse(_apiVersionHelper.GetVersion());
                }
            }
            return apiVersion;
        }

        private string GetRelationName() => GetApiVersion() < new Version(3, 4) ? "ContentToContentLocalization" : "ContentToContentVariant";

        private string GetEntityIdentifier(IEntity entity)
        {
            string relation = GetRelationName();
            string identifier = entity.Identifier;
            long? parent = (long?)entity.GetRelation<IChildToOneParentRelation>(relation)?.Parent;
            while (parent.HasValue)
            {
                var ip = _cmpHelper.GetEntity(parent.Value);
                if (ip != null)
                {
                    identifier = ip.Identifier;
                    parent = (long?) ip.GetRelation<IChildToOneParentRelation>(relation)?.Parent;
                }
                else
                {
                    parent = null;
                }
            }

            return identifier;
        }

        private static Language GetLanguage(IEntity entity, IWebMClient client)
        {
            long? parent = (long?)entity.GetRelation<IChildToOneParentRelation>("LocalizationToContent")?.Parent;
            if (!parent.HasValue)
                return (Language)null;
            IEntity result1 = client.Entities.GetAsync(parent.Value, EntityLoadConfiguration.Full).ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.IsNotNull((object)result1, string.Format("Could not fetch the {0} parent (id: {1}) (the language).", (object)"LocalizationToContent", (object)parent.Value));
            string propertyValue = result1.GetPropertyValue<string>("ValueName");
            Assert.IsNotNullOrEmpty(propertyValue, string.Format("There is a {0} parent (id: {1}), but it's ValueName property is null or empty.", (object)"LocalizationToContent", (object)parent.Value));
            Language result2;
            Assert.IsTrue(Language.TryParse(propertyValue, out result2), "Failed to parse the language (" + propertyValue + ").");
            return result2;
        }
    }
}
