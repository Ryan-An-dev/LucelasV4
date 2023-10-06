using AddressSearchManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AddressSearchManager.Converters
{
  public class AddressCommonConverter : JsonConverter<AddressCommon>
  {
    public override AddressCommon? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      JsonDocument document = JsonDocument.ParseValue(ref reader);
      Func<string, string> GetString = new(propName => document.RootElement.GetProperty(propName).GetString()!);
      Func<string, int> GetInt = new(propName =>
      {
        string strValue = GetString(propName);
        int.TryParse(strValue, out int intValue);
        return intValue;
      });

      AddressCommon common = new()
      {
        ErrorCode = GetString("errorCode"),
        ErrorMessage = GetString("errorMessage"),
        CurrentPage = GetInt("currentPage"),
        CountPerPage = GetInt("countPerPage"),
        TotalCount = GetInt("totalCount"),
      };

      return common;
    }

    public override void Write(Utf8JsonWriter writer, AddressCommon value, JsonSerializerOptions options)
    {
      throw new NotImplementedException();
    }
  }
}
