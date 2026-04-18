using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.copounModel
{
    public class Copoun
    {
        ///refer to copoun type (group)  50cash+10stars
        [Key]
        public string CopounDesc { get; set; }

        public TypeOfCopon CopounPaiedType { get; set; }  //دفع فوري او مسابقة او ميكس
        public int PointsToCollectCopoun { get; set; }
        public int Stars { get; set; }
        public int PaiedCash { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
