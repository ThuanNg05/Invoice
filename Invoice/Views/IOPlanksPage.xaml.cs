using System.Diagnostics;
using CommunityToolkit.WinUI.UI.Controls;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Core.Services;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Invoice.Views;

public sealed partial class IOPlanksPage : Page
{
    public IOPlanksViewModel ViewModel
    {
        get;
    }

    public IOPlanksPage()
    {
        ViewModel = App.GetService<IOPlanksViewModel>();
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

    private void ClearInputs(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
            ClearInputs(child);
        }
        PlankGrid.SelectedIndex = -1;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs(this);
        PlankGrid.SelectedItem = null;
        amount.IsEnabled = false;
        btnSave.IsEnabled = false;
        cbbPlankType.SelectedIndex = -1;
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selectedFrame)
        {
            await App.ShowMessageAsync("Thông báo", "Vui lòng chọn mã rập để lưu kho");
            return;
        }
        if (string.IsNullOrEmpty(amount.Text) || !int.TryParse(amount.Text, out int bigQty) || bigQty <= 0)
        {
            await App.ShowMessageAsync("Thông báo", "Vui lòng nhập số lượng nhập kho hợp lệ (>0)");
            amount.Focus(FocusState.Programmatic);
            return;
        }

        if (cbbPlankType.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Content == null)
        {
            await App.ShowMessageAsync("Thông báo", "Vui lòng chọn loại ván.");
            cbbPlankType.Focus(FocusState.Programmatic);
            return;
        }

        string typePlank = selectedItem.Content.ToString();
        long materialID;
        if (typePlank == "HP")
        {
            materialID = 10012;
        }
        else if (typePlank == "MDF")
        {
            materialID = 10013;
        }
        else
        {
            await App.ShowMessageAsync("Thông báo", $"Loại ván {typePlank} không hợp lệ.");
            cbbPlankType.Focus(FocusState.Programmatic);
            return;
        }

        try
        {
            btnSave.IsEnabled = false;
            ViewModel.IsLoading = true;

            var dataService = App.GetService<IDataService>();
            if (dataService is SupabaseDataService supabaseService)
            {
                bool isStockValid = await supabaseService.ValidateMaterialStock(materialID, bigQty);
                if (!isStockValid)
                {
                    await App.ShowMessageAsync("Thông báo", $"Ván {typePlank} không đủ trong kho để nhập ván. Vui lòng kiểm tra lại.");
                    return;
                }

                await supabaseService.ProcessInventoryTransaction(selectedFrame, bigQty, materialID);
                BtnReset_Click(null, null);
                amount.Text = string.Empty;
                await App.ShowMessageAsync("Thông báo", $"Đã xuất {bigQty} tấm '{materialID}' và nhập kho ván nhỏ thành công.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing inventory transaction: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", $"Đã có lỗi xảy ra khi lưu kho");
        }
        finally
        {
            ViewModel.IsLoading = false;
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