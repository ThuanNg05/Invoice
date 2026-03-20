using System.Diagnostics;
using CommunityToolkit.WinUI.UI.Controls;
using Invoice.Contracts.Services;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Core.Services;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class IOPlanksPage : Page
{
    private readonly IDialogService _dialogService;
    public IOPlanksViewModel ViewModel
    {
        get;
    }

    public IOPlanksPage()
    {
        ViewModel = App.GetService<IOPlanksViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
    }

    private void PlankGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is Frames selectedFrame)
        {
            FillData(selectedFrame);
            amount.IsEnabled = true;
            btnSave.IsEnabled = true;
            frameNO.Text = selectedFrame.FrameNO;
            txtDescribe.Text = selectedFrame.Description;
        }
    }

    private void FillData(Frames frame)
    {
        void SplitAndSet(string rawData, TextBox txtSize, TextBox txtAmount)
        {
            if (string.IsNullOrWhiteSpace(rawData))
            {
                txtSize.Text = string.Empty;
                txtAmount.Text = string.Empty;
                return;
            }

            int lastDashIndex = rawData.LastIndexOf('-');
            if (lastDashIndex > 0)
            {
                txtSize.Text = rawData.Substring(0, lastDashIndex);
                txtAmount.Text = rawData.Substring(lastDashIndex + 1);
            }
            else
            {
                txtSize.Text = rawData;
                txtAmount.Text = "0";
            }
        }

        SplitAndSet(frame.size1, size1, amount1);
        SplitAndSet(frame.size2, size2, amount2);
        SplitAndSet(frame.size3, size3, amount3);
        SplitAndSet(frame.size4, size4, amount4);
        SplitAndSet(frame.size5, size5, amount5);
        SplitAndSet(frame.size6, size6, amount6);
        SplitAndSet(frame.size7, size7, amount7);
        SplitAndSet(frame.size8, size8, amount8);
        SplitAndSet(frame.size9, size9, amount9);
        SplitAndSet(frame.size10, size10, amount10);
    }

    private void ClearInputs()
    {        
        StringHelper.ClearInputs(this);
        amount.IsEnabled = false;
        btnSave.IsEnabled = false;
        cbbPlankType.SelectedIndex = -1;
        PlankGrid.SelectedIndex = -1;
        PlankGrid.SelectedItem = null;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selectedFrame)
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn mã rập để lưu kho");
            return;
        }
        if (string.IsNullOrEmpty(amount.Text) || !int.TryParse(amount.Text, out int bigQty) || bigQty <= 0)
        {
            await _dialogService.ShowErrorAsync("Vui lòng nhập số lượng nhập kho hợp lệ (>0)");
            amount.Focus(FocusState.Programmatic);
            return;
        }

        if (cbbPlankType.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Content == null)
        {
            await _dialogService.ShowErrorAsync("Vui lòng chọn loại ván.");
            cbbPlankType.Focus(FocusState.Programmatic);
            return;
        }

        string typePlank = selectedItem.Content.ToString();
        var dataService = App.GetService<IDataService>();
        var materials = await dataService.GetMaterials();
        var targetMaterial = materials.FirstOrDefault(m => m.Name.Equals(typePlank, StringComparison.OrdinalIgnoreCase));

        if (targetMaterial == null)
        {
            await _dialogService.ShowErrorAsync($"Không tìm thấy vật tư loại '{typePlank}' trong hệ thống.");
            return;
        }

        long materialID = targetMaterial.ProductID;

        try
        {
            btnSave.IsEnabled = false;
            ViewModel.IsBusy = true;

            if (dataService is SupabaseDataService supabaseService)
            {
                // Note: Server-side validation is also performed in the RPC for data integrity.
                await supabaseService.ProcessInventoryTransaction(selectedFrame, bigQty, materialID);
                ClearInputs();
                amount.Text = string.Empty;
                await _dialogService.ShowSuccessAsync($"Đã xuất {bigQty} tấm '{targetMaterial.Name}' và nhập kho ván nhỏ thành công.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing inventory transaction: {ex.Message}");
            await _dialogService.ShowErrorAsync("Đã có lỗi xảy ra khi lưu kho", ex);
        }
        finally
        {
            ViewModel.IsBusy = false;
            btnSave.IsEnabled = true;
        }
    }

    private void Amount_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (string.IsNullOrEmpty(args.NewText))
        {
            return;
        }

        if (args.NewText.Any(c => !char.IsDigit(c)))
        {
            args.Cancel = true;
            return;
        }

        if (long.TryParse(args.NewText, out long value))
        {
            if (value <= 0)
            {
                args.Cancel = true;
            }
        }
        else
        {
            args.Cancel = true;
        }
    }
}