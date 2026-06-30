using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Abstractions
{
    public interface IPreferenceCacheService
    {
        Task<IEnumerable<Preference>> GetAllPreference();
        Task<IEnumerable<Preference>> GetRangePreference(List<Guid> ids);

        Task<Preference> GetPreferenceById(Guid id);
    }
}
