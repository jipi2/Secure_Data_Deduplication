using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class FileModel
    {
        public string? fileName { get; set; }
        public string? encFileName { get; set; }
        public string? filePath { get; set; }
        public byte[]? hash { get; set; }
        public byte[]?  key{ get; set; }
        public byte[]? iv { get; set; }
        public string base64Tag { get; set; }
        public FileStream? fileStream;

        public FileModel() { }
    }
}
