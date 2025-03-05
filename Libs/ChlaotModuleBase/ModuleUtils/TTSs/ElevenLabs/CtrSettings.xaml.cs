using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.Players;
using ESystem.Miscelaneous;
using Microsoft.WindowsAPICodePack.Shell;
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

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.ElevenLabs
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
      public ViewModel()
      {
        this.PropertyChanged += (s, e) =>
        {
          if (e.PropertyName == nameof(SelectedVoice))
            this.Settings.VoiceId = SelectedVoice.VoiceId;
        };
      }
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


      public ElevenLabsVoice SelectedVoice
      {
        get { return base.GetProperty<ElevenLabsVoice>(nameof(SelectedVoice))!; }
        set { base.UpdateProperty(nameof(SelectedVoice), value); }
      }


      public List<string> Models
      {
        get { return base.GetProperty<List<string>>(nameof(Models))!; }
        set { base.UpdateProperty(nameof(Models), value); }
      }


      public string SelectedModel
      {
        get { return base.GetProperty<string>(nameof(SelectedModel))!; }
        set { base.UpdateProperty(nameof(SelectedModel), value); }
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

      try
      {
        this.VM.Models = (await ElevenLabsTtsProvider.GetModelsAsync(this.VM.Settings.ApiKey)).OrderBy(q => q).ToList();
        this.logger.Log(ELogging.LogLevel.INFO, $"Successfully loaded {this.VM.Models.Count} models.");
      }
      catch (Exception ex)
      {
        this.logger.Log(ELogging.LogLevel.ERROR, $"Failed to download models. API key issue? Reason: " + ex.Message);
      }


      this.Cursor = c;
      btnReloadVoices.IsEnabled = true;
    }

    private async void btnPlayDemo_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      btn.IsEnabled = false;
      var c = this.Cursor;

      this.Cursor = Cursors.Wait;
      string url = (string)btn.Tag;
      byte[] bytes = await DownloadPreviewMp3Async(url);

      SimpleMp3Player player = new SimpleMp3Player();
      player.PlayAsync(bytes);

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
