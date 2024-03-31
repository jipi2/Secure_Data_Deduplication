using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class FileParamsDto
    { 
        public string base64Key { get; set; }
        public string base64Iv { get; set; }
        public string base64Tag { get; set; }
        public string fileName { get; set; }
    }
}
