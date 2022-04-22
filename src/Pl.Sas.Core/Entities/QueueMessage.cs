namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Đây là queue message model
    /// </summary>
    public class QueueMessage
    {
        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="id"></param>
        public QueueMessage(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Id message
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Cặp khóa và giá trị của message
        /// </summary>
        public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
    }
}