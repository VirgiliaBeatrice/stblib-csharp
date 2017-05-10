using System;
using System.IO.Ports;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;

namespace OmronOkaoSTBLib
{
    public class SerialHandler
    {
        //DataReceived event didnt work in Unity.
        public delegate int OnDataPushEventHandler(byte[] dataStream);
        public delegate void OnDataReceivingTeminatedEvnetHandler();
        public delegate void OnPreparedEventHandler();
        public delegate void OnSerialThreadingStoppedEventHandler();

        public static event OnPreparedEventHandler OnPrepared;
        public static event OnDataPushEventHandler OnDataPush;
        public static event OnSerialThreadingStoppedEventHandler OnSerialThreadingStopped;

        public enum BSPState
        {
            S9600, S38400, S115200, S230400, S460800, S921600
        }

        private static SerialPort _serialPort;
        private static Thread _readingThread;
        private bool _continuousMode = false;


        public static void CreateSerialHandler(string portName, BSPState bauState = BSPState.S921600)
        {
            SerialPortOpen(portName, bauState);
        }

        public static void DeleteSerialHandler()
        {
            
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

        public static void SerialPortClose()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.BaseStream.Flush();
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        public static void StartSerialReadingThread()
        {
            _readingThread = new Thread(ReadDataStream);
            _readingThread.Start();
        }

        private static void ReadDataStream()
        {
            int count = 0;
            int countMax = 6;
            byte[] recvBuffer = new byte[6];

            while (true)
            {
                try
                {
                    if (count < countMax)
                    {
                        recvBuffer[count] = (byte) _serialPort.ReadByte();
                        count++;
                    }
                    else
                    {
                        Console.WriteLine("Start to push data stream.");
                        int resCode = OnDataPush.Invoke(recvBuffer);

                        if (resCode == 0)
                        {
                            countMax = BitConverter.ToInt32(recvBuffer, 2);
                            recvBuffer = new byte[countMax];
                        }
                        else
                            break;

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
//            if (_continuousMode)
//            {
//                OnPrepared.BeginInvoke(null, null);
//            }
        }

        public static void SendMessage(byte[] byteBuffer)
        {
            _serialPort.Write(byteBuffer, 0, byteBuffer.Length);
//            StartSerialReadingThread();
        }

        // Main Entry for Test
        static void Main(string[] args)
        {
            Console.Write("System has started.\r\nPlease input serial port name:");
            string portName = Console.ReadLine();

            SerialPortOpen(portName);

            while (true) { }
        }
    }
}
