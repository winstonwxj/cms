using Datory.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SSCMS.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UploadType
    {
        [DataEnum(DisplayName = "Image")]
        Image,
        [DataEnum(DisplayName = "Audio")]
        Audio,
        [DataEnum(DisplayName = "Video")]
        Video,
        [DataEnum(DisplayName = "File")]
        File,
        [DataEnum(DisplayName = "Special")]
        Special
    }
}
