using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public interface ICMPExporter
    {
        Task<long?> AddToDam(string entityType, string name, string nameFieldInDAM, string identifier, IEnumerable<Tuple<string, string, bool>> requiredFields, IEnumerable<Tuple<string, string, bool>> optionalFields, CultureInfo culture);
        Task<string> GetCHIdentifier(long entityId);
        Task DeleteFromDAM(string identifier);
    }
}