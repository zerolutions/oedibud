using System.ComponentModel.DataAnnotations;

namespace oedibud.Models;

public enum EmployeeGroup
{
    [Display(Name = "E 15Ü")] E15U,
    [Display(Name = "E 15")] E15,
    [Display(Name = "E 14")] E14,
    [Display(Name = "E 13Ü")] E13U,
    [Display(Name = "E 13")] E13,
    [Display(Name = "E 12")] E12,
    [Display(Name = "E 11")] E11,
    [Display(Name = "E 10")] E10,
    [Display(Name = "E 9b")] E9b,
    [Display(Name = "E 9a")] E9a,
    [Display(Name = "E 8")] E8,
    [Display(Name = "E 7")] E7,
    [Display(Name = "E 6")] E6,
    [Display(Name = "E 5")] E5,
    [Display(Name = "E 4")] E4,
    [Display(Name = "E 3")] E3,
    [Display(Name = "E 2Ü")] E2U,
    [Display(Name = "E 2")] E2,
    [Display(Name = "E 1")] E1
}
