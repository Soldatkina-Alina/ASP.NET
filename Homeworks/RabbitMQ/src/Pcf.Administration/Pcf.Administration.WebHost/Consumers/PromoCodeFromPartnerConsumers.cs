using MassTransit;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Contracts;
using System.Threading.Tasks;

namespace Pcf.Administration.WebHost.Consumers
{
    public class PromoCodeFromPartnerConsumers : IConsumer<PromoCodeReceivedFromPartnerEvent>
    {
        private readonly IEmployeeService _employeeService;

        public PromoCodeFromPartnerConsumers(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        public async Task Consume(ConsumeContext<PromoCodeReceivedFromPartnerEvent> context)
        {
            if (!context.Message.PartnerManagerId.HasValue)
            {
                return;
            }

            await _employeeService.UpdateAppliedPromocodeAsync(context.Message.PartnerManagerId.Value);
        }
    }
}
