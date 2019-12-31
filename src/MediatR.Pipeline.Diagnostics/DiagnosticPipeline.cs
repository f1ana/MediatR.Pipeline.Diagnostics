using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline.Diagnostics.Constants;
using MediatR.Pipeline.Diagnostics.Events;
using MediatR.Pipeline.Diagnostics.Options;
using MediatR.Pipeline.Diagnostics.Resolvers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MediatR.Pipeline.Diagnostics {
    public class DiagnosticPipeline<TRequest, TResponse> : DiagnosticPipelineBase, IPipelineBehavior<TRequest, TResponse> {
        private readonly IOptions<MediatrDiagnostics> _options;

        public DiagnosticPipeline(IOptions<MediatrDiagnostics> options) {
            _options = options;
        }
        
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
                Payload = createPayload(request),
                RequestSubType = getSubType(request)
            };
        }

        private MediatrExceptionData createExceptionData(TRequest request, Guid requestGuid, double totalMilliseconds, Exception e) {
            return new MediatrExceptionData {
                RequestType = request.GetType(),
                RequestGuid = requestGuid,
                TotalMilliseconds = totalMilliseconds,
                Payload = createPayload(request),
                RequestSubType = getSubType(request),
                Exception = e
            };
        }

        private string createPayload(TRequest request) {
            return JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings {
                ContractResolver = new MaskingContractResolver(_options)
            });
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