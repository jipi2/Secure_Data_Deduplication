using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileFromCacheDto
    {
        public string base64EncFile { get; set; }
        public string  base64Tag { get; set; }
        //public string key { get; set; }
        //public string iv { get; set; }
        //public List<UsersEmailsFilenames> emailsFilenames { get; set; }
        public List<PersonalisedInfoDto> personalisedList { get; set; }
    }
}
