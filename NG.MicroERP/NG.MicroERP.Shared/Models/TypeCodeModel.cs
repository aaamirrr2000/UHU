using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TypeCodeModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? ListName { get; set; } = string.Empty;
    public string? ListValue { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public int SeqNo { get; set; } = 0;

}