using System;

namespace oedibud.Models;

public class ContractPayment
{
    public int Id { get; set; }

    // FK to Contract
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }

    // FK to Payment
    public int PaymentId { get; set; }
    public Payment? Payment { get; set; }

    // Percentage share 0..100
    public int SharePercent { get; set; }

    // Optional validity range
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
