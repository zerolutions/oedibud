using System;

namespace oedibud.Models;

public class Contract
{
    public int Id { get; set; }

    // FK to Employee
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public float Fte { get; set; }

    // Navigation: payments assigned to this contract (many-to-many via ContractPayment)
    public List<ContractPayment> ContractPayments { get; set; } = new();


}

