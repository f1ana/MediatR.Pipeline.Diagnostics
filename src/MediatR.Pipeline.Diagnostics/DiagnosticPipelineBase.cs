using System.Diagnostics;
using MediatR.Pipeline.Diagnostics.Constants;

namespace MediatR.Pipeline.Diagnostics {
    public abstract class DiagnosticPipelineBase {
        protected static DiagnosticSource _logger = new DiagnosticListener(DiagnosticListenerConstants.LISTENER_NAME);
    }
}