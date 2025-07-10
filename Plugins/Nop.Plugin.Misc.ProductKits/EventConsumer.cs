using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.ProductKits;
public class EventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPermissionService _permissionService;

    public EventConsumer(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return;

        eventMessage.RootMenuItem.InsertBefore("Categories",
            new AdminMenuItem
            {
                SystemName = "Nop.Plugin.Misc.ProductKits.List",
                Title = "Kits",
                Url = eventMessage.GetMenuItemUrl("ProductKits", "Index"),
                IconClass = "far fa-dot-circle",
                Visible = true,
            });

        await Task.CompletedTask;
    }
}