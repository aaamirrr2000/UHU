using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IEmployeesDevicesService
{
    Task<(bool, List<EmployeesDevicesModel>)>? Search(string Criteria = "");
    Task<(bool, EmployeesDevicesModel?)>? Get(int id);
    Task<(bool, EmployeesDevicesModel, string)> Post(EmployeesDevicesModel obj);
    Task<(bool, string)> Put(EmployeesDevicesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(EmployeesDevicesModel obj);
}