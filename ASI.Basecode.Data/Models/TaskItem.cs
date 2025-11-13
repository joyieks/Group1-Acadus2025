using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string UserAction { get; set; } = string.Empty;
        public string? Score { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Priority { get; set; }
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
    }
}
