using System;
using UnityEngine;

public class WAV
{
    public float[] LeftChannel { get; private set; }
    public int SampleCount { get; private set; }
    public int Frequency { get; private set; }

    public WAV(byte[] wav)
    {
        // Determine if the header is 44 or 46 bytes long (depending on whether the 'data' section has a subchunk header)
        int headerSize = 44; 
        if (BitConverter.ToInt32(wav, 36) != 0x61746164) // Checks for "data" in ASCII
        {
            headerSize = 46;
        }

        // Extract data from header
        Frequency = BitConverter.ToInt32(wav, 24);
        int byteDepth = wav[34] / 8;
        int channels = wav[22]; // Mono = 1, Stereo = 2

        // Calculate data section
        int dataStartIndex = headerSize;
        int dataLength = wav.Length - headerSize;
        SampleCount = dataLength / byteDepth;
        int sampleCount = dataLength / (byteDepth * channels);

        // Create audio channel
        LeftChannel = new float[sampleCount];

        // Convert and store audio data
        int resolution = BitConverter.ToInt16(wav, 34);
        int offset = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            switch (resolution)
            {
                case 16:
                    LeftChannel[i] = BitConverter.ToInt16(wav, dataStartIndex + offset) / (float)Int16.MaxValue;
                    offset += 2 * channels;
                    break;
                case 24:
                    LeftChannel[i] = BitConverter.ToInt32(new byte[] { wav[dataStartIndex + offset], wav[dataStartIndex + offset + 1], wav[dataStartIndex + offset + 2], 0 }) / (float)Int32.MaxValue;
                    offset += 3 * channels;
                    break;
                // Add additional cases for other bit depths if necessary
                default:
                    throw new Exception("Unsupported WAV bit depth: " + resolution);
            }
        }
    }
}
