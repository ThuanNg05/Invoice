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
using Invoice.Views;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.ViewModels;

public partial class CreateInvoiceViewModel : ObservableRecipient, IRecipient<ProductsSelectedMessage>
{
    private readonly IDataService _dataService;
    private readonly InvoicePdfService _pdfService;
    private readonly ILocalSettingsService _localSettingsService;
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

    private string _originalInvoiceId = string.Empty;
    private WindowEx? _productSelectionWindow = null;
    private WindowEx? _editInvoiceWindow = null;

    public CreateInvoiceViewModel(IDataService dataService, InvoicePdfService pdfService, ILocalSettingsService localSettingsService)
    {
        _dataService = dataService;
        _pdfService = pdfService;
        _localSettingsService = localSettingsService;
        WeakReferenceMessenger.Default.Register<ProductsSelectedMessage>(this);

        InvoiceItems.CollectionChanged += (s, e) =>
        {
            e.OldItems?.Cast<TempInvoice>().ToList().ForEach(DetachItemEvents);
            e.NewItems?.Cast<TempInvoice>().ToList().ForEach(AttachItemEvents);
        };

        _ = LoadDataAsync();
    }

    private void CloseProductSelectionWindow()
    {
        if (_productSelectionWindow != null)
        {
            _productSelectionWindow.Close();
            _productSelectionWindow = null;
        }
    }

    private void CloseEditInvoiceWindow()
    {
        if (_editInvoiceWindow != null)
        {
            _editInvoiceWindow.Close();
            _editInvoiceWindow = null;
        }
    }

    private void AttachItemEvents(TempInvoice item) =>
    item.PropertyChanged += OnInvoiceItemChanged;

    private void DetachItemEvents(TempInvoice item) =>
        item.PropertyChanged -= OnInvoiceItemChanged;

