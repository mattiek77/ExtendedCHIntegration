using System;
using System.Runtime.CompilerServices;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.SecurityModel;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Import
{
    public class SetAllRequiredFields : ImportEntityProcessor
    {
        public SetAllRequiredFields(BaseLog logger, CmpSettings settings) : base(logger, settings)
        {
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            var item = args.Item;
            var validators = ValidatorManager.BuildValidators(ValidatorsMode.ValidateButton, item);
            var options = new ValidatorOptions(true);
            ValidatorManager.Validate(validators, options);
            using (new SecurityDisabler())
            using (new EditContext(item))
            {
                validators.ForEach(val =>
                {
                    if (val.Result == ValidatorResult.CriticalError || val.Result == ValidatorResult.FatalError)
                    {
                        var field = item.Fields[val.FieldID];
                        string value = null;
                        switch (field.TypeKey)
                        {
                            case "single-line text":
                            case "multi-line text":
                            case "rich text":
                                value = Constants.DefaultString;
                                break;
                            case "number":
                                value = Constants.DefaultNumber;
                                break;
                            default:
                                value = Constants.DefaultID;
                                break;
                        }

                        item[val.FieldID] = value;
                    }
                });
            }
        }
    }
}