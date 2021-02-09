﻿using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OpenSleigh.Core.Messaging;

namespace OpenSleigh.Transport.AzureServiceBus
{
    public class QueueReferenceFactory : IQueueReferenceFactory
    {
        private readonly ConcurrentDictionary<Type, QueueReferences> _queueReferencesCache = new();
        private readonly Func<Type, QueueReferences> _defaultCreator;
        private readonly IServiceProvider _sp;

        public QueueReferenceFactory(IServiceProvider sp, Func<Type, QueueReferences> defaultCreator = null)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));

            _defaultCreator = defaultCreator ?? (messageType =>
            {
                var topicName = messageType.Name.ToLower();
                var subscriptionName = topicName + ".workers";
                return new QueueReferences(topicName, subscriptionName);
            });
        }

        public QueueReferences Create<TM>() where TM : IMessage
            => _queueReferencesCache.GetOrAdd(typeof(TM), k => CreateCore<TM>());

        private QueueReferences CreateCore<TM>()
            where TM : IMessage
        {
            var creator = _sp.GetService<QueueReferencesPolicy<TM>>();
            return (creator is null) ? _defaultCreator(typeof(TM)) : creator();
        }
    }

    public delegate QueueReferences QueueReferencesPolicy<TM>() where TM : IMessage;
}