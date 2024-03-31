using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{

    public class FileDedupDto
    {
        public string userEmail { get; set; }
        public string base64tag { get; set; }
        public string fileName { get; set; }
        
        public string base64key { get; set; }
        public string base64iv { get; set; }
    }
}
