﻿// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************
using System;

namespace ServiceLibrary.Models
{
    public class PerformanceLogModel : ServiceLogModel
    {
        public string ThreadID { get; set; }
        public string ActionName { get; set; }
        public Guid CorrelationId { get; set; }
        public double Duration { get; set; }
        public string Namespace { get; set; }
        public string Class { get; set; }
        public string Test { get; set; }
    }
}
