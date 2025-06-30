using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Data;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Domain;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Core.Domain.Media;

namespace NopValley.Plugins.Misc.Areas.Admin.Services
{
    /// <summary>
    /// Blog service
    /// </summary>
    public partial class ServicePostService : IServicePostService
    {
        #region Fields

        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<ServicePostCategory> _servicePostCategoryRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IRepository<ServicePost> _servicePostRepository;
        protected readonly IRepository<ServicePostPicture> _servicePostPictureRepository;
        protected readonly IRepository<Picture> _pictureRepository;

        #endregion

        #region Ctor

        public ServicePostService(
            IRepository<BlogComment> blogCommentRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IRepository<ServicePostCategory> serviceCategoryRepository,
            IWorkContext workContext,
            ICustomerService customerService,
            IRepository<ServicePost> servicePostRepository,
            IRepository<ServicePostPicture> servicePostPictureRepository,
            IRepository<Picture> pictureRepository)
        {
            _blogCommentRepository = blogCommentRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _servicePostCategoryRepository = serviceCategoryRepository;
            _workContext = workContext;
            _customerService = customerService;
            _servicePostRepository = servicePostRepository;
            _servicePostPictureRepository = servicePostPictureRepository;
            _pictureRepository = pictureRepository;
        }

        #endregion

        #region Methods

        #region Blog posts

        /// <summary>
        /// Deletes a blog post additional
        /// </summary>
        /// <param name="serviceAdditional">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteServicePostAsync(ServicePost servicePost)
        {
            await _servicePostRepository.DeleteAsync(servicePost);
        }       

        public virtual async Task DeleteServicePostsAsync(IList<ServicePost> serviceposts)
        {
            ArgumentNullException.ThrowIfNull(serviceposts);

            foreach (var servicepost in serviceposts)
                await DeleteServicePostAsync(servicepost);
        }
        /// <summary>
        /// Gets a blog post additional
        /// </summary>
        /// <param name="serviceAdditionalId">Blog post additional identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post
        /// </returns>
        public virtual async Task<ServicePost> GetServicePostByIdAsync(int servicePostId)
        {
            return await _servicePostRepository.GetByIdAsync(servicePostId, cache => default);
        }
        public virtual async Task<IList<ServicePost>> GetServicePostsByIdsAsync(int[] servicePostIds)
        {
            return await _servicePostRepository.GetByIdsAsync(servicePostIds, includeDeleted: false);
        }


        /// <summary>
        /// Inserts a blog post additional
        /// </summary>
        /// <param name="service">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertServicePostAsync(ServicePost servicePost)
        {
            await _servicePostRepository.InsertAsync(servicePost);
        }

        /// <summary>
        /// Updates the blog post additional
        /// </summary>
        /// <param name="service">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateServicePostAsync(ServicePost servicePost)
        {
            await _servicePostRepository.UpdateAsync(servicePost);
        }

        public virtual async Task<IPagedList<ServicePost>> GetAllServicePostsAsync(string servicePostName="", int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            return await _servicePostRepository.GetAllPagedAsync(async query =>
            {
               

                if (!string.IsNullOrWhiteSpace(servicePostName))
                    query = query.Where(c => c.Name.Contains(servicePostName));

                return query;
            }, pageIndex, pageSize);
        }


        #endregion


        #region Service Category

        public virtual async Task<ServicePostCategory> GetServicePostCategoryByNameAsync(string name)
        {
            var query = _servicePostCategoryRepository.Table;
            var serviceCategory = query.Where(category => category.Name == name).FirstOrDefault();
            return await Task.FromResult(serviceCategory);
        }

        /// <summary>
        /// Inserts a blog category
        /// </summary>
        /// <param name="serviceCategory">Service category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertServicePostCategoryAsync(ServicePostCategory servicePostCategory)
        {
            await _servicePostCategoryRepository.InsertAsync(servicePostCategory);
        }

        /// <summary>
        /// Updates the blog category
        /// </summary>
        /// <param name="serviceCategory">Blog category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateServicePostCategoryAsync(ServicePostCategory servicePostCategory)
        {
            await _servicePostCategoryRepository.UpdateAsync(servicePostCategory);
        }

        /// <summary>
        /// Deletes a blog category
        /// </summary>
        /// <param name="serviceCategory">Blog category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteServicePostCategoryAsync(ServicePostCategory servicePostCategory)
        {
            await _servicePostCategoryRepository.DeleteAsync(servicePostCategory);
        }

        /// <summary>
        /// Gets a blog category
        /// </summary>
        /// <param name="serviceCategoryId">Blog category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog category
        /// </returns>
        public virtual async Task<ServicePostCategory> GetServicePostCategoryByIdAsync(int servicePostCategoryId)
        {
            return await _servicePostCategoryRepository.GetByIdAsync(servicePostCategoryId);
        }

        /// <summary>
        /// Delete Categories
        /// </summary>
        /// <param name="blogcategories">Blog Categories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteServicePostCategoriesAsync(IList<ServicePostCategory> servicePostCategories)
        {
            if (servicePostCategories == null)
                throw new ArgumentNullException(nameof(servicePostCategories));

            foreach (var servicePostCategory in servicePostCategories)
                await DeleteServicePostCategoryAsync(servicePostCategory);
        }


        /// <summary>
        /// Gets blogCategories by identifier
        /// </summary>
        /// <param name="serviceCategoryIds">BlogCategory identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blogcategories
        /// </returns>
        public virtual async Task<IList<ServicePostCategory>> GetServicePostCategoriesByIdsAsync(int[] servicePostCategoryIds)
        {
            return await _servicePostCategoryRepository.GetByIdsAsync(servicePostCategoryIds, includeDeleted: false);
        }

