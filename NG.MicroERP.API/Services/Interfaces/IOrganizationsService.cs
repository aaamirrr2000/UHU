using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IOrganizationsService
{
    Task<(bool, List<OrganizationsModel>)>? Search(string Criteria = "");
    Task<(bool, OrganizationsModel?)>? Get(int id);
    Task<(bool, OrganizationsModel, string)> Post(OrganizationsModel obj);
    Task<(bool, string)> Put(OrganizationsModel obj);
    Task<(bool, string)> SetParent(OrganizationsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(OrganizationsModel obj);
}