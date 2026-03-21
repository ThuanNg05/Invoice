using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class IOActionsPage : Page
{
    private readonly IDialogService _dialogService;
    public IOActionsViewModel ViewModel
    {
        get;
    }

    public IOActionsPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<IOActionsViewModel>();
        _dialogService = App.GetService<IDialogService>();
        //ClearInput();
        btnAdd.IsEnabled = false;
        btnEdit.IsEnabled = false;
        btnDelete.IsEnabled = false;
        btnSave.IsEnabled = ViewModel.TransactionList.Count > 0;
    }

    private void ClearInput()
    {
        StringHelper.ClearInputs(this);
        cmbType.SelectedIndex = -1;
        txtAmount.Text = string.Empty;
        SourceDataGrid.SelectedItem = null;
        TransactionDataGrid.SelectedItem = null;

        btnAdd.IsEnabled = false;
        btnEdit.IsEnabled = false;
        btnDelete.IsEnabled = false;
        btnSave.IsEnabled = ViewModel.TransactionList.Count > 0;
        txtSearch.Focus(FocusState.Programmatic);
    }

    private async void BtnAddProduct_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtAmount.Text) || cmbType.SelectedIndex == -1)
        {            
            await _dialogService.ShowErrorAsync("Vui lòng nhập đủ thông tin (Số lượng, Hình thức).");
            return;
        }

        if (SourceDataGrid.SelectedItem is not InventoryItem selectedSourceItem)
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn sản phẩm từ danh sách.");
            return;
        }

        var actionType = cmbType.SelectedValue.ToString();
        int currentInventory = selectedSourceItem.Inventory;
        int amount = int.Parse(txtAmount.Text);

        var existingTransaction = ViewModel.TransactionList
            .FirstOrDefault(t => t.ProductID == selectedSourceItem.ProductID && t.ActionType == actionType);

        if (existingTransaction != null)
        {            
            if(await _dialogService.ShowConfirmAsync("Sản phẩm trùng lặp", $"Sản phẩm '{selectedSourceItem.Name}' với hình thức '{actionType}' đã có trong danh sách chờ.\nBạn có muốn cộng dồn số lượng không?", "Đồng ý"))
            {
                int totalAmount = existingTransaction.Amount + amount;

                if (actionType == "Xuất kho" && totalAmount > currentInventory)
                {
                    await _dialogService.ShowErrorAsync($"Không thể cộng dồn. Tổng số lượng xuất kho ({totalAmount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                    return;
                }

                existingTransaction.Amount = totalAmount;
            }            
        }
        else
        {
            if (actionType == "Xuất kho" && amount > currentInventory)
            {                
                await _dialogService.ShowErrorAsync($"Không thể thêm. Số lượng xuất kho ({amount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                return;
            }

            string itemSource = selectedSourceItem.Source ?? "PRODUCTS";

            var newTransaction = new WarehouseTransaction
            {
                ProductID = selectedSourceItem.ProductID,
                InvoiceID = null,
                Name = selectedSourceItem.Name,
                Amount = amount,
                ActionType = actionType,
                Note = $"Nguồn: {itemSource}"
            };

            ViewModel.TransactionList.Add(newTransaction);
        }
        ClearInput();
    }
    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (TransactionDataGrid.SelectedItem is WarehouseTransaction selectedItem)
        {
            if (!int.TryParse(txtAmount.Text, out int newAmount) || newAmount <= 0)
            {
                await _dialogService.ShowErrorAsync("Số lượng không hợp lệ.");
                return;
            }

            string actionType = cmbType.SelectedValue.ToString();
            int currentInventory = ViewModel.GetCurrentInventory(selectedItem.ProductID);
            if (actionType == "Xuất kho" && newAmount > currentInventory)
            {
                await _dialogService.ShowErrorAsync($"Không thể cập nhật. Số lượng xuất kho ({newAmount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                return;
            }

            selectedItem.Amount = newAmount;
            selectedItem.ActionType = actionType;
            selectedItem.Name = txtName.Text;

            ClearInput();
            await _dialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }
    }
    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (TransactionDataGrid.SelectedItem is WarehouseTransaction selected)
        {
            ViewModel.TransactionList.Remove(selected);
            ClearInput();
        }
    }
    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInput();
    }
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            btnSave.IsEnabled = false;
            await ViewModel.SaveData();
            await _dialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
            ClearInput();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Không thể lưu dữ liệu", ex);
        }
        finally
        {
            btnSave.IsEnabled = ViewModel.TransactionList.Count > 0;
        }
    }

    private void SourceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SourceDataGrid.SelectedItem is InventoryItem selected)
        {            
            txtName.Text = selected.Name;
            txtInventory.Text = selected.Inventory.ToString();
            txtAmount.Text = string.Empty;
            cmbType.SelectedIndex = -1;

            btnAdd.IsEnabled = true;
            btnEdit.IsEnabled = false;
            btnDelete.IsEnabled = false;
            TransactionDataGrid.SelectedItem = null;
        }
    }

    private void TransactionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TransactionDataGrid.SelectedItem is WarehouseTransaction selected)
        {            
            txtName.Text = selected.Name ?? "";
            txtAmount.Text = selected.Amount.ToString();
            cmbType.SelectedItem = selected.ActionType;

            txtInventory.Text = ViewModel.GetCurrentInventory(selected.ProductID).ToString();

            btnAdd.IsEnabled = false;
            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = true;
            SourceDataGrid.SelectedItem = null;
        }
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.Search(txtSearch.Text);
    }

    private void txtAmount_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }
}