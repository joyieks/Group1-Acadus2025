using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// View Component for rendering a statistics card with a title, value, and hamburger icon.
    /// This component provides a reusable, clean, and modern UI element for displaying key metrics
    /// in dashboards across the application (e.g., Student, Teacher, Admin views).
    /// </summary>
    public class StatisticsCardViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the Statistics Card View Component.
        /// </summary>
        /// <param name="title">The title text displayed on the card (e.g., "Unsubmitted items" or "Grading Queue").</param>
        /// <param name="value">The main value or number to display prominently on the card (e.g., 0, 5).</param>
        /// <param name="icon">The type of icon to display (e.g., "hamburger", "calendar", "checkmark").</param>
        /// <returns>An IViewComponentResult containing the rendered view with the provided data.</returns>
        public IViewComponentResult Invoke(string title, object value, string icon = "hamburger")
        {
            var model = new StatisticsCardModel
            {
                Title = title,
                Value = value.ToString(),
                Icon = icon
            };

            return View(model);
        }
    }

    /// <summary>
    /// Model class representing the data structure for the Statistics Card View Component.
    /// </summary>
    public class StatisticsCardModel
    {
        /// <summary>
        /// Gets or sets the title text for the statistics card.
        /// This is displayed next to the hamburger icon at the top of the card.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the value to be displayed prominently in the center of the card.
        /// This is typically a number or metric, shown in a large, vibrant green font.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the icon type for the statistics card.
        /// Supported values: "hamburger", "calendar", "checkmark", "activity", "graded", "courses".
        /// </summary>
        public string Icon { get; set; }
    }
}
