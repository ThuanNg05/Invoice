using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.Services;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.Services;

namespace Invoice.ViewModels;

public partial class CreateInvoiceViewModel : ViewModelBase, IRecipient<ProductsSelectedMessage>
{
    private readonly IDataService _dataService;
    private readonly InvoicePdfService _pdfService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IWindowService _windowService;
    public ObservableCollection<TempInvoice> InvoiceItems { get; } = new ObservableCollection<TempInvoice>();
    public ObservableCollection<Customers> Customers { get; } = new ObservableCollection<Customers>();

    [ObservableProperty]
    private Customers? _selectedCustomer;

    [ObservableProperty]
    private double _grandTotal;

    [ObservableProperty]
    private string _generatedInvoiceCode = string.Empty;

    [ObservableProperty]
    private bool _isEditing = false;

    private bool _isWalkIn = false;

    private string _originalInvoiceId = string.Empty;

    public CreateInvoiceViewModel(
        IDataService dataService, 
        InvoicePdfService pdfService, 
        ILocalSettingsService localSettingsService, 
        IWindowService windowService,
        IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
        _pdfService = pdfService;
        _localSettingsService = localSettingsService;
        _windowService = windowService;
        WeakReferenceMessenger.Default.Register<ProductsSelectedMessage>(this);

        InvoiceItems.CollectionChanged += (s, e) =>
        {
            e.OldItems?.Cast<TempInvoice>().ToList().ForEach(DetachItemEvents);
            e.NewItems?.Cast<TempInvoice>().ToList().ForEach(AttachItemEvents);
        };

        _ = LoadDataAsync();
    }

    private void CloseProductSelectionWindow() => _windowService.CloseProductSelectionWindow();

    private void CloseEditInvoiceWindow() => _windowService.CloseEditingWindow();

    private void AttachItemEvents(TempInvoice item) =>
    item.PropertyChanged += OnInvoiceItemChanged;

    private void DetachItemEvents(TempInvoice item) =>
        item.PropertyChanged -= OnInvoiceItemChanged;

