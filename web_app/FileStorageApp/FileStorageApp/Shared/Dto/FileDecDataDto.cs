using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileDecDataDto
    {
        public string base64key { get; set; }
        public string base64iv { get; set; }
        public string fileName { get; set; }
        public string tag { get; set; }
    }
}
