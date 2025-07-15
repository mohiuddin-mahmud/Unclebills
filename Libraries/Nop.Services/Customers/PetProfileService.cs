using Nop.Core.Domain.Customers;
using Nop.Data;


namespace Nop.Services.Customers;
public partial class PetProfileService : IPetProfileService
{
    #region Fields

    private readonly IRepository<PetProfile> _petProfileRepository;


    #endregion

    #region Ctor

    public PetProfileService(IRepository<PetProfile> petProfileRepository
      )
    {
        _petProfileRepository = petProfileRepository;

    }

    #endregion

    #region Methods

    public virtual async Task InsertPetProfile(PetProfile pet)
    {
        if (pet == null)
            throw new ArgumentNullException("pet");

        await _petProfileRepository.InsertAsync(pet);

    }

    public virtual async Task UpdatePetProfile(PetProfile pet)
    {
        if (pet == null)
            throw new ArgumentNullException("pet");

        await _petProfileRepository.UpdateAsync(pet);


    }

    public virtual async Task DeletePetProfile(PetProfile pet)
    {
        if (pet == null)
            throw new ArgumentNullException("pet");

        await _petProfileRepository.DeleteAsync(pet);

    }

    public virtual async Task<PetProfile> GetPetProfile(int id)
    {
        if (id == 0)
            return null;

        return await _petProfileRepository.GetByIdAsync(id);
    }

    public virtual async Task<IList<PetProfile>> GetPetProfiles(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentNullException("customerId");

        await Task.Yield();
        try
        {
            var query = _petProfileRepository.Table;
            return query.Where(o => o.CustomerId == customerId).ToList();
        }
        catch
        {
            return new List<PetProfile>();
        }
    }

    #endregion
}
