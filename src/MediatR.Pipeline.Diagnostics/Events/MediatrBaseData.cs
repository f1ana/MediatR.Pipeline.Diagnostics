using System;

namespace MediatR.Pipeline.Diagnostics.Events {
    public abstract class MediatrBaseData {
        public double TotalMilliseconds { get; set; }
        public Guid RequestGuid { get; set; }
        public Type RequestType { get; set; }
        public string RequestSubType { get; set; }
        public string Payload { get; set; }
    }
}