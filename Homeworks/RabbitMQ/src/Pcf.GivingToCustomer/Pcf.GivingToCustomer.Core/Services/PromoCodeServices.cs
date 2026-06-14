using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services
{
    //Вынос логики из котроллера в отдельный сервис
    public class PromoCodeServices : IPromoCodeServices
    {
        public readonly IRepository<PromoCode> _promoCodesRepository;
        public readonly IRepository<Preference> _preferencesRepository;
        public readonly IRepository<Customer> _customersRepository;

        public PromoCodeServices(IRepository<PromoCode> promoCodesRepository, IRepository<Preference> preferencesRepository, IRepository<Customer> customersRepository)
        {
            _promoCodesRepository = promoCodesRepository;
            _preferencesRepository = preferencesRepository;
            _customersRepository = customersRepository;
        }

        public async Task GivePromocodeEithPreferenceAsync(Guid promoCodeId, Guid partnerId, string promoCode, string serviceInfo, Guid preferenceId, DateTime beginDate, DateTime endDate)
        {
            //1. Получаем предпочтение по id
            var preference = await _preferencesRepository.GetByIdAsync(preferenceId);

            if (preference == null) { 
                throw new InvalidOperationException($"Предпочтение с Id {preferenceId} не найдено");
            }
            //2. Ищем клиентов с таким предпочтением
            var customers = await _customersRepository.GetWhere(x => x.Preferences.Any(u => u.PreferenceId == preference.Id));

            //3. Создаем промокод
            var promocde = new PromoCode()
            {
                Id = promoCodeId,
                PartnerId = partnerId,
                Code = promoCode,
                ServiceInfo = serviceInfo,
                BeginDate = beginDate,
                EndDate = endDate,
                Preference = preference,
                PreferenceId = preference.Id,
                Customers = customers.Select(x => new PromoCodeCustomer() 
                    { CustomerId = x.Id,
                    Customer = x,
                    PromoCodeId = promoCodeId }).ToList()
            };

            //4. Сохраняем промокод
            await _promoCodesRepository.AddAsync(promocde);
        }
    }
}
