using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Abstractions.Repositories
{
    public interface IEmployeeService
    {
        //Счетчик увеличения промокодов
        Task UpdateAppliedPromocodeAsync(Guid employeeId);
    }
}
