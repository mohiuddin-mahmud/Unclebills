using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Services
{
    /// <summary>
    /// Case Study service
    /// </summary>
    public partial interface IBlogAdditionalService
    {
        Task DeleteBlogAdditional(BlogPostAdditional blogAdditional);
        Task InsertBlogAdditional(BlogPostAdditional blogAdditional);
        Task UpdateBlogAdditional(BlogPostAdditional blogAdditional);
        Task<BlogPostAdditional> GetBlogAdditionalByIdAsync(int id);
        Task ClearBlogAdditionalCacheAsync();
        Task<BlogPostAdditional> GetBlogPostAdditionalByBlogPostIdAsync(int blogPostId);
    }
}