using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class IOActionsPage : Page
{
    public IOActionsViewModel ViewModel
    {
        get;
    }

    public IOActionsPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<IOActionsViewModel>();
        ClearInput();
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
        if (string.IsNullOrEmpty(txtID.Text) || string.IsNullOrEmpty(txtAmount.Text) || cmbType.SelectedIndex == -1)
        {            
            await App.ShowErrorAsync("Vui lòng nhập đủ thông tin (Số lượng, Hình thức).");
            return;
        }
        if (!int.TryParse(txtAmount.Text, out int amount) || amount <= 0)
        {
            await App.ShowErrorAsync("Số lượng phải là số dương.");
            return;
        }

        if (!long.TryParse(txtID.Text, out long productId))
        {
            await App.ShowErrorAsync("Mã sản phẩm không hợp lệ.");
            return;
        }

        string actionType = cmbType.SelectedValue.ToString();
        int currentInventory = int.Parse(txtInventory.Text);

        var existingTransaction = ViewModel.TransactionList
            .FirstOrDefault(t => t.ProductID == productId && t.ActionType == actionType);

        if (existingTransaction != null)
        {            
            if(await App.ShowConfirmAsync("Sản phẩm trùng lặp", $"Sản phẩm '{txtName.Text}' với hình thức '{actionType}' đã có trong danh sách chờ.\nBạn có muốn cộng dồn số lượng không?", "Đồng ý"))
            {
                int totalAmount = existingTransaction.Amount + amount;

                if (actionType == "Xuất kho" && totalAmount > currentInventory)
                {
                    await App.ShowErrorAsync($"Không thể cộng dồn. Tổng số lượng xuất kho ({totalAmount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                    return;
                }

                existingTransaction.Amount = totalAmount;
            }            
        }
        else
        {
            if (actionType == "Xuất kho" && amount > currentInventory)
            {                
                await App.ShowErrorAsync($"Không thể thêm. Số lượng xuất kho ({amount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                return;
            }

            var selectedSourceItem = SourceDataGrid.SelectedItem as InventoryItem;
            string itemSource = selectedSourceItem?.Source ?? "PRODUCTS";

            var newTransaction = new WarehouseTransaction
            {
                InvoiceID = null,
                ProductID = productId,
                Name = txtName.Text,
                Amount = amount,
                ActionType = actionType,
                Note = $"Nguồn: {itemSource}"
            };

            ViewModel.TransactionList.Add(newTransaction);
        }
        //btnSave.IsEnabled = true;
        ClearInput();
    }
    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (TransactionDataGrid.SelectedItem is WarehouseTransaction selectedItem)
        {
            if (!int.TryParse(txtAmount.Text, out int newAmount) || newAmount <= 0)
            {
                await App.ShowErrorAsync("Số lượng không hợp lệ.");
                return;
            }

            string actionType = cmbType.SelectedValue.ToString();
            int currentInventory = ViewModel.GetCurrentInventory(selectedItem.ProductID);
            if (actionType == "Xuất kho" && newAmount > currentInventory)
            {
                await App.ShowErrorAsync($"Không thể cập nhật. Số lượng xuất kho ({newAmount}) vượt quá tồn kho hiện tại ({currentInventory}).");
                return;
            }

            selectedItem.Amount = newAmount;
            selectedItem.ActionType = actionType;
            selectedItem.Name = txtName.Text;

            ClearInput();
            await App.ShowSuccessAsync("Cập nhật thành công.");
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
            ClearInput();
            await App.ShowSuccessAsync("Cập nhật kho thành công.");
        }
        catch (Exception ex)
        {
            await App.ShowErrorAsync("Không thể lưu dữ liệu", ex);
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
            txtID.Text = selected.ProductID.ToString();
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
            txtID.Text = selected.ProductID.ToString();
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