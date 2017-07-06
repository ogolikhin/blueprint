﻿using System;

namespace AdminStore.Models.Workflow
{
    public class WorkflowDto
    {
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; }
        public bool Status { get; set; }
    }
}