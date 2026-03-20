using System.Text.RegularExpressions;
using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Invoice.Views;

public sealed partial class PlanksPage : Page
{
    private readonly IDialogService _dialogService;
    public PlanksViewModel ViewModel
    {
        get;
    }

    public PlanksPage()
    {
        ViewModel = App.GetService<PlanksViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private void ClearInputs()
    {        
        StringHelper.ClearInputs(this);
        frameNO.Text = string.Empty;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        PlankGrid.SelectedIndex = -1;
        txtSize1.Focus(FocusState.Programmatic);
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var validation = ValidatePlankInput();
        if (!validation.IsValid)
        {
            await _dialogService.ShowErrorAsync(validation.Message);
            return;
        }

        try
        {
            var plank = new Frames
            {
                FrameNO = frameNO.Text.Trim(),
                size1 = string.IsNullOrWhiteSpace(txtSize1.Text) || string.IsNullOrWhiteSpace(txtAmount1.Text) ? null : $"{txtSize1.Text.Trim().ToLower()}-{txtAmount1.Text.Trim()}",
                size2 = string.IsNullOrWhiteSpace(txtSize2.Text) || string.IsNullOrWhiteSpace(txtAmount2.Text) ? null : $"{txtSize2.Text.Trim().ToLower()}-{txtAmount2.Text.Trim()}",
                size3 = string.IsNullOrWhiteSpace(txtSize3.Text) || string.IsNullOrWhiteSpace(txtAmount3.Text) ? null : $"{txtSize3.Text.Trim().ToLower()}-{txtAmount3.Text.Trim()}",
                size4 = string.IsNullOrWhiteSpace(txtSize4.Text) || string.IsNullOrWhiteSpace(txtAmount4.Text) ? null : $"{txtSize4.Text.Trim().ToLower()}-{txtAmount4.Text.Trim()}",
                size5 = string.IsNullOrWhiteSpace(txtSize5.Text) || string.IsNullOrWhiteSpace(txtAmount5.Text) ? null : $"{txtSize5.Text.Trim().ToLower()}-{txtAmount5.Text.Trim()}",
                size6 = string.IsNullOrWhiteSpace(txtSize6.Text) || string.IsNullOrWhiteSpace(txtAmount6.Text) ? null : $"{txtSize6.Text.Trim().ToLower()}-{txtAmount6.Text.Trim()}",
                size7 = string.IsNullOrWhiteSpace(txtSize7.Text) || string.IsNullOrWhiteSpace(txtAmount7.Text) ? null : $"{txtSize7.Text.Trim().ToLower()}-{txtAmount7.Text.Trim()}",
                size8 = string.IsNullOrWhiteSpace(txtSize8.Text) || string.IsNullOrWhiteSpace(txtAmount8.Text) ? null : $"{txtSize8.Text.Trim().ToLower()}-{txtAmount8.Text.Trim()}",
                size9 = string.IsNullOrWhiteSpace(txtSize9.Text) || string.IsNullOrWhiteSpace(txtAmount9.Text) ? null : $"{txtSize9.Text.Trim().ToLower()}-{txtAmount9.Text.Trim()}",
                size10 = string.IsNullOrWhiteSpace(txtSize10.Text) || string.IsNullOrWhiteSpace(txtAmount10.Text) ? null : $"{txtSize10.Text.Trim().ToLower()}-{txtAmount10.Text.Trim()}",
                Description = txtDescribe.Text.Trim().ToLower()
            };

            await ViewModel.AddFrameAsync(plank);
            await _dialogService.ShowSuccessAsync("Thêm thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Thêm thất bại", ex);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        if (await _dialogService.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa rập {selected.FrameNO}?", "Xóa"))
        {
            try
            {
                await ViewModel.DeleteFrameAsync(selected);
                await _dialogService.ShowSuccessAsync("Xóa thành công!");
                ClearInputs();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Xóa thất bại", ex);
            }
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        var validation = ValidatePlankInput();
        if (!validation.IsValid)
        {
            await _dialogService.ShowErrorAsync(validation.Message);
            return;
        }

        try
        {
            var tmpPlank = new Frames
            {
                FrameID = selected.FrameID,
                FrameNO = selected.FrameNO,
                size1 = string.IsNullOrWhiteSpace(txtSize1.Text) || string.IsNullOrWhiteSpace(txtAmount1.Text) ? null : $"{txtSize1.Text.Trim().ToLower()}-{txtAmount1.Text.Trim()}",
                size2 = string.IsNullOrWhiteSpace(txtSize2.Text) || string.IsNullOrWhiteSpace(txtAmount2.Text) ? null : $"{txtSize2.Text.Trim().ToLower()}-{txtAmount2.Text.Trim()}",
                size3 = string.IsNullOrWhiteSpace(txtSize3.Text) || string.IsNullOrWhiteSpace(txtAmount3.Text) ? null : $"{txtSize3.Text.Trim().ToLower()}-{txtAmount3.Text.Trim()}",
                size4 = string.IsNullOrWhiteSpace(txtSize4.Text) || string.IsNullOrWhiteSpace(txtAmount4.Text) ? null : $"{txtSize4.Text.Trim().ToLower()}-{txtAmount4.Text.Trim()}",
                size5 = string.IsNullOrWhiteSpace(txtSize5.Text) || string.IsNullOrWhiteSpace(txtAmount5.Text) ? null : $"{txtSize5.Text.Trim().ToLower()}-{txtAmount5.Text.Trim()}",
                size6 = string.IsNullOrWhiteSpace(txtSize6.Text) || string.IsNullOrWhiteSpace(txtAmount6.Text) ? null : $"{txtSize6.Text.Trim().ToLower()}-{txtAmount6.Text.Trim()}",
                size7 = string.IsNullOrWhiteSpace(txtSize7.Text) || string.IsNullOrWhiteSpace(txtAmount7.Text) ? null : $"{txtSize7.Text.Trim().ToLower()}-{txtAmount7.Text.Trim()}",
                size8 = string.IsNullOrWhiteSpace(txtSize8.Text) || string.IsNullOrWhiteSpace(txtAmount8.Text) ? null : $"{txtSize8.Text.Trim().ToLower()}-{txtAmount8.Text.Trim()}",
                size9 = string.IsNullOrWhiteSpace(txtSize9.Text) || string.IsNullOrWhiteSpace(txtAmount9.Text) ? null : $"{txtSize9.Text.Trim().ToLower()}-{txtAmount9.Text.Trim()}",
                size10 = string.IsNullOrWhiteSpace(txtSize10.Text) || string.IsNullOrWhiteSpace(txtAmount10.Text) ? null : $"{txtSize10.Text.Trim().ToLower()}-{txtAmount10.Text.Trim()}",
                Description = txtDescribe.Text.Trim().ToLower()
            };

            await ViewModel.UpdateFrameAsync(tmpPlank);
            await _dialogService.ShowSuccessAsync("Cập nhật thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Cập nhật thất bại", ex);
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
        PlankGrid.SelectedItem = null;
    }

    private void PlankGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        frameNO.Text = selected.FrameNO;
        frameNO.IsEnabled = false;
        txtDescribe.Text = selected.Description ?? "";

        // Size
        txtSize1.Text = GetSizePart(selected.size1, 0);
        txtSize2.Text = GetSizePart(selected.size2, 0);
        txtSize3.Text = GetSizePart(selected.size3, 0);
        txtSize4.Text = GetSizePart(selected.size4, 0);
        txtSize5.Text = GetSizePart(selected.size5, 0);
        txtSize6.Text = GetSizePart(selected.size6, 0);
        txtSize7.Text = GetSizePart(selected.size7, 0);
        txtSize8.Text = GetSizePart(selected.size8, 0);
        txtSize9.Text = GetSizePart(selected.size9, 0);
        txtSize10.Text = GetSizePart(selected.size10, 0);

        // Amount
        txtAmount1.Text = GetSizePart(selected.size1, 1);
        txtAmount2.Text = GetSizePart(selected.size2, 1);
        txtAmount3.Text = GetSizePart(selected.size3, 1);
        txtAmount4.Text = GetSizePart(selected.size4, 1);
        txtAmount5.Text = GetSizePart(selected.size5, 1);
        txtAmount6.Text = GetSizePart(selected.size6, 1);
        txtAmount7.Text = GetSizePart(selected.size7, 1);
        txtAmount8.Text = GetSizePart(selected.size8, 1);
        txtAmount9.Text = GetSizePart(selected.size9, 1);
        txtAmount10.Text = GetSizePart(selected.size10, 1);

        btnAdd.IsEnabled = false;
        btnUpdate.IsEnabled = true;
        btnDelete.IsEnabled = true;
    }

    private void Amount_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }

    // Helper function
    private string GetSizePart(object rawInput, int index)
    {
        if (rawInput == null) return string.Empty;
        string text = rawInput.ToString();
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var parts = text.Split('-');
        if (index < parts.Length) return parts[index];
        return string.Empty;
    }

    private (bool IsValid, string Message) ValidatePlankInput()
    {
        if (frameNO.Text.Trim() == "")
        {
            return (false, "Frame NO không được để trống.");
        }

        int validPairsCount = 0;

        for (int i = 1; i <= 10; i++)
        {
            var txtSize = this.FindName($"txtSize{i}") as TextBox;
            var txtAmount = this.FindName($"txtAmount{i}") as TextBox;

            if (txtSize == null || txtAmount == null) continue;

            string rawSize = txtSize.Text?.Trim();
            string sizeVal = rawSize?.ToLower() ?? "";
            string amountVal = txtAmount.Text?.Trim();

            bool hasSize = !string.IsNullOrEmpty(sizeVal);
            bool hasAmount = !string.IsNullOrEmpty(amountVal);

            if (hasSize && hasAmount)
            {
                if (rawSize != sizeVal)
                {
                    txtSize.Text = sizeVal;
                }

                if (!IsValidSizeFormat(sizeVal))
                {
                    return (false, $"Lỗi ở dòng {i}: Size '{rawSize}' không hợp lệ.\n" +
                                   "Yêu cầu:\n" +
                                   "- Định dạng 'RộngxCao' (VD: 50x107).\n" +
                                   "- Không bắt đầu bằng số 0.\n" +
                                   "- Tối đa 3 chữ số cho mỗi cạnh (1-999).");
                }
                if (!int.TryParse(amountVal, out _))
                {
                    return (false, $"Lỗi ở dòng {i}: Amount phải là một số nguyên.");
                }
                validPairsCount++;
            }
            else if (hasSize || hasAmount)
            {
                return (false, $"Dòng số {i} chưa nhập đủ dữ liệu (Cần cả Size và Amount).");
            }
        }

        if (validPairsCount >= 2)
        {
            return (true, "Hợp lệ");
        }
        else
        {
            return (false, "Cần nhập ít nhất 2 dòng dữ liệu (Size và Amount) hợp lệ.");
        }
    }

    private bool IsValidSizeFormat(string input)
    {        
        string pattern = @"^[1-9][0-9]{0,2}x[1-9][0-9]{0,2}$";

        return Regex.IsMatch(input, pattern);
    }
}