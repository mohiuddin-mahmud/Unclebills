//using Nop.Web.Framework.Models;
//using Nop.Web.Framework.Mvc.ModelBinding;

//namespace NopValley.Plugins.Misc.Areas.Admin.Models;

///// <summary>
///// Represents a blog content model
///// </summary>
//public partial record ServiceContentModel : BaseNopModel
//{
//    #region Ctor

//    public ServiceContentModel()
//    {
//        BlogPosts = new ServicePostSearchModel();
//        BlogComments = new BlogCommentSearchModel();
//        SearchTitle = new BlogPostSearchModel().SearchTitle;
//    }

//    #endregion

//    #region Properties

//    [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.List.SearchTitle")]
//    public string SearchTitle { get; set; }

//    public BlogPostSearchModel BlogPosts { get; set; }

//    public BlogCommentSearchModel BlogComments { get; set; }

//    #endregion
//}