using ESystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTts : ITTS
  {

    #region Fields

    private const string URL = "https://api.elevenlabs.io/v1";
    private const string URL_SPEECH_SUBROUTE = "text-to-speech";
    private const string URL_VOICES_SUBROUTE = "voices";
    private HttpClient? http = null;
    private List<ElevenLabsVoice>? voices = null;
    private readonly Dictionary<string, byte[]> speeches = new();

    #endregion Fields

    #region Properties

    public ElevenLabsTtsSettings Settings { get; set; }

    #endregion Properties

    #region Constructors

    public ElevenLabsTts(ElevenLabsTtsSettings settings)
    {
      this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    #endregion Constructors

    #region Methods

    public async Task<byte[]> ConvertAsync(string text)
    {
      if (speeches.ContainsKey(text) == false)
      {
        string url = $"{URL}/{URL_SPEECH_SUBROUTE}/{Settings.VoiceId}";
        string body = BuildHttpGetModelJson(text);
        var tmp = await DownloadSpeechAsync(url, body);
        speeches[text] = tmp;
      }
      byte[] ret = speeches[text];
      return ret;
    }

    public async Task<List<ElevenLabsVoice>> GetVoicesAsync()
    {
      if (this.voices == null)
      {
        string url = $"{URL}/{URL_VOICES_SUBROUTE}";
        VoicesResponse tmp = await DownloadVoicesAsync(url);
        this.voices = tmp.Voices.ToList();
      }
      List<ElevenLabsVoice> ret = voices;
      return ret;
    }

    private void InitHttpIfRequired()
    {
      if (http == null)
      {
        http = new HttpClient();
        http.DefaultRequestHeaders.Add("xi-api-key", Settings.API);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
      }
    }

    private async Task<VoicesResponse> DownloadVoicesAsync(string url)
    {
      InitHttpIfRequired();

      var response = await http!.GetAsync(url);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        throw new TtsApplicationException("Failed to download voices.",
          new ApplicationException($"GET request returned {response.StatusCode}:{response.Content}."));

      var tmp = await Functions.Try(
        async () => await response.Content.ReadAsStringAsync(),
        ex => new TtsApplicationException("Failed to read POST response stream.", ex));

      tmp = NormalizeSnakeCaseToUpperCase(tmp);

      VoicesResponse ret = JsonConvert.DeserializeObject<VoicesResponse>(tmp)!;

      return ret;
    }

    private string NormalizeSnakeCaseToUpperCase(string txt)
    {
      string p = @"""([^""]+?)"" *:";
      var matches = System.Text.RegularExpressions.Regex.Matches(txt, p);
      var matchesOrdered = matches.Where(q => q.Success).Where(q => q.Groups[1].Value.Contains("_")).OrderByDescending(q => q.Index);
      string convertSnakeToUpper(string src)
      {
        bool nextUpper = true;
        StringBuilder ret = new StringBuilder();
        for (int i = 0; i < src.Length; i++)
        {
          char c = src[i];
          if (c == '_')
            nextUpper = true;
          else
          {
            if (nextUpper)
            {
              ret.Append(char.ToUpper(c));
              nextUpper = false;
            }
            else
              ret.Append(c);
          }
        }
        return ret.ToString();
      }

      StringBuilder sb = new StringBuilder(txt);
      foreach (var match in matchesOrdered)
      {
        var snakeKey = match.Groups[1].Value;
        var upperKey = convertSnakeToUpper(snakeKey);
        sb.Replace(snakeKey, upperKey, match.Groups[1].Index, match.Groups[1].Value.Length);
      }

      string ret = sb.ToString();
      return ret;
    }

    private async Task<byte[]> DownloadSpeechAsync(string url, string body)
    {
      InitHttpIfRequired();

      var requestContent = new StringContent(body, System.Text.Encoding.Default, "application/json");
      var response = await http!.PostAsync(url, requestContent);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        throw new TtsApplicationException("Failed to download speech.",
          new ApplicationException($"POST request returned {response.StatusCode}:{response.Content}."));

      var ret = await Functions.Try(
        async () => await response.Content.ReadAsByteArrayAsync(),
        ex => new TtsApplicationException("Failed to read POST response stream.", ex));

      return ret;
    }

    private HttpGetModel BuildHttpGetModel(string text)
    {
      HttpGetModel ret = new HttpGetModel(
          text,
          new VoiceSettings(0.1, 0.75));
      return ret;
    }

    private string BuildHttpGetModelJson(string text)
    {
      string ret = Functions.Try(
        () => ConvertObjectToJson(BuildHttpGetModel(text)),
        e => new TtsApplicationException($"Failed to create a model for text '{text}'.", e));
      return ret;
    }

    private string ConvertObjectToJson(HttpGetModel model)
    {
      string ret;
      try
      {
        var settings = new JsonSerializerSettings
        {
          ContractResolver = new DefaultContractResolver
          {
            NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
          },
          Formatting = Formatting.Indented
        };
        ret = JsonConvert.SerializeObject(model, settings);
      }
      catch (Exception ex)
      {
        throw new TtsApplicationException($"Failed to convert {model.GetType().Name} to JSON.", ex);
      }
      return ret;
    }

    #endregion Methods
  }
}
