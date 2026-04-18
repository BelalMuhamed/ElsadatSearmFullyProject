using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CopounDtos
{
    public class CopounReqDto
    {
        public string copounDesc { get; set; }

        public TypeOfCopon copounPaiedType { get; set; }  // دفع فوري او مسابقة او ميكس
        public int pointsToCollectCopoun { get; set; }
        public int stars { get; set; }
        public int paiedCash { get; set; }
    }
}
