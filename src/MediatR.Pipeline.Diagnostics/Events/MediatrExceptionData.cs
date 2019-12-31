using System;

namespace MediatR.Pipeline.Diagnostics.Events {
    public class MediatrExceptionData : MediatrBaseData {
        public Exception Exception { get; set; }
    }
}