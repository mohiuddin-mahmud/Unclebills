using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Models;

namespace NopValley.Plugins.Misc.Data.Mapping
{
    /// <summary>
    /// Represents AutoMapper configuration for plugin models
    /// </summary>
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            CreateMap<ServicePostModel, ServicePost>();
            CreateMap<ServicePost, ServicePostModel>();

            CreateMap<ServicePostCategoryModel, ServicePostCategory>();
            CreateMap<ServicePostCategory, ServicePostCategoryModel>();

            CreateMap<PublicServicePostModel, ServicePost>();
            CreateMap<ServicePost, PublicServicePostModel>();

            CreateMap<ServicePostPictureModel, ServicePostPicture>();
            CreateMap<ServicePostPicture, ServicePostPictureModel>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 1;

        #endregion
    }
}