using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Services
{
    /// <summary>
    /// Section service
    /// </summary>
    public partial interface ISectionService
    {
        Task DeleteSectionAsync(Section section);
        Task InsertSectionAsync(Section section);
        Task UpdateSectionAsync(Section section);
        Task<Section> GetSectionByIdAsync(int id);
        Task ClearSectionCacheAsync();

        Task<IPagedList<Section>> GetAllSectionAsync(string sectionName, int languageId = 0,
           DateTime? dateFrom = null, DateTime? dateTo = null,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null);

        Task<IList<Section>> GetAllSectionAsync(int storeId = 0, bool showHidden = false);
        Task<IList<Section>> GetHtmlWidgetSectionsAsync(int caseStudyId = 0,
            int sectionId = 0);        

        Task<Section> GetHtmlWidgetSectionByIdAsync(int id = 0);
        Task<IList<GalleryPicture>> GetGalleryPicturesBySectionIdAsync(int sectionId);
        Task<GalleryPicture> GetGalleryPictureByIdAsync(int sectionPictureId);
        Task InsertGalleryPictureAsync(GalleryPicture galleryPicture);
        Task UpdateGalleryPictureAsync(GalleryPicture galleryPicture);
        Task<IDictionary<int, int[]>> GetSectionGalleyImagesIdsAsync(int[] sectionsIds);
        Task DeleteGalleryPictureAsync(GalleryPicture galleryPicture);

        Task<GalleryVideo> InsertVideoAsync(GalleryVideo video);
        Task<GalleryVideo> UpdateVideoAsync(GalleryVideo video);
        Task DeleteVideoAsync(GalleryVideo video);
        Task<GalleryVideo> GetGalleryVideoByIdAsync(int videoId);
        
    }
}