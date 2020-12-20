using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Valley.Net.Bindings.Serial;
using Valley.Net.Protocols.MeterBus;
using Valley.Net.Protocols.MeterBus.EN13757_2;
using Valley.Net.Protocols.MeterBus.EN13757_3;

namespace M_bussCore
{
    class Program
    {
        //private static int TIMEOUT_IN_SECONDS = 5;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("start testing");
                Meter_Scanner_Should_Find_Meter_When_Meter_Is_Connected_To_Collector().Wait();
                //Console.WriteLine("Test SUCCESS");

                //var r = Console.ReadLine().HexToBytes();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Test Failed Error: " + ex.Message);
            }
            Console.WriteLine("Finish. Press any key to exit");
            Console.ReadKey();
        }
        private static AutoResetEvent resetEvent = new AutoResetEvent(false);
        private static async Task Meter_Should_Respond_With_Ack_When_Sending_SND_NKE()
        {
            var resetEvent = new AutoResetEvent(false);

            var port = new SerialPort("COM8");
            port.BaudRate = 2400;

            var endpoint = new SerialBinding(port, (x, y) =>
            {
                return null;
            }, new MeterbusFrameSerializer());

            endpoint.PacketReceived += (sender, e) => resetEvent.Set();

            await endpoint.ConnectAsync(); 

            await endpoint.SendAsync(new ShortFrame((byte)ControlMask.SND_NKE, 10));

            if (resetEvent.WaitOne(TimeSpan.FromSeconds(3)) == true)
                Console.WriteLine("SUCCESS");
            else
                Console.WriteLine("FAIL");

            await endpoint.DisconnectAsync();
        }
        private static async Task Meter_Scanner_Should_Find_Meter_When_Meter_Is_Connected_To_Collector()
        {
            Console.WriteLine("start testing..");
            var port = new SerialPort("COM8");
            port.BaudRate = 2400;
            var binding = new SerialBinding(port, (x, y) => {
                Console.WriteLine("bytes to read: " + x.BytesToRead);
                // if(x.IsOpen && x.BytesToRead > 0)
                //     Console.WriteLine("should read data bytes to read: " + x.BytesToRead);
                //// return y.Deserialize(x.BaseStream);
                // var r = x.ReadByte();
                // Console.WriteLine(r);
                return null; ;
               // return null; 
            }, new MeterbusFrameSerializer());

            var master = new MBusMaster(binding);
            master.Meter += (sender, e) => Debug.WriteLine($"Found meter on address {e.Address.ToString("x2")}.");

            var addressCounter = 0x00;
            for (addressCounter = 0; addressCounter < 255; addressCounter++)
            {
                try
                {
                    Console.WriteLine($"Testing address: {addressCounter.ToString("X")}");
                    await master.Scan(new byte[] {(byte) addressCounter }, TimeSpan.FromMilliseconds(500));
                    Console.WriteLine("it works!! going next");
                    //break;
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine($"failed with message: {ex.Message}");
                }
            }
            Console.WriteLine("end test.");
        }
        private static async Task Meter_Telemetry_Should_Be_Retrieved_When_Querying_The_Collector()
        {
            Console.WriteLine("start testing..");
            var port = new SerialPort("COM8");
            port.BaudRate = 2400;
            var binding = new SerialBinding(port, (x, y) => null , new MeterbusFrameSerializer());

            var master = new MBusMaster(binding);

            var response = await master.RequestData(0x0a, TimeSpan.FromSeconds(1)) as VariableDataPacket;

            Debug.Assert(response != null);
        }
        private static void Master_Meter(object sender, MeterEventArgs e)
        {
            Console.WriteLine("master meter event", e.ToString());
            resetEvent.Set();
        }
    }
}
