using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopValley.Plugins.Misc.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace NopValley.Plugins.Misc.Services
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class BlogAdditionalService : IBlogAdditionalService
    {
        #region Fields    
        private readonly IRepository<BlogPostAdditional> _blogAdditionalRepository;
        private readonly IStaticCacheManager _staticCacheManager;
       
        #endregion

        #region Ctor

        public BlogAdditionalService(         
            IRepository<BlogPostAdditional> blogAdditionalRepository,
            IStaticCacheManager staticCacheManager)
        {
            _blogAdditionalRepository = blogAdditionalRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogAdditional(BlogPostAdditional blogAdditional)
        {
            await _blogAdditionalRepository.DeleteAsync(blogAdditional);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic
        /// </returns>
        public virtual async Task<BlogPostAdditional> GetBlogAdditionalByIdAsync(int id)
        {
            return await _blogAdditionalRepository.GetByIdAsync(id, cache => default);
        }

       

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertBlogAdditional(BlogPostAdditional blogAdditional)
        {
           await _blogAdditionalRepository.InsertAsync(blogAdditional);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateBlogAdditional(BlogPostAdditional blogAdditional)
        {
            await _blogAdditionalRepository.UpdateAsync(blogAdditional);
        }


        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by blog post title</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the smart categories
        /// </returns>
        //public virtual async Task<IPagedList<HtmlWidget>> GetAllHtmlWidgetAsync(string topicName, int languageId = 0,
        //    DateTime? dateFrom = null, DateTime? dateTo = null,
        //    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
        //{
        //    return await _megaMenuTopicRepository.GetAllPagedAsync(query =>
        //    {
        //        query = query.OrderBy(o => o.Name);
        //        return query;
        //    }, pageIndex, pageSize);
        //}
        public virtual async Task ClearBlogAdditionalCacheAsync()
        {
           await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<BlogPostAdditional>.Prefix);
           await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<BlogPostAdditional>.ByIdCacheKey);          

        }

        //public virtual async Task<BlogPostAdditional> GetBlogAdditionalByIdAsync(int blogPostAdditionalId)
        //{
        //    return await _blogPostAdditionalRepository.GetByIdAsync(blogPostAdditionalId, cache => default);
        //}

        public virtual async Task<BlogPostAdditional> GetBlogPostAdditionalByBlogPostIdAsync(int blogPostId)
        {
            var blogPostsAdditional = _blogAdditionalRepository.Table;

            blogPostsAdditional = blogPostsAdditional.Where(blogPost => blogPost.BlogPostId == blogPostId);

            return await blogPostsAdditional.FirstOrDefaultAsync();
        }

        #endregion
    }
}