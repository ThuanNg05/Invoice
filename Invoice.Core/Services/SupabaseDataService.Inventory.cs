using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using static Supabase.Postgrest.Constants;

namespace Invoice.Core.Services;

public partial class SupabaseDataService
{
    public async Task UpdateProductInventory(long productId, int amountChange)
    {
        await EnsureConnectionAsync();
        
        // Use an atomic increment RPC to avoid race conditions
        await _client.Rpc("increment_inventory", new
        {
            p_id = productId.ToString(),
            p_change = amountChange
        });
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
                        smallPlanksMap[sizeId] += totalSmallPlanks;
                    else
                        smallPlanksMap[sizeId] = totalSmallPlanks;
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

        if (smallPlanksMap.Count > 0)
        {
            // Prepare data for the atomic RPC
            // This ensures material consumption, plank production, and logging happen in ONE transaction
            await _client.Rpc("process_frame_to_planks", new
            {
                p_frame_no = frame.FrameNO,
                p_amount = amount,
                p_material_id = sourcePlankId,
                p_small_planks = smallPlanksMap
            });
            
            _cache.Invalidate(InMemoryCache.MATERIALS);
            _cache.Invalidate(InMemoryCache.PLANKS);
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
