namespace ModbusRtuEmulator
{
    public class TcpTuning
    {
        public int Port { get; set; }

        public TcpTuning()
        {
            Port = 8000;
        }
    }
}