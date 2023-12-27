//using ELogging;
//using System;
//using System.Collections.Generic;
//using System.IO.IsolatedStorage;
//using System.Linq;
//using System.Reflection.Metadata;
//using System.Security.AccessControl;
//using System.Security.Policy;
//using System.Text;
//using System.Threading.Tasks;
//using System.Timers;

//namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection
//{
//    public class SimConManagerMock : ISimConManager
//    {
//        public SimData SimData { get; private set; } = new();
//        private readonly NewLogHandler logHandler;
//        private List<Action> actions = new List<Action>();
//        private Timer? timer = null;
//        public static SimConManagerMock CreateTakeOff()
//        {
//            SimConManagerMock mck = new()
//            {
//                SimData = new SimData()
//                {
//                    Acceleration = 0,
//                    BankAngle = 0,
//                    Altitude = 500,
//                    Callsign = "TEST 001",
//                    GroundSpeed = 0,
//                    EngineCombustion = new bool[] { true, true, false, false },
//                    Height = 0,
//                    IndicatedSpeed = 0,
//                    IsSimPaused = true,
//                    ParkingBrakeSet = true,
//                    PushbackTugConnected = false,
//                    VerticalSpeed = 0
//                }
//            };

//            for (int i = 0; i < 3; i++)
//                mck.actions.Add(() => { });
//            mck.actions.Add(() => mck.SimData.IsSimPaused = false);

//            for (int i = 0; i < 3; i++)
//                mck.actions.Add(() => { });
//            mck.actions.Add(() => mck.SimData.ParkingBrakeSet = false);

//            for (int i = 0; i < 3; i++)
//                mck.actions.Add(() => { });
//            mck.actions.Add(() => mck.SimData.Acceleration = 5);
//            for (int i = 0; i < 28; i++)
//            {
//                int tmp = i * 5;
//                mck.actions.Add(() =>
//                {
//                    mck.SimData.IndicatedSpeed = tmp;
//                    mck.SimData.GroundSpeed = tmp;
//                });
//            }
//            mck.actions.Add(() => mck.SimData.Acceleration = 0);

//            mck.actions.Add(() => mck.SimData.VerticalSpeed = 950);
//            for (int i = 0; i < 10; i++)
//            {
//                int tmp = i * 16;
//                mck.actions.Add(() =>
//                {
//                    mck.SimData.Altitude = tmp + 500;
//                    mck.SimData.Height = tmp;
//                });
//            }

//            mck.actions.Add(() => mck.SimData.VerticalSpeed = 10000);
//            for (int i = 0; i < 100; i++)
//            {
//                int tmp = i * 200;
//                mck.actions.Add(() =>
//                {
//                    mck.SimData.Altitude = tmp + 500;
//                    mck.SimData.Height = tmp;
//                });
//            }
//            mck.actions.Add(() => mck.SimData.VerticalSpeed = 0);

//            return mck;
//        }

//        public static SimConManagerMock CreateLanding()
//        {
//            SimConManagerMock mck = new()
//            {
//                SimData = new SimData()
//                {
//                    Acceleration = 0,
//                    BankAngle = 0,
//                    Altitude = 1000,
//                    Callsign = "TEST 001",
//                    GroundSpeed = 120,
//                    EngineCombustion = new bool[] { true, true, false, false },
//                    Height = 500,
//                    IndicatedSpeed = 120,
//                    IsSimPaused = true,
//                    ParkingBrakeSet = false,
//                    PushbackTugConnected = false,
//                    VerticalSpeed = 0
//                }
//            };

//            for (int i = 0; i < 3; i++)
//                mck.actions.Add(() => { });
//            mck.actions.Add(() => mck.SimData.IsSimPaused = false);

//            mck.actions.Add(() => mck.SimData.VerticalSpeed = -700);
//            for (int i = 0; i < 11; i++)
//            {
//                int tmp = 500 - i * 50;
//                mck.actions.Add(() =>
//                {
//                    mck.SimData.Altitude = tmp + 500;
//                    mck.SimData.Height = tmp;
//                });
//            }
//            mck.actions.Add(() => mck.SimData.VerticalSpeed = 0);

//            mck.actions.Add(() => mck.SimData.Acceleration = -5);
//            for (int i = 0; i < 13; i++)
//            {
//                int tmp = 120 - i * 10;
//                mck.actions.Add(() =>
//                {
//                    mck.SimData.IndicatedSpeed = tmp;
//                    mck.SimData.GroundSpeed = tmp;
//                });
//            }
//            mck.actions.Add(() => mck.SimData.Acceleration = 0);

//            for (int i = 0; i < 3; i++)
//                mck.actions.Add(() => { });
//            mck.actions.Add(() => mck.SimData.ParkingBrakeSet = true);

//            return mck;
//        }

//        public event ISimConManager.SimSecondElapsedDelegate? SimSecondElapsed;

//        public SimConManagerMock()
//        {
//            logHandler = Logger.RegisterSender(this);
//        }

//        public void Close()
//        {
//            timer?.Stop();
//            timer = null;
//        }

//        public void Open()
//        {
//        }

//        private void timer_Elapsed(object? sender, ElapsedEventArgs e)
//        {
//            Action? a = actions.FirstOrDefault();
//            if (a != null)
//            {
//                actions.Remove(a);
//                a.Invoke();
//            }
//            SimSecondElapsed?.Invoke();
//        }

//        public void Start()
//        {
//            timer = new Timer(1000);
//            timer.Elapsed += timer_Elapsed;
//            timer.Enabled = true;
//        }
//    }
//}
