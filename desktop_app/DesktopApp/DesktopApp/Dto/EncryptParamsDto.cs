using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class EncryptParamsDto
    {
        public string userEmail { get; set; }
        public string fileName { get; set; }
        public string fileKey { get; set; }
        public string fileIv { get; set; }
    }
}
