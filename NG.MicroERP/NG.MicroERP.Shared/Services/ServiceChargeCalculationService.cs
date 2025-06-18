using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Services;

public class ServiceChargeCalculationService
{
    public string ServiceChargeType { get; private set; } = "NONE";
    public double ServiceCharge { get; private set; } = 0;

    public async Task InitializeAsync()
    {
        ServiceChargeType = "NONE";
        ServiceCharge = 0;

        var serviceChargeData = await Functions.GetAsync<List<TypeCodeModel>>($"TypeCode/Search/SERVICE CHARGES", true) ?? new List<TypeCodeModel>();

        if (serviceChargeData.Count > 0)
        {
            var saRaw = serviceChargeData.FirstOrDefault()?.ListValue ?? "0";

            if (saRaw.Contains("%"))
            {
                if (double.TryParse(saRaw.Replace("%", "").Trim(), out double percent))
                {
                    ServiceChargeType = "PERCENTAGE";
                    ServiceCharge = percent;
                }
            }
            else if (double.TryParse(saRaw, out double amount))
            {
                ServiceChargeType = "AMOUNT";
                ServiceCharge = amount;
            }
        }
    }
}

