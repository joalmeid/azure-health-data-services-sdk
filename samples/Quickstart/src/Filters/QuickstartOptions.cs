﻿
using AzureHealth.DataServices.Pipelines;

namespace Quickstart.Filters
{
    public class QuickstartOptions
    {
        public string FhirServerUrl { get; set; }

        public double RetryDelaySeconds { get; set; }

        public int MaxRetryAttempts { get; set; }

        public StatusType ExecutionStatusType { get; set; }

    }
}
