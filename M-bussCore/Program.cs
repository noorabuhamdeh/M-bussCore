using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Valley.Net.Bindings.Serial;
using Valley.Net.Protocols.MeterBus;
using Valley.Net.Protocols.MeterBus.EN13757_2;

namespace M_bussCore
{
    class Program
    {
        private const int TIMEOUT_IN_SECONDS = 3;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        public async Task Meter_Should_Respond_With_Ack_When_Sending_SND_NKE()
        {
            var resetEvent = new AutoResetEvent(false);

            var port = new SerialPort();
            port.BaudRate = 1200;

            var endpoint = new SerialBinding(port, (x, y) =>
            {
                return null;
            }, new MeterbusFrameSerializer());

            endpoint.PacketReceived += (sender, e) => resetEvent.Set();

            await endpoint.ConnectAsync();

            await endpoint.SendAsync(new ShortFrame((byte)ControlMask.SND_NKE, 0x0a));

            //Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS)));

            await endpoint.DisconnectAsync();
        }
    }
}
