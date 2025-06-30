using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Data
{
    //yyyy/mm/dd
    [NopMigration("2024/10/12 08:08:08", "NopValley.Misc base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<BlogPostAdditional>();
            Create.TableFor<ServicePostPicture>();
            
            //Create.TableFor<CategoryProductMapping>();
            //Create.TableFor<HomeCategory>();
            //Create.TableFor<HomeCategoryFilter>();            
            //Create.TableFor<HomeTab>();
            //Create.TableFor<HtmlWidget>();
            //Create.TableFor<ManufacturerAdditional>();
            //Create.TableFor<Testimonial>();
        }
    }
}