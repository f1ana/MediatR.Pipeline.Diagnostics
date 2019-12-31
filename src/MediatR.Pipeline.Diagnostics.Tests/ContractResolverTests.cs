using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR.Pipeline.Diagnostics.Constants;
using MediatR.Pipeline.Diagnostics.Events;
using MediatR.Pipeline.Diagnostics.Options;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace MediatR.Pipeline.Diagnostics.Tests {
    [TestFixture]
    public class ContractResolverTests {
        private IMockObserver _observer;
        private IDisposable _subscription;
        private DiagnosticPipeline<SampleQuery, int> _sut;

        [SetUp]
        public void Setup() {
            _observer = Substitute.For<IMockObserver>();
            _sut = new DiagnosticPipeline<SampleQuery, int>(generateMockOptions);
            _subscription = getListenerFromSut().Subscribe(_observer);
            _observer.When(x =>
                x.OnNext(Arg.Is<DiagnosticListener>(y => y.Name == DiagnosticListenerConstants.LISTENER_NAME))).Do(
                info => {
                    var x = info[0] as DiagnosticListener;
                    x.Subscribe(_observer);
                });
        }

        [TearDown]
        public void Teardown() {
            _subscription.Dispose();
        }

        [Test]
        public async Task ContractResolverTests_FiltersSensitiveProperties() {
            // arrange
            var capturedValues = new List<string>();
            
            _observer.When(x => x.OnNext(Arg.Any<KeyValuePair<string, object>>())).Do(info => {
                var kv = (KeyValuePair<string, object>) info[0];
                capturedValues.Add(((MediatrBaseData) kv.Value).Payload);
            });
            var expectedValues = new List<string> {
                "{\n  \"UserName\": null,\n  \"Something\": null\n}", "{\n  \"UserName\": null,\n  \"Something\": null\n}"
            };

            // act
            var result = await _sut.Handle(new SampleQuery(), CancellationToken.None,  async () => await Task.FromResult(1));

            // assert
            Assert.That(result, Is.EqualTo(1));
            capturedValues.Should().BeEquivalentTo(expectedValues);
        }
        
        private IOptions<MediatrDiagnostics> generateMockOptions => Microsoft.Extensions.Options.Options.Create(generateMockOptionsObject());

        private MediatrDiagnostics generateMockOptionsObject() {
            return new MediatrDiagnostics {
                MaskedProperties = new[] {"Password", "Token"}
            };
        }
        
        private DiagnosticListener getListenerFromSut() {
            return (DiagnosticListener) _sut.GetType().BaseType.GetField("_logger", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }
    }
}