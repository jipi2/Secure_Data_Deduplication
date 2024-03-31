using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class RecievedFilesDto
    {
        public string senderEmail { get; set; }
        public string fileName { get; set; }
        public string base64EncKey { get; set; }
        public string base64EncIv { get; set; }
    }
}
