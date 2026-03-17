using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Invoice.Core.Models;
using Invoice.ViewModels;

namespace Invoice.Views;

public sealed partial class MaterialsPage : Page
{
    public MaterialsViewModel ViewModel
    {
        get;
    }

    public MaterialsPage()
    {
        ViewModel = App.GetService<MaterialsViewModel>();
        InitializeComponent();
        ClearInputs();
    }

    private void ClearInputs()
    {
        StringHelper.ClearInputs(this);
        txtName.Focus(FocusState.Programmatic);
        btnAdd.IsEnabled = true;
        btnDelete.IsEnabled = false;
        btnUpdate.IsEnabled = false;        
    }

    private async Task RefreshData()
    {
        txtSearch_TextChanged(txtSearch, null);        
        txtTotal.IsReadOnly = true;
    }

    private async void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Tên SP không được rỗng.");
            txtName.Focus(FocusState.Programmatic);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtBasePrice.Text))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Đơn giá không được rỗng.");
            txtBasePrice.Focus(FocusState.Programmatic);
            return;
        }

        var material = new Materials
        {
            Name = txtName.Text.Trim(),
            BasePrice = decimal.TryParse(txtBasePrice.Text.Trim(), out decimal price) ? price : 0,
            Inventory = 0,
            MinAmount = int.TryParse(txtMinAmount.Text.Trim(), out int minAmt) ? minAmt : 0
        };

        await ViewModel.AddMaterialAsync(material);
        ClearInputs();
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;        
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Tên SP không được rỗng.");
            txtName.Focus(FocusState.Programmatic);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtBasePrice.Text))
        {
            await App.ShowMessageAsync("Lỗi xác thực", "Đơn giá không được rỗng.");
            txtBasePrice.Focus(FocusState.Programmatic);
            return;
        }
        
        var tmpMaterial = new Materials
        {
            ProductID = selected.ProductID,
            Name = txtName.Text.Trim(),
            BasePrice = decimal.TryParse(txtBasePrice.Text, out decimal price) ? price : 0,
            MinAmount = int.TryParse(txtMinAmount.Text, out int minAmt) ? minAmt : 0,
        };
        await ViewModel.UpdateMaterialAsync(tmpMaterial);
        ClearInputs();
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;
        await ViewModel.DeleteMaterialAsync(selected);
        ClearInputs();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
        RefreshData();
    }

    private void MaterialGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;        
        txtName.Text = selected.Name;
        txtBasePrice.Text = selected.BasePrice.ToString();
        txtMinAmount.Text = selected.MinAmount.ToString();
        txtTotal.Text = (selected.BasePrice * selected.Inventory).ToString();

        btnDelete.IsEnabled = true;
        btnUpdate.IsEnabled = true;
        btnAdd.IsEnabled = false;
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {        
        var searchText = txtSearch.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            MaterialGrid.ItemsSource = ViewModel.MaterialsCollection;
        }
        else
        {
            MaterialGrid.ItemsSource = ViewModel.AllMaterials
                .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Number_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }
}