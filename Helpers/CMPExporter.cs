using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace WKSkunkWorks.Foundation.DAM.Helpers
{
    public class CMPExporter : ICMPExporter
    {
        private IWebMClient Client=>_webClientFactory.GetClient;
        private readonly WebClientFactory _webClientFactory;


        public CMPExporter(WebClientFactory clientFactory)
        {
            _webClientFactory = clientFactory;
        }

        private async Task<IEntity> GetEntityByIdentifier(string identifier)
        {
            Query q = Query.CreateQuery(entities =>
                from e in entities
                where e.Identifier == identifier
                select e);
            return await Client.Querying.SingleAsync(q, EntityLoadConfiguration.Full).ConfigureAwait(false);
        }

        public async Task<long?> AddToDam(string entityType, string name, string nameFieldInDAM, string identifier, IEnumerable<Tuple<string, string, bool>> requiredFields, IEnumerable<Tuple<string, string, bool>> optionalFields, CultureInfo culture)
        {
            IEntity asset;
            if (string.IsNullOrEmpty(identifier))
            {
                asset = await Client.EntityFactory.CreateAsync(entityType).ConfigureAwait(false);
            }
            else
            {
                asset = await GetEntityByIdentifier(identifier).ConfigureAwait(false);
            }

            try
            {
                Sitecore.Diagnostics.Log.Info($"Adding item {name} - to the DAM", this);
                asset.SetPropertyValue(nameFieldInDAM, name);
                foreach (var field in requiredFields)
                {
                    if (!field.Item3)
                    {
                        asset.SetPropertyValue(field.Item2, field.Item1);
                    }
                    else
                    {
                        asset.SetPropertyValue(field.Item2, culture, field.Item1);
                    }
                }
                var id = await Client.Entities.SaveAsync(asset).ConfigureAwait(false);
                var optFields = optionalFields.ToList();
                if (optFields.Any())
                {
                    asset = await Client.Entities.GetAsync(id, EntityLoadConfiguration.Full).ConfigureAwait(false);
                    foreach (var field in optFields)
                    {
                        if (!field.Item3)
                        {
                            asset.SetPropertyValue(field.Item2, field.Item1);
                        }
                        else
                        {
                            asset.SetPropertyValue(field.Item2, culture, field.Item1);
                        }
                    }
                    await Client.Entities.SaveAsync(asset).ConfigureAwait(false);
                }
                Sitecore.Diagnostics.Log.Info($"Added item {name} - to the DAM", this);
                return id;
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error($"Error adding item to the DAM", ex, this);
                return null;
            }
        }

        public async Task DeleteFromDAM(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return;
            }

            var asset = await GetEntityByIdentifier(identifier).ConfigureAwait(true);
            if (asset.Id != null)
            {
                await Client.Entities.DeleteAsync(asset.Id.Value);
            }
        }
        public async Task<string> GetCHIdentifier(long entityId)
        {
            var entity = await Client.Entities.GetAsync(entityId, EntityLoadConfiguration.Minimal).ConfigureAwait(false);
            return entity.Identifier;
        }
    }
}