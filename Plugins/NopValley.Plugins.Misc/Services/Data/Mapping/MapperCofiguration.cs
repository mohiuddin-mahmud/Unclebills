using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopValley.Plugin.Widgets.References.Areas.Admin.Models;
using NopValley.Plugin.Widgets.References.Domain;
using NopValley.Plugin.Widgets.References.Models;

namespace NopValley.Plugin.Widgets.References.Data.Mapping
{
    /// <summary>
    /// Represents AutoMapper configuration for plugin models
    /// </summary>
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {          
            CreateMap<CaseStudyModel, CaseStudy>();
            CreateMap<CaseStudy, CaseStudyModel>();

            CreateMap<ReferenceModel, Reference>();
            CreateMap<Reference, ReferenceModel>();

            CreateMap<ReferenceCategoryModel, RefCategory>();
            CreateMap<RefCategory, ReferenceCategoryModel>();

            CreateMap<CaseStudySectionModel, Section>();
            CreateMap<Section, CaseStudySectionModel>();

			CreateMap<PublicCaseStudySectionModel, Section>();
			CreateMap<Section, PublicCaseStudySectionModel>();

            CreateMap<GalleryPicture, GalleryPictureModel>()
             .ForMember(model => model.OverrideAltAttribute, options => options.Ignore())
             .ForMember(model => model.OverrideTitleAttribute, options => options.Ignore())
             .ForMember(model => model.PictureUrl, options => options.Ignore());
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