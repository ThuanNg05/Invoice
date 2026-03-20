using System.Text.RegularExpressions;
using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class CustomersPage : Page
{
    private readonly IDialogService _dialogService;
    public CustomersViewModel ViewModel
    {
        get;
    }

    public CustomersPage()
    {
        ViewModel = App.GetService<CustomersViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private void ClearInputs()
    {
        StringHelper.ClearInputs(this);
        CmbPriceType.SelectedIndex = -1;
        CustomerGrid.SelectedItem = -1;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        txtName.Focus(FocusState.Programmatic);
    }

    private void CustomerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            txtName.Text = selected.Name;
            txtPhoneNo.Text = selected.Phone;
            CmbPriceType.SelectedItem = CmbPriceType.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == selected.PriceGroup);
            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = true;            
            btnDelete.IsEnabled = true;
        }
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await _dialogService.ShowErrorAsync("Vui lòng nhập tên khách hàng.");
            return;
        }

        if (!IsValidPhoneNumber(txtPhoneNo.Text))
        {
            await _dialogService.ShowErrorAsync("Số điện thoại không hợp lệ. Vui lòng nhập đúng chuẩn 10 số.");
            return;
        }

        try
        {
            var newCustomer = new Customers
            {
                Name = StringHelper.RemoveRedundantWhitespace(txtName.Text),
                Phone = txtPhoneNo.Text,
                PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString()
            };

            await ViewModel.AddCustomerAsync(newCustomer);
            await _dialogService.ShowSuccessAsync("Thêm khách hàng thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Thêm khách hàng thất bại", ex);
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                await _dialogService.ShowErrorAsync("Vui lòng nhập tên khách hàng.");
                return;
            }
            
            if(!IsValidPhoneNumber(txtPhoneNo.Text))
            {                
                await _dialogService.ShowErrorAsync("Số điện thoại đã tồn tại cho khách hàng khác. Vui lòng nhập số điện thoại khác.");
                return;                
            }

            try
            {
                selected.Name = txtName.Text;
                selected.Phone = txtPhoneNo.Text;
                selected.PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lẻ";
                await ViewModel.UpdateCustomerAsync(selected);
                await _dialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
                ClearInputs();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("FAILED_UPDATE".GetLocalized(), ex);
            }
        }
        else
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn khách hàng để sửa.");
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            if (await _dialogService.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa khách hàng {selected.Name}?", "Xác nhận"))
            {
                try
                {
                    await ViewModel.DeleteCustomerAsync(selected);
                    await _dialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("FAILED_DELETE".GetLocalized(), ex);
                }
            }
        }
        else
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn khách hàng để xoá.");
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
    }

    private bool IsValidPhoneNumber(string phone)
    {
        string pattern = @"^0(3|5|7|8|9)[0-9]{8}$";
        return Regex.IsMatch(phone, pattern);
    }

    private void txtPhoneNo_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }    

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = txtSearch.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            CustomerGrid.ItemsSource = ViewModel.CustomersCollection;
        }
        else
        {
            CustomerGrid.ItemsSource = ViewModel.AllCustomers
                .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                            c.Phone.Contains(searchText));
        }
    }
}
