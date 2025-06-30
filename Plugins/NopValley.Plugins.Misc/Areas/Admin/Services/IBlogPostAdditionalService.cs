using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Areas.Admin.Models;

namespace NopValley.Plugins.Misc.Areas.Admin.Services
{
    /// <summary>
    /// Blog service interface
    /// </summary>
    public partial interface IBlogPostAdditionalService
    {
        #region Blog posts

        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional);

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostAdditionalId">Blog post Additional identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post
        /// </returns>
        Task<BlogPostAdditional> GetBlogPostAdditionalByIdAsync(int blogPostAdditionalId);

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostAdditionalId">Blog post Additional identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post
        /// </returns>
        Task<BlogPostAdditional> GetBlogPostAdditionalByBlogPostIdAsync(int blogPostId);

        /// <summary>
        /// Inserts a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional);

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateBlogPostAdditionalAsync(BlogPostAdditional blogPostAdditional);

        //Task<IList<BlogPostAdditional>> GetBlogPostAdditionalByBlogCategoryIdAsync(int blogCategoryId);

        //Task<IList<BlogPostAdditional>> GetBlogPostAdditionalsByTopBlogPostAsync();
        Task<BlogPost> GetBlogPostByBlogPostIdAsync(int blogPostId);

        #endregion
    }
}
