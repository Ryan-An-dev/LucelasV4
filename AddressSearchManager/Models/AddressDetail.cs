using System.Text.Json.Serialization;

namespace AddressSearchManager.Models
{
  public class AddressDetail
  {
        public AddressDetail()
        {
            
        }
        [JsonPropertyName("roadAddr")]
        public string 도로명주소 { get; set; } = "";
        [JsonPropertyName("roadAddrPart1")]
        public string 도로명주소1 { get; set; } = "";
        [JsonPropertyName("roadAddrPart2")]
        public string 도로명주소2 { get; set; } = "";
        [JsonPropertyName("ibunAddr")]
        public string 지번 { get; set; } = "";
        [JsonPropertyName("engAddr")]
        public string 영문도로명주소 { get; set; } = "";
        [JsonPropertyName("zipNo")]
        public string 우편번호 { get; set; } = "";
        [JsonPropertyName("admCd")]
        public string 행정구역코드 { get; set; } = "";
        [JsonPropertyName("rnMgtSn")]
        public string 도로명코드 { get; set; } = "";
        [JsonPropertyName("bdMgtSn")]
        public string 건물관리번호 { get; set; } = "";
        [JsonPropertyName("detBdNmList")]
        public string 상세건물명 { get; set; } = "";
        [JsonPropertyName("bdNm")]
        public string 건물명 { get; set; } = "";
        [JsonPropertyName("bdKdcd")]
        public string 공동주택여부 { get; set; } = "";
        [JsonPropertyName("siNm")]
        public string 시도명 { get; set; } = "";
        [JsonPropertyName("sggNm")]
        public string 시군구명 { get; set; } = "";
        [JsonPropertyName("emdNm")]
        public string 읍면동명 { get; set; } = "";
        [JsonPropertyName("liNm")]
        public string 법정리명 { get; set; } = "";
        [JsonPropertyName("Rn")]
        public string 도로명 { get; set; } = "";
        [JsonPropertyName("udrtYn")]
        public string 지하여부 { get; set; } = "";
        [JsonPropertyName("buldMnnm")]
        public string 건물본번 { get; set; } = "";
        [JsonPropertyName("buldSlno")]
        public string 건물부번 { get; set; } = "";

  }
}