using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Profile;
public class UpdateProfileRequest
{
    //[Required]
    //[StringLength(100)]
    //public string FirstName { get; set; }

    //[Required]
    //[StringLength(100)]
    //public string LastName { get; set; }

    //[Required]
    //[StringLength(10)]
    //public string DefaultCurrency { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string DefaultCurrency { get; set; } = "";
    public string? Address { get; set; }
    public int? CityID { get; set; }
    public Gender? Gender { get; set; }
}
