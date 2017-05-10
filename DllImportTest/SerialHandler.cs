using System;
using System.IO.Ports;
using System.Threading;

namespace OmronOkaoSTBLib
{
    public class SerialHandler
    {
        //DataReceived event didnt work in Unity.
        public delegate int DataPushedEventHandler(byte[] dataStream);
        public delegate void DataReceivingTeminatedEvnetHandler();

        public delegate void OnSerialThreadingStoppedEventHandler();

        public delegate void OnPreparedEventHandler();

        public event DataPushedEventHandler OnDataPush;
        public event OnSerialThreadingStoppedEventHandler OnSerialThreadingStopped;

        public event OnPreparedEventHandler OnPrepared;

        public string PortName;
        public enum BSPState
        {
            S9600, S38400, S115200, S230400, S460800, S921600
        }
        public BSPState BaudRateState;

        private static SerialPort _serialPort;


        public bool IsRunning = false;
        public bool IsInvoked = false;

        private bool _continuousMode = false;

        // Use this for initialization
        static void Main(string[] args)
        {
            Console.Write("System has started.\r\nPlease input serial port name:");
            string portName = Console.ReadLine();
            
            SerialPortOpen(portName);

            while (true) { }
        }

//        public string

        public SerialHandler(string portName)
        {
            SerialPortOpen(portName);
        }

        //MEMO: http://www.alanzucconi.com/2016/12/01/asynchronous-serial-communication/
        public static void SerialPortOpen(string portName, BSPState baudRate = BSPState.S921600)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = (int) baudRate,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReadTimeout = 2000,
                ReadBufferSize = 80000,
                WriteBufferSize = 50,
            };

            _serialPort.Open();
            _serialPort.BaseStream.Flush();
            Console.WriteLine("Serial port {0}:{1} has been opened.", portName, baudRate);
        }

        public void SerialPortClose()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.BaseStream.Flush();
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        public Thread CreateSerialThread()
        {
            Thread readingThread = new Thread(ReadDataStream);

            return readingThread;
        }

        private void ReadDataStream()
        {
            int count = 0;
            byte[] recvBuffer = new byte[4096];

            while (true)
            {
                try
                {
                    Console.WriteLine("Start to read data stream.");

                    if (count < 4096)
                        recvBuffer[count] = (byte) _serialPort.ReadByte();
                    else
                    {
                        int resCode = OnDataPush.Invoke(recvBuffer);
                        if (resCode != 0) break;
                        count = 0;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Timeout!");
                    break;
                }
                catch (SystemException e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }

            _serialPort.BaseStream.Flush();
            OnSerialThreadingStopped.Invoke();
            if (_continuousMode)
            {
                OnPrepared.BeginInvoke(null, null);
            }
        }

        public void SendMessage(byte[] byteBuffer)
        {
            _serialPort.Write(byteBuffer, 0, byteBuffer.Length);
        }
    }

}
