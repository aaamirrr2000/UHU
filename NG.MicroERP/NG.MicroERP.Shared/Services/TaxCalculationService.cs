using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Services;

public class TaxCalculationService
{
    public double GST { get; private set; } = 0;

    public async Task InitializeAsync()
    {
        GST = 0;

        var Data = await Functions.GetAsync<List<TypeCodeModel>>($"TypeCode/Search/GST", true) ?? new List<TypeCodeModel>();

        if (Data.Count > 0)
        {
            var res = Data.FirstOrDefault()?.ListValue ?? "0";
            if (double.TryParse(res.Replace("%", "").Trim(), out double percent))
            {
                GST = percent;
            }
        }
    }
}

