using Microsoft.EntityFrameworkCore;
using oedibud.Data;
using oedibud.Models;

namespace oedibud.Services;

public sealed class ForecastEngine
{
    private readonly IDbContextFactory<BudgetDbContext> _dbFactory;
    private readonly TvLSalaryService _tvL;

    public ForecastEngine(IDbContextFactory<BudgetDbContext> dbFactory, TvLSalaryService tvL)
    {
        _dbFactory = dbFactory;
        _tvL = tvL;
    }

    public enum RootKind { Employee, Project }
    public enum NodeKind { Root, Intermediate, Allocation }

    /// <summary>
    /// Einheitliches Datenmodell für beide Tabellen.
    /// Level: 0 = Root (Employee/Project), 1 = Intermediate (Contract/Payment), 2 = Allocation (ContractPayment)
    /// RootId: EmployeeId bzw. ProjectId -> steuert expand/collapse wie bisher.
    /// </summary>
    public sealed record ForecastNode(
        RootKind RootKind,
        int RootId,
        NodeKind Kind,
        int Level,
        string Label,
        string Title,
        decimal[] Values,
        int? EmployeeId = null,
        int? ContractId = null,
        int? ProjectId = null,
        int? PaymentId = null,
        int? ContractPaymentId = null
    );

    public sealed class ForecastResult
    {
        public required List<DateTime> Columns { get; init; }
        public required List<ForecastNode> Nodes { get; init; }

        public required decimal[] EmployeeTotals { get; init; }
        public required decimal[] ProjectTotals { get; init; }
    }

