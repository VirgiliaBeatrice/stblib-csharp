using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmronOkaoSTBLib
{
    public class OmronManager
    {
        private static int _imageWidth;
        private static int _imageHeight;

        private static byte[] _dataPool = new byte[80000];
        private static int _receivedDataCount;

        private static readonly byte[] OmronCmdHex =
        {
            0x00, 0x01, 0x02, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,
            0x0e, 0x10, 0x11, 0x12, 0x13, 0x15, 0x20, 0x21, 0x22, 0x30
        };

        private static readonly short[] OmronCmdSendDataLength =
        {
            0x0000, 0x0001, 0x0000, 0x0003, 0x0008, 0x0000, 0x000C, 0x0000,
            0x0002, 0x0000, 0x0001, 0x0003, 0x0003, 0x0002, 0x0000, 0x0002,
            0x0000, 0x0004, 0x0000, 0x0000
        };

        public enum OmronCmd : byte
        {
            Version = 0, SetCamDir, GetCamDir, Exec, SetThreshold, GetThreshold, SetSize, GetSize,
            SetFaceAngel, GetFaceAngle, SetUartSpeed, RegisterFace, DeleteFace, DeleteUserFace,
            DeleteAllFace, ReadUserFaceInfo, SaveAlbum, ReadAlbum, WriteAlbum, FormateRom
        }
        public static OmronCmd SelectedCmd;

        public enum CameraDirection : byte
        {
            Deg0 = 0, Deg90, Deg180, Deg270
        }
        private static CameraDirection _selectedDirection = CameraDirection.Deg0;

        private static short BodyThreshold = 500;
        private static short HandThreshold = 500;
        private static short FaceThreshold = 500;
        private static short FaceRecogThreshold = 500;

        private static short BodySizeMin = 30;
        private static short BodySizeMax = 8192;
        private static short HandSizeMin = 40;
        private static short HandSizeMax = 8192;
        private static short FaceSizeMin = 64;
        private static short FaceSizeMax = 8192;

        public enum FaceDirYaw : byte
        {
            Deg30 = 0x00, Deg60, Deg90
        }
        public enum FaceDirRoll : byte
        {
            Deg15 = 0x00, Deg45
        }
        private static FaceDirYaw SelectedFaceDirYaw = FaceDirYaw.Deg30;
        private static FaceDirRoll SelectedFaceDirRoll = FaceDirRoll.Deg15;

        public enum UartBPS : byte
        {
            S9600 = 0, S38400, S115200, S230400, S460800, S92600
        }
        private static UartBPS SelectedBps = UartBPS.S9600;

        private static short RegisterUserId;
        private static byte RegisterDataId;

        private static short DeleteUserId;
        private static byte DeleterDataId;

        private static short DeleteUserId1;

        private static short ReadUserId;

        [Flags]
        public enum ExecSetting1 : byte
        {
            None = 0x00, Body = 0x01, Hand = 0x02, Face = 0x04, FaceDir = 0x08,
            Age = 0x10, Gender = 0x20, Gaze = 0x40, Blink = 0x80, All = 0xFF
        }

        private static bool IsBodyChecked = false;
        private static bool IsHandChecked = false;
        private static bool IsFaceChecked = true;
        private static bool IsFaceDirChecked = true;
        private static bool IsAgeChecked = false;
        private static bool IsGenderChecked = false;
        private static bool IsGazeChecked = true;
        private static bool IsBlinkChecked = false;

        [Flags]
        public enum ExecSetting2 : byte
        {
            None = 0x00, Emotion = 0x01, FaceRecog = 0x02
        }

        private static bool IsEmotionChecked = false;
        private static bool IsFaceRecogChecked = false;

        public enum ExecImageSetting : byte
        {
            NoImage = 0x00, Qvga = 0x01, HalfQvga = 0x02
        }
        private static ExecImageSetting ImageSetting;

        private static byte[] _currSettings = new byte[3];

        private static bool _isImageReceived;
        private static bool _isFaceDirCreated;

        [Flags]
        private enum CreateEventFlag : ushort
        {
            IsBodyCreated = 0x0001, IsHandCreated = 0x0002, IsFaceCreated = 0x0004, IsFaceDirCreated = 0x0008,
            IsAgeCreated = 0x0010, IsGenderCreated = 0x0020, IsGazeCreated = 0x0040, IsBlinkCreated = 0x0080,
            IsEmotionCreated = 0x0100, IsFaceRecogCreated = 0x0200, IsImageCreated = 0x0400
        }

        private static CreateEventFlag _omronCameraCreateEventFlag;

        private static bool _isOmronCameraCreated = false;
        private static bool _isOnCreate = false;

        private static bool ContinuousMode;
        private static byte[] _prevCmdSequence;

        public float ConfigurationDistance = 1.0f;
        private const float WideAngleHeightRange = 76.0f;
        private const float WideAngleWidthRange = 94.0f;

        public double ValidRangeHeight
        {
            get { return Math.Tan(WideAngleHeightRange / 2.0f) * ConfigurationDistance * 2.0f; }
        }
        public double ValidRangeWidth
        {
            get { return Math.Tan(WideAngleWidthRange / 2.0f) * ConfigurationDistance * 2.0f; }
        }
        public double ValidScaleHeight
        {
            get { return ValidRangeHeight / 1600.0f; }
        }
        public double ValidScaleWidth
        {
            get { return ValidRangeWidth / 1200.0f; }
        }

        private static OmronCamera _camera1;

        private static bool _nextFlag = false;

        private static void Main(string[] args)
        {
            Console.Write("System has started.\r\nPlease input serial port name:");
            string portName = Console.ReadLine();

            SerialHandler.CreateSerialHandler(portName);

            SerialHandler.OnDataPush += SerialHandlerOnDataPush;
            SerialHandler.OnSerialThreadingStopped += SerialHandlerOnSerialThreadingStopped;
            //SerialHandlerComponent.OnPrepared += SerialHandlerComponentOnOnPrepared;
            IntPtr stbHandle = STBLib.STBCreateHandle((uint)STBExecFlag.FaceTracking);

            while (true)
            {
                //                SendCommand(OmronCmd.Version);
                SendCommand(OmronCmd.Exec);
                while (!_nextFlag) { }
                _nextFlag = false;
                STBFrameResult result = new STBFrameResult();
                Console.WriteLine("Set Result: {0}", STBLib.STBSetFrameResult(stbHandle, ref _camera1.FrameResult));
                Console.WriteLine("Excute: {0}", STBLib.STBExecute(stbHandle));
                uint nCount = 0;
                STBFace[] faces = new STBFace[35];
                Console.WriteLine("Get Face Cmd: {0}", STBLib.STBGetFaces(stbHandle, ref nCount, faces));
                Console.WriteLine("Stablized Face Count: {0}", nCount);
            }
            //Create Handle
            //Set parameters

            //Set frame result
            
            //STBSetFrameResult(stbHandle, )

        }


        //        private static void SerialHandlerComponentOnOnPrepared()
        //        {
        //            //        Console.WriteLine("New command request.");
        //            SendCommand();
        //        }

        private static void SerialHandlerOnSerialThreadingStopped()
        {
            _dataPool = new byte[80000];
            _receivedDataCount = 0;

            _nextFlag = true;
            //        Console.WriteLine("Clear data pool.");
        }

        private static int SerialHandlerOnDataPush(byte[] dataStream)
        {
            Array.Copy(dataStream, 0, _dataPool, _receivedDataCount, dataStream.Length);
            _receivedDataCount += dataStream.Length;

            int resCode = ResponseParser(SelectedCmd, _dataPool.Take(_receivedDataCount).ToArray());

            switch (resCode)
            {
                case -1:
                    //TODO: fire an event of OnTerminate. (Replace OnFinished.)
                    Console.WriteLine("Invalid data received.\nTerminated!");
                    return -1;
                case 0:
                    //                    Console.WriteLine("Received data package is incompleted.\nWaiting for the next.");
                    return 0;
                case 1:
                    Console.WriteLine("Completed!");
                    return 1;
                default:
                    return -1;
            }
        }


        private static void SendCommand(OmronCmd selectedCmd)
        {
            byte[] cmdSeq = CmdFormatter(selectedCmd, OmronCmdSendDataLength[(int)selectedCmd]);
            SelectedCmd = selectedCmd;

            SerialHandler.SendMessage(cmdSeq);
            SerialHandler.StartSerialReadingThread();
        }

        private static byte[] CmdFormatter(OmronCmd cmd, int dataLength)
        {
            byte[] tmpCmdBuffer = new byte[4 + dataLength];

            //Data format: 0xFE CMD LSB MSB
            tmpCmdBuffer[0] = 0xFE;
            tmpCmdBuffer[1] = OmronCmdHex[(int)cmd];
            tmpCmdBuffer[2] = (byte)(dataLength % 256);
            tmpCmdBuffer[3] = (byte)(dataLength / 256);

            //Console.WriteLine(tmpCmdBuffer.ToString());

            switch (cmd)
            {
                case OmronCmd.SetCamDir:
                    tmpCmdBuffer[4] = (byte)_selectedDirection;
                    break;
                case OmronCmd.Exec:
                    _currSettings = ExecCmdFormatter();
                    Array.Copy(_currSettings, 0, tmpCmdBuffer, 4, dataLength);
                    break;
                case OmronCmd.SetThreshold:
                    Array.Copy(SetThresholdCmdFormatter(), 0, tmpCmdBuffer, 4, dataLength);
                    break;
                case OmronCmd.SetSize:
                    Array.Copy(SetSizeCmdFormatter(), 0, tmpCmdBuffer, 4, dataLength);
                    break;
                case OmronCmd.SetFaceAngel:
                    tmpCmdBuffer[4] = (byte)SelectedFaceDirYaw;
                    tmpCmdBuffer[5] = (byte)SelectedFaceDirRoll;
                    break;
                case OmronCmd.SetUartSpeed:
                    tmpCmdBuffer[4] = (byte)SelectedBps;
                    break;
                case OmronCmd.RegisterFace:
                    tmpCmdBuffer[4] = (byte)(RegisterUserId % 256);
                    tmpCmdBuffer[5] = (byte)(RegisterUserId / 256);
                    tmpCmdBuffer[6] = RegisterDataId;
                    break;
                case OmronCmd.DeleteFace:
                    tmpCmdBuffer[4] = (byte)(DeleteUserId % 256);
                    tmpCmdBuffer[5] = (byte)(DeleteUserId / 256);
                    tmpCmdBuffer[6] = DeleterDataId;
                    break;
                case OmronCmd.DeleteUserFace:
                    tmpCmdBuffer[4] = (byte)(DeleteUserId1 % 256);
                    tmpCmdBuffer[5] = (byte)(DeleteUserId1 / 256);
                    break;
                case OmronCmd.ReadUserFaceInfo:
                    tmpCmdBuffer[4] = (byte)(ReadUserId % 256);
                    tmpCmdBuffer[5] = (byte)(ReadUserId / 256);
                    break;
                case OmronCmd.ReadAlbum:
                    //TODO: Read albums from host and write it into device.
                    break;
                default:
                    Console.WriteLine("Command only. No data sending.");
                    break;
            }

            Console.WriteLine("Omron Command: " + BitConverter.ToString(tmpCmdBuffer));

            return tmpCmdBuffer;
        }

        private static byte[] ExecCmdFormatter()
        {
            byte[] setting = new byte[3];

            if (IsBodyChecked) setting[0] |= (byte)ExecSetting1.Body;
            if (IsHandChecked) setting[0] |= (byte)ExecSetting1.Hand;
            if (IsFaceChecked) setting[0] |= (byte)ExecSetting1.Face;
            if (IsFaceDirChecked) setting[0] |= (byte)ExecSetting1.FaceDir;
            if (IsAgeChecked) setting[0] |= (byte)ExecSetting1.Age;
            if (IsGenderChecked) setting[0] |= (byte)ExecSetting1.Gender;
            if (IsGazeChecked) setting[0] |= (byte)ExecSetting1.Gaze;
            if (IsBlinkChecked) setting[0] |= (byte)ExecSetting1.Blink;
            //        Console.WriteLine("Execute setting 1: " + Convert.ToString(setting[0], 2));

            if (IsEmotionChecked) setting[1] |= (byte)ExecSetting2.Emotion;
            if (IsFaceRecogChecked) setting[1] |= (byte)ExecSetting2.FaceRecog;
            //        Console.WriteLine("Execute setting 2: " + Convert.ToString(setting[1], 2));

            setting[2] = (byte)ImageSetting;
            //        Console.WriteLine("Execute image setting: " + ImageSetting);

            switch (ImageSetting)
            {
                case ExecImageSetting.HalfQvga:
                    _imageWidth = 160;
                    _imageHeight = 120;
                    break;
                case ExecImageSetting.Qvga:
                    _imageWidth = 320;
                    _imageHeight = 240;
                    break;
                default:
                    _imageWidth = 0;
                    _imageHeight = 0;
                    break;
            }

            return setting;
        }

        private static byte[] SetThresholdCmdFormatter()
        {
            byte[] output = new byte[8];

            output[0] = (byte)(BodyThreshold % 256);
            output[1] = (byte)(BodyThreshold / 256);
            output[2] = (byte)(HandThreshold % 256);
            output[3] = (byte)(HandThreshold / 256);
            output[4] = (byte)(FaceThreshold % 256);
            output[5] = (byte)(FaceThreshold / 256);
            output[6] = (byte)(FaceRecogThreshold % 256);
            output[7] = (byte)(FaceRecogThreshold / 256);

            return output;
        }

        private static byte[] SetSizeCmdFormatter()
        {
            byte[] output = new byte[12];

            output[0] = (byte)(BodySizeMin % 256);
            output[1] = (byte)(BodySizeMin / 256);
            output[2] = (byte)(BodySizeMax % 256);
            output[3] = (byte)(BodySizeMax / 256);
            output[4] = (byte)(HandSizeMin % 256);
            output[5] = (byte)(HandSizeMin / 256);
            output[6] = (byte)(HandSizeMax % 256);
            output[7] = (byte)(HandSizeMax / 256);
            output[8] = (byte)(FaceSizeMin % 256);
            output[9] = (byte)(FaceSizeMin / 256);
            output[10] = (byte)(FaceSizeMax % 256);
            output[11] = (byte)(FaceSizeMax / 256);

            return output;
        }


        private static int ResponseParser(OmronCmd cmd, byte[] dataStream)
        {
            byte syncCode = dataStream[0];
            if (syncCode != 0xFE) return -1;

            byte resCode = dataStream[1];
            if (resCode != 0x00) return -1;

            int dataLength = BitConverter.ToInt32(dataStream, 2);
            if (dataStream.Length < dataLength + 6) return 0;

            byte[] validData = new byte[dataLength];
            Array.Copy(dataStream, 6, validData, 0, dataLength);

            switch (cmd)
            {
                case OmronCmd.Version:
                    CmdVersionHandler(validData);
                    break;
                case OmronCmd.SetCamDir:
                    break;
                case OmronCmd.GetCamDir:
                    CmdGetCamDirHandler(validData);
                    break;
                case OmronCmd.Exec:
                    CmdExecHandler(validData);
                    break;
                case OmronCmd.SetThreshold:
                    break;
                case OmronCmd.GetThreshold:
                    CmdGetThresholdHandler(validData);
                    break;
                case OmronCmd.SetSize:
                    break;
                case OmronCmd.GetSize:
                    CmdGetSizeHandler(validData);
                    break;
                case OmronCmd.SetFaceAngel:
                    break;
                case OmronCmd.GetFaceAngle:
                    CmdGetFaceAngleHandler(validData);
                    break;
                case OmronCmd.SetUartSpeed:
                    break;
                case OmronCmd.RegisterFace:
                    break;
                case OmronCmd.DeleteFace:
                    break;
                case OmronCmd.DeleteUserFace:
                    break;
                case OmronCmd.DeleteAllFace:
                    break;
                case OmronCmd.ReadUserFaceInfo:
                    break;
                case OmronCmd.SaveAlbum:
                    break;
                case OmronCmd.ReadAlbum:
                    break;
                case OmronCmd.WriteAlbum:
                    break;
                case OmronCmd.FormateRom:
                    break;
            }

            //        _receivedDataCount = 0;
            //        _dataPool = new byte[80000];
            return 1;
        }

        private static void CmdVersionHandler(byte[] data)
        {
            string versionInfo = Encoding.UTF8.GetString(data.Take(12).ToArray()) +
                                 data[12] + "." + data[13] + "." + data[14] + "." + BitConverter.ToUInt32(data, 15);

            Console.WriteLine("Version Info: " + versionInfo);
        }

        private static void CmdExecHandler(byte[] data)
        {
            //TODO: Still have some problems after changing the image size to zero.
            _isOnCreate = false;
            _camera1 = new OmronCamera(data, _currSettings);

            _isOnCreate = true;
        }

        private static void CmdGetCamDirHandler(byte[] data)
        {
            switch ((CameraDirection)data[0])
            {
                case CameraDirection.Deg0:
                    _selectedDirection = CameraDirection.Deg0;
                    break;
                case CameraDirection.Deg90:
                    _selectedDirection = CameraDirection.Deg90;
                    break;
                case CameraDirection.Deg180:
                    _selectedDirection = CameraDirection.Deg180;
                    break;
                case CameraDirection.Deg270:
                    _selectedDirection = CameraDirection.Deg270;
                    break;
            }

            Console.WriteLine("Current direction setting: {0}", _selectedDirection);
        }

        private static void CmdGetThresholdHandler(byte[] data)
        {
            BodyThreshold = BitConverter.ToInt16(data, 0);
            HandThreshold = BitConverter.ToInt16(data, 2);
            FaceThreshold = BitConverter.ToInt16(data, 4);
            FaceRecogThreshold = BitConverter.ToInt16(data, 6);

            Console.WriteLine("Current Threshold Value - Body: {0}, Hand: {1}, Face: {2}, FaceRecog: {3}"
                , BodyThreshold, HandThreshold, FaceThreshold, FaceRecogThreshold);
        }

        private static void CmdGetSizeHandler(byte[] data)
        {
            BodySizeMin = BitConverter.ToInt16(data, 0);
            BodySizeMax = BitConverter.ToInt16(data, 2);
            HandSizeMin = BitConverter.ToInt16(data, 4);
            HandSizeMax = BitConverter.ToInt16(data, 6);
            FaceSizeMin = BitConverter.ToInt16(data, 8);
            FaceSizeMax = BitConverter.ToInt16(data, 10);

            Console.WriteLine("Current Size Value - Body Min.: {0}, Body Max.: {1}, Hand Min.: {2}, Hand Max.: {3}, Face Min.: {4}, Face Max.: {5}"
                , BodySizeMin, BodySizeMax, HandSizeMin, HandSizeMax, FaceSizeMin, FaceSizeMax);
        }

        private static void CmdGetFaceAngleHandler(byte[] data)
        {
            SelectedFaceDirYaw = (FaceDirYaw)data[0];
            SelectedFaceDirRoll = (FaceDirRoll)data[1];

            Console.WriteLine("Face Direction Setting - Yaw: {0}, Roll: {1}", SelectedFaceDirYaw, SelectedFaceDirRoll);
        }
    }
}
