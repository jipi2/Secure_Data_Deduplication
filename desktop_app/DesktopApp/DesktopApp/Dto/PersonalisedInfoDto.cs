using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class PersonalisedInfoDto
    {
        public string fileName { get; set; }
        public string base64key { get; set; }
        public string base64iv { get; set; }
        public string email { get; set; }
        public string UploadDate { get; set; }
    }
}
