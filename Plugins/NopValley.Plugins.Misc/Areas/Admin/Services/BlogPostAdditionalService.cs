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

namespace NopValley.Plugins.Misc.Areas.Admin.Services
{
    /// <summary>
    /// Blog service
    /// </summary>
    public partial class BlogPostAdditionalService : IBlogPostAdditionalService
    {
        #region Fields

        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<BlogPostAdditional> _blogPostAdditionalRepository;
        private readonly IRepository<ServicePostCategory> _blogCategoryRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IRepository<BlogPost> _blogPostRepository;
        #endregion

        #region Ctor

        public BlogPostAdditionalService(
            IRepository<BlogComment> blogCommentRepository,
            IRepository<BlogPostAdditional> blogPostAdditionalRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IRepository<ServicePostCategory> blogCategoryRepository,
            IWorkContext workContext,
            ICustomerService customerService,
            IRepository<BlogPost> blogPostRepository)
        {
            _blogCommentRepository = blogCommentRepository;
            _blogPostAdditionalRepository = blogPostAdditionalRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _blogCategoryRepository = blogCategoryRepository;
            _workContext = workContext;
            _customerService = customerService;
            _blogPostRepository = blogPostRepository;
        }

        #endregion

        #region Methods

        #region Blog posts

        /// <summary>
        /// Deletes a blog post additional
        /// </summary>
        /// <param name="blogPostAdditional">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional)
        {
            await _blogPostAdditionalRepository.DeleteAsync(blogPostAdditional);
        }

        /// <summary>
        /// Gets a blog post additional
        /// </summary>
        /// <param name="blogPostAdditionalId">Blog post additional identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post
        /// </returns>
        public virtual async Task<BlogPostAdditional> GetBlogPostAdditionalByIdAsync(int blogPostAdditionalId)
        {
            return await _blogPostAdditionalRepository.GetByIdAsync(blogPostAdditionalId, cache => default);
        }

        public virtual async Task<BlogPostAdditional> GetBlogPostAdditionalByBlogPostIdAsync(int blogPostId)
        {
            var blogPostsAdditional = _blogPostAdditionalRepository.Table;

            blogPostsAdditional = blogPostsAdditional.Where(blogPost => blogPost.BlogPostId == blogPostId);

            return await blogPostsAdditional.FirstOrDefaultAsync();
        }

        //public virtual async Task<IList<BlogPostAdditional>> GetBlogPostAdditionalByBlogCategoryIdAsync(int blogCategoryId)
        //{
        //    var blogPostsAdditional = _blogPostAdditionalRepository.Table;

        //    blogPostsAdditional = blogPostsAdditional.Where(blogPost => blogPost.BlogCategoryId == blogCategoryId);

        //    return await blogPostsAdditional.ToListAsync();
        //}

        //public virtual async Task<IList<BlogPostAdditional>> GetBlogPostAdditionalsByTopBlogPostAsync()
        //{
        //    var blogPostsAdditional = _blogPostAdditionalRepository.Table;

        //    blogPostsAdditional = blogPostsAdditional.Where(blogPostAdditional => blogPostAdditional.IsTopBlogPost == true).OrderByDescending(blogPostAdditional => blogPostAdditional.BlogPostId);

        //    return await blogPostsAdditional.ToListAsync();
        //}

        /// <summary>
        /// Inserts a blog post additional
        /// </summary>
        /// <param name="blogPost">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional)
        {
            await _blogPostAdditionalRepository.InsertAsync(blogPostAdditional);
        }

        /// <summary>
        /// Updates the blog post additional
        /// </summary>
        /// <param name="blogPost">Blog post additional</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional)
        {
            await _blogPostAdditionalRepository.UpdateAsync(blogPostAdditional);
        }


        public virtual async Task<BlogPost> GetBlogPostByBlogPostIdAsync(int blogPostId)
        {
            return await _blogPostRepository.GetByIdAsync(blogPostId, cache => default);
        }

        #endregion


        #region Blog Category

        public virtual async Task<ServicePostCategory> GetBlogCategoryByNameAsync(string name)
        {
            var query = _blogCategoryRepository.Table;
            var blogCategory = query.Where(category => category.Name == name).FirstOrDefault();
            return await Task.FromResult(blogCategory);
        }

        /// <summary>
        /// Inserts a blog category
        /// </summary>
        /// <param name="blogCategory">Blog category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertBlogCategoryAsync(ServicePostCategory blogCategory)
        {
            await _blogCategoryRepository.InsertAsync(blogCategory);
        }

        /// <summary>
        /// Updates the blog category
        /// </summary>
        /// <param name="blogCategory">Blog category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateBlogCategoryAsync(ServicePostCategory blogCategory)
        {
            await _blogCategoryRepository.UpdateAsync(blogCategory);
        }

        /// <summary>
        /// Deletes a blog category
        /// </summary>
        /// <param name="blogCategory">Blog category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogCategoryAsync(ServicePostCategory blogCategory)
        {
            await _blogCategoryRepository.DeleteAsync(blogCategory);
        }

        /// <summary>
        /// Gets a blog category
        /// </summary>
        /// <param name="blogCategoryId">Blog category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog category
        /// </returns>
        public virtual async Task<ServicePostCategory> GetBlogCategoryByIdAsync(int blogCategoryId)
        {
            return await _blogCategoryRepository.GetByIdAsync(blogCategoryId);
        }

        /// <summary>
        /// Delete Categories
        /// </summary>
        /// <param name="blogcategories">Blog Categories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogCategoriesAsync(IList<ServicePostCategory> blogCategories)
        {
            if (blogCategories == null)
                throw new ArgumentNullException(nameof(blogCategories));

            foreach (var blogCategory in blogCategories)
                await DeleteBlogCategoryAsync(blogCategory);
        }


        /// <summary>
        /// Gets blogCategories by identifier
        /// </summary>
        /// <param name="blogCategoryIds">BlogCategory identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blogcategories
        /// </returns>
        public virtual async Task<IList<ServicePostCategory>> GetBlogCategoriesByIdsAsync(int[] blogCategoryIds)
        {
            return await _blogCategoryRepository.GetByIdsAsync(blogCategoryIds, includeDeleted: false);
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
        public virtual async Task<IList<ServicePostCategory>> GetAllBlogCategoriesAsync(int storeId = 0, bool showHidden = false)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopServicePostCatalogDefaults.ServicePostCategoriesAllCacheKey,
                storeId,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                showHidden);

            //var blogCategories = await _staticCacheManager
            //    .GetAsync(key, async () => (await GetAllBlogCategoriesAsync(string.Empty, storeId, showHidden: showHidden)).ToList());

            var blogCategories = (await GetAllBlogCategoriesAsync(string.Empty, storeId, showHidden: showHidden)).ToList();

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
        public virtual async Task<IPagedList<ServicePostCategory>> GetAllBlogCategoriesAsync(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            return await _blogCategoryRepository.GetAllPagedAsync(async query =>
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
        //    var unsortedCategories = await _blogCategoryRepository.GetAllAsync(async query =>
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
    }
}