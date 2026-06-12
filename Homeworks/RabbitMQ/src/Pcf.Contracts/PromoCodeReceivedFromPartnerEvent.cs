using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.Contracts
{
    public record PromoCodeReceivedFromPartnerEvent
    {
        public Guid PromoCodeId { get; init; }

        public Guid PartnerId { get; init; }

        public string PromoCode { get; init; }

        public string ServiceInfo { get; init; }

        public Guid PreferenceId { get; init; }

        public DateTime BeginDate { get; init; }

        public DateTime EndDate { get; init; }

        public Guid? PartnerManagerId { get; init; }

    }
}
