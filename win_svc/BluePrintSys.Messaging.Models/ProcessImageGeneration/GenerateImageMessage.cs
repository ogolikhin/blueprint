﻿using NServiceBus;

namespace BluePrintSys.Messaging.Models.ProcessImageGeneration
{
    [Express]
    public class GenerateImageMessage : IMessage
    {
        public string ProcessJsonModel { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
    }
}

