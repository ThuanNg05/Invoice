namespace Invoice.Core.Contracts;

public class IsBusyMessage
{
    public bool IsBusy { get; }
    public string StatusMessage { get; }

    public IsBusyMessage(bool isBusy, string statusMessage = "")
    {
        IsBusy = isBusy;
        StatusMessage = statusMessage;
    }
}
