using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopValley.Plugin.Widgets.References.Domain;

namespace NopValley.Plugin.Widgets.References.Data
{
    //yyyy/mm/dd
    [NopMigration("2023/07/19 10:11:11:6455332", "NopValley.References base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<CaseStudy>();
            Create.TableFor<Reference>();
            Create.TableFor<Section>();
            Create.TableFor<GalleryPicture>();
            Create.TableFor<RefCategory>();            
            Create.TableFor<ReferenceCategoryMapping>();
        }        
    }
}