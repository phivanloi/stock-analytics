using System.Text.Json.Serialization;

namespace Pl.Sas.Core.Entities
{
    #region All stock import

    public class SsiAllStock
    {
        [JsonPropertyName("data")]
        public SsiStockItem[] Data { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    public class SsiStockItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; } = null!;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; } = null!;

        [JsonPropertyName("clientName")]
        public string ClientName { get; set; } = null!;

        [JsonPropertyName("clientNameEn")]
        public string ClientNameEn { get; set; } = null!;

        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("securityName")]
        public string SecurityName { get; set; } = null!;
    }

    #endregion All stock import
}
