using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public  class FileFromCacheDto_v2
    {
        public string base64Tag { get; set; }
        public string encFilePath { get; set; }
        public List<PersonalisedInfoDto> personalisedList { get; set; }
    }
}
