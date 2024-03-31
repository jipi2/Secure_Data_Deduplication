using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class RsaDto
    {
        public string base64PubKey { get; set; }
        public string base64EncPrivKey { get; set; }
    }
}
