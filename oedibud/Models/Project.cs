using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using oedibud.Models;

namespace oedibud.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime Start { get; set; } 
        public DateTime End { get; set; } 

        // Navigation
        public List<Payment> Payments { get; set; } = new();

        // Computed helper for UI
        [NotMapped]
        public decimal TotalAmount => Payments?.Sum(p => p.Amount) ?? 0m;
    }
}