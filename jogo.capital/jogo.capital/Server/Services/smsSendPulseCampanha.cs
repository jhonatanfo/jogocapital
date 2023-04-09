using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace jogo.capital.Server.Services // smsSendPulseCampanha
{

    public class CampanhaButton
    {
        [JsonProperty("text")]
        public string Text;
        [JsonProperty("link")]
        public string Link;
    }

    public class CampanhaImage
    {
        [JsonProperty("link")]
        public string Link;
    }

    public class CampanhaResendSms
    {
        [JsonProperty("status")]
        public bool Status;

        [JsonProperty("sms_text")]
        public string Text;

        [JsonProperty("sms_sender_name")]
        public string SenderName;
    }

    public enum CampanhaMessageType
    {
        Marketing = 2,
        Transactional = 3
    }

    public class CampanhaAdditional
    {
        [JsonProperty("button")]
        public CampanhaButton Button = null;

        [JsonProperty("image")]
        public CampanhaImage Image = null;

        [JsonProperty("resend_sms")]
        public CampanhaResendSms ResendSms = null;
    }

    public class Campanha
    {
        [JsonProperty("task_name")]
        public string Name;

        [JsonProperty("recipients")]
        public string[] Recipients = new string[] { };

        [JsonProperty("address_book")]
        public uint AddressBook = 0;

        [JsonProperty("message")]
        public string Message = "";

        [JsonProperty("message_live_time")]
        public uint MessageLiveTime = 60;

        [JsonProperty("sender_id")]
        public uint SenderId = 0;

        [JsonProperty("send_date")]
        [JsonConverter(typeof(ViberDateTimeConverter))]
        public DateTime SendDate = DateTime.Now;

        [JsonProperty("message_type")]
        public CampanhaMessageType MessageType = CampanhaMessageType.Marketing;

        [JsonProperty("additional")]
        public CampanhaAdditional Additional = null;
    }

    public class ViberDateTimeConverter : DateTimeConverterBase
    {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dt = (DateTime)value;

            if (dt == DateTime.Now || dt < DateTime.Now)
            {
                writer.WriteToken(JsonToken.String, "now");
            }
            else
            {
                writer.WriteToken(JsonToken.String, dt.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}