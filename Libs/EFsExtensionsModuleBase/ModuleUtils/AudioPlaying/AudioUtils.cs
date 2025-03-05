using ESystem.Asserting;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public static class AudioUtils
  {
    public static MemoryStream TrimSilence(Stream inputStream, float silenceThresholdDb = -40)
    {
      EAssert.Argument.IsNotNull(inputStream, nameof(inputStream));
      EAssert.Argument.IsTrue(silenceThresholdDb < 0, nameof(silenceThresholdDb), "Value must be non-positive.");

      inputStream.Position = 0;
      using var reader = new WaveFileReader(inputStream);
      // Get WAV format
      var format = reader.WaveFormat;
      int bitsPerSample = format.BitsPerSample;
      if (bitsPerSample != 16 && bitsPerSample != 32)
        throw new NotSupportedException("Only 16-bit and 32-bit PCM WAV files are supported.");

      int bytesPerSample = bitsPerSample / 8;
      int blockAlign = format.BlockAlign;
      int sampleCount = (int)(reader.Length / blockAlign);

      EAssert.IsTrue(bitsPerSample == 16 || bitsPerSample == 32, "Only 16 or 32 bits WAV format supported.");

      // Read audio data
      byte[] audioBytes = new byte[reader.Length]; // - 44]; // Exclude header
      reader.Read(audioBytes, 0, audioBytes.Length);

      // Convert byte[] to float[] for amplitude analysis
      float[] samples = new float[sampleCount];
      for (int i = 0; i < sampleCount; i++)
      {
        if (bitsPerSample == 16)
        {
          short sample = BitConverter.ToInt16(audioBytes, i * bytesPerSample);
          samples[i] = sample / 32768f; // Normalize 16-bit to -1.0 to 1.0
        }
        else if (bitsPerSample == 32)
        {
          int sample = BitConverter.ToInt32(audioBytes, i * bytesPerSample);
          samples[i] = sample / (float)int.MaxValue; // Normalize 32-bit
        }
      }

      // Find start and end points
      bool foundStart = false;
      int startSample = 0, endSample = 0;
      for (int i = 0; i < samples.Length; i++)
      {
        float amplitudeDb = 20 * (float)Math.Log10(Math.Abs(samples[i]) + 1e-10);
        if (amplitudeDb > silenceThresholdDb)
        {
          if (!foundStart)
          {
            startSample = i;
            foundStart = true;
          }
          endSample = i;
        }
      }

      // Compute new trimmed data size
      int newSampleCount = endSample - startSample + 1;
      byte[] trimmedAudioBytes = new byte[newSampleCount * bytesPerSample];

      // Copy relevant data
      Buffer.BlockCopy(audioBytes, startSample * bytesPerSample, trimmedAudioBytes, 0, trimmedAudioBytes.Length);

      // Write new WAV file
      var outputStream = new MemoryStream();
      using var writer = new WaveFileWriter(outputStream, format);
      writer.Write(trimmedAudioBytes, 0, trimmedAudioBytes.Length);
      writer.Flush();
      return outputStream;
    }
    public static byte[] TrimSilence(byte[] wavData, float silenceThresholdDb = -40)
    {
      using var inputStream = new MemoryStream(wavData);
      using var outputStream = TrimSilence(inputStream, silenceThresholdDb);
      byte[] ret = outputStream.ToArray();
      return ret;
    }

    public static byte[] AppendSilence(byte[] wavData, int silenceMs)
    {
      EAssert.Argument.IsNotNull(wavData, nameof(wavData));
      EAssert.Argument.IsTrue(silenceMs >= 0, nameof(silenceMs), "Value must be non-negative.");
      if (silenceMs == 0) return wavData;

      using (var inputStream = new MemoryStream(wavData))
      using (var reader = new WaveFileReader(inputStream))
      {
        // Get WAV format
        var format = reader.WaveFormat;
        int bitsPerSample = format.BitsPerSample;
        if (bitsPerSample != 16 && bitsPerSample != 32)
          throw new NotSupportedException("Only 16-bit and 32-bit PCM WAV files are supported.");

        int bytesPerSample = bitsPerSample / 8;
        int samplesPerMillisecond = format.SampleRate * format.Channels / 1000;
        int silenceSamples = samplesPerMillisecond * silenceMs;
        int silenceBytes = silenceSamples * bytesPerSample;

        // Generate silence (zero bytes)
        byte[] silenceData = new byte[silenceBytes];

        // Write new WAV file with added silence
        using (var outputStream = new MemoryStream())
        using (var writer = new WaveFileWriter(outputStream, format))
        {
          writer.Write(wavData, 44, wavData.Length - 44); // Copy original WAV data (skip header)
          writer.Write(silenceData, 0, silenceData.Length); // Append silence
          writer.Flush();
          return outputStream.ToArray();
        }
      }
    }

    //TODO if used, rewrite let it returns out stream
    public static void TrimWav(Stream inStream, Stream outStream, TimeSpan trimStart, TimeSpan trimEnd)
    {
      TimeSpan wavLength;
      float bpms;

      inStream.Position = 0;
      using (WaveFileReader wf = new(inStream))
      {
        wavLength = wf.TotalTime;
        bpms = wf.WaveFormat.AverageBytesPerSecond / 1000f;
      }

      if (trimStart + trimEnd > wavLength)
      {
        inStream.Position = 0;
        inStream.CopyTo(outStream);
      }
      else
      {
        int startPos = (int)Math.Round(trimStart.TotalMilliseconds * bpms);
        int endPos = (int)(Math.Round(wavLength.TotalMilliseconds * bpms) - Math.Round(trimEnd.TotalMilliseconds * bpms));

        inStream.Position = 0;
        using WaveFileReader reader = new(inStream);
        using WaveFileWriter writer = new(outStream, reader.WaveFormat);
        TrimWavFileReader(reader, writer, startPos, endPos);
      }
    }
    private static void TrimWavFileReader(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
    {
      reader.Position = startPos;
      byte[] buffer = new byte[reader.BlockAlign * 1024];
      while (reader.Position < endPos)
      {
        int bytesRequired = (int)(endPos - reader.Position);
        if (bytesRequired > 0)
        {
          int bytesToRead = Math.Min(bytesRequired, buffer.Length);
          bytesToRead -= bytesToRead % reader.WaveFormat.BlockAlign;
          int bytesRead = reader.Read(buffer, 0, bytesToRead);
          if (bytesRead > 0)
          {
            writer.Write(buffer, 0, bytesRead);
          }
          else
            break;
        }
      }
    }
  }
}
