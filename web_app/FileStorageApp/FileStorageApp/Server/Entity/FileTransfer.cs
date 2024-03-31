using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class FileTransfer
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int SenderId { get; set; }
        public User? Sender { get; set; }

        [ForeignKey("User")]
        public int ReceiverId { get; set; }
        //public User? Reciever { get; set; }
        public string FileName { get; set; }
        public string base64EncKey { get; set; }
        public string base64EncIv { get; set; }
        public bool isDeleted { get; set; }
    }
}
