namespace Invoice.Contracts.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string content);
    Task ShowSuccessAsync(string content);
    Task ShowErrorAsync(string content, Exception? ex = null);
    Task<bool> ShowConfirmAsync(string title, string content, string? primaryButton = null);
}
