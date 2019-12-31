namespace MediatR.Pipeline.Diagnostics.Tests {
    public class SampleQuery : IRequest<int> {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Something { get; set; }
    }
}