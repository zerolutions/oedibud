using System.ComponentModel.DataAnnotations.Schema;

namespace oedibud.Models
{
    public class Payment
    {
        public int Id { get; set; }

        // FK
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // Payment is now a period: Start .. End (Ablaufdatum)
        public DateTime Start { get; set; } = DateTime.Today;
        public DateTime End { get; set; } = DateTime.Today;
        public EmployeeGroup? DetecatedTo { get; set; } = null;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        // Navigation: assignments to contracts (many-to-many with extra fields)
        public List<ContractPayment> ContractPayments { get; set; } = new();
    }
}