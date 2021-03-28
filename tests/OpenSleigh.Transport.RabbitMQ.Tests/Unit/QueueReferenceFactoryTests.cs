using System;
using FluentAssertions;
using NSubstitute;
using OpenSleigh.Core;
using OpenSleigh.Core.Tests.Sagas;
using Xunit;

namespace OpenSleigh.Transport.RabbitMQ.Tests.Unit
{
    public class QueueReferenceFactoryTests
    {
        [Fact]
        public void Create_should_use_default_creator_when_none_defined()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = SystemInfo.New();
            var sut = new QueueReferenceFactory(sp, sysInfo, messageType =>
            {
                var exchangeName = messageType.Name.ToLower();
                var queueName = exchangeName + ".a";
                var dlExchangeName = exchangeName + ".b";
                var dlQueueName = dlExchangeName + ".c";
                return new QueueReferences(exchangeName, queueName, dlExchangeName, dlQueueName);
            });
            
            var message = DummyMessage.New();
            var result = sut.Create(message);
            result.Should().NotBeNull();
            result.ExchangeName.Should().Be("dummymessage");
            result.QueueName.Should().Be("dummymessage.a");
            result.DeadLetterExchangeName.Should().Be("dummymessage.b");
            result.DeadLetterQueue.Should().Be("dummymessage.b.c");
        }

        [Fact]
        public void Create_should_use_registered_creator()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = SystemInfo.New();
            var policy = new QueueReferencesPolicy<DummyMessage>(() =>
            {
                var exchangeName = "dummy";
                var queueName = exchangeName + ".a";
                var dlExchangeName = exchangeName + ".b";
                var dlQueueName = dlExchangeName + ".c";
                return new QueueReferences(exchangeName, queueName, dlExchangeName, dlQueueName);
            });
            sp.GetService(typeof(QueueReferencesPolicy<DummyMessage>))
                .Returns(policy);
            var sut = new QueueReferenceFactory(sp, sysInfo);
            
            var result = sut.Create<DummyMessage>();
            result.Should().NotBeNull();
            result.ExchangeName.Should().Be("dummy");
            result.QueueName.Should().Be("dummy.a");
            result.DeadLetterExchangeName.Should().Be("dummy.b");
            result.DeadLetterQueue.Should().Be("dummy.b.c");
        }

        [Fact]
        public void Create_should_return_valid_references()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = SystemInfo.New();
            var sut = new QueueReferenceFactory(sp, sysInfo);
            var message = DummyMessage.New();
            var result = sut.Create(message);
            result.Should().NotBeNull();
            result.ExchangeName.Should().Be("dummymessage");
            result.QueueName.Should().Be("dummymessage.workers");
            result.DeadLetterExchangeName.Should().Be("dummymessage.dead");
            result.DeadLetterQueue.Should().Be("dummymessage.dead.workers");
        }

        [Fact]
        public void Create_generic_should_return_valid_references()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = SystemInfo.New();
            var sut = new QueueReferenceFactory(sp, sysInfo);
            var result = sut.Create<DummyMessage>();
            result.Should().NotBeNull();
            result.ExchangeName.Should().Be("dummymessage");
            result.QueueName.Should().Be("dummymessage.workers");
            result.DeadLetterExchangeName.Should().Be("dummymessage.dead");
            result.DeadLetterQueue.Should().Be("dummymessage.dead.workers");
        }
        
        [Fact]
        public void Create_generic_should_return_valid_references_when_message_is_event()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = new SystemInfo(Guid.NewGuid(), "test");
            var sut = new QueueReferenceFactory(sp, sysInfo);
            var result = sut.Create<DummyEvent>();
            result.Should().NotBeNull();
            result.ExchangeName.Should().Be("dummyevent.test");
            result.QueueName.Should().Be("dummyevent.test.workers");
            result.DeadLetterExchangeName.Should().Be("dummyevent.test.dead");
            result.DeadLetterQueue.Should().Be("dummyevent.test.dead.workers");
        }

        [Fact]
        public void ctor_should_throw_if_input_null()
        {
            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            var sysInfo = SystemInfo.New();
            Assert.Throws<ArgumentNullException>(() => new QueueReferenceFactory(sp, null));
            Assert.Throws<ArgumentNullException>(() => new QueueReferenceFactory(null, sysInfo));
        }
    }
}
