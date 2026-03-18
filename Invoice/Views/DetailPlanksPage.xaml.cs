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
        btnDelete.IsEnabled = false;
    }

    private void ClearInputs()
    {
        size.Text = string.Empty;
        btnAdd.IsEnabled = true;
        btnDelete.IsEnabled = false;
        ListPlankGrid.SelectedItem = -1;
        size.Focus(FocusState.Programmatic);
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(size.Text))
        {
            await App.ShowErrorAsync("Kích thước không được bỏ trống.");
            size.Focus(FocusState.Programmatic);
            return;
        }
        if (!IsValidSizeFormat(size.Text.Trim().ToLower()))
        {
            await App.ShowErrorAsync("Kích thước không đúng định dạng. Vui lòng nhập theo định dạng 'DxR' (ví dụ: 20x30).");
            size.Focus(FocusState.Programmatic);
            return;
        }

        try
        {
            var newSize = new DetailPlanks
            {
                sizeID = size.Text.Trim().ToLower(),
                inventory = 0
            };
            await ViewModel.AddPlankAsync(newSize);
            await App.ShowSuccessAsync("Thêm thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await App.ShowErrorAsync("Thêm thất bại", ex);
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ListPlankGrid.SelectedItem is not DetailPlanks selected) return;
        if (await App.ShowConfirmAsync("Xác nhận xóa", $"Bạn có chắc muốn xóa kích thước {selected.sizeID}?", "Xóa"))
        {
            try
            {
                await ViewModel.DeletePlankAsync(selected);
                await App.ShowSuccessAsync("Xóa thành công!");
                ClearInputs();
            }
            catch (Exception ex)
            {
                await App.ShowErrorAsync("Xóa thất bại", ex);
            }
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ListPlankGrid.SelectedItem is not DetailPlanks selected) return;
        if (string.IsNullOrEmpty(size.Text))
        {
            await App.ShowErrorAsync("Kích thước không được bỏ trống");
            return;
        }
        if (!IsValidSizeFormat(size.Text))
        {
            await App.ShowErrorAsync("Định dạng không đúng, VD (DxR): 30x45");
            return;
        }

        try
        {
            var tmpPlank = new DetailPlanks
            {
                sizeID = size.Text.Trim().ToLower()
            };

            await ViewModel.UpdatePlankAsync(tmpPlank);
            await App.ShowSuccessAsync("Cập nhật thành công!");
            ClearInputs();
        }
        catch (Exception ex)
        {
            await App.ShowErrorAsync("Cập nhật thất bại", ex);
        }
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
        string pattern = @"^[1-9][0-9]{0,2}x[1-9][0-9]{0,2}$";

        return Regex.IsMatch(input, pattern);
    }
}