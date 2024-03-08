namespace JsonOrder.Maps;

public class Promotion
{
    public string ExternalId { get; set; }
    public string ProductId { get; set; }
    public string Action { get; set; }
    public string OverrideRate { get; set; }
    public DateTime? PromoDate { get; set; }
}
