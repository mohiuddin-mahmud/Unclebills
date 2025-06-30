namespace NopValley.Plugins.Misc.Domain
{
    public static class ActivityNames
    {
       
    }

    public static class AttributeNames
    {        
        public const string SERVICEPOSTPOST_BLOCK = "NopValley.Plugin.Misc.ServicePostPostPage.HideServicePostPostBlock";       
    }

    public static class CustomProperties
    {
        public const string ATTRIBUTESXML = "attributesXml";
    }

    public static class Constants
    {
        public const string CONTEXT_NAME = "nop_object_context_viaero_coveragechecker";

        public const string BASE_PLUGIN_PATH = "~/Plugins/NopValley.Misc";
        public const string PLUGIN_VIEW_PATH = BASE_PLUGIN_PATH + "/Views";
        public const string PLUGIN_ADMIN_VIEW_PATH = BASE_PLUGIN_PATH + "/Areas/Admin/Views";
        public const string PLUGIN_CSS_PATH = BASE_PLUGIN_PATH + "/css";
        public const string PLUGIN_SCRIPT_PATH = BASE_PLUGIN_PATH + "/Scripts";
        public const string PLUGIN_CONTENT_PATH = BASE_PLUGIN_PATH + "/content";

        public const string MAP_FILES_PATH = PLUGIN_CONTENT_PATH + "/mapfiles";
        public const string MAP_FILES_BASE_FILENAME = "/365_Coverage_88dBm";
        public const string ZIP_CSV_FILE = PLUGIN_CONTENT_PATH + "/CoverageZips.csv";
        public const string ZIP_CSV_FILE_BAK = ZIP_CSV_FILE + ".bak";
    }

   

    public static class ComponentNames
    {
        public class Public
        {
           
            public const string VIAERO_ADMIN_HEADER_LINKS_WIDGET = "NopValleyAdminHeaderLinksWidget";

            public const string SERVICEPOST_POST_DETAILS_ADDITIONAL = "ServicePostPostDetailsAdditionalWidget";
            public const string SERVICEPOST_POST_ADDITIONAL = "ServicePostPostAdditionalWidget";
            public const string SERVICEPOST_TOP_POSTS = "ServicePostTopPostsWidget";
            public const string SERVICEPOST_POST_FILTERBY_CATEGORY = "ServicePostPostFilterByCategoryWidget";
            public const string SERVICEPOST_SIDEBAR_SEARCH = "ServicePostSidebarSearchWidget";
            public const string SERVICEPOST_SIDEBAR_NEWSLETTER = "ServicePostSidebarNewsletterWidget";
            public const string SERVICEPOSTPOST_SIDEBAR_RELATED_POSTS = "ServicePostPostSidebarRelatedPostsWidget";
            public const string SERVICEPOSTLIST_SIDEBAR_FILTER_TOPICS = "ServicePostListSidebarFilterTopicsWidget";

          
        }

        public class Admin
        {
            public const string SERVICEPOST_POST_ADMIN_WIDGET = "ServicePostPostAdminWidget";            
        }
    }

    public static class GenericAttributeNames
    {
        public static class ServicePost
        {
            public const string IMAGE_ID = "NopValley.Plugin.Misc.ServicePost.ImageId";
            public const string IS_TOP_SERVICEPOST_POST = "NopValley.Plugin.Misc.ServicePost.IsTopServicePostPost";
            public const string CATEGORY = "NopValley.Plugin.Misc.ServicePost.Category";
        }
    }

    public static class MessageTemplateNames
    {
        public const string GET_IN_TOUCH = "NopValley.GetInTouch";
    }

    public static class ResourceNames
    {
        public const string HEADING_CONFIGURATION = "NopValley.ConfigurationHeading";


        public const string HEADING_CONFIGURATION_SERVICEPOSTS = "NopValley.Plugin.Misc.ServicePosts.ConfigurationHeading";

        public const string FIELDNAME_SERVICEPOST_NAME = "NopValley.Plugin.Misc.ServicePost.Name";
        public const string SERVICEPOST_ADD = "NopValley.Plugin.Misc.ServicePost.Add";
        public const string SERVICEPOST_DELETE = "NopValley.Plugin.Misc.ServicePost.Delete";

        public const string SERVICEPOST_EDIT = "NopValley.Plugin.Misc.ServicePost.Edit";
        public const string SERVICEPOST_BACK = "NopValley.Plugin.Misc.ServicePost.Back";
        public const string SERVICEPOST_INFO = "NopValley.Plugin.Misc.ServicePost.Info";
        public const string SERVICEPOST_BACKTOLIST = "NopValley.Plugin.Misc.ServicePost.BackToList";

        public const string SERVICEPOST_PAGETITLE = "NopValley.Plugin.Misc.ServicePost";
        public const string SERVICEPOST_LIST = "NopValley.Plugin.Misc.ServicePost.List";

        public const string FIELDNAME_SERVICEPOST_SELECTCSERVICEPOST = "NopValley.Plugin.Misc.ServicePost.SelectServicePost";

        public const string FIELDNAME_SERVICEPOST_UPDATE = "NopValley.Plugin.Misc.ServicePost.Message.Update";
        public const string SERVICEPOST_ACTIVITYLOG_DELETE = "NopValley.Plugin.Misc.ServicePost.ActivityLog.Delete";



        public const string SERVICEPOSTPAGE = "NopValley.Plugin.Misc.ServicePageTitle";


        #region ServicePostCategory

        public const string HEADING_CONFIGURATION_SERVICEPOSTCATEGORIES = "NopValley.Plugin.Misc.ServicePost.Categories.ConfigurationHeading";

        public const string FIELDNAME_SERVICEPOST_CATEGORYNAME = "NopValley.Plugin.Misc.ServicePost.CategoryName";
        public const string SERVICEPOSTCATEGORY_ADD = "NopValley.Plugin.Misc.ServicePost.Category.Add";
        public const string SERVICEPOSTCATEGORY_DELETE = "NopValley.Plugin.Misc.ServicePost.Category.Delete";
       
        public const string SERVICEPOSTCATEGORY_EDIT = "NopValley.Plugin.Misc.ServicePost.Category.Edit";
        public const string SERVICEPOSTCATEGORY_BACK = "NopValley.Plugin.Misc.ServicePost.Category.Back";
        public const string SERVICEPOSTCATEGORY_INFO = "NopValley.Plugin.Misc.ServicePost.Category.Info";
        public const string SERVICEPOSTCATEGORY_BACKTOLIST = "NopValley.Plugin.Misc.ServicePost.Category.BackToList";

        public const string SERVICEPOSTCATEGORY_PAGETITLE = "NopValley.Plugin.Misc.ServicePost.Category";
        public const string SERVICEPOSTCATEGORY_LIST = "NopValley.Plugin.Misc.ServicePost.ServicePost.Category.List";
       
        public const string FIELDNAME_SERVICEPOST_SELECTCSERVICEPOSTCATEGORY = "NopValley.Plugin.Misc.ServicePost.SelectServicePostCategory";

        public const string FIELDNAME_SERVICEPOST_CATEGORYUPDATE = "NopValley.Plugin.Misc.ServicePost.Message.CategoryUpdate";
        public const string SERVICEPOSTCATEGORY_ACTIVITYLOG_DELETE = "NopValley.Plugin.Misc.ServicePost.Category.ActivityLog.Delete";

        public static class ServicePost
        {
            public static class Fields
            {
                public const string PICTUREID = "NopValley.Plugin.Misc.ServicePost.Fields.PictureId";               
                public const string ISTOPSERVICEPOSTPOST = "NopValley.Plugin.Misc.ServicePost.Fields.IsTopServicePostPost";               
                public const string CATEGORY = "NopValley.Plugin.Misc.ServicePost.Fields.Category";                
                public const string CATEGORYNAME_REQUIRED = "NopValley.Plugin.Misc.ServicePost.Fields.Category.Name.Required";


            }
        }

        #endregion ServicePost      

    }

    public static class SitemapSystemNames
    {      

        public const string SERVICEPOST_HEADERSERVICEPOSTCATEGORIES = "NopValley.HeaderServicePostCategories";
       
    }
}