using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customers
{
    public partial class PetProfileService : IPetProfileService
    {
        #region Fields

        private readonly IRepository<PetProfile> _petProfileRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public PetProfileService(IRepository<PetProfile> petProfileRepository,
            IEventPublisher eventPublisher)
        {
            this._petProfileRepository = petProfileRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        public virtual void InsertPetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Insert(pet);

            _eventPublisher.EntityInserted(pet);
        }

        public virtual void UpdatePetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Update(pet);

            //event notification
            _eventPublisher.EntityUpdated(pet);
        }

        public virtual void DeletePetProfile(PetProfile pet)
        {
            if (pet == null)
                throw new ArgumentNullException("pet");

            _petProfileRepository.Delete(pet);

            _eventPublisher.EntityDeleted(pet);
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
