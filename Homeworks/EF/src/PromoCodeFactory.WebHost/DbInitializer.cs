using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.DataAccess.Data;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PromoCodeFactory.WebHost
{
    public static class DbInitializer 
    {
        private static DataContext context;

        public static void CreateNewBD(DataContext dbcontext)
        {
            context = dbcontext;
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedDb();
        }

        private static void SeedDb()
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

            //Проверка, что роли присоединились
            var employeesWithRoles = context.Employees
            .Include(e => e.Role)
            .ToList();
        }

    }
}
