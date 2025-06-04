using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IBillDetailService
{
    Task<(bool, List<BillDetailModel>)>? Search(string Criteria = "");
    Task<(bool, BillDetailModel?)>? Get(int id);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> BillItemStatus(int BillDetailId, string Status);
}

public class BillDetailService : IBillDetailService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BillDetailModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"

                        SELECT
                          Bill.SeqNo,
                          BillType,
                          Bill.PartyId,
                          Parties.Name as PartyName,
                          Bill.LocationId,
                          Locations.Name as LocationName,
                          BillDetail.Id,
                          BillDetail.ItemId,
                          Items.Name as Item,
                          BillDetail.StockCondition,
                          BillDetail.Qty,
                          BillDetail.UnitPrice,
                          BillDetail.DiscountAmount,
                          BillDetail.TaxAmount,
                          BillDetail.BillId,
                          Items.StockType,
                          BillDetail.Description,
                          BillDetail.ServingSize,
                          BillDetail.Status,
                          BillDetail.IsTakeAway
                        FROM BillDetail
                        LEFT JOIN Items on Items.id=BillDetail.itemid
                        LEFT JOIN Bill on Bill.id=BillDetail.billid
                        LEFT JOIN Parties on Parties.id=Bill.PartyId
                        LEFT JOIN Locations on Locations.id=Bill.LocationId

                    ";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " where " + Criteria;

        SQL += " Order by BillDetail.Id Desc";

        List<BillDetailModel> result = (await dapper.SearchByQuery<BillDetailModel>(SQL)) ?? new List<BillDetailModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, BillDetailModel?)>? Get(int id)
    {
        BillDetailModel result = (await dapper.SearchByID<BillDetailModel>("BillDetail", id)) ?? new BillDetailModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("BillDetail", id);
    }

    public async Task<(bool, string)> BillItemStatus(int BillDetailId, string Status)
    {
        try
        {
            string SQLUpdate = $@"UPDATE BillDetail SET 
					Status = '{Status!.ToUpper()}', 
					TranDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'
				WHERE Id = {BillDetailId};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }
}

