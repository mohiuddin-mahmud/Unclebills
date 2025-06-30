using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Services
{
    /// <summary>
    /// Represents default values related to catalog services
    /// </summary>
    public static partial class NopMiscDefaults
    {

        #region Caching defaults

        #region Testimonial


        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        //public static CacheKey SmartCategoriesByParentCategoryCacheKey => new("Nop.smartcategory.byparent.{0}-{1}-{2}-{3}", SmartCategoriesByParentCategoryPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// </remarks>
        // public static string SmartCategoriesByParentCategoryPrefix => "Nop.smartcategory.byparent.{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : show hidden records?
        /// </remarks>
        //public static CacheKey SmartCategoriesChildIdsCacheKey => new("Nop.smartcategory.childids.{0}-{1}-{2}-{3}", SmartCategoriesChildIdsPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// </remarks>
        //public static string SmartCategoriesChildIdsPrefix => "Nop.smartcategory.childids.{0}";
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : roles of the current user
        /// {2} : show hidden records?
        /// </remarks>
        //public static CacheKey SmartCategoriesAllCacheKey => new("Nop.smartcategory.all.{0}-{1}-{2}", NopEntityCacheDefaults<MegaMenuCategory>.AllPrefix);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : roles of the current user
        /// {2} : show hidden records?
        /// </remarks>
        //public static CacheKey MegaMenuTopicsAllCacheKey => new("Nop.megamenutopics.all.{0}-{1}-{2}", NopEntityCacheDefaults<MegaMenuCategory>.AllPrefix);


        /// <summary>
        /// Key for categories caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        //public static CacheKey SmartCategoriesListKey => new("Nop.pres.admin.smartcategories.list-{0}", SmartCategoriesListPrefixCacheKey);
        //public static string SmartCategoriesListPrefixCacheKey => "Nop.pres.admin.smartcategories.list";

        //public static CacheKey MegaMenuManufacturersListKey => new("Nop.pres.admin.megamenumanufacturers.list-{0}", MegaMenuManufacturersListPrefixCacheKey);
        // public static string MegaMenuManufacturersListPrefixCacheKey => "Nop.pres.admin.megamenumanufacturers.list";


        /// <summary>
        /// Key for category picture caching
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public static CacheKey TestimonialPictureModelKey => new("Nop.pres.testimonial.picture-{0}-{1}-{2}-{3}-{4}-{5}", TestimonialPicturePrefixCacheKey, TestimonialPicturePrefixCacheKeyById);
        public static string TestimonialPicturePrefixCacheKey => "Nop.pres.testimonial.picture";
        public static string TestimonialPicturePrefixCacheKeyById => "Nop.pres.testimonial.picture-{0}-";

        public static CacheKey RegisteredBannerPictureModelKey => new("Nop.pres.registeredbanner.picture-{0}-{1}-{2}-{3}-{4}-{5}", RegisteredBannerPicturePrefixCacheKey, RegisteredBannerPicturePrefixCacheKeyById);
        public static string RegisteredBannerPicturePrefixCacheKey => "Nop.pres.registeredbanner.picture";
        public static string RegisteredBannerPicturePrefixCacheKeyById => "Nop.pres.registeredbanner.picture-{0}-";


        public static CacheKey GuestBanner1PictureModelKey => new("Nop.pres.guestbanner1.picture-{0}-{1}-{2}-{3}-{4}-{5}", GuestBanner1PicturePrefixCacheKey, GuestBanner1PicturePrefixCacheKeyById);
        public static string GuestBanner1PicturePrefixCacheKey => "Nop.pres.guestbanner1.picture";
        public static string GuestBanner1PicturePrefixCacheKeyById => "Nop.pres.guestbanner1.picture-{0}-";

        public static CacheKey GuestBanner2PictureModelKey => new("Nop.pres.guestbanner2.picture-{0}-{1}-{2}-{3}-{4}-{5}", GuestBanner2PicturePrefixCacheKey, GuestBanner2PicturePrefixCacheKeyById);
        public static string GuestBanner2PicturePrefixCacheKey => "Nop.pres.guestbanner2.picture";
        public static string GuestBanner2PicturePrefixCacheKeyById => "Nop.pres.guestbanner2.picture-{0}-";









        #endregion
        #endregion
    }
}