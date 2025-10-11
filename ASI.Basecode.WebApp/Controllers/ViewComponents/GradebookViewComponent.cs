using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for rendering the gradebook table with student grades and activities.
    /// </summary>
    public class GradebookViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the Gradebook ViewComponent with the provided model.
        /// </summary>
        /// <param name="model">The GradebookViewModel containing course and student grade data.</param>
        /// <returns>The ViewComponent result.</returns>
        public IViewComponentResult Invoke(GradebookViewModel model)
        {
            return View(model);
        }
    }
}
