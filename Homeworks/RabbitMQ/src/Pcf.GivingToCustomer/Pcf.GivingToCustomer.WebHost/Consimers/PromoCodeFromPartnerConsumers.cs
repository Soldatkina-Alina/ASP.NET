using MassTransit;
using Pcf.Contracts;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.Consimers
{
    public class PromoCodeFromPartnerConsumers : IConsumer<PromoCodeReceivedFromPartnerEvent>
    {
        private readonly IPromoCodeServices _promoCodeServices;

        public PromoCodeFromPartnerConsumers(IPromoCodeServices promoCodeServices)
        {
            _promoCodeServices = promoCodeServices;
        }

        public async Task Consume(ConsumeContext<PromoCodeReceivedFromPartnerEvent> context)
        {
            var message = context.Message;

            //Выдача промокодов клиентам, которые были получены от партнера
            await _promoCodeServices.GivePromocodeEithPreferenceAsync(message.PromoCodeId, message.PartnerId,
                message.PromoCode, message.ServiceInfo, message.PreferenceId, message.BeginDate, message.EndDate);
        }
    }
}
