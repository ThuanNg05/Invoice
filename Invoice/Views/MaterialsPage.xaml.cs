using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Invoice.Helpers;

namespace Invoice.Views;

public sealed partial class MaterialsPage : Page
{
    private readonly IDialogService _dialogService;
    public MaterialsViewModel ViewModel
    {
        get;
    }

    public MaterialsPage()
    {
        ViewModel = App.GetService<MaterialsViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
        btnAdd.IsEnabled = true;
        btnDelete.IsEnabled = false;
        btnUpdate.IsEnabled = false;
        txtTotal.IsReadOnly = true;
    }

    private void ClearInputs()
    {
        StringHelper.ClearInputs(this);
        txtName.Focus(FocusState.Programmatic);
        btnAdd.IsEnabled = true;
        btnDelete.IsEnabled = false;
        btnUpdate.IsEnabled = false;
        txtTotal.IsReadOnly = true;
        MaterialGrid.SelectedItem = null;
    }  

    private async void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await _dialogService.ShowErrorAsync("Tên SP không được rỗng.");
            txtName.Focus(FocusState.Programmatic);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtBasePrice.Text))
        {
            await _dialogService.ShowErrorAsync("Đơn giá không được rỗng.");
            txtBasePrice.Focus(FocusState.Programmatic);
            return;
        }

        try
        {
            var material = new Materials
            {
                Name = txtName.Text.Trim(),
                BasePrice = decimal.TryParse(txtBasePrice.Text.Trim().Replace(".", ""), out decimal price) ? price : 0,
                Inventory = 0,
                MinAmount = int.TryParse(txtMinAmount.Text.Trim(), out int minAmt) ? minAmt : 0,
                Unit = txtUnit.Text.Trim()
            };

            await ViewModel.AddMaterialAsync(material);            
            ClearInputs();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("FAILED_ADD".GetLocalized(), ex);
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;        
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            await _dialogService.ShowErrorAsync("Tên SP không được rỗng.");
            txtName.Focus(FocusState.Programmatic);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtBasePrice.Text))
        {
            await _dialogService.ShowErrorAsync("Đơn giá không được rỗng.");
            txtBasePrice.Focus(FocusState.Programmatic);
            return;
        }
        
        try
        {
            decimal basePrice = decimal.TryParse(txtBasePrice.Text.Trim().Replace(".", ""), out decimal price) ? price : 0;
            var tmpMaterial = new Materials
            {
                ProductID = selected.ProductID,
                Name = txtName.Text.Trim(),
                BasePrice = basePrice,
                Inventory = selected.Inventory,
                TotalLine = basePrice * selected.Inventory,
                MinAmount = int.TryParse(txtMinAmount.Text, out int minAmt) ? minAmt : 0,
                Unit = txtUnit.Text.Trim()
            };
            await ViewModel.UpdateMaterialAsync(tmpMaterial);            
            ClearInputs();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("FAILED_UPDATE".GetLocalized(), ex);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;
        if (await _dialogService.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa vật tư {selected.Name}?", "Xóa"))
        {
            try
            {
                await ViewModel.DeleteMaterialAsync(selected);                
                ClearInputs();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("FAILED_DELETE".GetLocalized(), ex);
            }
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();        
    }

    private void MaterialGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedItem is not Materials selected) return;        
        txtName.Text = selected.Name;
        txtBasePrice.Text = selected.BasePrice.ToString("N0");
        txtMinAmount.Text = selected.MinAmount.ToString();
        txtTotal.Text = (selected.BasePrice * selected.Inventory).ToString("N0");
        txtUnit.Text = selected.Unit;

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
            MaterialGrid.ItemsSource = ViewModel.MaterialsCollection
                .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }
    }
}
