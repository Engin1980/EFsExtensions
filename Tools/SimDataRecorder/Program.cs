

using SimDataRecorder;

Console.WriteLine("Initializing");
SimConManager smc = new SimConManager();
smc.OnData += Smc_OnData;

void Smc_OnData(MockPlaneData data)
{
  var fields = data.GetType().GetFields();
  foreach (var field in fields)
  {
    var val = field.GetValue(data);
    Console.WriteLine($"{field.Name:-20} = {val}");
  }
  Console.WriteLine("\n///\n");
}

smc.Start();

Console.WriteLine("Running");
Thread.Sleep(1000);


Console.WriteLine("Stopping");
smc.StopAsync();

Console.WriteLine("Done");

