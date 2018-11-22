using System.IO.Ports;

namespace ModbusRtuEmulator
{
    public class PortTuning
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadBufferSize { get; set; }
        public int WriteBufferSize { get; set; }

        public PortTuning()
        {
            PortName = "COM1";
            BaudRate = 9600;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ReadTimeout = 500;
            WriteTimeout = 500;
            ReadBufferSize = 250;
            WriteBufferSize = 250;
        }
    }
}