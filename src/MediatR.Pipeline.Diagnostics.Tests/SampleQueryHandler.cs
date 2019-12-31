using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Diagnostics.Tests {
    public class SampleQueryHandler : IRequestHandler<SampleQuery, int> {
        public async Task<int> Handle(SampleQuery request, CancellationToken cancellationToken) {
            return await Task.FromResult(1);
        }
    }
}