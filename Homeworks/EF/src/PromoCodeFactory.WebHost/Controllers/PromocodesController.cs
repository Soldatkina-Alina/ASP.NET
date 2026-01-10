using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController
        : ControllerBase
    {
        private readonly IRepository<PromoCode> _promoCodeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;

        public PromocodesController(IRepository<PromoCode> promoCodeRepository, IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository)
        {
            _promoCodeRepository = promoCodeRepository;
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
        }
        /// <summary>
        /// Получить все промокоды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            var promoCodes = await _promoCodeRepository.GetAllAsync();
            var response = promoCodes.Select(pc => new PromoCodeShortResponse
            {
                Id = pc.Id,
                Code = pc.Code,
                ServiceInfo = pc.ServiceInfo,
                BeginDate = pc.BeginDate.ToString("dd-MM-yyyy"),
                EndDate = pc.EndDate.ToString("dd-MM-yyyy"),
                PartnerName = pc.PartnerName
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            var preference = await _preferenceRepository.GetAllAsync(r=> r.Name == request.Preference);
            var targetPreference = preference.FirstOrDefault();

            if (targetPreference == null)
            {
                return NotFound("Preference not found");
            }

            // Найти клиентов с этим предпочтением
            var customerPreferences = await _customerRepository.GetAllAsync(c => c.Preferences.Any(cp => cp.PreferenceId == targetPreference.Id));
            var customersWithPreference = customerPreferences.ToList();

            if (!customersWithPreference.Any())
            {
                return BadRequest("No customers with this preference");
            }

            // Создать и сохранить промокоды для каждого клиента
            var promoCodesToCreate = customersWithPreference.Select(customer => new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = request.PromoCode,
                ServiceInfo = request.ServiceInfo,
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                PartnerName = request.PartnerName,
                Customer = customer,
                Preference = targetPreference
            }).ToList();

            foreach (var promoCode in promoCodesToCreate)
            {
                await _promoCodeRepository.CreateAsync(promoCode);
            }

            return Ok();
        }
    }
}