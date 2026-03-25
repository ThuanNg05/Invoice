using Invoice.Contracts.Services;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Core.Helpers;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;

namespace Invoice.Views;

public sealed partial class ProductsPage : Page
{
    private readonly IDialogService _dialogService;
    private DetailPrice? _currentPriceConfig;
    private DispatcherTimer _searchDebounceTimer;
    public ProductsViewModel ViewModel
    {
        get;
    }

    public ProductsPage()
    {
        ViewModel = App.GetService<ProductsViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
        InitDebounce();
        Loaded += ProductsPage_Loaded;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private void InitDebounce()
    {
        _searchDebounceTimer = new DispatcherTimer();
        _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(500);
        _searchDebounceTimer.Tick += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            ViewModel.Search(txtSearch.Text);
        };
    }

    private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
    {
        var dataService = App.GetService<IDataService>();
        var prices = await dataService.GetPrice();
        _currentPriceConfig = prices.FirstOrDefault();
    }

    private void ClearInputs()
    {
        StringHelper.ClearInputs(this);
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        ProductGrid.SelectedItem = null;
        txtName.Focus(FocusState.Programmatic);
    }

    private async void ProductGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not ProductSummary summary) return;
        await ViewModel.LoadProductForEditingAsync(summary.ProductID);
        var selected = ViewModel.SelectedProductFull;

        if (selected == null) return;
        btnAdd.IsEnabled = false;
        btnDelete.IsEnabled = true;
        btnUpdate.IsEnabled = true;
        txtName.Text = selected.Name;
        txtSizeID.Text = selected.SizeID ?? string.Empty;
        txtBasePrice.Text = selected.BasePrice.ToString();
        txtPriceOdd.Text = selected.PriceOdd.ToString();
        txtPriceEven.Text = selected.PriceEven.ToString();
        txtWage.Text = selected.PrWage.ToString();
        txtKieng.Text = selected.sKieng.ToString();
        txtNhL.Text = selected.sNhL.ToString();
        txtNhN.Text = selected.sNhN.ToString();
        txtG_l.Text = selected.sG_l.ToString();
        txtG_n.Text = selected.sG_n.ToString();
        txtDl.Text = selected.sDl.ToString();
        txtHau.Text = selected.sHau.ToString();
        txtLua.Text = selected.sLua.ToString();
        txtKt.Text = selected.sKt.ToString();
        txtOc.Text = selected.sOc.ToString();
        txtNhom.Text = selected.sNhom.ToString();
        txt7f.Text = selected.s7f.ToString();
        txt2D.Text = selected.s2D.ToString();
        txtDecal.Text = selected.sDecal.ToString();
        txtMDFodd.Text = selected.mdfOdd.ToString();
        txtMDFeven.Text = selected.mdfEven.ToString();
        txtHPodd.Text = selected.hpOdd.ToString();
        txtHPeven.Text = selected.hpEven.ToString();
        txtHoanh.Text = selected.hoanh.ToString();
        txtLieng.Text = selected.lieng.ToString();
        txtTG.Text = selected.tg.ToString();
        btnAdd.IsEnabled = false;
        btnUpdate.IsEnabled = true;
        btnDelete.IsEnabled = true;
    }

    private Products CreateProductFromInputs()
    {
        string sizeIdInput = txtSizeID.Text.Trim();
        var product = new Products
        {
            Name = StringHelper.RemoveRedundantWhitespace(txtName.Text),
            SizeID = string.IsNullOrEmpty(sizeIdInput) ? null : sizeIdInput,
            BasePrice = StringHelper.ParseDouble(txtBasePrice.Text),
            PriceOdd = (int)StringHelper.ParseDouble(txtPriceOdd.Text),
            PriceEven = (int)StringHelper.ParseDouble(txtPriceEven.Text),
            PrWage = StringHelper.ParseDouble(txtWage.Text),
            sKieng = StringHelper.ParseDouble(txtKieng.Text),
            sNhL = StringHelper.ParseDouble(txtNhL.Text),
            sNhN = StringHelper.ParseDouble(txtNhN.Text),
            sG_l = StringHelper.ParseDouble(txtG_l.Text),
            sG_n = StringHelper.ParseDouble(txtG_n.Text),
            sDl = StringHelper.ParseDouble(txtDl.Text),
            sHau = StringHelper.ParseDouble(txtHau.Text),
            sLua = StringHelper.ParseDouble(txtLua.Text),
            sKt = StringHelper.ParseDouble(txtKt.Text),
            sOc = StringHelper.ParseDouble(txtOc.Text),
            sNhom = StringHelper.ParseDouble(txtNhom.Text),
            s7f = StringHelper.ParseDouble(txt7f.Text),
            s2D = StringHelper.ParseDouble(txt2D.Text),
            sDecal = StringHelper.ParseDouble(txtDecal.Text),
            mdfOdd = StringHelper.ParseDouble(txtMDFodd.Text),
            mdfEven = StringHelper.ParseDouble(txtMDFeven.Text),
            hpOdd = StringHelper.ParseDouble(txtHPodd.Text),
            hpEven = StringHelper.ParseDouble(txtHPeven.Text),
            hoanh = StringHelper.ParseDouble(txtHoanh.Text),
            lieng = StringHelper.ParseDouble(txtLieng.Text),
            tg = StringHelper.ParseDouble(txtTG.Text)
        };

        return product;
    }

    private async void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        if (await ValidateProductInputs())
        {
            var product = CreateProductFromInputs();
            await ViewModel.AddProductAsync(product);
            ClearInputs();
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is ProductSummary selected)
        {
            if (await ValidateProductInputs())
            {
                var product = CreateProductFromInputs();
                product.ProductID = selected.ProductID;
                await ViewModel.UpdateProductAsync(product);
                ClearInputs();
            }
        }
        else
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn sản phẩm để sửa.");
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is ProductSummary selected)
        {
            if (await _dialogService.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa sản phẩm {selected.Name}?", "Xác nhận"))
            {
                await ViewModel.DeleteProductAsync(selected.ProductID);
                ClearInputs();
            }
        }
        else
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn sản phẩm để xoá.");
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
    }

    private async Task<bool> ValidateProductInputs(bool isUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await _dialogService.ShowErrorAsync("Vui lòng nhập tên sản phẩm.");
            txtName.Focus(FocusState.Programmatic);
            return false;
        }
        
        string sizeId = txtSizeID.Text.Trim();
        if (!string.IsNullOrEmpty(sizeId))
        {
            var dataService = App.GetService<IDataService>();
            var planks = await dataService.GetPlanks();
            if (!planks.Any(p => p.sizeID == sizeId))
            {                
                await _dialogService.ShowErrorAsync($"Kích thước ({sizeId}) không tồn tại");
                txtSizeID.Focus(FocusState.Programmatic);
                return false;
            }
        }

        if (!double.TryParse(txtWage.Text, out double wage) || wage <= 0)
        {
            await _dialogService.ShowErrorAsync("Tiền công phải khác 0");
            txtWage.Focus(FocusState.Programmatic);
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtBasePrice.Text) || string.IsNullOrWhiteSpace(txtPriceOdd.Text) || string.IsNullOrWhiteSpace(txtPriceEven.Text))
        {
            await _dialogService.ShowErrorAsync("Giả sỉ, lẻ và giá gốc không được để trống");
            txtBasePrice.Focus(FocusState.Programmatic);
            return false;
        }
        
        if (!BusinessValidation.IsValidPriceLength(txtBasePrice.Text, txtPriceOdd.Text))
        {
            await _dialogService.ShowErrorAsync("Giá lẻ không hợp lệ so với giá gốc.");
            txtPriceOdd.Focus(FocusState.Programmatic);
            return false;
        }

        if (!BusinessValidation.IsValidPriceLength(txtBasePrice.Text, txtPriceEven.Text))
        {
            await _dialogService.ShowErrorAsync("Giá sỉ không hợp lệ so với giá gốc.");
            txtPriceEven.Focus(FocusState.Programmatic);
            return false;
        }

        return true;
    }

    private void ProductGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        int index = e.Row.GetIndex();
        if (ViewModel == null) return;
        if (index >= ViewModel.Source.Count - 1 && !ViewModel.IsBusy)
        {
            _ = ViewModel.LoadMoreDataAsync();
        }
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounceTimer.Stop();
        ViewModel.Search(txtSearch.Text);
        _searchDebounceTimer.Start();
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = ViewModel.PlankSizes
                .Where(p => p.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            sender.ItemsSource = suggestions;
        }
    }

    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        sender.Text = args.SelectedItem.ToString();
    }

    private void computeBasePrice(object sender, RoutedEventArgs args)
    {
        //CalculateBasePrice();
        if (_currentPriceConfig == null) return;

        try
        {
            double wage = StringHelper.ParseDouble(txtWage.Text);

            double total = wage +
                (StringHelper.ParseDouble(txtKieng.Text) * _currentPriceConfig.PrKieng) +
                (StringHelper.ParseDouble(txtNhL.Text) * _currentPriceConfig.PrNhL) +
                (StringHelper.ParseDouble(txtNhN.Text) * _currentPriceConfig.PrNhN) +
                (StringHelper.ParseDouble(txtG_l.Text) * _currentPriceConfig.PrG_l) +
                (StringHelper.ParseDouble(txtG_n.Text) * _currentPriceConfig.PrG_n) +
                (StringHelper.ParseDouble(txtDl.Text) * _currentPriceConfig.PrDl) +
                (StringHelper.ParseDouble(txtHau.Text) * _currentPriceConfig.PrHau) +
                (StringHelper.ParseDouble(txtLua.Text) * _currentPriceConfig.PrLua) +
                (StringHelper.ParseDouble(txtKt.Text) * _currentPriceConfig.PrKt) +
                (StringHelper.ParseDouble(txtOc.Text) * _currentPriceConfig.PrOc) +
                (StringHelper.ParseDouble(txtNhom.Text) * _currentPriceConfig.PrNhom) +
                (StringHelper.ParseDouble(txt7f.Text) * _currentPriceConfig.Pr7f) +
                (StringHelper.ParseDouble(txt2D.Text) * _currentPriceConfig.Pr2D) +
                (StringHelper.ParseDouble(txtDecal.Text) * _currentPriceConfig.PrDecal);

            txtBasePrice.Text = total.ToString("N0");
        }
        catch
        {
        }
    }
}
