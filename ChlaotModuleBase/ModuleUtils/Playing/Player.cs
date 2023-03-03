using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  public class Player
  {
    public delegate void PlayerDelegate(Player sender);

    public event PlayerDelegate? PlaybackFinished;

    public event PlayerDelegate? PlaybackStarted;

    public readonly int length;
    private readonly SoundPlayer soundPlayer;
    public Player(byte[] bytes)
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

    public void PlayAsync()
    {
      Task t = new(this.Play);
      t.Start();
    }
  }
}
