using System.Data;
using System.Diagnostics;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using Supabase.Realtime.PostgresChanges;
using static Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;

namespace Invoice.Core.Services;

public class SupabaseDataService : IDataService
{
    private readonly SupabaseClient _client;
    private readonly InMemoryCache _cache = new();



    public bool IsCacheValid(string entityName) => _cache.IsValid(entityName);
    public void InvalidateCache(string entityName) => _cache.Invalidate(entityName);
    public void InvalidateAllCaches() => _cache.Clear();



    public IEnumerable<Customers> CachedCustomers { get; private set; } = [];
    public IEnumerable<Materials> CachedMaterials { get; private set; } = [];
    public IEnumerable<Frames> CachedFrames { get; private set; } = [];
    public IEnumerable<DetailPlanks> CachedPlanks { get; private set; } = [];
    public IEnumerable<DetailPrice> CachedPrices { get; private set; } = [];

    public SupabaseDataService(SupabaseClient client) => _client = client;

    private async Task EnsureConnectionAsync()
    {
        try
        {
            if (_client.Auth.CurrentSession == null)
            {
                await _client.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Supabase Initialization Error: {ex.Message}");
            throw new Exception("Không thể kết nối với máy chủ Supabase. Vui lòng kiểm tra kết nối mạng.", ex);
        }
    }



    // Customers
    public async Task<IEnumerable<Customers>> GetCustomers(bool forceRefresh = false)
    {
        //if (!forceRefresh && CachedCustomers.Any())
        //    return CachedCustomers;

        //await EnsureConnectionAsync();
        //var response = await _client.From<Customers>().Get();
        //CachedCustomers = response.Models.OrderBy(c => c.Name).ToList();        
        //return CachedCustomers;

        if (!forceRefresh && _cache.TryGet<List<Customers>>(InMemoryCache.CUSTOMERS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Customers");
            return cached;
        }
        
        Debug.WriteLine("[CACHE MISS] Customers — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<Customers>().Get();
        var sorted = response.Models.OrderBy(c => c.Name).ToList();

        _cache.Set(InMemoryCache.CUSTOMERS, sorted, TimeSpan.FromMinutes(5));

        return sorted;
    }
    public async Task AddCustomer(Customers customer)
    {
        //await EnsureConnectionAsync();        
        //var response = await _client.From<Customers>().Insert(customer);
        //var newCustomer = response.Models.FirstOrDefault();
        //if (newCustomer != null)
        //{
        //    customer.CustomerID = newCustomer.CustomerID;
        //    var list = CachedCustomers.ToList();
        //    list.Add(customer);
        //    CachedCustomers = list;
        //}        

        await EnsureConnectionAsync();
        var response = await _client.From<Customers>().Insert(customer);
        var newCustomer = response.Models.FirstOrDefault();

        if (newCustomer != null)
        {
            customer.CustomerID = newCustomer.CustomerID;
        }

        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }
    public async Task DeleteCustomer(long customerId)
    {
        //await EnsureConnectionAsync();
        //await _client.From<Customers>().Where(c => c.CustomerID == customerId).Delete();
        //var list = CachedCustomers.ToList();
        //var item = list.FirstOrDefault(c => c.CustomerID == customerId);
        //if (item != null)
        //{
        //    list.Remove(item);
        //    CachedCustomers = list;
        //}
        await EnsureConnectionAsync();
        await _client.From<Customers>().Where(c => c.CustomerID == customerId).Delete();
        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }
    public async Task UpdateCustomer(Customers customer)
    {
        //await EnsureConnectionAsync();
        //await _client.From<Customers>().Update(customer);

        //var list = CachedCustomers.ToList();
        //var item = list.FirstOrDefault(c => c.CustomerID == customer.CustomerID);
        //if (item != null)
        //{
        //    item.Name = customer.Name;
        //    item.Phone = customer.Phone;
        //    item.PriceGroup = customer.PriceGroup;
        //    CachedCustomers = list;
        //}

        await EnsureConnectionAsync();
        await _client.From<Customers>().Update(customer);
        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }
    


    // Materials
    public async Task<IEnumerable<Materials>> GetMaterials(bool forceRefresh = false)
    {
        //await EnsureConnectionAsync();
        //var response = await _client.From<Materials>().Get();
        //var sorted = response.Models.OrderBy(m => m.Name).ToList();
        //CachedMaterials = sorted;
        //return sorted;
        if (!forceRefresh && _cache.TryGet<List<Materials>>(InMemoryCache.MATERIALS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Materials");
            return cached;
        }

        Debug.WriteLine("[CACHE MISS] Materials — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<Materials>().Get();
        var sorted = response.Models.OrderBy(m => m.Name).ToList();

        _cache.Set(InMemoryCache.MATERIALS, sorted, TimeSpan.FromMinutes(5));
        return sorted;
    }
    public async Task AddMaterial(Materials material)
    {
        //await EnsureConnectionAsync();
        //var response = await _client.From<Materials>().Insert(material);
        //var newMaterial = response.Models.FirstOrDefault();
        //if (newMaterial != null)
        //{
        //    material.ProductID = newMaterial.ProductID;
        //    var list = CachedMaterials.ToList();
        //    list.Add(material);
        //    CachedMaterials = list;
        //}
        await EnsureConnectionAsync();
        var response = await _client.From<Materials>().Insert(material);
        var newMaterial = response.Models.FirstOrDefault();

        if (newMaterial != null)
        {
            material.ProductID = newMaterial.ProductID;
        }

        _cache.Invalidate(InMemoryCache.MATERIALS);
    }
    public async Task DeleteMaterial(long productId)
    {
        //await EnsureConnectionAsync();
        //await _client.From<Materials>().Where(m => m.ProductID == productId).Delete();
        //var list = CachedMaterials.ToList();
        //var item = list.FirstOrDefault(m => m.ProductID == productId);
        //if (item != null)
        //{
        //    list.Remove(item);
        //    CachedMaterials = list;
        //}
        await EnsureConnectionAsync();
        await _client.From<Materials>().Where(m => m.ProductID == productId).Delete();
        _cache.Invalidate(InMemoryCache.MATERIALS);
    }
    public async Task UpdateMaterial(Materials material)
    {
        //await EnsureConnectionAsync();
        //await _client.From<Materials>().Update(material);

        //var list = CachedMaterials.ToList();        
        //var item = list.FirstOrDefault(m => m.ProductID == material.ProductID);
        //if (item != null)
        //{
        //    item.Name = material.Name;
        //    item.BasePrice = material.BasePrice;
        //    item.Inventory = material.Inventory;
        //    item.MinAmount = material.MinAmount;
        //    CachedMaterials = list;
        //}
        await EnsureConnectionAsync();
        await _client.From<Materials>().Update(material);
        _cache.Invalidate(InMemoryCache.MATERIALS);
    }
    


    // Frames
    public async Task<IEnumerable<Frames>> GetFrames(bool forceRefresh = false)
    {
        //await EnsureConnectionAsync();
        //var response = await _client.From<Frames>().Get();
        //var sorted = response.Models.OrderBy(p => p.FrameNO).ToList();
        //CachedFrames = sorted;
        //return sorted;
        if (!forceRefresh && _cache.TryGet<List<Frames>>(InMemoryCache.FRAMES, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Frames");
            return cached;
        }

        Debug.WriteLine("[CACHE MISS] Frames — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<Frames>().Get();
        var sorted = response.Models.OrderBy(p => p.FrameNO).ToList();

        _cache.Set(InMemoryCache.FRAMES, sorted, TimeSpan.FromMinutes(5));
        return sorted;
    }
    public async Task AddFrame(Frames frame)
    {
        //await EnsureConnectionAsync();
        //var response = await _client.From<Frames>().Insert(frame);
        //var newFrame = response.Models.FirstOrDefault();
        //if (newFrame != null)
        //{
        //    frame.FrameNO = newFrame.FrameNO;
        //    var list = CachedFrames.ToList();
        //    list.Add(frame);
        //    CachedFrames = list;
        //}
        await EnsureConnectionAsync();
        var response = await _client.From<Frames>().Insert(frame);
        var newFrame = response.Models.FirstOrDefault();

        if (newFrame != null)
        {
            frame.FrameNO = newFrame.FrameNO;
        }

        _cache.Invalidate(InMemoryCache.FRAMES);
    }
    public async Task DeleteFrame(int? frameID)
    {
        //await EnsureConnectionAsync();
        //if (frameID == null) return;
        //await _client.From<Frames>().Where(p => p.FrameID == frameID).Delete();
        //var list = CachedFrames.ToList();
        //var item = list.FirstOrDefault(p => p.FrameID == frameID);
        //if (item != null)
        //{
        //    list.Remove(item);
        //    CachedFrames = list;
        //}
        await EnsureConnectionAsync();
        if (frameID == null)
        {
            return;
        }

        await _client.From<Frames>().Where(p => p.FrameID == frameID).Delete();
        _cache.Invalidate(InMemoryCache.FRAMES);
    }
    public async Task UpdateFrame(Frames frame)
    {
        //await EnsureConnectionAsync();
        //await _client.From<Frames>().Update(frame);
        //var list = CachedFrames.ToList();
        //var item = list.FirstOrDefault(p => p.FrameID == frame.FrameID);
        //if (item != null)
        //{
        //    item.FrameNO = frame.FrameNO;                        
        //    item.size1 = frame.size1;
        //    item.size2 = frame.size2;
        //    item.size3 = frame.size3;
        //    item.size4 = frame.size4;
        //    item.size5 = frame.size5;
        //    item.size6 = frame.size6;
        //    item.size7 = frame.size7;
        //    item.size8 = frame.size8;
        //    item.size9 = frame.size9;
        //    item.size10 = frame.size10;
        //    item.Description = frame.Description;
        //    CachedFrames = list;
        //}
        await EnsureConnectionAsync();
        await _client.From<Frames>().Update(frame);
        _cache.Invalidate(InMemoryCache.FRAMES);
    }



    // Planks
    public async Task<IEnumerable<DetailPlanks>> GetPlanks(bool forceRefresh = false)
    {
        if(!forceRefresh && _cache.TryGet<List<DetailPlanks>>(InMemoryCache.PLANKS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Planks");
            return cached;
        }
        Debug.WriteLine("[CACHE MISS] Planks — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<DetailPlanks>().Get();
        var sorted = response.Models.OrderBy(p => p.sizeID).ToList();

        _cache.Set(InMemoryCache.PLANKS, sorted, TimeSpan.FromMinutes(5));
        return sorted;
    }
    public async Task AddPlank(DetailPlanks plank)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<DetailPlanks>().Insert(plank);
        var newPlank = response.Models.FirstOrDefault();
        if (newPlank != null)
        {
            plank.sizeID = newPlank.sizeID;
            var list = CachedPlanks.ToList();
            list.Add(plank);
            CachedPlanks = list;
        }
    }
    public async Task DeletePlank(string plankId)
    {
        await EnsureConnectionAsync();
        if (plankId == null)
        {
            return;
        }

        await _client.From<DetailPlanks>().Where(p => p.sizeID == plankId).Delete();
        var list = CachedPlanks.ToList();
        var item = list.FirstOrDefault(p => p.sizeID == plankId);
        if (item != null)
        {
            list.Remove(item);
            CachedPlanks = list;
        }
    }
    public async Task UpdatePlank(DetailPlanks plank)
    {
        await EnsureConnectionAsync();
        await _client.From<DetailPlanks>().Update(plank);
        var list = CachedPlanks.ToList();
        var item = list.FirstOrDefault(p => p.sizeID == plank.sizeID);
        if (item != null)
        {
            item.sizeID = plank.sizeID;
            CachedPlanks = list;
        }
    }



    // Detail prices
    public async Task<IEnumerable<DetailPrice>> GetPrice()
    {
        await EnsureConnectionAsync();        
        var response = await _client.From<DetailPrice>().Get();          
        CachedPrices = response.Models;    
        return response.Models;
    }
    public async Task UpdatePrice(DetailPrice prices)
    {
        await EnsureConnectionAsync();
        await _client.From<DetailPrice>()
                 .Where(x => x.ConfigID == prices.ConfigID)
                 .Update(prices);        

        var list = CachedPrices.ToList();
        var item = list.FirstOrDefault(p => p.ConfigID == prices.ConfigID);
        if (item != null)
        {
            item.ConfigID = prices.ConfigID;
            item.PrKieng = prices.PrKieng;
            item.PrNhL = prices.PrNhL;
            item.PrNhN = prices.PrNhN;
            item.PrG_l = prices.PrG_l;
            item.PrG_n = prices.PrG_n;
            item.PrDl = prices.PrDl;
            item.PrHau = prices.PrHau;
            item.PrLua = prices.PrLua;
            item.PrKt = prices.PrKt;
            item.PrOc = prices.PrOc;
            item.PrNhom = prices.PrNhom;
            item.Pr7f = prices.Pr7f;
            item.Pr2D = prices.Pr2D;
            item.PrDecal = prices.PrDecal;
            
            CachedPrices = list;
        }
    }



    // Invoice History    
    public async Task<IEnumerable<History>> GetInvoiceHistory(DateTime? fromDate, DateTime? toDate, long? customerId)
    {
        await EnsureConnectionAsync();        
        var query = _client.From<InvoiceDetail>()
                       .Select("*, Invoice:invoices!inner(*)");        

        if (fromDate.HasValue)
        {            
            string fromStr = fromDate.Value.ToString("yyyy-MM-dd");
            query = query.Filter("Invoice.created_date", Operator.Equals, fromStr);

            if (toDate.HasValue)
            {                
                string toStr = toDate.Value.ToString("yyyy-MM-dd");
                query = query.Filter("Invoice.created_date", Operator.GreaterThanOrEqual, fromStr)
                             .Filter("Invoice.created_date", Operator.LessThanOrEqual, toStr);
            }            
        }

        if (customerId.HasValue)
        {            
            query = query.Filter("Invoice.customer_id", Operator.Equals, customerId.Value.ToString());
        }

        var response = await query.Get();
        
        var result = response.Models.Select(d => new History
        {
            InvoiceID = d.InvoiceID,            
            CreatedDate = DateTime.TryParse(d.Invoice?.CreatedDate, out var date) ? date : DateTime.MinValue,
            CustomerName = d.CustomerName,
            ProductID = d.ProductID,
            ProductName = d.ProductName,
            SellPrice = d.SellPrice,
            Amount = d.Amount,
            LineTotal = d.LineTotal ?? 0,
            Note = d.Note
        });

        return [.. result.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.InvoiceID)];
    }



    // Transactions Frames
    public async Task ProcessInventoryTransaction(Frames frame, int amount, long? sourcePlankId = null)
    {
        await EnsureConnectionAsync();

        // Mapping split string with "-" to sizeID and amount, then accumulate total small planks
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

        // Check what kind of plank (HP or MDF)
        if (sourcePlankId.HasValue)
        {
            // Get material where id = sourcePlankId
            var materialResponse = await _client.From<Materials>()
                                        .Where(m => m.ProductID == sourcePlankId.Value)
                                        .Single();

            // Update inventory of big plank
            if (materialResponse != null)
            {                
                await _client.From<Materials>()
                             .Where(m => m.ProductID == sourcePlankId.Value)
                             .Set(x => x.Inventory, materialResponse.Inventory - amount)
                             .Update();

                // Save transaction for big plank OUT
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

                // DetailPlanks.sizeID is string, but ProductID in WarehouseTransaction is long.
                // This logic might need further review if sizeID references Products/Materials.
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



    // Products
    public async Task<IEnumerable<ProductSummary>> GetProducts(int skip, int take, string query)
    {
        await EnsureConnectionAsync();

        var request = _client.From<Products>().Select("product_id,name,base_price,price_odd,price_even,inventory");
        if (!string.IsNullOrEmpty(query))
        {
            request = request.Filter("name", Operator.ILike, $"%{query}%");
        }

        var response = await request
                            .Range(skip, skip + take - 1)
                            .Order("product_id", Ordering.Ascending)
                            .Get();
        
        return response.Models.Select(p => new ProductSummary
        {
            ProductID = p.ProductID,
            Name = p.Name,
            BasePrice = p.BasePrice,
            PriceOdd = p.PriceOdd,
            PriceEven = p.PriceEven,
            Inventory = p.Inventory
        });
    }
    public async Task<Products> GetProductById(long productId)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Products>()
                                    .Where(p => p.ProductID == productId)
                                    .Single();
        return response;
    }
    public async Task AddProduct(Products product)
    {
        await EnsureConnectionAsync();
        await _client.From<Products>().Insert(product);
    }
    public async Task UpdateProduct(Products product)
    {
        await EnsureConnectionAsync();
        await _client.From<Products>().Update(product);
    }
    public async Task DeleteProduct(long productId)
    {
        await EnsureConnectionAsync();
        await _client.From<Products>().Where(p => p.ProductID == productId).Delete();
    }



    // Helper functions for Invoices and Inventory
    public async Task<IEnumerable<InvoiceDetail>> GetInvoiceDetails(string invoiceID)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<InvoiceDetail>().Where(i => i.InvoiceID == invoiceID).Get();
        return response.Models;
    }

    public async Task<int> GetInvoiceCountByDate(DateTime date)
    {
        await EnsureConnectionAsync();
        string dateString = date.ToString("yyyy-MM-dd");
        var response = await _client.From<Invoices>().Where(x => x.CreatedDate == dateString)
            .Count(CountType.Exact);
        return response;
    }

    public async Task AddInvoice(Invoices invoice, IEnumerable<InvoiceDetail> details, IEnumerable<WarehouseTransaction> transactions)
    {
        await EnsureConnectionAsync();

        try
        {            
            await _client.Rpc("create_full_invoice", new
            {
                invoice_data = invoice,
                details_data = details ?? new List<InvoiceDetail>(),
                transactions_data = transactions ?? new List<WarehouseTransaction>()
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create invoice: {ex.Message}");            
        }
    }
    
    public async Task DeleteInvoiceAndRevertInventory(string invoiceId)
    {
        await EnsureConnectionAsync();

        try 
        {            
            await _client.Rpc("delete_invoice_and_revert", new
            {
                p_invoice_id = invoiceId
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi xoá hoá đơn: {ex.Message}");
        }
    }
    
    public async Task UpdateProductInventory(long productId, int amountChange)
    {
        await EnsureConnectionAsync();
        var productResponse = await _client.From<Products>()
                               .Where(p => p.ProductID == productId)
                               .Get();
        var product = productResponse.Models.FirstOrDefault();

        if (product != null)
        {
            product.Inventory += amountChange;                    
            await _client.From<Products>()
                         .Where(p => p.ProductID == productId)
                         .Set(x => x.Inventory, product.Inventory) 
                         .Update();
            return;
        }

        var materialResponse = await _client.From<Materials>()
                                        .Where(m => m.ProductID == productId)
                                        .Get();
        var material = materialResponse.Models.FirstOrDefault();

        if (material != null)
        {
            material.Inventory += amountChange;
            await _client.From<Materials>()
                         .Where(m => m.ProductID == productId)
                         .Set(x => x.Inventory, material.Inventory)
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

    public async Task<IEnumerable<Products>> GetAllProducts()
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Products>()
                                    .Order("product_id", Ordering.Ascending).Get();
        return response.Models;
    }

    public async Task SubscribeToProductsRealtime(Action<string, Products> onDataChanged)
    {
        try
        {
            await EnsureConnectionAsync();

            // Safely attempt to connect. ConnectAsync is idempotent in most Supabase SDK versions
            // or we just catch if it's already connected.
            await _client.Realtime.ConnectAsync();

            await _client.From<Products>()
                         .On(PostgresChangesOptions.ListenType.All, (sender, change) =>
                         {
                             var product = change.Model<Products>();
                             onDataChanged?.Invoke(change.Event.ToString().ToUpper(), product);
                         });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Realtime Subscription Error: {ex.Message}");
            // We don't throw here to avoid crashing the UI if realtime fails, 
            // as the app can still function with REST.
        }
    }

    public async Task<IEnumerable<Invoices>> GetAllInvoices()
    {
        await EnsureConnectionAsync();        
        var response = await _client.From<Invoices>().Get();        
        return response.Models;
    }
   
}