using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MediatR.Pipeline.Diagnostics.Tests {
    public interface IMockObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>> {
    }
}