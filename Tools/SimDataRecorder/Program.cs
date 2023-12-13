namespace SimDataRecorder
{
  public class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      Console.WriteLine("Initializing");
      SimConManager smc = new SimConManager();
      smc.OnData += Smc_OnData;

      smc.Start();
      smc.RequestDataManually();

      Console.WriteLine("Running");
      Thread.Sleep(5000);

      Console.WriteLine("Stopping");
      smc.StopAsync();

      Console.WriteLine("Done");
    }

    private static void Smc_OnData(MockPlaneData data)
    {
      var fields = data.GetType().GetFields();
      foreach (var field in fields)
      {
        var val = field.GetValue(data);
        Console.WriteLine($"{field.Name:-20} = {val}");
      }
      Console.WriteLine("\n///\n");
    }
  }
}

