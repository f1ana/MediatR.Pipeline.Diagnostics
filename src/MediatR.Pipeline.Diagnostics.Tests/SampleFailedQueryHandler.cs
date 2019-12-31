using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Diagnostics.Tests {
    public class SampleFailedQueryHandler : IRequestHandler<SampleFailedQuery, int> {
        public async Task<int> Handle(SampleFailedQuery request, CancellationToken cancellationToken) {
            throw new Exception();
        }
    }
}