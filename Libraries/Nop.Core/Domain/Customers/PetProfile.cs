namespace Nop.Core.Domain.Customers;

public partial class PetProfile : BaseEntity
{
    public string CustomerId { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string Gender { get; set; }
}