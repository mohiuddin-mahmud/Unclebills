using Nop.Core.Domain.Customers;
using Nop.Data;


namespace Nop.Services.Customers
{
    public partial class PetProfileService : IPetProfileService
    {
        #region Fields

        private readonly IRepository<PetProfile> _petProfileRepository;
   

        #endregion

        #region Ctor

        public PetProfileService(IRepository<PetProfile> petProfileRepository
          )
        {
            this._petProfileRepository = petProfileRepository;
   
        }

        #endregion

        #region Methods

        public virtual void InsertPetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Insert(pet);

        }

        public virtual void UpdatePetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Update(pet);


        }

        public virtual void DeletePetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Delete(pet);

        }

        public virtual PetProfile GetPetProfile(int id)
        {
            if (id == 0)
                return null;

            return _petProfileRepository.GetById(id);
        }

        public virtual IList<PetProfile> GetPetProfiles(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId");

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
}
