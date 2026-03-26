using System.Diagnostics;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Supabase.Realtime.PostgresChanges;
using static Supabase.Postgrest.Constants;

namespace Invoice.Core.Services;

public partial class SupabaseDataService
{
    // Products
    public async Task<IEnumerable<ProductSummary>> GetProducts(int skip, int take, string query)
    {
        await EnsureConnectionAsync();

        var request = _client.From<Products>()
                             .Select("product_id,name,base_price,price_odd,price_even,inventory")
                             .Where(p => p.Status == 1);
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

    public async Task<Products?> GetProductById(long productId)
    {
        await EnsureConnectionAsync();
        
        var productResponse = await _client.From<Products>()
                                    .Where(p => p.ProductID == productId)
                                    .Get();
        var product = productResponse.Models.FirstOrDefault();
        
        if (product != null) 
        {
            return product;
        }

        var materialResponse = await _client.From<Materials>()
                                        .Where(m => m.ProductID == productId)
                                        .Get();
        var material = materialResponse.Models.FirstOrDefault();
        if (material != null)
        {
            return new Products
            {
                ProductID = material.ProductID,
                Name = material.Name,
                BasePrice = (double)material.BasePrice,
                Inventory = material.Inventory
            };
        }

        var plankResponseDirect = await _client.From<DetailPlanks>()
                                     .Where(p => p.sizeID == productId.ToString())
                                     .Get();
        var plankDirect = plankResponseDirect.Models.FirstOrDefault();
        if (plankDirect != null)
        {
            return new Products
            {
                ProductID = productId,
                Name = $"Ván {plankDirect.sizeID}",
                Inventory = plankDirect.inventory
            };
        }

        return null;
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

    public async Task<Products?> GetProductByName(string name)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Products>()
                                    .Where(p => p.Name == name)
                                    .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task HardDeleteProduct(long productId)
    {
        await EnsureConnectionAsync();
        await _client.From<Products>()
                    .Where(p => p.ProductID == productId)
                    .Delete();
    }

    public async Task DeleteProduct(long productId)
    {
        await EnsureConnectionAsync();
        await _client.From<Products>()
                    .Where(p => p.ProductID == productId)
                    .Set(p => p.Status, 0)
                    .Update();
    }

    public async Task<IEnumerable<Products>> GetAllProducts(bool forceRefresh = false)
    {
        if (!forceRefresh && _cache.TryGet<List<Products>>(InMemoryCache.PRODUCTS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Products");
            return cached;
        }

        Debug.WriteLine("[CACHE MISS] Products — fetching from server");
        await EnsureConnectionAsync();
        var response = await _client.From<Products>()
                                    .Where(p => p.Status == 1)
                                    .Order("product_id", Ordering.Ascending).Get();
        
        var products = response.Models;
        _cache.Set(InMemoryCache.PRODUCTS, products, TimeSpan.FromMinutes(5));
        return products;
    }

    public async Task SubscribeToProductsRealtime(Action<string, Products> onDataChanged)
    {
        try
        {
            await EnsureConnectionAsync();
            await _client.Realtime.ConnectAsync();

            await _client.From<Products>()
                         .On(PostgresChangesOptions.ListenType.All, (sender, change) =>
                         {
                             var product = change.Model<Products>();
                             if (product != null && product.Status == 1)
                             {
                                 onDataChanged?.Invoke(change.Event.ToString().ToUpper(), product);
                             }
                         });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Realtime Subscription Error: {ex.Message}");
        }
    }

    // Materials
    public async Task<IEnumerable<Materials>> GetMaterials(bool forceRefresh = false)
    {
        if (!forceRefresh && _cache.TryGet<List<Materials>>(InMemoryCache.MATERIALS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Materials");
            return cached;
        }

        Debug.WriteLine("[CACHE MISS] Materials — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<Materials>()
                                    .Where(m => m.Status == 1)
                                    .Get();
        var sorted = response.Models.OrderBy(m => m.Name).ToList();

        _cache.Set(InMemoryCache.MATERIALS, sorted, TimeSpan.FromMinutes(5));
        return sorted;
    }

    public async Task AddMaterial(Materials material)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Materials>().Insert(material);
        var newMaterial = response.Models.FirstOrDefault();

        if (newMaterial != null)
        {
            material.ProductID = newMaterial.ProductID;
        }

        _cache.Invalidate(InMemoryCache.MATERIALS);
    }

    public async Task<Materials?> GetMaterialByName(string name)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Materials>()
                                    .Where(m => m.Name == name)
                                    .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task HardDeleteMaterial(long productId)
    {
        await EnsureConnectionAsync();
        await _client.From<Materials>()
                    .Where(m => m.ProductID == productId)
                    .Delete();
        _cache.Invalidate(InMemoryCache.MATERIALS);
    }

    public async Task DeleteMaterial(long productId)
    {
        await EnsureConnectionAsync();
        await _client.From<Materials>()
                    .Where(m => m.ProductID == productId)
                    .Set(m => m.Status, 0)
                    .Update();
        _cache.Invalidate(InMemoryCache.MATERIALS);
    }

    public async Task UpdateMaterial(Materials material)
    {
        await EnsureConnectionAsync();
        await _client.From<Materials>().Update(material);
        _cache.Invalidate(InMemoryCache.MATERIALS);
    }

    // Frames
    public async Task<IEnumerable<Frames>> GetFrames(bool forceRefresh = false)
    {
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
        await EnsureConnectionAsync();
        if (frameID == null) return;

        await _client.From<Frames>().Where(p => p.FrameID == frameID).Delete();
        _cache.Invalidate(InMemoryCache.FRAMES);
    }

    public async Task UpdateFrame(Frames frame)
    {
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

        var response = await _client.From<DetailPlanks>()
                                    .Where(p => p.Status == 1)
                                    .Get();
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
        }
        _cache.Invalidate(InMemoryCache.PLANKS);
    }

    public async Task<DetailPlanks?> GetPlankByName(string name)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<DetailPlanks>()
                                    .Where(p => p.sizeID == name)
                                    .Get();
        return response.Models.FirstOrDefault();
    }

    public async Task HardDeletePlank(string plankId)
    {
        await EnsureConnectionAsync();
        if (plankId == null) return;
        await _client.From<DetailPlanks>()
                    .Where(p => p.sizeID == plankId)
                    .Delete();
        _cache.Invalidate(InMemoryCache.PLANKS);
    }

    public async Task DeletePlank(string plankId)
    {
        await EnsureConnectionAsync();
        if (plankId == null) return;

        await _client.From<DetailPlanks>()
                    .Where(p => p.sizeID == plankId)
                    .Set(p => p.Status, 0)
                    .Update();
        _cache.Invalidate(InMemoryCache.PLANKS);
    }

    public async Task UpdatePlank(DetailPlanks plank)
    {
        await EnsureConnectionAsync();
        await _client.From<DetailPlanks>().Update(plank);
        _cache.Invalidate(InMemoryCache.PLANKS);
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
}
