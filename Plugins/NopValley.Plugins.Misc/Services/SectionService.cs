using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Presentation;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Services
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class SectionService : ISectionService
    {
        #region Fields


        private readonly IRepository<Section> _sectionRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        protected readonly IRepository<GalleryPicture> _galleryPictureRepository;
        protected readonly IRepository<GalleryVideo> _galleryVideoRepository;
        #endregion

        #region Ctor

        public SectionService(  
            IRepository<Section> sectionRepository,       
            IStaticCacheManager staticCacheManager,
            IRepository<GalleryPicture> galleryPictureRepository,
            IRepository<GalleryVideo> galleryVideoRepository)
        {
            _sectionRepository = sectionRepository;
            _staticCacheManager = staticCacheManager;
            _galleryPictureRepository = galleryPictureRepository;
            _galleryVideoRepository= galleryVideoRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteSectionAsync(Section section)
        {
            await _sectionRepository.DeleteAsync(section);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic
        /// </returns>
        public virtual async Task<Section> GetSectionByIdAsync(int id)
        {
            return await _sectionRepository.GetByIdAsync(id, cache => default);
        }

       

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertSectionAsync(Section section)
        {
           await _sectionRepository.InsertAsync(section);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateSectionAsync(Section section)
        {
            await _sectionRepository.UpdateAsync(section);
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
        //public virtual async Task<IPagedList<Section>> GetAllSectionAsync(string topicName, int languageId = 0,
        //    DateTime? dateFrom = null, DateTime? dateTo = null,
        //    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
        //{
        //    return await _megaMenuTopicRepository.GetAllPagedAsync(query =>
        //    {
        //        query = query.OrderBy(o => o.Name);
        //        return query;
        //    }, pageIndex, pageSize);
        //}
        public virtual async Task ClearSectionCacheAsync()
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<Section>.Prefix);
        }


        public virtual async Task<IPagedList<Section>> GetAllSectionAsync(string caseStudyName, int languageId = 0,
           DateTime? dateFrom = null, DateTime? dateTo = null,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
        {
            return await _sectionRepository.GetAllPagedAsync(query =>
            {
                query = query.OrderBy(o => o.DisplayOrder);
                return query;
            }, pageIndex, pageSize);
        }


        public virtual async Task<IList<Section>> GetAllSectionAsync(int storeId = 0, bool showHidden = false)
        {
            var sections = await GetAllSectionAsync(string.Empty, showHidden: showHidden);
            return sections; ;
        }

        public virtual async Task<IList<Section>> GetHtmlWidgetSectionsAsync(int caseStudyId = 0,
           int sectionId = 0)
        {           

            var query = _sectionRepository.Table;
            if (caseStudyId > 0)
                query = query.Where(psa => psa.HtmlWidgetId == caseStudyId);
            if (sectionId > 0)
                query = query.Where(psa => psa.Id == sectionId);

            query = query.OrderBy(psa => psa.DisplayOrder).ThenBy(psa => psa.Id);

            await Task.Yield();
            return await query.ToListAsync();
        }

        public virtual async Task<Section> GetHtmlWidgetSectionByIdAsync(int id = 0)
        {

            var query = _sectionRepository.Table;
            if (id > 0)
                query = query.Where(cs => cs.Id == id);
           
            query = query.OrderBy(cs => cs.DisplayOrder).ThenBy(cs => cs.Id);

            await Task.Yield();
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product pictures
        /// </returns>
        public virtual async Task<IList<GalleryPicture>> GetGalleryPicturesBySectionIdAsync(int sectionId)
        {
            var query = from pp in _galleryPictureRepository.Table
                        where pp.SectionId == sectionId
                        orderby pp.DisplayOrder, pp.Id
                        select pp;

            var galleryPictures = await query.ToListAsync();

            return galleryPictures;
        }


        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product picture
        /// </returns>
        public virtual async Task<GalleryPicture> GetGalleryPictureByIdAsync(int sectionPictureId)
        {
            return await _galleryPictureRepository.GetByIdAsync(sectionPictureId);
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertGalleryPictureAsync(GalleryPicture galleryPicture)
        {
            await _galleryPictureRepository.InsertAsync(galleryPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateGalleryPictureAsync(GalleryPicture galleryPicture)
        {
            await _galleryPictureRepository.UpdateAsync(galleryPicture);
        }

        /// <summary>
        /// Get the IDs of all gallery images 
        /// </summary>
        /// <param name="productsIds">Products IDs</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the all picture identifiers grouped by product ID
        /// </returns>
        public async Task<IDictionary<int, int[]>> GetSectionGalleyImagesIdsAsync(int[] sectionsIds)
        {
            var productPictures = await _galleryPictureRepository.Table
                .Where(p => sectionsIds.Contains(p.SectionId))
                .ToListAsync();

            return productPictures.GroupBy(p => p.SectionId).ToDictionary(p => p.Key, p => p.Select(p1 => p1.PictureId).ToArray());
        }

        /// <summary>
        /// Deletes a gallery picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteGalleryPictureAsync(GalleryPicture galleryPicture)
        {
            await _galleryPictureRepository.DeleteAsync(galleryPicture);
        }
        #endregion

        /// <summary>
        /// Inserts a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        public virtual async Task<GalleryVideo> InsertVideoAsync(GalleryVideo video)
        {
            await _galleryVideoRepository.InsertAsync(video);
            return video;
        }

        /// <summary>
        /// Updates the video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        public virtual async Task<GalleryVideo> UpdateVideoAsync(GalleryVideo video)
        {
            await _galleryVideoRepository.UpdateAsync(video);

            return video;
        }

        /// <summary>
        /// Deletes a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteVideoAsync(GalleryVideo video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            await _galleryVideoRepository.DeleteAsync(video);
        }

        public virtual async Task<GalleryVideo> GetGalleryVideoByIdAsync(int videoId)
        {
            return await _galleryVideoRepository.GetByIdAsync(videoId);
        }
    }
}