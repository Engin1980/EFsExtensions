using ESystem.Miscelaneous;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  /// <summary>
  /// Interaction logic for CtrElevenLabsSettings.xaml
  /// </summary>
  public partial class CtrSettings : UserControl
  {
    private readonly ELogging.Logger logger = ELogging.Logger.Create(nameof(ElevenLabsTtsModule) + "+Settings");
    private readonly ViewModel VM;

    internal class ViewModel : NotifyPropertyChanged
    {
      public ElevenLabsTtsSettings Settings
      {
        get { return base.GetProperty<ElevenLabsTtsSettings>(nameof(Settings))!; }
        set { base.UpdateProperty(nameof(Settings), value); }
      }

      public List<ElevenLabsVoice> Voices
      {
        get { return base.GetProperty<List<ElevenLabsVoice>>(nameof(Voices))!; }
        set { base.UpdateProperty(nameof(Voices), value); }
      }
    }

    public CtrSettings(ElevenLabsTtsSettings settings)
    {
      InitializeComponent();
      this.DataContext = VM = new()
      {
        Settings = settings
      };
    }

    public CtrSettings()
    {
      InitializeComponent();
      this.DataContext = VM = null!;
    }

    private async void btnReloadVoices_Click(object sender, RoutedEventArgs e)
    {
      btnReloadVoices.IsEnabled = false;
      var c = this.Cursor;
      this.Cursor = Cursors.Wait;
      try
      {
        this.VM.Voices = (await ElevenLabsTtsProvider.GetVoicesAsync(this.VM.Settings.ApiKey)).OrderBy(q => q.Name).ToList();
        this.logger.Log(ELogging.LogLevel.INFO, $"Successfully loaded {this.VM.Voices.Count} voices.");
      }
      catch (Exception ex)
      {
        this.logger.Log(ELogging.LogLevel.ERROR, $"Failed to download voices. API key issue? Reason: " + ex.Message);
      }
      this.Cursor = c;
      btnReloadVoices.IsEnabled = true;
    }

    //private async void btnTestSpeech_Click(object sender, RoutedEventArgs e)
    //{
    //  string s = txtTestSpeech.Text.Trim();
    //  if (s.Length == 0) return;

    //  btnTestSpeech.IsEnabled = false;
    //  try
    //  {
    //    byte[] mp3 = await this.Tts.ConvertAsync(s);
    //    Play(mp3);
    //  }
    //  catch (Exception ex)
    //  {
    //    this.logger.Log(ELogging.LogLevel.ERROR, "Failed to generate or play test speech. Reason: " + ex.Message);
    //  }

    //  btnTestSpeech.IsEnabled = true;
    //}

    private class SimplePlayer
    {
      private MemoryStream? ms;
      private WaveStream? mp3stream;
      private WaveOutEvent? player;
      private bool isUsed = false;

      public void PlayAsync(byte[] mp3)
      {
        Debug.Assert(isUsed == false, "The same simple player cannot be used twice.");
        isUsed = true;

        ms = new MemoryStream(mp3);
        mp3stream = new Mp3FileReader(ms);
        player = new WaveOutEvent();
        player.PlaybackStopped += Player_PlaybackStopped;
        player.Init(mp3stream);
        player.Play();
      }

      private void Player_PlaybackStopped(object? sender, EventArgs e)
      {
        player!.Stop();
        mp3stream!.Dispose();
        ms!.Dispose();
        player!.Dispose();
      }
    }

    private void Play(byte[] mp3)
    {
      SimplePlayer sp = new SimplePlayer();
      sp.PlayAsync(mp3);
    }

    private async void btnPlayDemo_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      btn.IsEnabled = false;
      var c = this.Cursor;

      this.Cursor = Cursors.Wait;
      string url = (string)btn.Tag;
      byte[] bytes = await DownloadPreviewMp3Async(url);
      Play(bytes);

      this.Cursor = c;
      btn.IsEnabled = true;
    }

    private async Task<byte[]> DownloadPreviewMp3Async(string previewUrl)
    {
      byte[] ret;
      HttpClient http = new();
      var res = await http.GetAsync(previewUrl);
      if (res.IsSuccessStatusCode)
        ret = await res.Content.ReadAsByteArrayAsync();
      else
        throw new TtsApplicationException($"Failed to download previev from '{previewUrl}'.", new ApplicationException($"HTTP:{res.StatusCode}, body:{res.Content.ToString()}."));
      return ret;
    }
  }
}
