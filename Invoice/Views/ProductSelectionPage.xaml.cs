using CommunityToolkit.WinUI.UI.Controls;
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
        if (string.IsNullOrEmpty(txtName.Text))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Tên hàng hoá không được bỏ trống.");
            return;
        }

        string cleanedPriceText = txtBasePrice.Text.Replace(",", "").Replace(".", "");
        if (!double.TryParse(cleanedPriceText, out double sellPrice) || cleanedPriceText.Equals("0"))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Đơn giá không hợp lệ.");
            txtBasePrice.Focus(FocusState.Programmatic);
            txtBasePrice.SelectAll();
            return;
        }

        int amount = 1;
        if (!string.IsNullOrEmpty(txtAmount.Text))
        {
            if (!int.TryParse(txtAmount.Text, out amount) || amount > _currentInventory)
            {
                await App.ShowMessageAsync("Lỗi xác thực", "Số lượng phải nhỏ hơn tồn kho.");
                return;
            }
        }

        if (amount > _currentInventory)
        {
            await App.ShowMessageAsync("Lỗi tồn kho", $"Số lượng nhập ({amount}) vượt quá tồn kho hiện tại ({_currentInventory}).");
            return;
        }

        var mainVM = App.GetService<CreateInvoiceViewModel>();
        string currentID = txtProductID.Text;

        if (mainVM.ProductExists(currentID))
        {
            ContentDialog dialog = new()
            {
                Title = "Xác nhận",
                Content = $"Sản phẩm '{txtName.Text}' đã có trong hóa đơn.\nBạn có muốn cộng thêm số lượng không?",
                PrimaryButtonText = "Có",
                CloseButtonText = "Không",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    mainVM.IncreaseProductAmount(currentID, amount);
                });

                ClearSelection();
                txtSearch.Focus(FocusState.Programmatic);
            }
            return;
        }

        var newItem = new TempInvoice
        {
            MaxStock = _currentInventory,
            ProductID = txtProductID.Text ?? string.Empty,
            ProductName = StringHelper.CleanStringSimple(txtName.Text.Trim()),
            SellPrice = (int)sellPrice,
            Amount = amount,
            Note = txtNote.Text?.Trim() ?? string.Empty
        };

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            mainVM.AddInvoiceItem(newItem);
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
        ProductGrid.SelectedIndex = -1;
        txtProductID.Text = string.Empty;
        txtName.Text = string.Empty;
        txtBasePrice.Text = string.Empty;
        txtAmount.Text = string.Empty;
        txtSearch.Text = string.Empty;
        txtNote.Text = string.Empty;
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
            txtProductID.Text = summary.ProductID;
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
        if (index >= ViewModel.Source.Count - 1 && !ViewModel.IsLoading)
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