    private void OnInvoiceItemChanged(object? s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TempInvoice.LineTotal))
            RecalculateGrandTotal();
    }

    public void Receive(ProductsSelectedMessage message)
    {
        //Debug.WriteLine($"[DEBUG] Sản phẩm: {message.Product.Name} | Tồn kho (Inventory): {message.Product.Inventory} | Số lượng chọn: {message.Amount}");

        int inputAmount = message.Amount <= 0 ? 1 : message.Amount;
        int currentStock = message.Product.Inventory;

        var existingItem = InvoiceItems.FirstOrDefault(x => x.ProductID == message.Product.ProductID);

        if (existingItem != null && message.IsMerge)
        {
            existingItem.MaxStock = currentStock;
            int newTotal = existingItem.Amount + inputAmount;

            if (currentStock > 0 && newTotal > currentStock)
            {
                existingItem.Amount = currentStock;
            }
            else
            {
                existingItem.Amount = newTotal;
            }
        }
        else
        {
            int finalAmount = inputAmount;
            if (currentStock > 0 && inputAmount > currentStock)
            {
                finalAmount = currentStock;
            }
            else if (currentStock <= 0)
            {
                finalAmount = 0;
            }

            var newItem = new TempInvoice
            {
                MaxStock = currentStock,
                ProductID = message.Product.ProductID,
                ProductName = message.Product.Name,
                SellPrice = message.FinalPrice,
                Note = message.Note,
                Amount = finalAmount
            };

            InvoiceItems.Add(newItem);
        }
        RecalculateGrandTotal();
    }

    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            Customers.Clear();
            var data = await _dataService.GetCustomers();
            foreach (var item in data)
            {
                Customers.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    partial void OnSelectedCustomerChanged(Customers? value)
    {
        if (value != null)
        {
            _isWalkIn = false;
        }

        if (!IsEditing)
        {
            if (value != null)
            {
                _ = GenerateCodeAsync();
            }
            else
            {
                GeneratedInvoiceCode = string.Empty;
            }
        }
    }

    public void ResetInvoice()
    {
        CloseProductSelectionWindow();
        CloseEditInvoiceWindow();
        InvoiceItems.Clear();
        IsEditing = false;
        _isWalkIn = false;
        SelectedCustomer = null;
        GeneratedInvoiceCode = string.Empty;
        GrandTotal = 0;        
    }

    public async Task GenerateOfficialPdfAsync()
    {
        CloseProductSelectionWindow();        
        string? rootFolderPath;
        try
        {
            rootFolderPath = await _localSettingsService.ReadSettingAsync<string>("InvoiceStoragePath");
        }
        catch
        {
            rootFolderPath = null;
        }

        if (string.IsNullOrEmpty(rootFolderPath) || !Directory.Exists(rootFolderPath))
        {
            rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        string customerName = SelectedCustomer?.Name ?? "Khách Lẻ";
        string customerPhone = SelectedCustomer?.Phone ?? string.Empty;

        string safeCustomerName = RemoveInvalidFilePathCharacters(customerName);
        string customerFolderPath = Path.Combine(rootFolderPath, "Khách Hàng", safeCustomerName);

        if (!Directory.Exists(customerFolderPath))
        {
            Directory.CreateDirectory(customerFolderPath);
        }

        string fileName = $"{GeneratedInvoiceCode}.pdf";
        string finalFilePath = Path.Combine(customerFolderPath, fileName);

        await _pdfService.GenerateOfficialAsync(InvoiceItems, customerName, customerPhone, GeneratedInvoiceCode, DateTime.Now, finalFilePath);

        if (File.Exists(finalFilePath))
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(finalFilePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }
        else
        {
            await DialogService.ShowErrorAsync("FILE_NOTE_FOUND".GetLocalized());
        }
    }

    public async Task GenerateTempPdfAsync()
    {
        CloseProductSelectionWindow();        
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "TEMP.pdf");

        await _pdfService.GenerateTempAsync(InvoiceItems, filePath);

        if (File.Exists(filePath))
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }
        else
        {
            await DialogService.ShowErrorAsync("FILE_NOTE_FOUND".GetLocalized());
        }
    }   

    public async Task OpenProductSelection()
    {
        if (SelectedCustomer == null && !_isWalkIn)
        {
            var result = await DialogService.ShowTwoOptionsAsync("Chọn loại khách", "Vui lòng chọn loại khách hàng trước khi chọn sản phẩm:", "Khách Lẻ", "Khách cố định");

            if (result == true) // Khách Lẻ
            {
                _isWalkIn = true;
                var walkInCustomer = Customers.FirstOrDefault(c => c.Name == "Khách Lẻ");
                if (walkInCustomer != null)
                {
                    SelectedCustomer = walkInCustomer;
                }
            }
            else if (result == false) // Khách cố định
            {
                await DialogService.ShowMessageAsync("Thông báo", "Vui lòng nhập và chọn khách hàng cố định.");
                return;
            }
            else // Cancel
            {
                return;
            }
        }

        Customers? customerToPass = SelectedCustomer;
        _windowService.OpenProductSelectionWindow(customerToPass, InvoiceItems);
        await Task.CompletedTask;
    }

    public async Task<bool> SaveInvoice()
    {
        CloseProductSelectionWindow();

        if (string.IsNullOrEmpty(GeneratedInvoiceCode))
        {
            await GenerateCodeAsync();
        }        

        if (!InvoiceItems.Any())
        {
            await DialogService.ShowErrorAsync("Hoá đơn rỗng");
            return false;
        }

        bool result = false;
        await ExecuteAsync(async () =>
        {
            if (IsEditing)
            {
                await _dataService.DeleteInvoiceAndRevertInventory(_originalInvoiceId);
            }

            var newInvoice = new Invoices
            {
                InvoiceID = GeneratedInvoiceCode,
                CustomerID = SelectedCustomer?.CustomerID,
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Total = (int)GrandTotal
            };

            string customerName = SelectedCustomer?.Name ?? "Khách Lẻ";

            var detailsList = InvoiceItems.Select(item => new InvoiceDetail
            {
                InvoiceID = GeneratedInvoiceCode,
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                SellPrice = item.SellPrice,
                Amount = item.Amount,
                Note = item.Note,
                CustomerName = customerName
            }).ToList();

            var transactionList = InvoiceItems.Select(item => new WarehouseTransaction
            {
                InvoiceID = GeneratedInvoiceCode,
                ProductID = item.ProductID,                
                Name = item.ProductName,
                Amount = -item.Amount, // Negative for export so stock decreases
                ActionType = "Export",
                CreatedDate = DateTime.Now,
                SourceType = "PRODUCT",
                Note = $"Xuất hoá đơn {customerName}"
            }).ToList();
            await _dataService.AddInvoice(newInvoice, detailsList, transactionList);

            await GenerateOfficialPdfAsync();

            App.MainWindow.Activate();

            await DialogService.ShowSuccessAsync("Đã lưu hoá đơn");
            result = true;
        }, "Lỗi khi lưu hóa đơn");

        return result;
    }

    public void OpenHistoryToEdit()
    {
        _windowService.OpenEditingInvoiceWindow((invoiceId) =>
        {
            App.MainWindow?.DispatcherQueue?.TryEnqueue(async () =>
            {
                await LoadInvoiceForEdit(invoiceId);
            });
        });
    }

    public async Task LoadInvoiceForEdit(string invoiceID)
    {
        await ExecuteAsync(async () =>
        {
            ResetInvoice();
            var details = await _dataService.GetInvoiceDetails(invoiceID);
            if (details == null || !details.Any()) return;

            IsEditing = true;
            GeneratedInvoiceCode = invoiceID;
            _originalInvoiceId = invoiceID;

            var firstDetail = details.First();
            var customerName = firstDetail.CustomerName;
            
            if (customerName == "Khách Lẻ")
            {
                SelectedCustomer = Customers.FirstOrDefault(x => x.Name == "Khách Lẻ");
                _isWalkIn = true;
            }
            else
            {
                SelectedCustomer = Customers.FirstOrDefault(x => x.Name == customerName) ?? new Customers { Name = StringHelper.NormalizeVietnameseName(customerName) };
            }

            foreach (var item in details)
            {
                var currentProductInfo = await _dataService.GetProductById(item.ProductID);
                int currentInventoryInDB = currentProductInfo != null ? currentProductInfo.Inventory : 0;
                int trueMaxStock = currentInventoryInDB + item.Amount;

                var tempItem = new TempInvoice
                {
                    ProductID = item.ProductID,
                    ProductName = item.ProductName,
                    SellPrice = item.SellPrice,
                    Note = item.Note,
                    MaxStock = trueMaxStock,
                    Amount = item.Amount
                };

                tempItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(TempInvoice.LineTotal)) RecalculateGrandTotal();
                };

                InvoiceItems.Add(tempItem);
            }

            RecalculateGrandTotal();
        }, "LOAD_FAILED".GetLocalized());
    }

    public void AddInvoiceItem(TempInvoice newItem)
    {
        InvoiceItems.Add(newItem);
        newItem.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TempInvoice.LineTotal))
            {
                RecalculateGrandTotal();
            }
        };
        RecalculateGrandTotal();
    }

    public bool ProductExists(long productId)
    {
        return InvoiceItems.Any(x => x.ProductID == productId);
    }

    public void IncreaseProductAmount(long productId, int amountToAdd)
    {
        var item = InvoiceItems.FirstOrDefault(x => x.ProductID == productId);
        if (item != null)
        {
            item.Amount += amountToAdd;
        }
    }

    public void RecalculateGrandTotal()
    {
        GrandTotal = InvoiceItems.Sum(x => x.LineTotal);
    }

    private string RemoveInvalidFilePathCharacters(string filename)
    {
        string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var r = new System.Text.RegularExpressions.Regex(string.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(regexSearch)));
        return r.Replace(filename, "_").Trim();
    }

    public async Task GenerateCodeAsync()
    {
        await ExecuteAsync(async () =>
        {
            DateTime today = DateTime.Now;
            string datePart = today.ToString("ddMMyyyy");

            int maxSequence = await _dataService.GetMaxInvoiceSequenceByDate(today);
            int nextNumber = maxSequence + 1;

            string numberPart = nextNumber.ToString("D4");
            string namePart = StringHelper.GetNormalizedLastName(SelectedCustomer?.Name);

            GeneratedInvoiceCode = $"{datePart}-{numberPart}-{namePart}";
        }, "Lỗi tạo mã hóa đơn");
    }

}
