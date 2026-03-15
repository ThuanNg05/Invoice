using System.Text.RegularExpressions;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class DetailPlanksPage : Page
{
    public DetailPlanksViewModel ViewModel
    {
        get;
    }

    public DetailPlanksPage()
    {
        ViewModel = App.GetService<DetailPlanksViewModel>();
        InitializeComponent();
        ClearInputs();
    }

    private void ClearInputs()
    {
        size.Text = string.Empty;
        btnAdd.IsEnabled = true;
        btnDelete.IsEnabled = false;
        ListPlankGrid.SelectedIndex = -1;
        size.Focus(FocusState.Programmatic);
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(size.Text))
        {
            await App.ShowMessageAsync("Thông báo", "Kích thước không được bỏ trống.");
            size.Focus(FocusState.Programmatic);
            return;
        }
        if (!IsValidSizeFormat(size.Text.Trim().ToLower()))
        {
            await App.ShowMessageAsync("Lỗi nhập liệu", "Kích thước không đúng định dạng. Vui lòng nhập theo định dạng 'DxR' (ví dụ: 20x30).");
            size.Focus(FocusState.Programmatic);
            return;
        }
        var newSize = new DetailPlanks
        {
            sizeID = size.Text.Trim().ToLower(),
            inventory = 0
        };
        await ViewModel.AddPlankAsync(newSize);
        ClearInputs();
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ListPlankGrid.SelectedItem is not DetailPlanks selected) return;
        await ViewModel.DeletePlankAsync(selected);
        ClearInputs();
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ListPlankGrid.SelectedItem is not DetailPlanks selected) return;
        if (!IsValidSizeFormat(size.Text))
        {
            await App.ShowMessageAsync("Lỗi nhập liệu", "Định dạng không đúng, VD (DxR): 30x45");
            return;
        }
        if (string.IsNullOrEmpty(size.Text))
        {
            await App.ShowMessageAsync("Lỗi", "Kích thước không được bỏ trống");
            return;
        }
        var tmpPlank = new DetailPlanks
        {
            sizeID = size.Text.Trim().ToLower()
        };

        await ViewModel.UpdatePlankAsync(tmpPlank);
        ClearInputs();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        ClearInputs();
    }

    private void ListPlankGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListPlankGrid.SelectedItem is not DetailPlanks selected) return;
        size.Text = selected.sizeID;
        btnAdd.IsEnabled = false;
        btnDelete.IsEnabled = true;
    }

    // Helper function        
    private bool IsValidSizeFormat(string input)
    {
        // Cập nhật Regex:
        // ^            : Bắt đầu chuỗi
        // [1-9]        : Số đầu tiên phải là 1-9 (Chặn số 0)
        // [0-9]{0,2}   : Theo sau là tối đa 2 chữ số nữa (Tổng cộng tối đa 3 số: 1 -> 999)
        // x            : Dấu x ở giữa
        // [1-9][0-9]{0,2} : Logic tương tự cho số thứ 2
        // $            : Kết thúc chuỗi

        string pattern = @"^[1-9][0-9]{0,2}x[1-9][0-9]{0,2}$";

        return Regex.IsMatch(input, pattern);
    }
}