using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.Services;
using Invoice.Core.Contracts;

namespace Invoice.ViewModels;

public partial class ViewModelBase : ObservableRecipient
{
    protected readonly IDialogService DialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    // Alias for backward compatibility with XAML bindings
    public bool IsLoading
    {
        get => IsBusy;
        set => IsBusy = value;
    }

    public ViewModelBase(IDialogService dialogService)
    {
        DialogService = dialogService;
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsBusy) || e.PropertyName == nameof(StatusMessage))
        {
            WeakReferenceMessenger.Default.Send(new IsBusyMessage(IsBusy, StatusMessage));
        }
    }

    protected async Task ExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DialogService.ShowErrorAsync(errorMessage ?? "Đã xảy ra lỗi", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
