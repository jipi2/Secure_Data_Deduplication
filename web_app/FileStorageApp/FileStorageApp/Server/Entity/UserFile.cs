using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class UserFile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("FileMetadata")]
        public int? FileId { get; set; }
        public FileMetadata FileMetadata { get; set; }
        public string FileName { get; set; }
        public string? Key { get; set; }
        public string? Iv { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
