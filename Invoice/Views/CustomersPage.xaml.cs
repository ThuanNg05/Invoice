using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class CustomersPage : Page
{
    public CustomersViewModel ViewModel
    {
        get;
    }

    public CustomersPage()
    {
        ViewModel = App.GetService<CustomersViewModel>();
        InitializeComponent();
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
        }
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await App.ShowErrorAsync("Vui lòng nhập tên khách hàng.");
            return;
        }

        try
        {
            var newCustomer = new Customers
            {
                Name = txtName.Text,
                Phone = txtPhoneNo.Text,
                PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lẻ"
            };

            await ViewModel.AddCustomerAsync(newCustomer);
            await App.ShowSuccessAsync("Thêm khách hàng thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await App.ShowErrorAsync("Thêm khách hàng thất bại", ex);
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            try
            {
                selected.Name = txtName.Text;
                selected.Phone = txtPhoneNo.Text;
                selected.PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lẻ";
                await ViewModel.UpdateCustomerAsync(selected);
                await App.ShowSuccessAsync("Cập nhật thành công!");
                ClearInputs();
            }
            catch (Exception ex)
            {
                await App.ShowErrorAsync("Cập nhật thất bại", ex);
            }
        }
        else
        {
            await App.ShowErrorAsync("Vui lòng chọn khách hàng để sửa.");
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            if (await App.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa khách hàng {selected.Name}?", "Xóa"))
            {
                try
                {
                    await ViewModel.DeleteCustomerAsync(selected);
                    await App.ShowSuccessAsync("Xóa thành công!");
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    await App.ShowErrorAsync("Xóa thất bại", ex);
                }
            }
        }
        else
        {
            await App.ShowErrorAsync("Vui lòng chọn khách hàng để xoá.");
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
    }

    private void txtPhoneNo_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }

    private void txtPhoneNo_TextChanged(object sender, TextChangedEventArgs e)
    {
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