    public async Task<ForecastResult> BuildAsync(DateTime startMonth, int months, CancellationToken ct = default)
    {
        var columns = BuildColumns(startMonth, months);
        var first = columns.First();
        var last = columns.Last();

        using var db = _dbFactory.CreateDbContext();

        // Load once (wie vorher)
        var employees = await db.Employees.ToListAsync(ct);
        var contracts = await db.Contracts
            .Include(c => c.ContractPayments)
                .ThenInclude(cp => cp.Payment)
                    .ThenInclude(p => p.Project)
            .ToListAsync(ct);

        var projects = await db.Projects
            .Include(pr => pr.Payments)
                .ThenInclude(p => p.ContractPayments)
                    .ThenInclude(cp => cp.Contract)
                        .ThenInclude(c => c.Employee)
            .ToListAsync(ct);

        // Visible index map statt Columns.FindIndex(...) (output-identisch, nur schneller)
        var visibleIndex = BuildVisibleIndexMap(columns);

        var nodes = new List<ForecastNode>(capacity: 1024);

        // ---------------------------
        // 1) Employee-Hierarchie bauen
        // ---------------------------
        foreach (var emp in employees.OrderBy(e => e.Name))
        {
            // Verträge im Zeitraum
            var empContracts = contracts
                .Where(c => c.EmployeeId == emp.Id && c.End >= first && c.Start <= last)
                .ToList();

            // pro Vertrag: Vertragskosten (negativ) + pro cp: greedy Alloc (positiv)
            var contractCost = new Dictionary<int, decimal[]>(empContracts.Count);
            var perContractPaymentAlloc = new Dictionary<int, Dictionary<int, decimal[]>>(empContracts.Count);

            foreach (var c in empContracts)
            {
                var monthlyCostFunc = BuildMonthlyCostFunc(
                    contractStart: c.Start,
                    contractEnd: c.End,
                    employee: emp,
                    fte: (decimal)c.Fte
                );

                // monthly costs per visible month
                var monthlyCost = new decimal[months];
                for (int m = 0; m < months; m++)
                    monthlyCost[m] = monthlyCostFunc(MonthStart(columns[m]));

                // per payment allocation dict
                var perPaymentAlloc = new Dictionary<int, decimal[]>(c.ContractPayments.Count);

                // dictionary (wie vorher; wird nicht weiter verwendet, bleibt aber "same semantics")
                var assignedByMonth = new Dictionary<DateTime, decimal>();
                foreach (var col in columns)
                    assignedByMonth[MonthStart(col)] = 0m;

                foreach (var cp in c.ContractPayments)
                {
                    var p = cp.Payment;
                    if (p is null) continue;

                    var alloc = new decimal[months];

                    // clamp cp-range to payment-range (wie vorher)
                    var rawStart = cp.Start ?? p.Start;
                    var start = rawStart < p.Start ? p.Start : rawStart;

                    var rawEnd = cp.End ?? p.End;
                    var end = rawEnd > p.End ? p.End : rawEnd;

                    var startMonthCp = MonthStart(start);
                    var endMonthCp = MonthStart(end);

                    if (startMonthCp > endMonthCp)
                    {
                        perPaymentAlloc[cp.Id] = alloc;
                        continue;
                    }

                    var remaining = (p.Amount * emp.BruttoFactor);

                    // greedy month-by-month (exakt wie vorher)
                    for (var m = startMonthCp; m <= endMonthCp && remaining > 0; m = m.AddMonths(1))
                    {
                        var need = monthlyCostFunc(m) * cp.SharePercent / 100m;
                        if (need <= 0) continue;

                        var take = Math.Min(need, remaining);

                        if (visibleIndex.TryGetValue(MonthKey(m), out var idx))
                            alloc[idx] += take;

                        assignedByMonth[m] = take; // wie vorher "="
                        remaining -= take;
                    }

                    perPaymentAlloc[cp.Id] = alloc;
                }

                perContractPaymentAlloc[c.Id] = perPaymentAlloc;

                // contract row values: negative costs only (wie vorher)
                var contractArr = new decimal[months];
                for (int m = 0; m < months; m++)
                    contractArr[m] = -monthlyCost[m];

                contractCost[c.Id] = contractArr;
            }

            // Employee root values: sum allocations + sum contract costs (wie vorher)
            var empValues = new decimal[months];

            foreach (var byContract in perContractPaymentAlloc.Values)
            {
                foreach (var alloc in byContract.Values)
                {
                    for (int m = 0; m < months; m++)
                        empValues[m] += alloc[m];
                }
            }

            foreach (var cc in contractCost.Values)
            {
                for (int m = 0; m < months; m++)
                    empValues[m] += cc[m];
            }

            // Root node (Employee)
            nodes.Add(new ForecastNode(
                RootKind: RootKind.Employee,
                RootId: emp.Id,
                Kind: NodeKind.Root,
                Level: 0,
                Label: emp.Name,
                Title: "",
                Values: empValues,
                EmployeeId: emp.Id
            ));

            // Children: Contract + ContractPayment
            foreach (var c in empContracts)
            {
                nodes.Add(new ForecastNode(
                    RootKind: RootKind.Employee,
                    RootId: emp.Id,
                    Kind: NodeKind.Intermediate,
                    Level: 1,
                    Label: $"{c.FtePercent}% {emp.Group} | {c.Start:MMM yy} - {c.End:MMM yy}",
                    Title: $"Vertrags-ID {c.Id}",
                    Values: contractCost[c.Id],
                    EmployeeId: emp.Id,
                    ContractId: c.Id
                ));

                foreach (var cp in c.ContractPayments)
                {
                    var p = cp.Payment;
                    if (p is null) continue;

                    // alloc array lookup (wie vorher)
                    var allocValues = new decimal[months];
                    if (perContractPaymentAlloc.TryGetValue(c.Id, out var dict) &&
                        dict.TryGetValue(cp.Id, out var cpAlloc))
                    {
                        allocValues = cpAlloc;
                    }

                    nodes.Add(new ForecastNode(
                        RootKind: RootKind.Employee,
                        RootId: emp.Id,
                        Kind: NodeKind.Allocation,
                        Level: 2,
                        Label: $" {cp.SharePercent}% {p.DetecatedTo} | {((p.Project?.Title) ?? "")} PM-{p.Id}",
                        Title: $"Projekt-ID {p.ProjectId} | Projektmittel-ID {p.Id} | Gewidmet {p.DetecatedTo} | Projektmittel: {p.Start:MMM yy} - {p.End:MMM yy} | Vertragszuweisung: {cp.Start:MMM yy} - {cp.End:MMM yy} | Betrag {p.Amount:C0}",
                        Values: allocValues,
                        EmployeeId: emp.Id,
                        ContractId: c.Id,
                        ProjectId: p.ProjectId,
                        PaymentId: p.Id,
                        ContractPaymentId: cp.Id
                    ));
                }
            }
        }

        // Employee Totals = sum of employee root nodes (wie vorher)
        var employeeTotals = SumTotals(nodes.Where(n => n.RootKind == RootKind.Employee && n.Level == 0), months);

        // ---------------------------
        // 2) Project-Hierarchie bauen
        // ---------------------------
        foreach (var project in projects.OrderBy(pr => pr.Title))
        {
            var paymentResults = new Dictionary<int, decimal[]>();
            var paymentAllocations = new Dictionary<int, Dictionary<int, decimal[]>>();

            foreach (var payment in project.Payments.Where(p => p.End >= first && p.Start <= last))
            {
                var paymentStart = MonthStart(payment.Start);
                var paymentEnd = MonthStart(payment.End);

                var periodMonths = MonthsInclusive(paymentStart, paymentEnd).ToList();
                var assignedByMonth = periodMonths.ToDictionary(m => m, _ => 0m);

                var perPaymentAlloc = new Dictionary<int, decimal[]>();

                foreach (var cp in payment.ContractPayments)
                {
                    var contract = cp.Contract;
                    var employee = contract?.Employee;

                    if (contract is null || employee is null)
                    {
                        perPaymentAlloc[cp.Id] = new decimal[months];
                        continue;
                    }

                    var monthlyCostFunc = BuildMonthlyCostFunc(
                        contractStart: contract.Start,
                        contractEnd: contract.End,
                        employee: employee,
                        fte: (decimal)contract.Fte
                    );

                    var cpAlloc = new decimal[months];

                    var cpStart = cp.Start.HasValue ? MonthStart(cp.Start.Value) : paymentStart;
                    var cpEnd = cp.End.HasValue ? MonthStart(cp.End.Value) : paymentEnd;

                    for (var m = cpStart; m <= cpEnd; m = m.AddMonths(1))
                    {
                        if (m < paymentStart || m > paymentEnd) continue;

                        var need = monthlyCostFunc(m) * cp.SharePercent / 100m;
                        assignedByMonth[m] += need;

                        if (visibleIndex.TryGetValue(MonthKey(m), out var idx))
                            cpAlloc[idx] = need; // wie vorher "="
                    }

                    perPaymentAlloc[cp.Id] = cpAlloc;
                }

                paymentAllocations[payment.Id] = perPaymentAlloc;

                // paymentValues: amount - cumulativeAssigned (wie vorher)
                var paymentValues = new decimal[months];
                var cumulativeAssigned = 0m;

                for (int i = 0; i < periodMonths.Count; i++)
                {
                    var m = periodMonths[i];

                    if (visibleIndex.TryGetValue(MonthKey(m), out var idx))
                        paymentValues[idx] = payment.Amount - cumulativeAssigned;

                    cumulativeAssigned += assignedByMonth[m];
                }

                paymentResults[payment.Id] = paymentValues;
            }

            // projectMonthlyNet = sum(paymentValues) - sum(cpAlloc)
            var projectMonthlyNet = new decimal[months];

            foreach (var payVals in paymentResults.Values)
                for (int m = 0; m < months; m++)
                    projectMonthlyNet[m] += payVals[m];

            foreach (var allocDict in paymentAllocations.Values)
                foreach (var cpVals in allocDict.Values)
                    for (int m = 0; m < months; m++)
                        projectMonthlyNet[m] -= cpVals[m];

            // Root node (Project)
            nodes.Add(new ForecastNode(
                RootKind: RootKind.Project,
                RootId: project.Id,
                Kind: NodeKind.Root,
                Level: 0,
                Label: project.Title,
                Title: $"Projekt-ID {project.Id} | {project.Start:MMM yy} - {project.End:MMM yy} | Gesamt {project.TotalAmount:C0}",
                Values: projectMonthlyNet.ToArray(),
                ProjectId: project.Id
            ));

            // Children: Payment + ContractPayment
            foreach (var payment in project.Payments.OrderBy(p => p.Start))
            {
                if (payment.End < first || payment.Start > last) continue;

                if (!paymentResults.TryGetValue(payment.Id, out var paymentValues))
                    paymentValues = new decimal[months];

                nodes.Add(new ForecastNode(
                    RootKind: RootKind.Project,
                    RootId: project.Id,
                    Kind: NodeKind.Intermediate,
                    Level: 1,
                    Label: $"PM-{payment.Id} {payment.DetecatedTo} | {payment.Start:MMM yy} - {payment.End:MMM yy}",
                    Title: $"Projektmittel-ID {payment.Id} | Betrag {payment.Amount:C0} | Gewidmet {payment.DetecatedTo}",
                    Values: paymentValues,
                    ProjectId: project.Id,
                    PaymentId: payment.Id
                ));

                if (!paymentAllocations.TryGetValue(payment.Id, out var allocDict))
                    allocDict = new Dictionary<int, decimal[]>();

                foreach (var cp in payment.ContractPayments.OrderByDescending(x => x.SharePercent))
                {
                    var cpAlloc = allocDict.TryGetValue(cp.Id, out var cpValues) ? cpValues : new decimal[months];
                    var negAlloc = cpAlloc.Select(v => -v).ToArray(); // wie vorher

                    nodes.Add(new ForecastNode(
                        RootKind: RootKind.Project,
                        RootId: project.Id,
                        Kind: NodeKind.Allocation,
                        Level: 2,
                        Label: $" {cp.SharePercent}% {cp.Contract?.Employee?.Name ?? "-"} | AV-{cp.ContractId}",
                        Title: $"Vertrag-ID {cp.ContractId} | Zuweisung {cp.Start:MMM yy} - {cp.End:MMM yy} | Projektmittel-ID {payment.Id}",
                        Values: negAlloc,
                        ProjectId: project.Id,
                        PaymentId: payment.Id,
                        ContractId: cp.ContractId,
                        ContractPaymentId: cp.Id,
                        EmployeeId: cp.Contract?.EmployeeId
                    ));
                }
            }
        }

        var projectTotals = SumTotals(nodes.Where(n => n.RootKind == RootKind.Project && n.Level == 0), months);

        return new ForecastResult
        {
            Columns = columns,
            Nodes = nodes,
            EmployeeTotals = employeeTotals,
            ProjectTotals = projectTotals
        };
    }

