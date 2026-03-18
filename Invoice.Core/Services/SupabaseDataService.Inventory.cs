using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using static Supabase.Postgrest.Constants;

namespace Invoice.Core.Services;

public partial class SupabaseDataService
{
    public async Task UpdateProductInventory(long productId, int amountChange)
    {
        await EnsureConnectionAsync();
        
        var productResponse = await _client.From<Products>()
                               .Where(p => p.ProductID == productId)
                               .Get();
        var product = productResponse.Models.FirstOrDefault();

        if (product != null)
        {
            await _client.From<Products>()
                         .Where(p => p.ProductID == productId)
                         .Set(x => x.Inventory, product.Inventory + amountChange) 
                         .Update();
            return;
        }

        var materialResponse = await _client.From<Materials>()
                                        .Where(m => m.ProductID == productId)
                                        .Get();
        var material = materialResponse.Models.FirstOrDefault();

        if (material != null)
        {
            await _client.From<Materials>()
                         .Where(m => m.ProductID == productId)
                         .Set(x => x.Inventory, material.Inventory + amountChange)
                         .Update();
            return;
        }

        var plankResponse = await _client.From<DetailPlanks>()
                                     .Where(p => p.sizeID == productId.ToString())
                                     .Get();
        var plank = plankResponse.Models.FirstOrDefault();
        if (plank != null)
        {
            await _client.From<DetailPlanks>()
                         .Where(p => p.sizeID == productId.ToString())
                         .Set(x => x.inventory, plank.inventory + amountChange)
                         .Update();
        }
    }
    
    public async Task AddWarehouseTransaction(WarehouseTransaction transaction)
    {
        await EnsureConnectionAsync();
        await _client.From<WarehouseTransaction>().Insert(transaction);
        await UpdateProductInventory(transaction.ProductID, transaction.FinalChange);
    }

    public async Task<IEnumerable<WarehouseTransaction>> GetWarehouseTransactions()
    {
        await EnsureConnectionAsync();
        var response = await _client.From<WarehouseTransaction>().Order("created_date", Ordering.Descending).Get();
        return response.Models;
    }

    public async Task ProcessInventoryTransaction(Frames frame, int amount, long? sourcePlankId = null)
    {
        await EnsureConnectionAsync();

        var smallPlanksMap = new Dictionary<string, int>();
        void ParseAndAccumulate(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return;

            int lastDashIndex = rawData.LastIndexOf('-');
            if (lastDashIndex > 0)
            {
                string sizeId = rawData[..lastDashIndex].Trim();
                string amountStr = rawData[(lastDashIndex + 1)..].Trim();

                if (int.TryParse(amountStr, out int amountPerFrame))
                {
                    int totalSmallPlanks = amountPerFrame * amount;

                    if (smallPlanksMap.ContainsKey(sizeId))
                    {
                        smallPlanksMap[sizeId] += totalSmallPlanks;
                    }
                    else
                    {
                        smallPlanksMap[sizeId] = totalSmallPlanks;
                    }
                }
            }
        }

        ParseAndAccumulate(frame.size1);
        ParseAndAccumulate(frame.size2);
        ParseAndAccumulate(frame.size3);
        ParseAndAccumulate(frame.size4);
        ParseAndAccumulate(frame.size5);
        ParseAndAccumulate(frame.size6);
        ParseAndAccumulate(frame.size7);
        ParseAndAccumulate(frame.size8);
        ParseAndAccumulate(frame.size9);
        ParseAndAccumulate(frame.size10);

        if (sourcePlankId.HasValue)
        {
            var materialResponse = await _client.From<Materials>()
                                        .Where(m => m.ProductID == sourcePlankId.Value)
                                        .Single();

            if (materialResponse != null)
            {                
                await _client.From<Materials>()
                             .Where(m => m.ProductID == sourcePlankId.Value)
                             .Set(x => x.Inventory, materialResponse.Inventory - amount)
                             .Update();

                var transOut = new WarehouseTransaction
                {
                    ProductID = sourcePlankId.Value,
                    Amount = -amount,
                    ActionType = "Xuất kho",
                    Note = $"Cắt rập: {frame.FrameNO}",
                    CreatedDate = DateTime.Now
                };
                await _client.From<WarehouseTransaction>().Insert(transOut);
            }
        }

        if(smallPlanksMap.Count > 0)
        {
            var sizeIds = smallPlanksMap.Keys.ToList();

            var existingPlanksResponse = await _client.From<DetailPlanks>()
                                             .Filter("size_id", Operator.In, sizeIds)
                                             .Get();

            var existingPlanks = existingPlanksResponse.Models;
            var planksToUpsert = new List<DetailPlanks>();
            var transactionsToInsert = new List<WarehouseTransaction>();

            foreach (var item in smallPlanksMap)
            {
                string sizeId = item.Key;
                int quantityToAdd = item.Value;
                int finalInventory = quantityToAdd;
                
                var existingItem = existingPlanks.FirstOrDefault(p => p.sizeID == sizeId);

                if (existingItem != null)
                {
                    finalInventory = existingItem.inventory + quantityToAdd;
                }
                
                planksToUpsert.Add(new DetailPlanks
                {
                    sizeID = sizeId,
                    inventory = finalInventory
                });

                if (long.TryParse(sizeId, out long sizeIdLong))
                {
                    transactionsToInsert.Add(new WarehouseTransaction
                    {
                        ProductID = sizeIdLong,
                        Amount = quantityToAdd,
                        ActionType = "Nhập kho",
                        Note = $"Cắt rập từ {amount} tấm lớn ({frame.FrameNO})",
                        CreatedDate = DateTime.Now
                    });
                }
            }

            if (planksToUpsert.Count > 0)
            {
                await _client.From<DetailPlanks>().Upsert(planksToUpsert);
            }
            
            if (transactionsToInsert.Count > 0)
            {
                await _client.From<WarehouseTransaction>().Insert(transactionsToInsert);
            }
        }        
    }        

    public async Task<bool> ValidateMaterialStock(long productId, int requiredAmount)
    {
        await EnsureConnectionAsync();

        try
        {            
            var response = await _client.From<Materials>()
                                        .Where(m => m.ProductID == productId)
                                        .Single();

            return response != null && response.Inventory >= requiredAmount;
        }
        catch
        {
            return false;
        }
    }
}