    private void OnInvoiceItemChanged(object s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TempInvoice.LineTotal))
            RecalculateGrandTotal();
    }

    public void Receive(ProductsSelectedMessage message)
    {
        Debug.WriteLine($"[DEBUG] Sản phẩm: {message.Product.Name} | Tồn kho (Inventory): {message.Product.Inventory} | Số lượng chọn: {message.Amount}");

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
        try
        {
            Customers.Clear();
            var data = await _dataService.GetCustomers();
            foreach (var item in data)
            {
                Customers.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    }

    partial void OnSelectedCustomerChanged(Customers? value)
    {
        if (value != null && !IsEditing)
        {
            GenerateCodeAsync();
        }
    }

    public void ResetInvoice()
    {
        CloseProductSelectionWindow();
        CloseEditInvoiceWindow();
        InvoiceItems.Clear();
        SelectedCustomer = null;
        IsEditing = false;
        GeneratedInvoiceCode = string.Empty;
        _grandTotal = 0;
        RecalculateGrandTotal();
    }

    public async Task GenerateOfficialPdfAsync()
    {
        if (SelectedCustomer == null) return;

        string rootFolderPath;
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

        string safeCustomerName = RemoveInvalidFilePathCharacters(SelectedCustomer.Name);
        string customerFolderPath = Path.Combine(rootFolderPath, "Khách Hàng", safeCustomerName);

        if (!Directory.Exists(customerFolderPath))
        {
            Directory.CreateDirectory(customerFolderPath);
        }

        string fileName = $"{GeneratedInvoiceCode}.pdf";
        string finalFilePath = Path.Combine(customerFolderPath, fileName);

        await Task.Run(() =>
        {
            _pdfService.GenerateOfficial(InvoiceItems, SelectedCustomer.Name, SelectedCustomer.Phone, GeneratedInvoiceCode, DateTime.Now, finalFilePath);
        });

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
            await App.ShowMessageAsync("Lỗi", "File không tồn tại sau khi tạo.");
        }
    }

    public async Task GenerateTempPdfAsync()
    {
        CloseProductSelectionWindow();
        if (SelectedCustomer == null) return;
        try
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "TEMP.pdf";
            string filePath = Path.Combine(documentsPath, fileName);

            var pdfService = new InvoicePdfService();

            try
            {
                pdfService.GenerateTemp(InvoiceItems, filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating PDF: {ex.Message}");
                return;
            }

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
                await App.ShowMessageAsync("Lỗi", "File không tồn tại sau khi tạo.");
            }
        }
        catch (Exception ex)
        {
            await App.ShowMessageAsync("Lỗi", $"Không thể tạo phiếu tạm: {ex.Message}");
        }
    }

    public async Task OpenProductSelection()
    {
        if (SelectedCustomer == null)
        {
            await App.ShowMessageAsync("Thông báo", "Vui lòng chọn khách hàng trước");
            return;
        }

        if (_productSelectionWindow != null)
        {
            _productSelectionWindow.Activate();
            return;
        }

        var newWindow = new WindowEx();
        newWindow.Title = "Chọn sản phẩm";
        newWindow.Height = 800;
        newWindow.Width = 1200;
        newWindow.CenterOnScreen();

        _productSelectionWindow = newWindow;
        newWindow.Closed += (sender, args) =>
        {
            _productSelectionWindow = null;
        };

        var frame = new Frame();
        newWindow.Content = frame;

        var navParam = new ProductSelectionNavigationParameter
        {
            PriceGroup = SelectedCustomer.PriceGroup
        };

        frame.Navigate(typeof(ProductSelectionPage), navParam);

        newWindow.Activate();
    }

    public async Task SaveInvoice()
    {
        // Step process to save an invoice
        // 1. Close product selection window if open
        // 2. Validate customer selection and invoice items
        // 3. If editing, delete original invoice and revert inventory
        // 4. Create new invoice object
        // 5. Create invoice details list
        // 6. Create warehouse transactions list
        // 7. Save invoice, details, and transactions to database
        // 8. Generate official PDF
        // 9. Show success message and reset invoice

        CloseProductSelectionWindow();
        if (SelectedCustomer == null || !InvoiceItems.Any())
        {
            await App.ShowMessageAsync("Thông báo", "Chưa chọn khách hàng hoặc không có dữ liệu.");
            return;
        }

        // Debug previewer here
        //await GenerateOfficialPdfAsync();

        try
        {
            if (IsEditing)
            {
                await _dataService.DeleteInvoiceAndRevertInventory(_originalInvoiceId);
            }

            var newInvoice = new Invoices
            {
                InvoiceID = GeneratedInvoiceCode,
                CustomerID = SelectedCustomer.CustomerID,
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Total = (int)GrandTotal
            };

            var detailsList = InvoiceItems.Select(item => new InvoiceDetail
            {
                InvoiceID = GeneratedInvoiceCode,
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                SellPrice = item.SellPrice,
                Amount = item.Amount,
                Note = item.Note,
                CustomerName = SelectedCustomer.Name
            }).ToList();

            var transactionList = InvoiceItems.Select(item => new WarehouseTransaction
            {
                ProductID = item.ProductID,
                InvoiceID = GeneratedInvoiceCode,
                Amount = item.Amount,
                ActionType = "Xuất kho",
                CreatedDate = DateTime.Now,
                Note = $"Xuất hoá đơn {SelectedCustomer.Name}"
            }).ToList();

            await _dataService.AddInvoice(newInvoice, detailsList, transactionList);

            await GenerateOfficialPdfAsync();

            App.MainWindow.Activate();

            await App.ShowMessageAsync("Thông báo", "Lưu thành công.");
            ResetInvoice();
        }
        catch (Exception ex)
        {
            await App.ShowMessageAsync("Lỗi", $"Không thể lưu hoá đơn: {ex.Message}");
            Debug.WriteLine($"Error saving invoice: {ex.Message}");
        }
    }

    public void OpenHistoryToEdit()
    {
        if (_editInvoiceWindow != null)
        {
            _editInvoiceWindow.Activate();
            return;
        }

        var newWindow = new WindowEx();
        newWindow.Title = "Chọn hoá đơn cũ";
        newWindow.Height = 800;
        newWindow.Width = 1200;
        newWindow.CenterOnScreen();

        _editInvoiceWindow = newWindow;

        newWindow.Closed += (sender, args) =>
        {
            _editInvoiceWindow = null;
        };

        var frame = new Frame();
        newWindow.Content = frame;

        var navParam = new EditingInvoiceNavigationParameter
        {
            OnInvoiceSelected = (invoiceId) =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    await LoadInvoiceForEdit(invoiceId);
                    CloseEditInvoiceWindow();
                });
            }
        };
        frame.Navigate(typeof(EditingInvoicePage), navParam);
        newWindow.Activate();
    }

    public async Task LoadInvoiceForEdit(string invoiceID)
    {
        ResetInvoice();
        var details = await _dataService.GetInvoiceDetails(invoiceID);
        if (details == null || !details.Any()) return;

        IsEditing = true;
        GeneratedInvoiceCode = invoiceID;
        _originalInvoiceId = invoiceID;

        var firstDetail = details.First();
        var customerName = firstDetail.CustomerName;
        SelectedCustomer = Customers.FirstOrDefault(x => x.Name == customerName) ?? new Customers { Name = customerName };

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
        if (SelectedCustomer == null)
        {
            return;
        }

        DateTime today = DateTime.Now;
        string datePart = today.ToString("ddMMyyyy");

        int currentCount = await _dataService.GetInvoiceCountByDate(today);
        int nextNumber = currentCount + 1;

        string numberPart = nextNumber.ToString("D4");
        string namePart = StringHelper.GetNormalizedLastName(SelectedCustomer.Name);

        GeneratedInvoiceCode = $"{datePart}-{numberPart}-{namePart}";
    }

}