using System;

namespace oedibud.Models;

public class PaymentAssignment
{
    public int Id { get; set; }

    // FK to Payment
    public int PaymentId { get; set; }
    public Payment? Payment { get; set; }

    // FK to Employee
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Percentage share 0..100
    public int SharePercent { get; set; }

    // Optional validity range
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
