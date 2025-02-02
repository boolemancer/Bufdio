﻿using System;
using System.Collections.Generic;
using Bufdio.Bindings.PortAudio;
using Bufdio.Exceptions;
using Bufdio.Utilities;
using Bufdio.Utilities.Extensions;
using FFmpeg.AutoGen;

namespace Bufdio
{
    /// <summary>
    /// Provides functionalities to retrieve, configure and manage current Bufdio environment
    /// that affects the whole library configuration.
    /// </summary>
    public static class BufdioLib
    {
        internal static class Constants
        {
            // Currently, resampling and outputing audio buffers only works with Float32 format.
            // Thats why these variables exists, we might need to add customizations in the future.
            public const AVSampleFormat FFmpegSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_FLT;
            public const PaBinding.PaSampleFormat PaSampleFormat = PaBinding.PaSampleFormat.paFloat32;
        }
        
        private static AudioDevice _defaultOutputDevice;
        private static List<AudioDevice> _outputDevices;

        /// <summary>
        /// Gets whether or not the FFmpeg is already initialized.
        /// </summary>
        public static bool IsFFmpegInitialized { get; private set; }
        
        /// <summary>
        /// Gets whether or not the PortAudio library is already initialized.
        /// </summary>
        /// <exception cref="BufdioException">Thrown if PortAudio is not initialized.</exception>
        public static bool IsPortAudioInitialized { get; private set; }

        /// <summary>
        /// Gets default output device information that is used by the current system.
        /// </summary>
        /// <exception cref="BufdioException">Thrown if PortAudio is not initialized.</exception>
        public static AudioDevice DefaultOutputDevice
        {
            get
            {
                Ensure.That<BufdioException>(IsPortAudioInitialized, "PortAudio is not initialized.");
                return _defaultOutputDevice;
            }
        }
        
        /// <summary>
        /// Gets list of available audio output devices in the current system.
        /// Will throws <see cref="BufdioException"/> if PortAudio is not initialized.
        /// </summary>
        public static IReadOnlyCollection<AudioDevice> OutputDevices
        {
            get
            {
                Ensure.That<BufdioException>(IsPortAudioInitialized, "PortAudio is not initialized.");
                return _outputDevices;
            }
        }

        /// <summary>
        /// Initializes and register FFmpeg functionalities by providing path to FFmpeg native libraries.
        /// Or just leave directory parameter empty in order to use system-wide libraries.
        /// Will returns immediately if already initialized.
        /// </summary>
        /// <param name="ffmpegDirectory">
        /// Path to FFmpeg native libaries, leave this empty to use system-wide libraries.
        /// </param>
        public static void InitializeFFmpeg(string ffmpegDirectory = default)
        {
            if (IsFFmpegInitialized)
            {
                return;
            }

            ffmpeg.RootPath = ffmpegDirectory ?? "";
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);
            IsFFmpegInitialized = true;
        }

        /// <summary>
        /// Initializes and register PortAudio functionalities by providing path to PortAudio native libary.
        /// Bufdio will use PortAudio to sends audio buffers to the output device.
        /// This is required and cannot use system-wide libraries. Will returns immediately if already initialized.
        /// </summary>
        /// <param name="portAudioPath">
        /// Path to port audio native libary, eg: portaudio.dll, libportaudio.so, libportaudio.dylib.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when given path is null.</exception>
        /// <exception cref="BufdioException">Thrown when output device is not available.</exception>
        public static void InitializePortAudio(string portAudioPath)
        {
            if (IsPortAudioInitialized)
            {
                return;
            }
            
            Ensure.NotNull(portAudioPath, nameof(portAudioPath));
            PaBinding.InitializeBindings(new LibraryLoader(portAudioPath));
            PaBinding.Pa_Initialize();
            
            var deviceCount = PaBinding.Pa_GetDeviceCount();
            Ensure.That<BufdioException>(deviceCount > 0, "No output devices are available.");

            var defaultDevice = PaBinding.Pa_GetDefaultOutputDevice();
            _defaultOutputDevice = defaultDevice.PaGetPaDeviceInfo().PaToAudioDevice(defaultDevice);
            _outputDevices = new List<AudioDevice>();
            
            for (var i = 0; i < deviceCount; i++)
            {
                var deviceInfo = i.PaGetPaDeviceInfo();

                if (deviceInfo.maxOutputChannels > 0)
                {
                    _outputDevices.Add(deviceInfo.PaToAudioDevice(i));
                }
            }

            IsPortAudioInitialized = true;
        }
    }
}
