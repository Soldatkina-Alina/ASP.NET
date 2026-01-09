using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController
        : ControllerBase
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;
        private readonly IRepository<PromoCode> _promoCodeRepository;

        public CustomersController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository, IRepository<PromoCode> promoCodeRepository)
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
            _promoCodeRepository = promoCodeRepository;
        }

        /// <summary>
        /// Получить список всех клиентов
        /// </summary>
        /// <returns>Список клиентов</returns>
        [HttpGet]
        public async Task<ActionResult<List<CustomerShortResponse>>> GetCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            var response = customers.Select(c => new CustomerShortResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Получить клиента по ID с предпочтениями и промокодами
        /// </summary>
        /// <param name="id">ID клиента</param>
        /// <returns>Данные клиента</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdWithIncludesAsync(id, c => c.Preferences, c => c.PromoCodes);

            if (customer == null)
            {
                return NotFound();
            }

            var preferenceIds = customer.Preferences?.Select(cp => cp.PreferenceId).ToList() ?? new List<Guid>();
            var preferences = await _preferenceRepository.GetByIdsAsync(preferenceIds);

            var response = new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Preferences = customer.Preferences?.Select(cp => new PreferenceResponse
                {
                    CustomerId = cp.CustomerId,
                    PreferenceId = cp.PreferenceId,
                    PreferenceName = preferences.FirstOrDefault(p => p.Id == cp.PreferenceId)?.Name ?? "Unknown"
                }).ToList() ?? new List<PreferenceResponse>(),
                PromoCodes = customer.PromoCodes?.Select(pc => new PromoCodeShortResponse
                {
                    Id = pc.Id,
                    Code = pc.Code,
                    ServiceInfo = pc.ServiceInfo,
                    BeginDate = pc.BeginDate.ToString("dd-MM-yyyy"),
                    EndDate = pc.EndDate.ToString("dd-MM-yyyy"),
                    PartnerName = pc.PartnerName
                }).ToList() ?? new List<PromoCodeShortResponse>()
            };

            return Ok(response);
        }

        /// <summary>
        /// Создать нового клиента с предпочтениями
        /// </summary>
        /// <param name="request">Данные для создания клиента</param>
        /// <returns>Результат создания</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Preferences = new List<CustomerPreference>()
            };

            if (request.PreferenceIds != null && request.PreferenceIds.Any())
            {
                var selectedPreferences = await _preferenceRepository.GetByIdsAsync(request.PreferenceIds);

                foreach (var pref in selectedPreferences)
                {
                    customer.Preferences.Add(new CustomerPreference
                    {
                        CustomerId = customer.Id,
                        PreferenceId = pref.Id,
                        Customer = customer,
                        Preference = pref
                    });
                }
            }

            await _customerRepository.CreateAsync(customer);

            //Привязать существующие PromoCodes к Customer
            if (request.PromoCodeIds != null && request.PromoCodeIds.Any())
            {
                var promoCodes = await _promoCodeRepository.GetByIdsAsync(request.PromoCodeIds);

                foreach (var promoCode in promoCodes)
                {
                    promoCode.Customer = customer;
                    await _promoCodeRepository.UpdateAsync(promoCode);
                }
            }

            return StatusCode(201);
        }

        /// <summary>
        /// Назначить промокод клиенту
        /// </summary>
        /// <param name="customerId">ID клиента</param>
        /// <param name="promoCodeId">ID промокода</param>
        /// <returns>Результат назначения</returns>
        [HttpPut("{customerId}/promocodes/{promoCodeId}")]
        public async Task<IActionResult> AssignPromoCodeToCustomerAsync(Guid customerId, Guid promoCodeId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            var promoCode = await _promoCodeRepository.GetByIdAsync(promoCodeId);
            if (promoCode == null)
            {
                return NotFound("PromoCode not found");
            }

            promoCode.Customer = customer;
            await _promoCodeRepository.UpdateAsync(promoCode);

            return NoContent();
        }

        /// <summary>
        /// Обновить данные клиента и его предпочтения
        /// </summary>
        /// <param name="id">ID клиента</param>
        /// <param name="request">Обновленные данные клиента</param>
        /// <returns>Результат обновления</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdWithIncludesAsync(id, c => c.Preferences);

            if (customer == null)
            {
                return NotFound();
            }

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;

            // Обновить предпочтения
            customer.Preferences.Clear();

            if (request.PreferenceIds != null && request.PreferenceIds.Any())
            {
                var selectedPreferences = await _preferenceRepository.GetByIdsAsync(request.PreferenceIds);

                foreach (var pref in selectedPreferences)
                {
                    customer.Preferences.Add(new CustomerPreference
                    {
                        CustomerId = customer.Id,
                        PreferenceId = pref.Id,
                        Customer = customer,
                        Preference = pref
                    });
                }
            }

            await _customerRepository.UpdateAsync(customer);

            // Обновить PromoCodes - отвязать старые и привязать новые
            if (request.PromoCodeIds != null && request.PromoCodeIds.Any())
            {
                var promoCodes = await _promoCodeRepository.GetByIdsAsync(request.PromoCodeIds);

                foreach (var promoCode in promoCodes)
                {
                    promoCode.Customer = customer;
                    await _promoCodeRepository.UpdateAsync(promoCode);
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Удалить клиента по ID
        /// </summary>
        /// <param name="id">ID клиента</param>
        /// <returns>Результат удаления</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            await _customerRepository.DeleteAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Получить предпочтения клиента по ID
        /// </summary>
        /// <param name="id">ID клиента</param>
        /// <returns>Список предпочтений клиента</returns>
        [HttpGet("{id}/preferences")]
        public async Task<ActionResult<List<PreferenceResponse>>> GetCustomerPreferencesAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdWithIncludesAsync(id, c => c.Preferences);

            if (customer == null)
            {
                return NotFound();
            }

            var preferenceIds = customer.Preferences?.Select(cp => cp.PreferenceId).ToList() ?? new List<Guid>();
            var preferences = await _preferenceRepository.GetByIdsAsync(preferenceIds);

            var response = customer.Preferences?.Select(cp => new PreferenceResponse
            {
                CustomerId = cp.CustomerId,
                PreferenceId = cp.PreferenceId,
                PreferenceName = preferences.FirstOrDefault(p => p.Id == cp.PreferenceId)?.Name ?? "Unknown"
            }).ToList() ?? new List<PreferenceResponse>();

            return Ok(response);
        }
    }
}
