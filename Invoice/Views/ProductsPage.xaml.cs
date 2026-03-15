using CommunityToolkit.WinUI.UI.Controls;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Invoice.Views;

public sealed partial class ProductsPage : Page
{
    private DetailPrice? _currentPriceConfig;
    private DispatcherTimer _searchDebounceTimer;
    public ProductsViewModel ViewModel
    {
        get;
    }

    public ProductsPage()
    {
        ViewModel = App.GetService<ProductsViewModel>();
        InitializeComponent();
        Loaded += ProductsPage_Loaded;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        txtProductID.IsReadOnly = false;
        txtSearch.Focus(FocusState.Programmatic);
    }

    private void InitDebounce()
    {
        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchDebounceTimer.Tick += async (s, e) =>
        {
            _searchDebounceTimer.Stop();
            await ViewModel.ReloadFirstPage();
        };
    }

    private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
    {
        var dataService = App.GetService<IDataService>();
        var prices = await dataService.GetPrice();
        _currentPriceConfig = prices.FirstOrDefault();
    }

    private void ClearInputs(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBox textBox &&
                !string.IsNullOrEmpty(textBox.Name) &&
                textBox.Name.StartsWith("txt"))
            {
                textBox.Text = string.Empty;
            }
            ClearInputs(child);
        }
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        txtProductID.IsReadOnly = false;
        txtProductID.Focus(FocusState.Programmatic);
    }

    private void RefreshData()
    {
        ViewModel.Search(txtSearch.Text);
    }

    private async void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        if (await ValidateProductInputs() == false)
        {
            return;
        }
        var product = CreateProductFromInputs();

        try
        {
            await ViewModel.AddProductAsync(product);
            await App.ShowMessageAsync("Thông báo", "Thêm sản phẩm thành công!");
            BtnReset_Click(null, null);
        }
        catch (Exception ex)
        {
            await App.ShowMessageAsync("Lỗi", $"Thêm thất bại: {ex.Message}");
        }
    }

    private double ParseDouble(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return double.TryParse(text.Trim(), System.Globalization.NumberStyles.Any, null, out double val) ? val : 0;
    }

    private Products CreateProductFromInputs()
    {
        return new Products
        {
            ProductID = StringHelper.CleanStringSimple(txtProductID.Text.Trim()),
            Name = StringHelper.CleanStringSimple(txtName.Text.Trim()),
            BasePrice = ParseDouble(txtBasePrice.Text),
            PriceOdd = (int)ParseDouble(txtPriceOdd.Text),
            PriceEven = (int)ParseDouble(txtPriceEven.Text),
            PrWage = ParseDouble(txtWage.Text),
            sKieng = ParseDouble(txtKieng.Text),
            sNhL = ParseDouble(txtNhL.Text),
            sNhN = ParseDouble(txtNhN.Text),
            sG_l = ParseDouble(txtG_l.Text),
            sG_n = ParseDouble(txtG_n.Text),
            sDl = ParseDouble(txtDl.Text),
            sHau = ParseDouble(txtHau.Text),
            sLua = ParseDouble(txtLua.Text),
            sKt = ParseDouble(txtKt.Text),
            sOc = ParseDouble(txtOc.Text),
            sNhom = ParseDouble(txtNhom.Text),
            s7f = ParseDouble(txt7f.Text),
            s2D = ParseDouble(txt2D.Text),
            sDecal = ParseDouble(txtDecal.Text),
            mdfOdd = ParseDouble(txtMDFodd.Text),
            mdfEven = ParseDouble(txtMDFeven.Text),
            hpOdd = ParseDouble(txtHPodd.Text),
            hpEven = ParseDouble(txtHPeven.Text),
            hoanh = ParseDouble(txtHoanh.Text),
            lieng = ParseDouble(txtLieng.Text),
            tg = ParseDouble(txtTG.Text)
        };
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not ProductSummary selectedSummary) return;
        if (await ValidateProductInputs() == false) return;

        var product = CreateProductFromInputs();

        try
        {
            await ViewModel.UpdateProductAsync(product);
            await App.ShowMessageAsync("Thông báo", "Cập nhật thành công!");
            ClearInputs(this);
            //RefreshData();
        }
        catch (Exception ex)
        {
            await App.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {ex.Message}");
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not ProductSummary selected) return;
        ContentDialog deleteDialog = new()
        {
            Title = "Xác nhận xóa",
            Content = $"Bạn có chắc muốn xóa sản phẩm {selected.ProductID}?",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await deleteDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                await ViewModel.DeleteProductAsync(selected.ProductID);
                await App.ShowMessageAsync("Thông báo", "Xóa thành công!");
                BtnReset_Click(null, null);
            }
            catch (Exception ex)
            {
                await App.ShowMessageAsync("Lỗi", $"Xóa thất bại: {ex.Message}");
            }
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs(this);
        RefreshData();
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
        txtProductID.Text = selected.ProductID;
        txtProductID.IsReadOnly = true;
        txtName.Text = selected.Name;
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

    private void ProductGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        int index = e.Row.GetIndex();
        if (ViewModel == null) return;
        if (index >= ViewModel.Source.Count - 1 && !ViewModel.IsLoading)
        {
            _ = ViewModel.LoadMoreDataAsync();
        }
    }

    private async Task<bool> ValidateProductInputs()
    {
        if (string.IsNullOrWhiteSpace(txtProductID.Text))
        {
            await App.ShowMessageAsync("Validation Error", "Mã sản phẩm không được bỏ trống.");
            txtProductID.Focus(FocusState.Programmatic);
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await App.ShowMessageAsync("Validation Error", "Tên sản phẩm không được bỏ trống.");
            txtName.Focus(FocusState.Programmatic);
            return false;
        }

        if (!double.TryParse(txtWage.Text, out double wage) || wage <= 0)
        {
            await App.ShowMessageAsync("Validation Error", "Tiền công phải lớn hơn 0.");
            txtWage.Focus(FocusState.Programmatic);
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtBasePrice.Text) || string.IsNullOrWhiteSpace(txtPriceOdd.Text) || string.IsNullOrWhiteSpace(txtPriceEven.Text))
        {
            await App.ShowMessageAsync("Validation Error", "Giá gốc, sỉ và lẻ không được bỏ trống.");
            txtBasePrice.Focus(FocusState.Programmatic);
            return false;
        }

        int sCount = 0;

        var sControls = new List<TextBox> {
            txtKieng, txtNhL, txtNhN, txtG_l, txtG_n, txtDl, txtHau,
            txtLua, txtKt, txtOc, txtNhom, txt7f, txt2D, txtDecal
        };

        foreach (var ctrl in sControls)
        {
            if (double.TryParse(ctrl.Text, out double val) && val > 0)
            {
                sCount++;
            }
        }

        if (sCount < 3)
        {
            await App.ShowMessageAsync("Validation Error", $"Bạn mới nhập {sCount} thành phần. Cần ít nhất 3 thành phần khác 0.");
            return false;
        }

        return true;
    }

    private void CalculateBasePrice()
    {
        if (_currentPriceConfig == null) return;

        try
        {
            double wage = ParseDouble(txtWage.Text);

            double total = wage +
                (ParseDouble(txtKieng.Text) * _currentPriceConfig.PrKieng) +
                (ParseDouble(txtNhL.Text) * _currentPriceConfig.PrNhL) +
                (ParseDouble(txtNhN.Text) * _currentPriceConfig.PrNhN) +
                (ParseDouble(txtG_l.Text) * _currentPriceConfig.PrG_l) +
                (ParseDouble(txtG_n.Text) * _currentPriceConfig.PrG_n) +
                (ParseDouble(txtDl.Text) * _currentPriceConfig.PrDl) +
                (ParseDouble(txtHau.Text) * _currentPriceConfig.PrHau) +
                (ParseDouble(txtLua.Text) * _currentPriceConfig.PrLua) +
                (ParseDouble(txtKt.Text) * _currentPriceConfig.PrKt) +
                (ParseDouble(txtOc.Text) * _currentPriceConfig.PrOc) +
                (ParseDouble(txtNhom.Text) * _currentPriceConfig.PrNhom) +
                (ParseDouble(txt7f.Text) * _currentPriceConfig.Pr7f) +
                (ParseDouble(txt2D.Text) * _currentPriceConfig.Pr2D) +
                (ParseDouble(txtDecal.Text) * _currentPriceConfig.PrDecal);

            txtBasePrice.Text = total.ToString("N0");
        }
        catch
        {
        }
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounceTimer.Stop();
        ViewModel.Search(txtSearch.Text);
        _searchDebounceTimer.Start();
    }

    private void inputDecimal(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c) && c != '.');

        if (!args.Cancel && args.NewText.Count(c => c == '.') > 1)
        {
            args.Cancel = true;
        }
    }

    private void computeBasePrice(object sender, RoutedEventArgs args)
    {
        CalculateBasePrice();
    }
}