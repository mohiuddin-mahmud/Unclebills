using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the blog model factory
    /// </summary>
    public partial interface IServicePostAdminModelFactory
    {

        Task<ServicePostModel> PrepareServicePostModelAsync(ServicePostModel model, ServicePost servicePost);

        Task<ServicePostSearchModel> PrepareServicePostSearchModelAsync(ServicePostSearchModel searchModel);

        Task<ServicePostListModel> PrepareServicePostListModelAsync(ServicePostSearchModel searchModel);



        Task<ServicePostCategoryModel> PrepareServicePostCategoryModelAsync(ServicePostCategoryModel model, ServicePostCategory servicePostCategory);
        Task<ServicePostCategorySearchModel> PrepareServicePostCategorySearchModelAsync(ServicePostCategorySearchModel searchModel);

        Task<ServicePostCategoryListModel> PrepareServicePostCategoryListModelAsync(ServicePostCategorySearchModel searchModel);
        Task<ServicePostPictureListModel> PrepareServicePostPictureListModelAsync(ServicePostPictureSearchModel searchModel, ServicePost servicePost);
        //ServicePostPictureSearchModel PrepareServicePostPictureSearchModel(ServicePostPictureSearchModel searchModel, ServicePost servicePost);
    }
}