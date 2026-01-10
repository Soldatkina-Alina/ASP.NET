using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Data
{
    public static class FakeData
    {
        public static (List<Role> roles, List<Employee> employees, List<Preference> preferences,List<Customer> customers, List<PromoCode> promoCodes) GetAllData()
        {
            // 1. Создаем Роли (каждый экземпляр уникальный)
            var adminRole = new Role()
            {
                Id = Guid.Parse("53729686-a368-4eeb-8bfa-cc69b6050d02"),
                Name = "Admin",
                Description = "Администратор",
            };

            var managerRole = new Role()
            {
                Id = Guid.Parse("b0ae7aac-5493-45cd-ad16-87426a5e7665"),
                Name = "PartnerManager",
                Description = "Партнерский менеджер"
            };

            var roles = new List<Role> { adminRole, managerRole };

            // 2. Создаем Preferences
            var theaterPreference = new Preference()
            {
                Id = Guid.Parse("ef7f299f-92d7-459f-896e-078ed53ef99c"),
                Name = "Театр",
            };

            var familyPreference = new Preference()
            {
                Id = Guid.Parse("c4bda62e-fc74-4256-a956-4760b3858cbd"),
                Name = "Семья",
            };

            var childrenPreference = new Preference()
            {
                Id = Guid.Parse("76324c47-68d2-472d-abb8-33cfa8cc0c84"),
                Name = "Дети",
            };

            var preferences = new List<Preference>
            {
                theaterPreference,
                familyPreference,
                childrenPreference
            };

            // 3. Создаем Employees с ссылками на уникальные Role объекты
            var employee1 = new Employee()
            {
                Id = Guid.Parse("451533d5-d8d5-4a11-9c7b-eb9f14e1a32f"),
                Email = "owner@somemail.ru",
                FirstName = "Иван",
                LastName = "Сергеев",
                Role = adminRole, 
                AppliedPromocodesCount = 5
            };

            var employee2 = new Employee()
            {
                Id = Guid.Parse("f766e2bf-340a-46ea-bff3-f1700b435895"),
                Email = "andreev@somemail.ru",
                FirstName = "Петр",
                LastName = "Андреев",
                Role = managerRole, 
                AppliedPromocodesCount = 10
            };

            var employees = new List<Employee> { employee1, employee2 };

            // 4. Создаем Customers с CustomerPreferences
            var customerId = Guid.Parse("a6c8c6b1-4349-45b0-ab31-244740aaf0f0");
            var customer = new Customer()
            {
                Id = customerId,
                Email = "ivan_sergeev@mail.ru",
                FirstName = "Иван",
                LastName = "Петров",
                PromoCodes = null,
                Preferences = new List<CustomerPreference>()
            };

            // Создаем CustomerPreference со ссылками на объекты
            var customerPreference = new CustomerPreference
            {
                CustomerId = customerId,
                PreferenceId = theaterPreference.Id,
                Customer = customer,
                Preference = theaterPreference
            };

            var customerPreferenceFamilyPreference = new CustomerPreference
            {
                CustomerId = customerId,
                PreferenceId = familyPreference.Id,
                Customer = customer,
                Preference = familyPreference
            };

            // Добавляем связь в коллекцию
            customer.Preferences.Add(customerPreference);
            customer.Preferences.Add(customerPreferenceFamilyPreference);
            // Также добавляем обратную связь в Preference (если есть коллекция)

            var customers = new List<Customer> { customer };

            // 5. Создаем PromoCodes со ссылками на объекты
            var promoCode = new PromoCode()
            {
                Id = Guid.Parse("d3b5f4e1-3c4a-4f7e-9f0e-1234567890ab"),
                Code = "PROMO2024",
                ServiceInfo = "Промокод на скидку 20% в 2026 году",
                BeginDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                PartnerName = "Компания А",
                Customer = customer, // Ссылка на уникальный объект customer
                Preference = theaterPreference // Ссылка на уникальный объект Preference
            };

            var promoCodes = new List<PromoCode> { promoCode };

            return (roles, employees, preferences, customers, promoCodes);
        }
    }
}
