namespace Pl.Sas.WebDashboard.Models
{
    public class WriteLogModel
    {
        public string Host { get; set; }
        public string Message { get; set; }
        public string FullMessage { get; set; }
        public byte Type { get; set; }
    }
}
