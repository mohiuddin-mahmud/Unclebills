namespace Nop.Core.Domain.RewardsCertificates;
public partial class RewardsCertificate : BaseEntity
{
    public string ExtraValueCardNumber { get; set; }
    public string RewardCode { get; set; }
    public double AmountRemaining { get; set; }
    public DateTime ExpiresOn { get; set; }
    public string CustomerEmail { get; set; }
}
