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

    public class Payment
    {
        public int Id { get; set; }

        // FK
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;
        public EmployeeGroup? DetecatedTo { get; set; } = null;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        // Navigation: assignments to contracts (many-to-many with extra fields)
        public List<ContractPayment> ContractAssignments { get; set; } = new();

        // Navigation: assignments to employees removed (direct employee assignments not supported)
    }
}