using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ModbusRtuEmulator
{
    public partial class EmulatorForm : Form
    {
        private readonly BackgroundWorker _worker;

        private static readonly ConcurrentDictionary<string, ModbusItem> DictModbusItems =
            new ConcurrentDictionary<string, ModbusItem>();

        private static readonly object Mutelock = new object();
        private static bool _mute;

        public EmulatorForm()
        {
            InitializeComponent();
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
        }

        private void EmulatorForm_Load(object sender, EventArgs e)
        {
            var ports = new List<string>(SerialPort.GetPortNames());
            ports.Sort();
            cbPort.Items.AddRange(new List<object>(ports).ToArray());
            cbPort.Items.Add("TCP");
             cbPort.Text = cbPort.Items[0].ToString();
            //--------------------
            cbPort.SelectionChangeCommitted -= cbPort_SelectionChangeCommitted;
            dudBaudRate.SelectedItemChanged -= dudBaudRate_SelectedItemChanged;
            nudPort.ValueChanged -= nudPort_ValueChanged;
            try
            {
                LoadProperties();
                dudBaudRate.Visible = cbPort.Text.StartsWith("COM");
                nudPort.Visible = !dudBaudRate.Visible;
            }
            finally
            {
                cbPort.SelectionChangeCommitted += cbPort_SelectionChangeCommitted;
                dudBaudRate.SelectedItemChanged += dudBaudRate_SelectedItemChanged;
                nudPort.ValueChanged += nudPort_ValueChanged;
            }
            var porttuning = new PortTuning
                {
                    PortName = cbPort.Text,
                    BaudRate = 115200
                };
            int br;
            if (int.TryParse(dudBaudRate.Text, out br))
                porttuning.BaudRate = br;
            else
                dudBaudRate.Text = @"115200";
            lbPortTuned.Text = String.Format("{0}, {1}", porttuning.PortName, porttuning.BaudRate);
            _worker.DoWork += (o, args) =>
                {
                    var worker = (BackgroundWorker) o;

                    #region работа с последовательным портом

                    var pt = args.Argument as PortTuning;
                    if (pt != null)
                    {
                        using (var sport = new SerialPort())
                        {
                            #region настройка порта

                            try
                            {
                                sport.PortName = pt.PortName;
                                sport.BaudRate = pt.BaudRate;
                                sport.Parity = pt.Parity;
                                sport.DataBits = pt.DataBits;
                                sport.StopBits = pt.StopBits;
                                sport.Handshake = pt.Handshake;
                                sport.ReadTimeout = pt.ReadTimeout;
                                sport.WriteTimeout = pt.WriteTimeout;
                                sport.ReadBufferSize = pt.ReadBufferSize;
                                sport.WriteBufferSize = pt.WriteBufferSize;
                                sport.DataReceived += sport_DataReceived;
                                sport.ErrorReceived += sport_ErrorReceived;
                            }
                            catch (Exception ex)
                            {
                                Say = "Ошибка настройки порта: " + ex.Message;
                            }

                            #endregion настройка порта

                            if (new List<string>(SerialPort.GetPortNames()).Contains(pt.PortName))
                            {
                                try
                                {
                                    sport.Open();
                                    Say = String.Format("Порт {0} открыт.", pt.PortName);
                                    var _continue = true;
                                    while (_continue)
                                        _continue = !worker.CancellationPending;
                                    sport.Close();
                                    Say = String.Format("Порт {0} закрыт.", pt.PortName);
                                }
                                catch (Exception ex)
                                {
                                    Say = "Ошибка порта: " + ex.Message;
                                }
                            }
                            else
                                Say = String.Format("Порта {0} не существует.", pt.PortName);
                        }
                    }

                    #endregion работа с последовательным портом

                    #region работа с TCP портом

                    var tt = args.Argument as TcpTuning;
                    if (tt != null)
                    {
                        const int socketTimeOut = 90000;
                        var listener = new TcpListener(IPAddress.Any, tt.Port)
                            {
                                Server = {SendTimeout = socketTimeOut, ReceiveTimeout = socketTimeOut}
                            };
                        Say = String.Format("Сокет TCP({0}) прослушивается...", tt.Port);
                        do
                        {
                            Thread.Sleep(1);
                            try
                            {
                                listener.Start(10);
                                // Buffer for reading data
                                var bytes = new Byte[256];

                                while (!listener.Pending())
                                {
                                    Thread.Sleep(1);
                                    if (!worker.CancellationPending) continue;
                                    listener.Stop();
                                    args.Cancel = true;
                                    Say = String.Format("Сокет TCP({0}) - остановка прослушивания.", tt.Port);
                                    return;
                                }
                                var clientData = listener.AcceptTcpClient();
                                // создаем отдельный поток для каждого подключения клиента
                                ThreadPool.QueueUserWorkItem(arg =>
                                    {
                                        try
                                        {                                           
                                            // Get a stream object for reading and writing
                                            var stream = clientData.GetStream();
                                            int count;
                                            // Loop to receive all the data sent by the client.
                                            while ((count = stream.Read(bytes, 0, bytes.Length)) != 0)
                                            {
                                                Thread.Sleep(1);
                                                var list = new List<string>();
                                                for (var i = 0; i < count; i++) list.Add(String.Format("{0}", bytes[i]));
                                                Say = "Q:" + String.Join(",", list);

                                                if (count < 6) continue;
                                                var header1 = Convert.ToUInt16(bytes[0] * 256 + bytes[1]);
                                                var header2 = Convert.ToUInt16(bytes[2] * 256 + bytes[3]);
                                                var packetLen = Convert.ToUInt16(bytes[4] * 256 + bytes[5]);
                                                if (count != packetLen + 6) continue;
                                                var nodeAddr = bytes[6];
                                                var funcCode = bytes[7];
                                                var startAddr = Convert.ToUInt16(bytes[8] * 256 + bytes[9]);
                                                var regCount = Convert.ToUInt16(bytes[10] * 256 + bytes[11]);
                                                var singleValue = Convert.ToUInt16(bytes[10] * 256 + bytes[11]);
                                                EnsureNode(nodeAddr);
                                                List<byte> answer;
                                                byte bytesCount;
                                                string childName;
                                                ModbusItem modbusitem;
                                                ModbusHoldingRegister modbusHr;
                                                switch (funcCode)
                                                {
                                                    case 3: // - read holding registers
                                                    case 4: // - read input registers
                                                        answer = new List<byte>();
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header1)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header2)));
                                                        bytesCount = Convert.ToByte(regCount*2);
                                                        packetLen = Convert.ToUInt16(bytesCount + 3); // 
                                                        answer.AddRange(BitConverter.GetBytes(Swap(packetLen)));
                                                        answer.Add(nodeAddr);
                                                        answer.Add(funcCode);
                                                        answer.Add(bytesCount);
                                                        for (var addr = 0; addr < regCount; addr++)
                                                        {
                                                            if (funcCode == 3)
                                                                EnsureModbusHR(nodeAddr, startAddr + addr);
                                                            else
                                                                EnsureModbusAI(nodeAddr, startAddr + addr);
                                                            childName = String.Format(funcCode == 3 ? "Node{0}.HR{1}" : "Node{0}.AI{1}", nodeAddr, startAddr + addr);
                                                            while (!DictModbusItems.TryGetValue(childName, out modbusitem)) Thread.Sleep(10);
                                                            UInt16 value;
                                                            if (funcCode == 3)
                                                            {
                                                                modbusHr = (ModbusHoldingRegister) modbusitem;
                                                                value = BitConverter.ToUInt16(BitConverter.GetBytes(modbusHr.IntValue), 0);
                                                            }
                                                            else
                                                            {
                                                                var modbusAi = (ModbusAnalogInput)modbusitem;
                                                                value = BitConverter.ToUInt16(BitConverter.GetBytes(modbusAi.IntValue), 0);

                                                            }
                                                            answer.AddRange(BitConverter.GetBytes(Swap(value)));
                                                        }
                                                        lock (Mutelock)
                                                        {
                                                            if (!_mute)
                                                            {
                                                                list.Clear();
                                                                list.AddRange(answer.Select(t => String.Format("{0}", t)));
                                                                Say = "A:" + String.Join(",", list);
                                                                var msg = answer.ToArray();
                                                                stream.Write(msg, 0, msg.Length);
                                                            }
                                                        }
                                                        break;
                                                    case 6: // write one register
                                                        answer = new List<byte>();
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header1)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header2)));
                                                        bytesCount = Convert.ToByte(regCount*2);
                                                        packetLen = Convert.ToUInt16(bytesCount + 3); // 
                                                        answer.AddRange(BitConverter.GetBytes(Swap(packetLen)));
                                                        answer.Add(nodeAddr);
                                                        answer.Add(funcCode);
                                                        answer.AddRange(BitConverter.GetBytes(Swap(startAddr)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(singleValue)));
                                                        EnsureModbusHR(nodeAddr, startAddr);
                                                        childName = String.Format("Node{0}.HR{1}", nodeAddr, startAddr);
                                                        while (!DictModbusItems.TryGetValue(childName, out modbusitem)) Thread.Sleep(10);
                                                        modbusHr = (ModbusHoldingRegister)modbusitem;
                                                        modbusHr.IntValue = BitConverter.ToInt16(BitConverter.GetBytes(singleValue), 0);
                                                        while (!DictModbusItems.TryUpdate(childName, modbusHr, modbusHr)) Thread.Sleep(10);
                                                        lock (Mutelock)
                                                        {
                                                            if (!_mute)
                                                            {
                                                                list.Clear();
                                                                list.AddRange(answer.Select(t => String.Format("{0}", t)));
                                                                Say = "A:" + String.Join(",", list);
                                                                var msg = answer.ToArray();
                                                                stream.Write(msg, 0, msg.Length);
                                                            }
                                                        }
                                                        break;
                                                    case 16: // write several registers
                                                        answer = new List<byte>();
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header1)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(header2)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(6)));
                                                        answer.Add(nodeAddr);
                                                        answer.Add(funcCode);
                                                        answer.AddRange(BitConverter.GetBytes(Swap(startAddr)));
                                                        answer.AddRange(BitConverter.GetBytes(Swap(regCount)));
                                                        var bytesToWrite = bytes[12];
                                                        if (bytesToWrite != regCount * 2) break;
                                                        var n = 13;
                                                        for (var i = 0; i < regCount; i++)
                                                        {
                                                            var value = Convert.ToUInt16(bytes[n] * 256 + bytes[n + 1]);
                                                            EnsureModbusHR(nodeAddr, startAddr + i);
                                                            childName = String.Format("Node{0}.HR{1}", nodeAddr, startAddr + i);
                                                            while (!DictModbusItems.TryGetValue(childName, out modbusitem)) Thread.Sleep(10);
                                                            modbusHr = (ModbusHoldingRegister)modbusitem;
                                                            modbusHr.IntValue = BitConverter.ToInt16(BitConverter.GetBytes(value), 0);
                                                            while (!DictModbusItems.TryUpdate(childName, modbusHr, modbusHr)) Thread.Sleep(10);
                                                            n = n + 2;
                                                        }
                                                        lock (Mutelock)
                                                        {
                                                            if (!_mute)
                                                            {
                                                                list.Clear();
                                                                list.AddRange(answer.Select(t => String.Format("{0}", t)));
                                                                Say = "A:" + String.Join(",", list);
                                                                var msg = answer.ToArray();
                                                                stream.Write(msg, 0, msg.Length);
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                            // Shutdown and end connection
                                            clientData.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            //throw new Exception(ex.Message);
                                            if (!worker.CancellationPending) Say = "Ошибка: " + ex.Message;
                                        }
                                    });
                            }
                            catch (SocketException exception)
                            {
                                if (!worker.CancellationPending)
                                    Say = String.Format("Ошибка приёма: {0}", exception.Message);
                                break;
                            }
                        } while (!worker.CancellationPending);
                        listener.Stop();
                        Say = String.Format("Сокет TCP({0}) - остановка прослушивания.", tt.Port);
                    }

                    #endregion работа с TCP портом

                };
            ReopenPort();
        }

        private static ushort Swap(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            var buff = bytes[0];
            bytes[0] = bytes[1];
            bytes[1] = buff;
            return BitConverter.ToUInt16(bytes, 0);
        }

        private void LoadProperties()
        {
            var filename = Path.ChangeExtension(
                Application.ExecutablePath, ".ini");
            if (!File.Exists(filename)) return;
            var lines = File.ReadAllLines(filename);
            string section = "";
            foreach (var line in lines)
            {
                if (line.Trim().Length == 0) continue;
                if (line.Trim().StartsWith("[") && line.Trim().EndsWith("]"))
                    section = line.Trim(new[] {'[', ']'});
                else
                {
                    var arr = line.Split(new[] {'='});
                    if (arr.Length == 2)
                    {
                        var param = arr[0];
                        var value = arr[1];
                        switch (section)
                        {
                            case "Listening":
                                switch (param)
                                {
                                    case "PortName":
                                        cbPort.Text = value;
                                        break;
                                    case "BaudRate":
                                        dudBaudRate.Text = value;
                                        break;
                                    case "EthernetPort":
                                        int port;
                                        if (int.TryParse(value, out port))
                                            nudPort.Value = port;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void SaveProperties()
        {
            var lines = new List<string>
                {
                    "[Listening]",
                    "PortName=" + cbPort.Text,
                    "BaudRate=" + dudBaudRate.Text,
                    "EthernetPort=" + Convert.ToInt32(nudPort.Value).ToString("0"),
                    ""
                };
            File.WriteAllLines(Path.ChangeExtension(
                Application.ExecutablePath, ".ini"), lines);
        }

        private void cbPort_SelectionChangeCommitted(object sender, EventArgs e)
        {
            cbPort.SelectionChangeCommitted -= cbPort_SelectionChangeCommitted;
            dudBaudRate.Visible = cbPort.Text.StartsWith("COM");
            nudPort.Visible = !dudBaudRate.Visible;
            ReopenPort();
            cbPort.SelectionChangeCommitted += cbPort_SelectionChangeCommitted;
        }

        private void dudBaudRate_SelectedItemChanged(object sender, EventArgs e)
        {
            dudBaudRate.SelectedItemChanged -= dudBaudRate_SelectedItemChanged;
            ReopenPort();
            dudBaudRate.SelectedItemChanged += dudBaudRate_SelectedItemChanged;
        }

        private void nudPort_ValueChanged(object sender, EventArgs e)
        {
            nudPort.ValueChanged -= nudPort_ValueChanged;
            ReopenPort();
            nudPort.ValueChanged += nudPort_ValueChanged;
        }

        private void sport_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            var sp = (SerialPort) sender;
            switch (e.EventType)
            {
                case SerialError.Frame: // Оборудованием обнаружена ошибка кадрирования.
                    sp.DiscardInBuffer();
                    break;
                case SerialError.Overrun: // Переполнение буфера символов.
                    // Следующий символ потерян.
                    sp.DiscardInBuffer();
                    break;
                case SerialError.RXOver: // Переполнение входного буфера.
                    //  Во входном буфере нет места, или после символа
                    // конца файла (EOF) получен еще один символ.
                    sp.DiscardInBuffer();
                    break;
                case SerialError.RXParity: // Оборудованием обнаружена ошибка четности.
                    sp.DiscardInBuffer();
                    break;
                case SerialError.TXFull: // Приложение пытается передать символ, однако
                    //выходной буфер заполнен.
                    sp.DiscardOutBuffer();
                    break;
            }
            Say = "Ошибка порта: " + e.EventType;
        }

        //прием данных порта
        private void sport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sport = (SerialPort) sender;
            var count = sport.BytesToRead;
            if (count < 8) return;
            var bytes = new byte[count];
            if (sport.Read(bytes, 0, count) != count) return;
            var list = new List<string>();
            for (var i = 0; i < count; i++) list.Add(String.Format("{0}", bytes[i]));
            Say = "Q:" + String.Join(",", list);
            var nodeAddr = bytes[0];
            var funcCode = bytes[1];
            var startAddr = Convert.ToUInt16(bytes[2] * 256 + bytes[3]);
            var regCount = Convert.ToUInt16(bytes[4] * 256 + bytes[5]);
            var checkSum = Convert.ToUInt16(bytes[6] + bytes[7] * 256); // в КС байты не переставляются
            if (checkSum != Crc(bytes, count - 2))
            {
                Say = "Check summ error";
                return;
            }
            EnsureNode(nodeAddr);
            switch (funcCode)
            {
                case 3: // - holding registers
                case 4: // - input registers
                    var answer = new List<byte>();
                    var bytesCount = Convert.ToByte(regCount * 2);
                    answer.Add(nodeAddr);
                    answer.Add(funcCode);
                    answer.Add(bytesCount);
                    for (var addr = 0; addr < regCount; addr++)
                    {
                        if (funcCode == 3)
                            EnsureModbusHR(nodeAddr, startAddr + addr);
                        else
                            EnsureModbusAI(nodeAddr, startAddr + addr);
                        var childName = String.Format(funcCode == 3 ? "Node{0}.HR{1}" : "Node{0}.AI{1}", nodeAddr, startAddr + addr);
                        ModbusItem modbusitem;
                        while (!DictModbusItems.TryGetValue(childName, out modbusitem)) Thread.Sleep(10);
                        UInt16 value;
                        if (funcCode == 3)
                        {
                            var modbusHr = (ModbusHoldingRegister) modbusitem;
                            value = BitConverter.ToUInt16(BitConverter.GetBytes(modbusHr.IntValue), 0);
                        }
                        else
                        {
                            var modbusAi = (ModbusAnalogInput)modbusitem;
                            value = BitConverter.ToUInt16(BitConverter.GetBytes(modbusAi.IntValue), 0);
                        }
                        answer.AddRange(BitConverter.GetBytes(Swap(value)));
                    }
                    // добавить контрольную сумму
                    answer.AddRange(BitConverter.GetBytes(Crc(answer.ToArray(), answer.Count)));

                    lock (Mutelock)
                    {
                        if (!_mute)
                        {
                            list.Clear();
                            list.AddRange(answer.Select(t => String.Format("{0}", t)));
                            Say = "A:" + String.Join(",", list);
                            var msg = answer.ToArray();
                            if (sport.IsOpen)
                                sport.Write(msg, 0, msg.Length);
                        }
                    }
                    break;
            }            
            // очистка буфера при получении всех байтов или мусора
            if (sport.IsOpen) sport.DiscardInBuffer();
        }

        public static ushort Crc(IList<byte> buff, int len)
        {   // контрольная сумма MODBUS RTU
            ushort result = 0xFFFF;
            if (len <= buff.Count)
            {
                for (var i = 0; i < len; i++)
                {
                    result ^= buff[i];
                    for (var j = 0; j < 8; j++)
                    {
                        var flag = (result & 0x0001) > 0;
                        result >>= 1;
                        if (flag) result ^= 0xA001;
                    }
                }
            }
            return result;
        }

        private void cbMute_CheckedChanged(object sender, EventArgs e)
        {
            lock (Mutelock)
            {
                _mute = cbMute.Checked;
            }
        }

        private void EnsureNode(byte nodeAddr)
        {
            var method = new MethodInvoker(() =>
                {
                    var nodeName = "Node" + nodeAddr;
                    var nodeText = "Контроллер " + nodeAddr.ToString("D2");
                    var nodes = tvTree.Nodes.Find(nodeName, false);
                    if (nodes.Length != 0) return;
                    var childName = String.Format("Node{0}", nodeAddr);
                    DictModbusItems.TryAdd(childName, new ModbusNode {Key = childName});
                    var node = new TreeNode
                        {
                            Name = nodeName,
                            Text = nodeText,
                            Tag = DictModbusItems
                        };
                    tvTree.BeginUpdate();
                    try
                    {
                        tvTree.Nodes.Add(node);
                        tvTree.Sort();
                    }
                    finally
                    {
                        tvTree.EndUpdate();
                    }
                });
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method();
        }

        private void EnsureModbusHR(byte nodeAddr, int addr)
        {
            EnsureNode(nodeAddr);
            var method = new MethodInvoker(() =>
                {
                    var nodeName = "Node" + nodeAddr;
                    var childName = String.Format("Node{0}.HR{1}", nodeAddr, addr);
                    var childText = String.Format("4{0:D4} ({1})", addr + 1, addr.ToString("X2"));
                    var nodes = tvTree.Nodes.Find(nodeName, false);
                    if (nodes.Length == 0) return;
                    if (nodes[0].Nodes.Find(childName, false).Length > 0) return;
                    DictModbusItems.TryAdd(childName, new ModbusHoldingRegister { Key = childName });
                    var node = new TreeNode
                        {
                            Name = childName,
                            Text = childText,
                            Tag = DictModbusItems
                        };
                    tvTree.BeginUpdate();
                    try
                    {
                        nodes[0].Nodes.Add(node);
                        tvTree.Sort();
                    }
                    finally
                    {
                        tvTree.EndUpdate();
                    }
                });
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method();
        }

        private void EnsureModbusAI(byte nodeAddr, int addr)
        {
            EnsureNode(nodeAddr);
            var method = new MethodInvoker(() =>
            {
                var nodeName = "Node" + nodeAddr;
                var childName = String.Format("Node{0}.AI{1}", nodeAddr, addr);
                var childText = String.Format("3{0:D4} ({1})",addr + 1, addr.ToString("X2"));
                var nodes = tvTree.Nodes.Find(nodeName, false);
                if (nodes.Length == 0) return;
                if (nodes[0].Nodes.Find(childName, false).Length > 0) return;
                DictModbusItems.TryAdd(childName, new ModbusAnalogInput { Key = childName });
                var node = new TreeNode
                {
                    Name = childName,
                    Text = childText,
                    Tag = DictModbusItems
                };
                tvTree.BeginUpdate();
                try
                {
                    nodes[0].Nodes.Add(node);
                    tvTree.Sort();
                }
                finally
                {
                    tvTree.EndUpdate();
                }
            });
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method();
        }

        private string Say
        {
            set
            {
                const int maxlines = 11;
                var method = new MethodInvoker(() =>
                    {
                        lbMessages.BeginUpdate();
                        try
                        {
                            lbMessages.Items.Add(String.Format("{0} : {1}", DateTime.Now.ToString("HH:mm:ss.fff"), value));
                            if (lbMessages.Items.Count > maxlines)
                                lbMessages.Items.RemoveAt(0);
                        }
                        finally
                        {
                            lbMessages.EndUpdate();
                        }
                    });
                if (InvokeRequired)
                    BeginInvoke(method);
                else
                    method();
            }
        }

        private void ReopenPort()
        {
            _worker.CancelAsync();
            while (_worker.IsBusy) Application.DoEvents();
            var portName = cbPort.Text;
            if (new List<string>(SerialPort.GetPortNames()).Contains(portName))
            {
                var porttuning = new PortTuning
                    {
                        PortName = portName,
                        BaudRate = 115200
                    };
                int br;
                if (int.TryParse(dudBaudRate.Text, out br))
                    porttuning.BaudRate = br;
                else
                    dudBaudRate.Text = @"115200";
                lbPortTuned.Text = String.Format("{0}, {1}",
                                                 porttuning.PortName, porttuning.BaudRate);
                _worker.RunWorkerAsync(porttuning);
            }
            else
                switch (portName)
                {
                    case "TCP":
                        {
                            var tcptuning = new TcpTuning
                                {
                                    Port = Convert.ToInt32(nudPort.Value)
                                };
                            lbPortTuned.Text = String.Format("TCP, порт: {0}",
                                                             tcptuning.Port);
                            _worker.RunWorkerAsync(tcptuning);
                        }
                        break;
                    case "UDP":
                        {
                            var udptuning = new UdpTuning
                                {
                                    Port = Convert.ToInt32(nudPort.Value)
                                };
                            lbPortTuned.Text = String.Format("UDP, порт: {0}",
                                                             udptuning.Port);
                            _worker.RunWorkerAsync(udptuning);
                        }
                        break;
                }
        }

        private void tvTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == DictModbusItems)
            {
                ModbusItem fablitem;
                if (DictModbusItems.TryGetValue(e.Node.Name, out fablitem))
                    pgProps.SelectedObject = fablitem;
            }
            else
                pgProps.SelectedObject = null;
        }

        private void pgProps_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pgProps.Refresh();
        }

        private void tvTree_MouseDown(object sender, MouseEventArgs e)
        {
            var node = tvTree.GetNodeAt(e.Location);
            tvTree.SelectedNode = node;
            if (node == null)
                pgProps.SelectedObject = null;
        }

        private void EmulatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _worker.CancelAsync();
            while (_worker.IsBusy) Application.DoEvents();
            SaveProperties();
        }

        private void tsmiClear_Click(object sender, EventArgs e)
        {
            if (tvTree.Nodes.Count > 0 &&
                MessageBox.Show(@"Удалить текущие накопленные данные?",
                                @"Очистка текущих накопленных данных",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                //DictFablItems.Clear();
                tvTree.Nodes.Clear();
                Say = "Текущие накопленные данные удалены.";
            }
        }

        private void tsmiLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            Cursor = Cursors.WaitCursor;
            try
            {
                LoadFile(openFileDialog1.FileName);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadFile(string filename)
        {
            if (!File.Exists(filename)) return;
            pgProps.SelectedObject = null;
            if (tvTree.Nodes.Count > 0 &&
                MessageBox.Show(@"Удалить текущие накопленные данные?",
                                @"Загрузка ранее сохранённых данных",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                //DictFablItems.Clear();
                tvTree.Nodes.Clear();
                Say = "Текущие накопленные данные удалены.";
            }
            var lines = File.ReadAllLines(filename);
            var coll = new NameValueCollection();
            foreach (var line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    coll.Clear();
                    var key = line.Trim(new[] {'[', ']'});
                    var names = key.Split(new[] {'.'});
                    var node = names[0];
                    byte inode;
                    if (node.StartsWith("Node") &&
                        byte.TryParse(node.Substring(4), out inode))
                    {
                        //EnsureNode(inode);
                        switch (names.Length)
                        {
                            case 3:
                                {
                                    var alg = names[1];
                                    ushort ialg;
                                    if (alg.StartsWith("Alg") &&
                                        ushort.TryParse(alg.Substring(3), out ialg))
                                    {
                                        //EnsureAlgo(inode, ialg);
                                        var name = names[2];
                                        byte ipar, iout;
                                        if (name.StartsWith("Par") &&
                                            byte.TryParse(name.Substring(3), out ipar))
                                        {
                                            //EnsureAlgoParam(inode, ialg, ipar);
                                            coll.Add("Key", String.Format("Node{0}.Alg{1}.Par{2}", inode, ialg, ipar));
                                        }
                                        else if (name.StartsWith("Out") &&
                                                 byte.TryParse(name.Substring(3), out iout))
                                        {
                                            //EnsureAlgoOut(inode, ialg, iout);
                                            coll.Add("Key", String.Format("Node{0}.Alg{1}.Out{2}", inode, ialg, iout));
                                        }
                                    }
                                }
                                break;
                            case 2:
                                var kontur = names[1];
                                var sinr = names[1];
                                byte ikontur;
                                byte inr;
                                if (kontur.StartsWith("Kontur") &&
                                    byte.TryParse(kontur.Substring(6), out ikontur))
                                {
                                    //EnsureKontur(inode, ikontur);
                                    coll.Add("Key", String.Format("Node{0}.Kontur{1}", inode, ikontur));
                                    break;
                                }
                                if (sinr.StartsWith("INR") &&
                                    byte.TryParse(sinr.Substring(3), out inr))
                                {
                                    //EnsureInr(inode, inr);
                                    coll.Add("Key", String.Format("Node{0}.INR{1}", inode, inr));
                                    break;
                                }
                                if (sinr.StartsWith("KD") &&
                                    byte.TryParse(sinr.Substring(2), out inr))
                                {
                                    //EnsureKd(inode, inr);
                                    coll.Add("Key", String.Format("Node{0}.KD{1}", inode, inr));
                                    break;
                                }
                                Say = names[1];
                                break;
                            case 1:
                                //EnsureNode(inode);
                                coll.Add("Key", String.Format("Node{0}", inode));
                                break;
                        }
                    }
                }
                else if (line.Trim().Length > 0)
                {
                    var values = line.Split(new[] {'='});
                    coll.Add(values[0], values.Length == 2 ? values[1] : "");
                }
                else if (coll.Count > 1)
                {
                    var key = coll["Key"] ?? "";
                    //FablItem fablitem;
                    //if (DictFablItems.TryGetValue(key, out fablitem))
                    //    fablitem.LoadProperties(coll);
                }
            }
            Say = "Ранее накопленный данные загружены.";
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            SaveFile(Path.ChangeExtension(Application.ExecutablePath, ".tree"));
        }

        private void SaveFile(string filename)
        {
            var name = filename;
            var lines = new List<string>();
            //foreach (var key in DictFablItems.Keys)
            //{
            //    lines.Add(String.Format("[{0}]", key));
            //    FablItem fablitem;
            //    if (!DictFablItems.TryGetValue(key, out fablitem)) continue;
            //    var coll = new NameValueCollection();
            //    fablitem.SaveProperties(coll);
            //    lines.AddRange(coll.AllKeys
            //                       .Select(key1 => String.Format("{0}={1}", key1, coll[key1])));
            //    lines.Add("");
            //}
            File.WriteAllLines(name, lines.ToArray(), System.Text.Encoding.UTF8);
            Say = "Текущие накопленные данные сохранены.";
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var node = tvTree.SelectedNode;
            tsmiDeleteSplitter.Visible = tsmiDelete.Visible = node != null;
        }

        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            var node = tvTree.SelectedNode;
            if (node == null) return;
            //FablItem fablitem;
            //if (DictFablItems.TryRemove(node.Name, out fablitem))
            //    tvTree.Nodes.Remove(node);
        }
    }
}