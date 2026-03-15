using Invoice.ViewModels;
using Invoice.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Invoice.Views;

public sealed partial class CreateInvoicePage : Page
{
    public CreateInvoiceViewModel ViewModel
    {
        get;
    }

    public CreateInvoicePage()
    {
        ViewModel = App.GetService<CreateInvoiceViewModel>();
        InitializeComponent();
        this.DataContext = ViewModel;
    }

    private async void BtnAddProduct_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.OpenProductSelection();
    }

    private void ResetInvoice_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetInvoice();
        asbCustomer.Text = string.Empty;
        txtPhoneNo.Text = string.Empty;
        txtTypePrice.Text = string.Empty;
    }

    private async void CopyInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.InvoiceItems.Count == 0)
        {
            await App.ShowMessageAsync("Thông báo", "Không có dữ liệu để tạo phiếu.");
            return;
        }

        if (ViewModel.SelectedCustomer == null)
        {
            await App.ShowMessageAsync("Thông báo", "Vui lòng chọn khách hàng để sinh mã phiếu.");
            return;
        }

        await ViewModel.GenerateTempPdfAsync();
    }

    private async void SaveInvoice_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveInvoice();
        ResetInvoice_Click(sender, e);
    }

    private async void EditInvoice_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenHistoryToEdit();
    }

    private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TempInvoice item)
        {
            ViewModel.InvoiceItems.Remove(item);
            ViewModel.RecalculateGrandTotal();
        }
    }

    private async void asbCustomer_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var filtered = ViewModel.Customers
                            .Where(c => c.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                            .ToList();

            sender.ItemsSource = filtered;

            if (ViewModel.SelectedCustomer?.Name != sender.Text)
            {
                ViewModel.SelectedCustomer = null;
            }
        }
    }

    private async void asbCustomer_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Core.Models.Customers customer)
        {
            ViewModel.SelectedCustomer = customer;
            txtPhoneNo.Text = customer.Phone;
            txtTypePrice.Text = customer.PriceGroup;
        }
    }

    private void AmountTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }

    private async void AmountTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox?.DataContext is TempInvoice currentItem)
        {
            if (int.TryParse(textBox.Text, out int newValue))
            {
                if (newValue > currentItem.MaxStock)
                {
                    textBox.Text = currentItem.MaxStock.ToString();

                    await App.ShowMessageAsync("Cảnh báo kho hàng",
                    $"Sản phẩm '{currentItem.ProductName}' chỉ còn tồn: {currentItem.MaxStock}.\nHệ thống đã tự động lấy số lượng tối đa.");
                }
            }
            else if (newValue < 1)
            {
                textBox.Text = "1";
            }
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (ViewModel.SelectedCustomer != null)
        {
            asbCustomer.Text = ViewModel.SelectedCustomer.Name;
            txtPhoneNo.Text = ViewModel.SelectedCustomer.Phone;
            txtTypePrice.Text = ViewModel.SelectedCustomer.PriceGroup;
        }
    }
}