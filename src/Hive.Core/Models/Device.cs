namespace Hive.Core.Models;

public class Device
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public FamilyAccount? FamilyAccount { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ActivationCode { get; set; } = string.Empty;
    public DeviceSize Size { get; set; }
    public bool IsActivated { get; set; }
}

public enum DeviceSize
{
    Ten,
    Fifteen,
    TwentySeven
}
