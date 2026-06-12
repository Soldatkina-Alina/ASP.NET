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

    }
}
