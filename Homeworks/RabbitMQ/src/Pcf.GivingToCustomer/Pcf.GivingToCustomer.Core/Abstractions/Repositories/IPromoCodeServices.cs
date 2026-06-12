using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Abstractions.Repositories
{
    public interface IPromoCodeServices
    {
        Task GivePromocodeEithPreferenceAsync(Guid promoCodeId, Guid partnerId, string promoCode, string serviceInfo, Guid preferenceId,  DateTime beginDate, DateTime endDate);
    }
}
