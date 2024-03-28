using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimesheetBE.Models.UtilityModels
{
    public class MicrosoftIdTokenDetailsModel
    {
        [JsonProperty("aud", NullValueHandling = NullValueHandling.Ignore)]
        public string Aud { get; set; }

        [JsonProperty("iss", NullValueHandling = NullValueHandling.Ignore)]
        public string Iss { get; set; }

        [JsonProperty("iat", NullValueHandling = NullValueHandling.Ignore)]
        public int? Iat { get; set; }

        [JsonProperty("nbf", NullValueHandling = NullValueHandling.Ignore)]
        public int? Nbf { get; set; }

        [JsonProperty("exp", NullValueHandling = NullValueHandling.Ignore)]
        public int? Exp { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce { get; set; }

        [JsonProperty("oid", NullValueHandling = NullValueHandling.Ignore)]
        public string Oid { get; set; }

        [JsonProperty("preferred_username", NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredUsername { get; set; }

        [JsonProperty("rh", NullValueHandling = NullValueHandling.Ignore)]
        public string Rh { get; set; }

        [JsonProperty("sub", NullValueHandling = NullValueHandling.Ignore)]
        public string Sub { get; set; }

        [JsonProperty("tid", NullValueHandling = NullValueHandling.Ignore)]
        public string Tid { get; set; }

        [JsonProperty("uti", NullValueHandling = NullValueHandling.Ignore)]
        public string Uti { get; set; }

        [JsonProperty("ver", NullValueHandling = NullValueHandling.Ignore)]
        public string Ver { get; set; }

    }
}