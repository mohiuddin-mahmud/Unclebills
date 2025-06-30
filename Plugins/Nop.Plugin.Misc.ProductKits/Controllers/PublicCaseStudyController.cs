using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;
using NopValley.Plugins.Misc.Factories;
using NopValley.Plugins.Misc.Models;
using NopValley.Plugins.Misc.Services;

namespace NopValley.Plugins.Misc.Controllers
{    
    public partial class PublicCaseStudyController : BasePluginController
	{
        #region Fields
       

        private readonly IPublicCaseStudyModelFactory _publicCaseStudyModelFactory;
		private readonly ICaseStudyService _caseStudyService;
		public string viewPath = "~/Plugins/References/Views/CaseStudy/";

        #endregion

        #region Ctor

        public PublicCaseStudyController(
            IPublicCaseStudyModelFactory publicCaseStudyModelFactory,
			ICaseStudyService caseStudyService)
        {     
            _publicCaseStudyModelFactory = publicCaseStudyModelFactory;
            _caseStudyService = caseStudyService;
		}

        #endregion

        #region methods

        public virtual async Task<IActionResult> CaseStudies()
        {   
            var caseStudies = await _publicCaseStudyModelFactory.PrepareCaseStudyListModelAsync();
            return View(viewPath + "List.cshtml", caseStudies);
        }

		public virtual async Task<IActionResult> CaseStudy(int caseStudyId)
        {
			var caseStudy = await _caseStudyService.GetCaseStudyByIdAsync(caseStudyId);
			//if (caseStudy == null)
			//	return InvokeHttp404();

			var model = new PublicCaseStudyModel();
			await _publicCaseStudyModelFactory.PrepareCaseStudyModelAsync(model, caseStudy);

			return View(viewPath + "CaseStudy.cshtml", model);
		}

		#endregion
	}
}