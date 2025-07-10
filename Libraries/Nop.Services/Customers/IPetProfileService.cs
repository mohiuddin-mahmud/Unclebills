using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial interface IPetProfileService
    {
        /// <summary>
        /// Inserts a pet profile
        /// </summary>
        /// <param name="pet">pet profile</param>
        void InsertPetProfile(PetProfile pet);

        /// <summary>
        /// Updates the PetProfile
        /// </summary>
        /// <param name="pet">The pet profile</param>
        void UpdatePetProfile(PetProfile pet);

        /// <summary>
        /// Deletes a PetProfile
        /// </summary>
        /// <param name="pet">The pet profile</param>
        void DeletePetProfile(PetProfile pet);

        /// <summary>
        /// Gets specific PetProfile
        /// </summary>
        /// <param name="id">The Pet Profile Id</param>
        /// <returns>Order</returns>
        PetProfile GetPetProfile(int id);

        /// <summary>
        /// Gets all PetProfile associated with customer
        /// </summary>
        /// <param name="customerId">The CounterPoint customer Id</param>
        /// <returns>Order</returns>
        IList<PetProfile> GetPetProfiles(string customerId);
    }
}
