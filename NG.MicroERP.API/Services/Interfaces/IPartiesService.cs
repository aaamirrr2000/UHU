using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IPartiesService
{
    Task<(bool, List<PartiesModel>)>? Search(string Criteria = "");
    Task<(bool, PartiesModel?)>? Get(int id);
    Task<(bool, PartiesModel, string)> Post(PartiesModel obj);
    Task<(bool, string)> Put(PartiesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartiesModel obj);
}

