using System.Text.RegularExpressions;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Invoice.Views;

public sealed partial class PlanksPage : Page
{
    public PlanksViewModel ViewModel
    {
        get;
    }

    public PlanksPage()
    {
        ViewModel = App.GetService<PlanksViewModel>();
        InitializeComponent();
        ClearInputs(this);
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

        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        PlankGrid.SelectedIndex = -1;

        size1.Focus(FocusState.Programmatic);
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidatePlankInput().IsValid)
        {
            await App.ShowMessageAsync("Lỗi nhập liệu", ValidatePlankInput().Message);
            return;
        }

        var plank = new Frames
        {
            FrameNO = frameNO.Text.Trim(),
            size1 = string.IsNullOrWhiteSpace(size1.Text) || string.IsNullOrWhiteSpace(amount1.Text) ? null : $"{size1.Text.Trim().ToLower()}-{amount1.Text.Trim()}",
            size2 = string.IsNullOrWhiteSpace(size2.Text) || string.IsNullOrWhiteSpace(amount2.Text) ? null : $"{size2.Text.Trim().ToLower()}-{amount2.Text.Trim()}",
            size3 = string.IsNullOrWhiteSpace(size3.Text) || string.IsNullOrWhiteSpace(amount3.Text) ? null : $"{size3.Text.Trim().ToLower()}-{amount3.Text.Trim()}",
            size4 = string.IsNullOrWhiteSpace(size4.Text) || string.IsNullOrWhiteSpace(amount4.Text) ? null : $"{size4.Text.Trim().ToLower()}-{amount4.Text.Trim()}",
            size5 = string.IsNullOrWhiteSpace(size5.Text) || string.IsNullOrWhiteSpace(amount5.Text) ? null : $"{size5.Text.Trim().ToLower()}-{amount5.Text.Trim()}",
            size6 = string.IsNullOrWhiteSpace(size6.Text) || string.IsNullOrWhiteSpace(amount6.Text) ? null : $"{size6.Text.Trim().ToLower()}-{amount6.Text.Trim()}",
            size7 = string.IsNullOrWhiteSpace(size7.Text) || string.IsNullOrWhiteSpace(amount7.Text) ? null : $"{size7.Text.Trim().ToLower()}-{amount7.Text.Trim()}",
            size8 = string.IsNullOrWhiteSpace(size8.Text) || string.IsNullOrWhiteSpace(amount8.Text) ? null : $"{size8.Text.Trim().ToLower()}-{amount8.Text.Trim()}",
            size9 = string.IsNullOrWhiteSpace(size9.Text) || string.IsNullOrWhiteSpace(amount9.Text) ? null : $"{size9.Text.Trim().ToLower()}-{amount9.Text.Trim()}",
            size10 = string.IsNullOrWhiteSpace(size10.Text) || string.IsNullOrWhiteSpace(amount10.Text) ? null : $"{size10.Text.Trim().ToLower()}-{amount10.Text.Trim()}",
            Description = txtDescribe.Text.Trim().ToLower()
        };

        await ViewModel.AddFrameAsync(plank);
        ClearInputs(this);
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        await ViewModel.DeleteFrameAsync(selected);
        ClearInputs(this);
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        if (!ValidatePlankInput().IsValid)
        {
            await App.ShowMessageAsync("Lỗi nhập liệu", ValidatePlankInput().Message);
            return;
        }
        var tmpPlank = new Frames
        {
            FrameID = selected.FrameID,
            FrameNO = selected.FrameNO,
            size1 = string.IsNullOrWhiteSpace(size1.Text) || string.IsNullOrWhiteSpace(amount1.Text) ? null : $"{size1.Text.Trim().ToLower()}-{amount1.Text.Trim()}",
            size2 = string.IsNullOrWhiteSpace(size2.Text) || string.IsNullOrWhiteSpace(amount2.Text) ? null : $"{size2.Text.Trim().ToLower()}-{amount2.Text.Trim()}",
            size3 = string.IsNullOrWhiteSpace(size3.Text) || string.IsNullOrWhiteSpace(amount3.Text) ? null : $"{size3.Text.Trim().ToLower()}-{amount3.Text.Trim()}",
            size4 = string.IsNullOrWhiteSpace(size4.Text) || string.IsNullOrWhiteSpace(amount4.Text) ? null : $"{size4.Text.Trim().ToLower()}-{amount4.Text.Trim()}",
            size5 = string.IsNullOrWhiteSpace(size5.Text) || string.IsNullOrWhiteSpace(amount5.Text) ? null : $"{size5.Text.Trim().ToLower()}-{amount5.Text.Trim()}",
            size6 = string.IsNullOrWhiteSpace(size6.Text) || string.IsNullOrWhiteSpace(amount6.Text) ? null : $"{size6.Text.Trim().ToLower()}-{amount6.Text.Trim()}",
            size7 = string.IsNullOrWhiteSpace(size7.Text) || string.IsNullOrWhiteSpace(amount7.Text) ? null : $"{size7.Text.Trim().ToLower()}-{amount7.Text.Trim()}",
            size8 = string.IsNullOrWhiteSpace(size8.Text) || string.IsNullOrWhiteSpace(amount8.Text) ? null : $"{size8.Text.Trim().ToLower()}-{amount8.Text.Trim()}",
            size9 = string.IsNullOrWhiteSpace(size9.Text) || string.IsNullOrWhiteSpace(amount9.Text) ? null : $"{size9.Text.Trim().ToLower()}-{amount9.Text.Trim()}",
            size10 = string.IsNullOrWhiteSpace(size10.Text) || string.IsNullOrWhiteSpace(amount10.Text) ? null : $"{size10.Text.Trim().ToLower()}-{amount10.Text.Trim()}",
            Description = txtDescribe.Text.Trim().ToLower()
        };

        await ViewModel.UpdateFrameAsync(tmpPlank);
        ClearInputs(this);
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs(this);
        PlankGrid.SelectedItem = null;
    }

    private void PlankGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlankGrid.SelectedItem is not Frames selected) return;
        frameNO.Text = selected.FrameNO;
        frameNO.IsEnabled = false;
        txtDescribe.Text = selected.Description ?? "";

        // Size
        size1.Text = GetSizePart(selected.size1, 0);
        size2.Text = GetSizePart(selected.size2, 0);
        size3.Text = GetSizePart(selected.size3, 0);
        size4.Text = GetSizePart(selected.size4, 0);
        size5.Text = GetSizePart(selected.size5, 0);
        size6.Text = GetSizePart(selected.size6, 0);
        size7.Text = GetSizePart(selected.size7, 0);
        size8.Text = GetSizePart(selected.size8, 0);
        size9.Text = GetSizePart(selected.size9, 0);
        size10.Text = GetSizePart(selected.size10, 0);

        // Amount
        amount1.Text = GetSizePart(selected.size1, 1);
        amount2.Text = GetSizePart(selected.size2, 1);
        amount3.Text = GetSizePart(selected.size3, 1);
        amount4.Text = GetSizePart(selected.size4, 1);
        amount5.Text = GetSizePart(selected.size5, 1);
        amount6.Text = GetSizePart(selected.size6, 1);
        amount7.Text = GetSizePart(selected.size7, 1);
        amount8.Text = GetSizePart(selected.size8, 1);
        amount9.Text = GetSizePart(selected.size9, 1);
        amount10.Text = GetSizePart(selected.size10, 1);

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
            var txtSize = this.FindName($"size{i}") as TextBox;
            var txtAmount = this.FindName($"amount{i}") as TextBox;

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