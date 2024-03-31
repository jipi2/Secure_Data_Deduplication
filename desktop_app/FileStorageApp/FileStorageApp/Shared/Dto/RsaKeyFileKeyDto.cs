using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class RsaKeyFileKeyDto
    {
        public string pubKey { get; set; }
        public string fileKey { get; set; }
        public string  fileIv { get; set; }
    }
}
