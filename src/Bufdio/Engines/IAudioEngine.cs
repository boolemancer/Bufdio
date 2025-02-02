﻿using System;

namespace Bufdio.Engines
{
    /// <summary>
    /// An interface that interact with output audio device for sending audio buffers.
    /// <para>Implements: <see cref="IDisposable"/>.</para>
    /// </summary>
    public interface IAudioEngine : IDisposable
    {
        /// <summary>
        /// Sends audio samples to the output device (this is should be a blocking calls).
        /// </summary>
        /// <param name="samples">Audio samples in <c>Float32</c> format.</param>
        void Send(Span<float> samples);
    }
}
