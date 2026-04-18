using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CoponCollectionRepresentiveRate;
public class CoponCollectionRepresentiveRateDTO
{
    public int Id { get; set; }
    public int NumberOfCopons { get; set; }
    public decimal Cashed { get; set; }
    public DateTime CreatedAt { get; set; } 
    public string? CreateBy { get; set; }
    public string? UpdateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeleteBy { get; set; }
    public DateTime? DeleteAt { get; set; }
}
