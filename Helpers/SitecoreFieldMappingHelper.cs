using Sitecore.Data.Fields;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Sitecore;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public class SitecoreFieldMappingHelper
    {
        private const string Html = "HTML";
        private const string RichText = "rich text";
        private const string MultiLine = "multi-line text";
        private const string SingleLine = "single-line text";
        private const string DateField = "date";
        private const string DateTimeField = "datetime";
        private const string CHDateTimeFormat = "yyyyMMdd'T'HHmmss";



        public string PerformFieldMapAlteration(string source, Field destField)
        {
            if (string.IsNullOrWhiteSpace(source))
                return destField.Value;

            switch (destField.TypeKey)
            {
                case RichText:
                    return CorrectToXhtml(source);
                case MultiLine:
                case SingleLine:
                    return HttpUtility.HtmlDecode(source);
                case DateTimeField:
                case DateField:
                    return DateTimeFieldFormat(source);
                default:
                    return source;
            }
        }

        private string DateTimeFieldFormat(string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;
            var trimmedSource = source.Contains(":")
                ? source.Substring(0, source.IndexOf(":", StringComparison.Ordinal))
                : source.TrimEnd('Z');
            var isvalid = DateTime.TryParseExact(trimmedSource, CHDateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var dt);
            if (!isvalid)
                return string.Empty;

            return DateUtil.ToIsoDate(dt);
        }

        private string CorrectToXhtml(string source)
        {
            try
            {
                var reader = new Sgml.SgmlReader
                {
                    DocType = Html,
                    InputStream = new StringReader(source)
                };
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                var node = doc.SelectSingleNode("/html");
                return node?.InnerXml ?? source;
            }
            catch
            {
                return source;
            }
        }
    }
}