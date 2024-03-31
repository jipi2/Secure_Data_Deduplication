using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class BlobFileParamsDto
    {
        public string FileName { get; set; }
        public string FileKey { get; set; }
        public string FileIv { get; set; }
        public string base64tag { get; set; }
    }
}
