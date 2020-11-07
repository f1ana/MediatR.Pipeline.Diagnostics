using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline.Diagnostics.Constants;
using MediatR.Pipeline.Diagnostics.Events;

namespace MediatR.Pipeline.Diagnostics {
    public class DiagnosticPipeline<TRequest, TResponse> : DiagnosticPipelineBase, IPipelineBehavior<TRequest, TResponse> {
        
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next) {
            var id = Guid.NewGuid();
            var sw = new Stopwatch();

            if (_logger.IsEnabled(DiagnosticListenerConstants.REQUEST_STARTED))
                _logger.Write(DiagnosticListenerConstants.REQUEST_STARTED, createEventData(request, id, 0));

            try {
                sw.Start();
                var r = await next().ConfigureAwait(false);
                sw.Stop();

                if (_logger.IsEnabled(DiagnosticListenerConstants.REQUEST_COMPLETED))
                    _logger.Write(DiagnosticListenerConstants.REQUEST_COMPLETED, createEventData(request, id, sw.ElapsedMilliseconds));

                return r;
            }
            catch (Exception e) {
                sw.Stop();
                if (_logger.IsEnabled(DiagnosticListenerConstants.REQUEST_ERROR))
                    _logger.Write(DiagnosticListenerConstants.REQUEST_ERROR, createExceptionData(request, id, sw.ElapsedMilliseconds, e));

                throw;
            }
        }

        private MediatrEventData createEventData(TRequest request, Guid requestGuid, double totalMilliseconds) {
            return new MediatrEventData {
                RequestType = request.GetType(),
                RequestGuid = requestGuid,
                TotalMilliseconds = totalMilliseconds,
                RequestSubType = getSubType(request)
            };
        }

        private MediatrExceptionData createExceptionData(TRequest request, Guid requestGuid, double totalMilliseconds, Exception e) {
            return new MediatrExceptionData {
                RequestType = request.GetType(),
                RequestGuid = requestGuid,
                TotalMilliseconds = totalMilliseconds,
                RequestSubType = getSubType(request),
                Exception = e
            };
        }

        private string getSubType(TRequest request) {
            if (!(request is IBaseRequest)) {
                return "Unknown";
            }
            
            switch (request) {
                case IRequest<Unit> _:
                    return "Command";
                default:
                    return "Query";
            }
        }
    }
}