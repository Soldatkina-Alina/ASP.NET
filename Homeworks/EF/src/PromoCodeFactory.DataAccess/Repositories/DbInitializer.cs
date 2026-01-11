using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.DataAccess.Data;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class DbInitializer: IDbInitializerRepository
    {
        private readonly DataContext context;

        public DbInitializer(DataContext dataContext)
        {
            context = dataContext;
        }

        public void InitializeDb()
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedDb();

            //context.AddRange(FakeDataFactory.Employees);
            //context.SaveChanges();

            //context.AddRange(FakeDataFactory.Preferences);
            //context.SaveChanges();

            //context.AddRange(FakeDataFactory.Customers);
            //context.SaveChanges();


            //ОШИБКА
            //context.AddRange(FakeDataFactory.Roles);
            //context.SaveChanges();

            //ОШИБКА
            //context.AddRange(FakeDataFactory.PromoCodes);
            //context.SaveChanges();

            LocalCheck();
        }
    
        public void CreateNewBD(DataContext dbcontext)
        {
            // Проверяем, если данные уже есть, не добавляем
            if (context.Roles.Any() || context.Preferences.Any() || context.Customers.Any())
            {
                return;
            }

            SeedDb();
        }

        private void SeedDb()
        {
            var (roles, employees, preferences, customers, promoCodes) = FakeData.GetAllData();

            context.Roles.AddRange(roles);
            context.SaveChanges();

            context.Preferences.AddRange(preferences);
            context.SaveChanges();

            context.Employees.AddRange(employees);
            context.SaveChanges();

            // Для CustomerPreferences нужно очистить навигационные свойства
            foreach (var customer in customers)
            {
                foreach (var cp in customer.Preferences)
                {
                    cp.Customer = null;
                    cp.Preference = null;
                }
            }
            context.Customers.AddRange(customers);
            context.SaveChanges();

            context.PromoCodes.AddRange(promoCodes);
            context.SaveChanges();

            LocalCheck();
        }

        private void LocalCheck()
        {
            //Проверки
            var employeesWithRoles = context.Employees
            .Include(e => e.Role)
            .ToList();

            var customers = context.Customers
            .Include(p => p.PromoCodes)
            .ToList();

            var prompcodes = context.PromoCodes
            .Include(c => c.Customer)
            .Include(p => p.Preference)
            .ToList();

            var Preference = context.Preferences.ToList();

            var Role = context.Roles.ToList(); // только две из трех, если из FakeDataFactory
        }

    }
}
