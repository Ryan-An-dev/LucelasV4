using System.Text.Json.Serialization;

namespace AddressSearchManager.Models
{
  public class AddressCommon
  {
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = "";

    [JsonPropertyName("countPerPage")]
    public int CountPerPage { get; set; } = 0;

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; } = 0;

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = "";

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; } = 0;

    public int TotalPage { get => 1 + (TotalCount -1) / CountPerPage; }
  }
}