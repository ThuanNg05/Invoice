namespace Invoice.Core.Contracts;

public class DatabaseChangedMessage
{
    public string EntityName { get; }

    public DatabaseChangedMessage(string entityName)
    {
        EntityName = entityName;
    }
}
