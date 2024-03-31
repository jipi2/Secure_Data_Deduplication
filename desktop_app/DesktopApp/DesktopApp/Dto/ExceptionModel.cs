using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class ExceptionModel:Exception
    {
        public string Message { get; set; }
        public int ErrorCode;

        public ExceptionModel(string message, int code)
        {
            Message = message;
            ErrorCode = code;
        }
    }
}
