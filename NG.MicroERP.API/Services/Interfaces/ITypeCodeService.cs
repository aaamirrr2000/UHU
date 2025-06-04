using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface ITypeCodeService
{
    Task<(bool, List<TypeCodeModel>)>? Search(string Criteria = "");
    Task<(bool, TypeCodeModel?)>? Get(int id);
    Task<(bool, TypeCodeModel, string)> Post(TypeCodeModel obj);
    Task<(bool, string)> Put(TypeCodeModel obj);
    Task<(bool, string)> Delete(int id);
}