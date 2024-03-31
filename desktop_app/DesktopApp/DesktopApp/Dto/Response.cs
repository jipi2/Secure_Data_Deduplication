using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class Response
    {
        public bool Succes { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
    }
}
