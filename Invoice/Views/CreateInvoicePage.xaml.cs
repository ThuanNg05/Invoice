using Invoice.ViewModels;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Invoice.Views;

public sealed partial class CreateInvoicePage : Page
{
    private readonly IDialogService _dialogService;

    public CreateInvoiceViewModel ViewModel
    {
        get;
    }

    public CreateInvoicePage()
    {
        ViewModel = App.GetService<CreateInvoiceViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
        this.DataContext = ViewModel;
    }

    private async void BtnAddProduct_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.OpenProductSelection();
    }

    private void ResetInvoice_Click(object sender, RoutedEventArgs e)
    {
        asbCustomer.Text = string.Empty;
        txtPhoneNo.Text = string.Empty;
        txtTypePrice.Text = string.Empty;
        ViewModel.ResetInvoice();
    }

    private async void CopyInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.InvoiceItems.Count == 0)
        {
            await _dialogService.ShowErrorAsync("Không có dữ liệu để tạo phiếu.");
            return;
        }        

        await ViewModel.GenerateTempPdfAsync();
    }

    private async void SaveInvoice_Click(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox?.DataContext is TempInvoice currentItem)
        {
            if (int.TryParse(textBox.Text, out int newValue))
            {
                if (newValue > currentItem.MaxStock)
                {
                    textBox.Text = currentItem.MaxStock.ToString();

                    await _dialogService.ShowSuccessAsync($"Sản phẩm '{currentItem.ProductName}' chỉ còn tồn: {currentItem.MaxStock}.\nHệ thống đã tự động lấy số lượng tối đa.");
                }
            }
            else if (newValue < 1)
            {
                textBox.Text = "1";
            }
        }

        bool success = await ViewModel.SaveInvoice();
        if (success)
        {
            ResetInvoice_Click(sender, e);
        }
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
        if (args.SelectedItem is Customers customer)
        {
            ViewModel.SelectedCustomer = customer;
            txtPhoneNo.Text = customer.Phone;
            txtTypePrice.Text = customer.PriceGroup;
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