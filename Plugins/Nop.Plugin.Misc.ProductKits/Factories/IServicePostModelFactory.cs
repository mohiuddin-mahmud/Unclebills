using Nop.Core.Domain.Vendors;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Common;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Models;
using System.Threading.Tasks;

namespace NopValley.Plugins.Misc.Factories;

/// <summary>
/// Represents the interface of the common models factory
/// </summary>
public partial interface IServicePostModelFactory
{
    /// <summary>
    /// Prepare the logo model
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the logo model
    /// </returns>
    Task PrepareServicePostModelAsync(PublicServicePostModel model, ServicePost servicePost);

    Task<PublicServicePostListModel> PrepareServicePostListModelAsync();


}