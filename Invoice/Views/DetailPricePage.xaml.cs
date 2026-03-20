using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class DetailPricePage : Page
{
    private readonly IDialogService _dialogService;
    public DetailPriceViewModel ViewModel
    {
        get;
    }

    public DetailPricePage()
    {
        ViewModel = App.GetService<DetailPriceViewModel>();
        _dialogService = App.GetService<IDialogService>();
        InitializeComponent();
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tmpPrice = new DetailPrice
            {
                ConfigID = 1,
                PrKieng = double.TryParse(txtKieng.Text, out double kieng) ? kieng : 0,
                PrNhL = double.TryParse(txtNhL.Text, out double nhl) ? nhl : 0,
                PrNhN = double.TryParse(txtNhN.Text, out double nhn) ? nhn : 0,
                PrG_l = double.TryParse(txtG_l.Text, out double gl) ? gl : 0,
                PrG_n = double.TryParse(txtG_n.Text, out double gn) ? gn : 0,
                PrDl = double.TryParse(txtDl.Text, out double dl) ? dl : 0,
                PrHau = double.TryParse(txtHau.Text, out double hau) ? hau : 0,
                PrLua = double.TryParse(txtLua.Text, out double lua) ? lua : 0,
                PrKt = double.TryParse(txtKt.Text, out double kt) ? kt : 0,
                PrOc = double.TryParse(txtOc.Text, out double oc) ? oc : 0,
                PrNhom = double.TryParse(txtNhom.Text, out double nhom) ? nhom : 0,
                Pr7f = double.TryParse(txt7f.Text, out double go7f) ? go7f : 0,
                Pr2D = double.TryParse(txt2D.Text, out double do2d) ? do2d : 0,
                PrDecal = double.TryParse(txtDecal.Text, out double decal) ? decal : 0
            };
            await ViewModel.UpdateDetailPriceAsync(tmpPrice);
            await _dialogService.ShowSuccessAsync("Cập nhật thành công!");
        } catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Cập nhật thất bại:", ex);
        }
    }
}