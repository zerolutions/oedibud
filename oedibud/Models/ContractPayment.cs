using System;
using System.ComponentModel.DataAnnotations.Schema;

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
    public decimal ContractShare { get; set; }

    [NotMapped]
    public decimal ContractSharePercent
    {
        get => decimal.Floor(ContractShare * 10000) / 100; // round down to 2 decimals
        set => ContractShare = value / 100;
    }

    // Optional validity range
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
