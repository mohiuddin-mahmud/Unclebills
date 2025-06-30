using Nop.Core.Configuration;

namespace NopValley.Plugins.Misc
{
    public class MiscSettings : ISettings
    {       
        public bool EnableReference { get; set; }
        public bool EnableCaseStudy { get; set; }
		public int PageSize { get; set; }

		public string ReferenceHtml { get; set; }
		public string CaseStudyHtml { get; set; }
        public string ReferenceUrl { get; set; }
        public string CaseStudyUrl { get; set; }
        public string CaseStudyListPageHeader { get; set; }
        public bool ShowGalleryThumb { get; set; }
        public bool ShowPictureCaption { get; set; }
        public int SelectedTemplateId { get; set; }

    }
}