        /// <summary>
        /// Gets all BlogCategories
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the BlogCategories
        /// </returns>
        public virtual async Task<IList<ServicePostCategory>> GetAllServicePostCategoriesAsync(int storeId = 0, bool showHidden = false)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopServicePostCatalogDefaults.ServicePostCategoriesAllCacheKey,
                storeId,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                showHidden);

            //var blogCategories = await _staticCacheManager
            //    .GetAsync(key, async () => (await GetAllBlogCategoriesAsync(string.Empty, storeId, showHidden: showHidden)).ToList());

            var blogCategories = (await GetAllServicePostCategoriesAsync(string.Empty, storeId, showHidden: showHidden)).ToList();

            return blogCategories;
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        public virtual async Task<IPagedList<ServicePostCategory>> GetAllServicePostCategoriesAsync(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            return await _servicePostCategoryRepository.GetAllPagedAsync(async query =>
            {
                await _workContext.GetCurrentVendorAsync();
                //apply store mapping constraints
                //query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                if (!string.IsNullOrWhiteSpace(categoryName))
                    query = query.Where(c => c.Name.Contains(categoryName));

                return query;
            }, pageIndex, pageSize);




            //sort categories
            //var sortedCategories = await SortBlogCategoriesForTreeAsync(unsortedCategories);

            ////paging
            //return new PagedList<BlogCategory>(sortedCategories, pageIndex, pageSize);
        }

        //public virtual async Task<IPagedList<BlogCategory>> GetAllBlogCategoriesAsync(string categoryName, int storeId = 0,
        //    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        //{
        //    var unsortedCategories = await _servicePostCategoryRepository.GetAllAsync(async query =>
        //    {
        //        await _workContext.GetCurrentVendorAsync();
        //        //apply store mapping constraints
        //        //query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        //        if (!string.IsNullOrWhiteSpace(categoryName))
        //            query = query.Where(c => c.Name.Contains(categoryName));

        //        return query;
        //    });

        //    //sort categories
        //    var sortedCategories = await SortBlogCategoriesForTreeAsync(unsortedCategories);

        //    //paging
        //    return new PagedList<BlogCategory>(sortedCategories, pageIndex, pageSize);
        //}

        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent category identifier</param>
        /// <param name="ignoreCategoriesWithoutExistingParent">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sorted categories
        /// </returns>
        protected virtual async Task<IList<ServicePostCategory>> SortBlogCategoriesForTreeAsync(IList<ServicePostCategory> source
            )
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<ServicePostCategory>();

            foreach (var cat in source.ToList())
            {
                result.Add(cat);
                result.AddRange(await SortBlogCategoriesForTreeAsync(source));
            }

            if (result.Count == source.Count)
                return result;

            //find blog categories without parent in provided category source and insert them into result
            foreach (var cat in source)
                if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    result.Add(cat);

            return result;
        }

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="allCategories">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the formatted breadcrumb
        /// </returns>
        //public virtual async Task<string> GetFormattedBreadCrumbAsync(BlogCategory category, IList<BlogCategory> allCategories = null,
        //    string separator = ">>", int languageId = 0)
        //{
        //    var result = string.Empty;

        //    var breadcrumb = await GetCategoryBreadCrumbAsync(category, allCategories, true);
        //    for (var i = 0; i <= breadcrumb.Count - 1; i++)
        //    {
        //        var categoryName = await _localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name, languageId);
        //        result = string.IsNullOrEmpty(result) ? categoryName : $"{result} {separator} {categoryName}";
        //    }

        //    return result;
        //}

        #endregion



        #endregion



        #region ServicePost pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteServicePostPictureAsync(ServicePostPicture servicePostPicture)
        {
            await _servicePostPictureRepository.DeleteAsync(servicePostPicture);
        }

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product pictures
        /// </returns>
        public virtual async Task<IList<ServicePostPicture>> GetServicePostPicturesByProductIdAsync(int servicePostId)
        {
            var query = from pp in _servicePostPictureRepository.Table
                        where pp.ServicePostId == servicePostId
                        orderby pp.DisplayOrder, pp.Id
                        select pp;

            var productPictures = await query.ToListAsync();

            return productPictures;
        }

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product picture
        /// </returns>
        public virtual async Task<ServicePostPicture> GetServicePostPictureByIdAsync(int servicePostPictureId)
        {
            return await _servicePostPictureRepository.GetByIdAsync(servicePostPictureId, cache => default);
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertServicePostPictureAsync(ServicePostPicture servicePostPicture)
        {
            await _servicePostPictureRepository.InsertAsync(servicePostPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateServicePostPictureAsync(ServicePostPicture servicePostPicture)
        {
            await _servicePostPictureRepository.UpdateAsync(servicePostPicture);
        }

        /// <summary>
        /// Get the IDs of all product images 
        /// </summary>
        /// <param name="productsIds">Products IDs</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the all picture identifiers grouped by product ID
        /// </returns>
        public async Task<IDictionary<int, int[]>> GetServicePostsImagesIdsAsync(int[] servicePostsIds)
        {
            var servicePostPictures = await _servicePostPictureRepository.Table
                .Where(p => servicePostsIds.Contains(p.ServicePostId))
                .ToListAsync();

            return servicePostPictures.GroupBy(p => p.ServicePostId).ToDictionary(p => p.Key, p => p.Select(p1 => p1.PictureId).ToArray());
        }

        public virtual async Task<IList<Picture>> GetPicturesByServicePostIdAsync(int servicePostId, int recordsToReturn = 0)
        {
            if (servicePostId == 0)
                return new List<Picture>();

            var query = from p in _pictureRepository.Table
                        join pp in _servicePostPictureRepository.Table on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder, pp.Id
                        where pp.ServicePostId == servicePostId
                        select p;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = await query.ToListAsync();

            return pics;
        }

        #endregion
    }
}