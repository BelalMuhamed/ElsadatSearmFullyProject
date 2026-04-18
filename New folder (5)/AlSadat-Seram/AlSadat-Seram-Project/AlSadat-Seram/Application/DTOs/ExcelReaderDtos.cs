using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ExcelReaderDtos
    {
        public class ExcelReadResult<T>
        {
            public List<T> data { get; set; } = new();
            public List<ExcelError> errors { get; set; } = new();
        }

        public class ExcelError
        {
            public int Row { get; set; }
            public string Column { get; set; }
            public string Message { get; set; }
        }
    }
}
