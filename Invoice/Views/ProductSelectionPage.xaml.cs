using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI.Controls;
using Invoice.Core.Contracts;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Invoice.Views;

public sealed partial class ProductSelectionPage : Page
{
    public ProductSelectionViewModel ViewModel
    {
        get;
    }
    private string _priceGroup = string.Empty;
    private int _currentInventory = 0;

    public ProductSelectionPage()
    {
        ViewModel = App.GetService<ProductSelectionViewModel>();
        this.InitializeComponent();
        ClearSelection();
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (ProductGrid.SelectedItem is not ProductSummary summary)
        {
            await App.ShowErrorAsync("Vui lòng chọn một sản phẩm từ danh sách.");
            return;
        }

        if (string.IsNullOrEmpty(txtName.Text))
        {
            await App.ShowErrorAsync("Tên hàng hoá không được bỏ trống.");
            return;
        }

        string cleanedPriceText = txtBasePrice.Text.Replace(",", "").Replace(".", "");
        if (!int.TryParse(cleanedPriceText, out int sellPrice) || cleanedPriceText.Equals("0"))
        {
            await App.ShowErrorAsync("Đơn giá không hợp lệ.");
            txtBasePrice.Focus(FocusState.Programmatic);
            txtBasePrice.SelectAll();
            return;
        }

        int amount = 1;
        if (!string.IsNullOrEmpty(txtAmount.Text))
        {
            if (!int.TryParse(txtAmount.Text, out amount) || amount <= 0)
            {
                await App.ShowErrorAsync("Số lượng không hợp lệ.");
                return;
            }
        }

        if (amount > _currentInventory)
        {
            await App.ShowErrorAsync($"Số lượng nhập ({amount}) vượt quá tồn kho hiện tại ({_currentInventory}).");
            return;
        }

        var fullProduct = await ViewModel.GetProductDetailAsync(summary.ProductID);
        if (fullProduct == null) return;

        // Send message instead of manual VM manipulation
        WeakReferenceMessenger.Default.Send(new ProductsSelectedMessage
        {
            Product = fullProduct,
            Amount = amount,
            FinalPrice = sellPrice,
            Note = txtNote.Text?.Trim() ?? string.Empty,
            IsMerge = true // Default to merging if product exists
        });

        ClearSelection();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearSelection();
        txtSearch_TextChanged(txtSearch, null);
    }

    private void ClearSelection()
    {
        StringHelper.ClearInputs(this);
        ProductGrid.SelectedIndex = -1;
        txtTotal.Text = "0";
        txtName.IsReadOnly = false;
        _currentInventory = 0;

        btnAdd.IsEnabled = false;
        txtSearch.Focus(FocusState.Programmatic);
    }

    private async void ProductGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProductGrid.SelectedItem is ProductSummary summary)
        {            
            txtName.Text = summary.Name;
            btnAdd.IsEnabled = false;

            var fullProduct = await ViewModel.GetProductDetailAsync(summary.ProductID);

            if (fullProduct != null)
            {
                _currentInventory = summary.Inventory;
                txtName.IsReadOnly = true;

                string group = _priceGroup.Trim();

                if (group.Equals("Lẻ", StringComparison.OrdinalIgnoreCase))
                    txtBasePrice.Text = fullProduct.PriceOdd.ToString();
                else if (group.Equals("Sỉ", StringComparison.OrdinalIgnoreCase))
                    txtBasePrice.Text = fullProduct.PriceEven.ToString();
                else
                    txtBasePrice.Text = fullProduct.BasePrice.ToString();

                CalculateTotal();
                btnAdd.IsEnabled = true;
            }
        }
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
        ViewModel.FilterData(txtSearch.Text);
    }

    private void CalculateTotal()
    {
        string cleanedPriceText = txtBasePrice.Text.Replace(",", "");
        string cleanedAmountText = txtAmount.Text.Replace(",", "");

        if (double.TryParse(cleanedPriceText, out double price) &&
            int.TryParse(cleanedAmountText, out int amount))
        {
            double total = price * amount;
            txtTotal.Text = total.ToString("N0");
        }
        else
        {
            txtTotal.Text = "0";
        }
    }

    private void Amount_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }

    private void Input_TextChanged(object sender, TextChangedEventArgs e)
    {
        CalculateTotal();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ProductSelectionNavigationParameter param)
        {
            _priceGroup = param.PriceGroup;
        }
        else if (e.Parameter is string group)
        {
            _priceGroup = group;
        }

        if (ViewModel.Source.Count == 0)
        {
            await ViewModel.ReloadFirstPage();
        }
    }
}