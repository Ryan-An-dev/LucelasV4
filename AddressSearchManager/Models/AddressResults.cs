using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AddressSearchManager.Models
{
  public class AddressResults
  {
    [JsonPropertyName("common")]
    public AddressCommon Common { get; set; } = default!;
    [JsonPropertyName("juso")]
    public List<AddressDetail> Details { get; set; } = default!;
  }
}
