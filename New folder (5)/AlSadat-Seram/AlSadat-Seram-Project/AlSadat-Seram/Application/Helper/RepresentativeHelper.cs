using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper;
public class RepresentativeHelper
{
    public string? RepresentativeCode { get; set; }
    public string? RepresentativeName { get; set; } 
    public string? CityName { get; set; }
    public bool IsActive { get; set; }
    public RepresentiveType RepresentiveType { get; set; }
}

