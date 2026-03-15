using System.Linq;
using Invoice.ViewModels;
using Invoice.Core.Models;
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
            await App.ShowMessageAsync("Lỗi", "Vui lòng nhập tên khách hàng.");
            return;
        }

        var newCustomer = new Customers
        {
            Name = txtName.Text,
            Phone = txtPhoneNo.Text,
            PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lẻ"
        };

        await ViewModel.AddCustomerAsync(newCustomer);
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            selected.Name = txtName.Text;
            selected.Phone = txtPhoneNo.Text;
            selected.PriceGroup = (CmbPriceType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lẻ";

            await ViewModel.UpdateCustomerAsync(selected);
        }
        else
        {
            await App.ShowMessageAsync("Lỗi", "Vui lòng chọn khách hàng để sửa.");
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerGrid.SelectedItem is Customers selected)
        {
            await ViewModel.DeleteCustomerAsync(selected);
        }
        else
        {
            await App.ShowMessageAsync("Lỗi", "Vui lòng chọn khách hàng để xoá.");
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        txtName.Text = string.Empty;
        txtPhoneNo.Text = string.Empty;
        CmbPriceType.SelectedIndex = -1;
        CustomerGrid.SelectedItem = null;
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
