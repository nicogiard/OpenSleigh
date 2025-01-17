﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSleigh.Core.Utils
{
    public interface IPersistenceSerializer
    {        
        ValueTask<byte[]> SerializeAsync<T>(T state, CancellationToken cancellationToken = default);
        ValueTask<T> DeserializeAsync<T>(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);       
    }
}