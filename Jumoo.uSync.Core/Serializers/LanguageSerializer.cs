﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Jumoo.uSync.Core.Interfaces;
using System.Xml.Linq;
using Jumoo.uSync.Core.Extensions;
using Umbraco.Core.Logging;

namespace Jumoo.uSync.Core.Serializers
{
    public class LanguageSerializer : SyncBaseSerializer<ILanguage>
    {
        private IPackagingService _packagingService;
        private ILocalizationService _localizationService;
        public LanguageSerializer(string itemType) : base(itemType)
        {
            _packagingService = ApplicationContext.Current.Services.PackagingService;
            _localizationService = ApplicationContext.Current.Services.LocalizationService;
        }

        internal override SyncAttempt<ILanguage> DeserializeCore(XElement node)
        {
            var culture = node.Attribute("CultureAlias");
            var fName = node.Attribute("FriendlyName");
            if (culture == null || fName == null)
                return SyncAttempt<ILanguage>.Fail(node.NameFromNode(), ChangeType.Import, "missing Alias or Name");

            // by name
            ILanguage item = _localizationService.GetLanguageByCultureCode(fName.Value);

            // by iso code
            if (item == null)
                item = _localizationService.GetLanguageByIsoCode(culture.Value);

            // create a new one
            if (item == null)
                item = new Language(culture.Value);            

            // all that failed
            if (item == null)
                return SyncAttempt<ILanguage>.Fail(node.NameFromNode(), ChangeType.Import, "Unable to import language");

            // it worked update stuff..
            item.IsoCode = culture.Value;
            item.CultureName = fName.Value; 
            _localizationService.Save(item);

            return SyncAttempt<ILanguage>.Succeed(item.CultureName, item, ChangeType.Import);
        }

        internal override SyncAttempt<XElement> SerializeCore(ILanguage item)
        {
            var node = _packagingService.Export(item);
            return SyncAttempt<XElement>.SucceedIf(
                node != null,
                item.CultureName,
                node,
                typeof(ILanguage),
                ChangeType.Export);
        }

        public override bool IsUpdate(XElement node)
        {
            var nodeHash = node.GetSyncHash();
            if (string.IsNullOrEmpty(nodeHash))
                return true;

            var culture = node.Attribute("CultureAlias");
            if (culture == null)
                return true;

            var item = ApplicationContext.Current.Services.LocalizationService.GetLanguageByIsoCode(culture.Value);
            if (item == null)
                return true;

            var attempt = Serialize(item);
            if (!attempt.Success)
                return true;

            var itemHash = attempt.Item.GetSyncHash();

            LogHelper.Debug<ILanguage>(">> IsUpdated: {0} : {1}", () => !nodeHash.Equals(itemHash), () => item.CultureName);

            return (!nodeHash.Equals(itemHash));
        }
    }
}
