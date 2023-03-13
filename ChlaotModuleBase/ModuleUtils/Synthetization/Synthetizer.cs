using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization
{
  public class Synthetizer
  {
    private static class WavFileTrimmer
    {
      public static void Trim(Stream inStream, Stream outStream, TimeSpan trimStart, TimeSpan trimEnd)
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
          TrimWavFile(reader, writer, startPos, endPos);
        }
      }
      private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
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

    private readonly SpeechSynthesizer synthetizer;
    private readonly TimeSpan trimStart;
    private readonly TimeSpan trimEnd;
    public Synthetizer(SynthetizerSettings s)
    {
      this.synthetizer = new SpeechSynthesizer();
      this.synthetizer.SelectVoice(s.Voice);
      this.synthetizer.Rate = s.Rate;
      this.trimStart = s.StartTrimMilisecondsTimeSpan;
      this.trimEnd = s.EndTrimMilisecondsTimeSpan;
    }

    private Synthetizer()
    {
      this.synthetizer = new();
      this.trimStart = TimeSpan.Zero;
      this.trimEnd = TimeSpan.Zero;
    }

    public static Synthetizer CreateDefault()
    {
      return new Synthetizer();
    }

    public byte[] Generate(string value)
    {
      MemoryStream tmp = new();
      this.synthetizer.SetOutputToWaveStream(tmp);
      this.synthetizer.Speak(value);

      MemoryStream ret = new();
      if (trimStart.TotalMilliseconds > 0 || trimEnd.TotalMilliseconds > 0)
        WavFileTrimmer.Trim(tmp, ret, trimStart, trimEnd);
      else
        ret = tmp;

      return ret.ToArray();
    }
  }
}
