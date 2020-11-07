using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR.Pipeline.Diagnostics.Constants;
using NSubstitute;
using NUnit.Framework;

namespace MediatR.Pipeline.Diagnostics.Tests {
    [TestFixture]
    public class UnitTests {
        private IMockObserver _observer;
        private IDisposable _subscription;
        private DiagnosticPipeline<SampleQuery, int> _sut;
        
        [SetUp]
        public void Setup() {
            _observer = Substitute.For<IMockObserver>();
            _sut = new DiagnosticPipeline<SampleQuery, int>();
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
        public async Task PipelineTests_CanRunSampleQuery() {
            var result = await _sut.Handle(new SampleQuery(), CancellationToken.None, async () => await Task.FromResult(1));

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task PipelineTests_BroadcastsSuccessfulQuery() {
            // arrange
            var capturedKeys = new List<string>();
            _observer.When(x => x.OnNext(Arg.Any<KeyValuePair<string, object>>())).Do(info => {
                var kv = (KeyValuePair<string, object>) info[0];
                capturedKeys.Add(kv.Key);
            });
            var expectedKeys = new List<string> {
                DiagnosticListenerConstants.REQUEST_STARTED, DiagnosticListenerConstants.REQUEST_COMPLETED
            };

            // act
            var result = await _sut.Handle(new SampleQuery(), CancellationToken.None,  async () => await Task.FromResult(1));

            // assert
            Assert.That(result, Is.EqualTo(1));
            capturedKeys.Should().BeEquivalentTo(expectedKeys);
        }

        [Test]
        public async Task PipelineTests_BroadcastsFailedQuery() {
            // arrange
            var capturedKeys = new List<string>();
            _observer.When(x => x.OnNext(Arg.Any<KeyValuePair<string, object>>())).Do(info => {
                var kv = (KeyValuePair<string, object>) info[0];
                capturedKeys.Add(kv.Key);
            });
            var expectedKeys = new List<string> {
                DiagnosticListenerConstants.REQUEST_STARTED, DiagnosticListenerConstants.REQUEST_ERROR
            };
        
            // act
            Func<Task> act = async () => { await _sut.Handle(new SampleQuery(), CancellationToken.None, async () => throw new InvalidOperationException()); };
            await act.Should().ThrowAsync<InvalidOperationException>();
        
            // assert
            capturedKeys.Should().BeEquivalentTo(expectedKeys);
        }
        
        private DiagnosticListener getListenerFromSut() {
            return (DiagnosticListener) _sut.GetType().BaseType.GetField("_logger", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }
    }
}