using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IInvoiceDetailService
{
    Task<(bool, List<InvoiceDetailModel>)>? Search(string Criteria = "");
    Task<(bool, InvoiceDetailModel?)>? Get(int id);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> InvoiceItemStatus(int InvoiceDetailId, string Status, int SoftDelete=0);
}

public class InvoiceDetailService : IInvoiceDetailService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<InvoiceDetailModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"

                        SELECT
                          Invoice.SeqNo,
                          InvoiceType,
                          Invoice.PartyId,
                          Parties.Name as PartyName,
                          Invoice.LocationId,
                          Locations.Name as LocationName,
                          Invoice.TableId,
                          RestaurantTables.TableNumber,
                          RestaurantTables.TableLocation,
                          InvoiceDetail.Id,
                          InvoiceDetail.ItemId,
                          Items.Name as Item,
                          InvoiceDetail.StockCondition,
                          InvoiceDetail.Qty,
                          InvoiceDetail.UnitPrice,
                          InvoiceDetail.DiscountAmount,
                          InvoiceDetail.InvoiceId,
                          Items.StockType,
                          InvoiceDetail.Description,
                          InvoiceDetail.ServingSize,
                          Invoice.Status as InvoiceStatus,
                          InvoiceDetail.Status as InvoiceDetailStatus,
                          InvoiceDetail.Person
                        FROM InvoiceDetail
                        LEFT JOIN Items on Items.id=InvoiceDetail.itemid
                        LEFT JOIN Invoice on Invoice.id=InvoiceDetail.Invoiceid
                        LEFT JOIN Parties on Parties.id=Invoice.PartyId
                        LEFT JOIN Locations on Locations.id=Invoice.LocationId
                        LEFT JOIN RestaurantTables on RestaurantTables.id=Invoice.TableId
                        WHERE InvoiceDetail.IsSoftDeleted = 0

                    ";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by InvoiceDetail.Id Desc";

        List<InvoiceDetailModel> result = (await dapper.SearchByQuery<InvoiceDetailModel>(SQL)) ?? new List<InvoiceDetailModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, InvoiceDetailModel?)>? Get(int id)
    {
        InvoiceDetailModel result = (await dapper.SearchByID<InvoiceDetailModel>("InvoiceDetail", id)) ?? new InvoiceDetailModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("InvoiceDetail", id);
    }

    public async Task<(bool, string)> InvoiceItemStatus(int InvoiceDetailId, string Status, int SoftDelete=0)
    {
        try
        {
            string SQLUpdate = $@"UPDATE InvoiceDetail SET 
					                Status = '{Status!.ToUpper()}', 
					                TranDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                                    IsSoftDeleted = {SoftDelete}
				                WHERE Id = {InvoiceDetailId};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }
}

