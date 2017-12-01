﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactDetails
    {
        [DataMember(Name = "Id")]
        public int UserId { get; set; }

        [DataMember]
        public ReviewParticipantRole Role { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Approval { get; set; }

        [DataMember]
        public ViewStateType ViewState { get; set; }

        private DateTime? _eSignatureTimestamp;
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ESignatureTimestamp {
            get
            {
                return _eSignatureTimestamp;
            }
            set
            {
                _eSignatureTimestamp = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?)null;
            }
        }

        [DataMember]
        public IEnumerable<string> MeaningOfSignature { get; set; }
    }
}