    // -----------------------
    // Helper
    // -----------------------
    private static List<DateTime> BuildColumns(DateTime startMonth, int months)
    {
        var s = MonthStart(startMonth);
        var cols = new List<DateTime>(months);
        for (int i = 0; i < months; i++)
            cols.Add(s.AddMonths(i));
        return cols;
    }

    private Func<DateTime, decimal> BuildMonthlyCostFunc(DateTime contractStart, DateTime contractEnd, Employee employee, decimal fte)
    {
        var cStart = MonthStart(contractStart);
        var cEnd = MonthStart(contractEnd);

        return (DateTime monthStart) =>
        {
            var m = MonthStart(monthStart);
            if (m < cStart || m > cEnd) return 0m;

            int level = employee.GetLevelAt(m);
            var sal = _tvL.GetSalary(employee.Group, level);
            return sal * fte;
        };
    }

    private static DateTime MonthStart(DateTime d) => new(d.Year, d.Month, 1);

    private static IEnumerable<DateTime> MonthsInclusive(DateTime startMonth, DateTime endMonth)
    {
        for (var m = MonthStart(startMonth); m <= MonthStart(endMonth); m = m.AddMonths(1))
            yield return m;
    }

    private static int MonthKey(DateTime monthStart) => (monthStart.Year * 100) + monthStart.Month;

    private static Dictionary<int, int> BuildVisibleIndexMap(IReadOnlyList<DateTime> columns)
    {
        var map = new Dictionary<int, int>(columns.Count);
        for (int i = 0; i < columns.Count; i++)
            map[MonthKey(MonthStart(columns[i]))] = i;
        return map;
    }

    private static decimal[] SumTotals(IEnumerable<ForecastNode> rootNodes, int months)
    {
        var totals = new decimal[months];
        foreach (var n in rootNodes)
            for (int m = 0; m < months; m++)
                totals[m] += n.Values[m];
        return totals;
    }
}
