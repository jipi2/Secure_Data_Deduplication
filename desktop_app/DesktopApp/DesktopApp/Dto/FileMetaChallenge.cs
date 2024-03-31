using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class FileMetaChallenge
    {
        public string id { get; set; }
        public string n1 { get; set; }
        public string n2 { get; set; }
        public string n3 { get; set; }
    }
}
