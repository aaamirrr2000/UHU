using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IDineinOrderStatusService
{
    Task<(bool, List<DineinOrderStatusModel>)>? Search(string Criteria = "");
}
