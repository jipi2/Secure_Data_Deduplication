using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class Resp
    {
        [Key]
        public int Id { get; set; }
        public int Position_1 { get; set; }
        public int Position_2 { get; set; }
        public int Position_3 { get; set; }

        public string Answer { get; set; }
        public bool wasUsed { get; set; }

        [ForeignKey("FileMetadata")]
        public int FileMetadataId { get; set; }

        public FileMetadata FileMetadata { get; set; }
    }
}
