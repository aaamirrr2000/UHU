using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class PartiesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? PartyType { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public string? ParentName { get; set; }
    public double CustomerRating { get; set; } = 0;
    public string? CustomerClass { get; set; } = string.Empty;
    public DateTime? CustomerSince { get; set; } = DateTime.Today;
    public int SalesPersonId { get; set; } = 0;
    public double CreditLimit { get; set; } = 0;
    public int PaymentTermsId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? Account { get; set; }
    public string? NTN { get; set; } = string.Empty;
    public string? STN { get; set; } = string.Empty;
    public int IsRegistered { get; set; }
    public int IsFiler { get; set; }
    public string? Address { get; set; } = string.Empty;
    public int CityId { get; set; } = 0;
    public string? AreaName { get; set; }
    public string? Latitude { get; set; } = string.Empty;
    public string? Longitude { get; set; } = string.Empty;
    public int Radius { get; set; } = 0;
    public string? ContactPerson { get; set; } = string.Empty;
    public string? ContactDesignation { get; set; } = string.Empty;
    public string? ContactEmail { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public int PriceListId { get; set; } = 0;  // References TypeCode.Id where ListName='PRICE LIST'
    public int IsActive { get; set; } = 0;
    public int IsApproved { get; set; } = 0;
    public int ApprovedBy { get; set; } = 0;
    public DateTime? ApprovedOn { get; set; } = DateTime.Today;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName => string.Format("{0} ({1})", Name, Code);

}

public class PartyBankDetailsModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PartyId { get; set; }
    public int BankId { get; set; }
    public string? BankName { get; set; }
    public string? AccountTitle { get; set; }
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? BranchCode { get; set; }
    public int IsPrimary { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
    public byte[]? RowVersion { get; set; }

}

public class PartyContactsModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PartyId { get; set; }
    public string? ContactType { get; set; }
    public string? ContactValue { get; set; }
    public int IsPrimary { get; set; }
    public string? PointOfContact { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
    public byte[]? RowVersion { get; set; }

}

public class PartyDocumentsModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PartyId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? PointOfContact { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
    public byte[]? RowVersion { get; set; }

}

public class PartyFinancialsModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PartyId { get; set; }
    public string? Description { get; set; }
    public string? ValueType { get; set; }
    public double Value { get; set; }
    public string? PercentageAmount { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
    public byte[]? RowVersion { get; set; }

}

public class PartyVehiclesModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int PartyId { get; set; } = 0;
    public string? VehicleRegNo { get; set; } = string.Empty;
    public string? EngineNo { get; set; } = string.Empty;
    public string? ChasisNo { get; set; } = string.Empty;
    public string? VehicleType { get; set; } = string.Empty;
    public string? Model { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();
    public string? MakeType { get; set; } = string.Empty;

}
