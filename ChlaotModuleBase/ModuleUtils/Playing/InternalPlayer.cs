using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  internal class InternalPlayer
  {
    public delegate void PlayerDelegate(InternalPlayer sender);

    public event PlayerDelegate? PlaybackFinished;

    public event PlayerDelegate? PlaybackStarted;

    public readonly int length;
    private readonly SoundPlayer soundPlayer;
    public InternalPlayer(byte[] bytes)
    {
      MemoryStream stream = new(bytes);
      this.soundPlayer = new SoundPlayer(stream);
      this.length = bytes.Length;
    }
    public void Play()
    {
      PlaybackStarted?.Invoke(this);
      this.soundPlayer.PlaySync();
      PlaybackFinished?.Invoke(this);
    }

    internal void PlayAsync()
    {
      Task t = new(this.Play);
      t.Start();
    }
  }
}
