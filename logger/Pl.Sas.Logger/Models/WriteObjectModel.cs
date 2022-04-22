using System.ComponentModel.DataAnnotations;

namespace Pl.Sas.Logger.Models
{
    public class WriteObjectModel
    {
        [MaxLength(64)]
        public string? Host { get; set; }
        [MaxLength(256)]
        public string? Message { get; set; }
        public string? FullMessage { get; set; }
        public byte Type { get; set; }
    }
}
