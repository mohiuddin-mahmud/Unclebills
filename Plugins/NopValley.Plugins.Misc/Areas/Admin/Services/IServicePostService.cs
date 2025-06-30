using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopValley.Plugins.Misc.Domain;
using Nop.Core.Domain.Media;

namespace NopValley.Plugins.Misc.Areas.Admin.Services
{
    /// <summary>
    /// Service service interface
    /// </summary>
    public partial interface IServicePostService
    {
        #region Service posts
        Task<IPagedList<ServicePost>> GetAllServicePostsAsync(string servicePostName="", int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        /// <summary>
        /// Deletes a service post
        /// </summary>
        /// <param name="service">Service post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteServicePostsAsync(IList<ServicePost> serviceposts);
        /// <summary>
        /// Gets a service post
        /// </summary>
        /// <param name="serviceAdditionalId">Service post Additional identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the service post
        /// </returns>
        Task<ServicePost> GetServicePostByIdAsync(int servicePostId);

        /// <summary>
        /// Inserts a service post
        /// </summary>
        /// <param name="service">Service post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertServicePostAsync(ServicePost servicePost);
        Task UpdateServicePostAsync(ServicePost servicePost);
        Task DeleteServicePostAsync(ServicePost servicePost);

        Task<IList<ServicePost>> GetServicePostsByIdsAsync(int[] servicePostIds);

        Task<ServicePostCategory> GetServicePostCategoryByNameAsync(string name); 
        Task InsertServicePostCategoryAsync(ServicePostCategory servicePostCategory);
        Task UpdateServicePostCategoryAsync(ServicePostCategory servicePostCategory);

                

        Task DeleteServicePostCategoryAsync(ServicePostCategory servicePostCategory);
        Task<ServicePostCategory> GetServicePostCategoryByIdAsync(int servicePostCategoryId);
        Task DeleteServicePostCategoriesAsync(IList<ServicePostCategory> servicePostCategories);
        Task<IList<ServicePostCategory>> GetServicePostCategoriesByIdsAsync(int[] servicePostCategoryIds);
        Task<IList<ServicePostCategory>> GetAllServicePostCategoriesAsync(int storeId = 0, bool showHidden = false);
        Task<IPagedList<ServicePostCategory>> GetAllServicePostCategoriesAsync(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        #endregion

        Task DeleteServicePostPictureAsync(ServicePostPicture servicePostPicture);
        Task<IList<ServicePostPicture>> GetServicePostPicturesByProductIdAsync(int servicePostId);
        Task<ServicePostPicture> GetServicePostPictureByIdAsync(int servicePostPictureId);
        Task InsertServicePostPictureAsync(ServicePostPicture servicePostPicture);
        Task UpdateServicePostPictureAsync(ServicePostPicture servicePostPicture);
        Task<IDictionary<int, int[]>> GetServicePostsImagesIdsAsync(int[] servicePostsIds);

        Task<IList<Picture>> GetPicturesByServicePostIdAsync(int productId, int recordsToReturn = 0);
    }
}
