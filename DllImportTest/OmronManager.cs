using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmronOkaoSTBLib
{
    public class OmronManager
    {
        private byte[] _rawData;
        private Color32[] _rawRGBColor;
        private int _imageWidth;
        private int _imageHeight;

        private byte[] _dataPool = new byte[80000];
        private int _receivedDataCount;

        private readonly byte[] _omronCmdHex =
        {
            0x00, 0x01, 0x02, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,
            0x0e, 0x10, 0x11, 0x12, 0x13, 0x15, 0x20, 0x21, 0x22, 0x30
        };

        private readonly short[] _omronCmdSendDataLength =
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
        public OmronCmd SelectedCmd;

        public enum CameraDirection : byte
        {
            Deg0 = 0, Deg90, Deg180, Deg270
        }
        public CameraDirection SelectedDirection;

        public short BodyThreshold = 500;
        public short HandThreshold = 500;
        public short FaceThreshold = 500;
        public short FaceRecogThreshold = 500;

        public short BodySizeMin = 30;
        public short BodySizeMax = 8192;
        public short HandSizeMin = 40;
        public short HandSizeMax = 8192;
        public short FaceSizeMin = 64;
        public short FaceSizeMax = 8192;

        public enum FaceDirYaw : byte
        {
            Deg30 = 0x00, Deg60, Deg90
        }
        public enum FaceDirRoll : byte
        {
            Deg15 = 0x00, Deg45
        }
        public FaceDirYaw SelectedFaceDirYaw = FaceDirYaw.Deg30;
        public FaceDirRoll SelectedFaceDirRoll = FaceDirRoll.Deg15;

        public enum UartBPS : byte
        {
            S9600 = 0, S38400, S115200, S230400, S460800, S92600
        }
        public UartBPS SelectedBPS = UartBPS.S9600;

        public short RegisterUserID;
        public byte RegisterDataID;

        public short DeleteUserID;
        public byte DeleterDataID;

        public short DeleteUserID1;

        public short ReadUserID;

        [Flags]
        public enum ExecSetting1 : byte
        {
            None = 0x00, Body = 0x01, Hand = 0x02, Face = 0x04, FaceDir = 0x08,
            Age = 0x10, Gender = 0x20, Gaze = 0x40, Blink = 0x80, All = 0xFF
        }

        public bool IsBodyChecked = false;
        public bool IsHandChecked = false;
        public bool IsFaceChecked = false;
        public bool IsFaceDirChecked = false;
        public bool IsAgeChecked = false;
        public bool IsGenderChecked = false;
        public bool IsGazeChecked = false;
        public bool IsBlinkChecked = false;

        [Flags]
        public enum ExecSetting2 : byte
        {
            None = 0x00, Emotion = 0x01, FaceRecog = 0x02
        }

        public bool IsEmotionChecked = false;
        public bool IsFaceRecogChecked = false;

        public enum ExecImageSetting : byte
        {
            NoImage = 0x00, QVGA = 0x01, HalfQVGA = 0x02
        }
        public ExecImageSetting ImageSetting;

        private byte[] _currSettings = new byte[3];

        private bool _isImageReceived;
        private bool _isFaceDirCreated;

        [Flags]
        private enum CreateEventFlag : ushort
        {
            IsBodyCreated = 0x0001, IsHandCreated = 0x0002, IsFaceCreated = 0x0004, IsFaceDirCreated = 0x0008,
            IsAgeCreated = 0x0010, IsGenderCreated = 0x0020, IsGazeCreated = 0x0040, IsBlinkCreated = 0x0080,
            IsEmotionCreated = 0x0100, IsFaceRecogCreated = 0x0200, IsImageCreated = 0x0400
        }

        private CreateEventFlag _omronCameraCreateEventFlag;

        private bool _isOmronCameraCreated = false;
        private bool _isOnCreate = false;

        public bool ContinuousMode;
        private byte[] _prevCmdSequence;

        public float ConfigurationDistance = 1.0f;
        private const float WideAngleHeightRange = 76.0f;
        private const float WideAngleWidthRange = 94.0f;

        public double _validRangeHeight
        {
            get { return Math.Tan(WideAngleHeightRange / 2.0f) * ConfigurationDistance * 2.0f; }
        }
        public double _validRangeWidth
        {
            get { return Math.Tan(WideAngleWidthRange / 2.0f) * ConfigurationDistance * 2.0f; }
        }
        public double _validScaleHeight
        {
            get { return _validRangeHeight / 1600.0f; }
        }
        public double _validScaleWidth
        {
            get { return _validRangeWidth / 1200.0f; }
        }

        static void Main(string[] args)
        {
            SerialHandler serialPort = new SerialHandler();
        }

        // Use this for initialization
        void Start()
        {
            _canvas = GameObject.Find("Canvas");
            _omronCamera = GameObject.Find("OmronCamera");

            _bodyGameObjects = new List<GameObject>();
            _handGameObjects = new List<GameObject>();
            _faceGameObjects = new List<GameObject>();

            SerialHandlerComponent.OnDataPush += SerialHandlerComponentOnDataPush;
            SerialHandlerComponent.OnSerialThreadingStopped += SerialHandlerComponentOnOnSerialThreadingStopped;
            SerialHandlerComponent.OnPrepared += SerialHandlerComponentOnOnPrepared;
        }

        private void SerialHandlerComponentOnOnPrepared()
        {
            //        Debug.LogFormat("New command request.");
            SendCommand();
        }

        private void SerialHandlerComponentOnOnSerialThreadingStopped()
        {
            _dataPool = new byte[80000];
            _receivedDataCount = 0;
            //        Debug.LogFormat("Clear data pool.");
        }

        private int SerialHandlerComponentOnDataPush(byte[] dataStream)
        {
            //        Debug.Log("DataPushed event has been invoked.");

            Array.Copy(dataStream, 0, _dataPool, _receivedDataCount, dataStream.Length);
            _receivedDataCount += dataStream.Length;

            if (_receivedDataCount > 6)
            {
                //            Debug.LogFormat("Total Counts: {0}", _receivedDataCount);
                int resCode = ResponseParser(SelectedCmd, _dataPool.Take(_receivedDataCount).ToArray());

                switch (resCode)
                {
                    case -1:
                        //TODO: fire an event of OnTerminate. (Replace OnFinished.)
                        Debug.Log("Invalid data received.\nTerminated!");
                        return -1;
                    case 0:
                        //                    Debug.Log("Received data package is incompleted.\nWaiting for the next.");
                        return 0;
                    case 1:
                        Debug.LogFormat("Completed!");
                        return 1;
                }
            }

            return 0;
        }

        // Update is called once per frame
        void Update()
        {
            //TODO: Game object creating function
            if (_isOnCreate && !_isOmronCameraCreated)
            {
                //            Debug.LogFormat("Omron Camera Objects are creating.");

                foreach (var body in Camera1.Bodies)
                {
                    GameObject tmpGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tmpGameObject.name = string.Format("Body[{0}]", Guid.NewGuid());
                    tmpGameObject.transform.position = new Vector3(body.CoordinateX, body.CoordinateY, ConfigurationDistance);

                    _bodyGameObjects.Add(tmpGameObject);
                }

                foreach (var hand in Camera1.Hands)
                {
                    GameObject tmpGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tmpGameObject.name = string.Format("Hand[{0}]", Guid.NewGuid());
                    tmpGameObject.transform.position = new Vector3(hand.CoordinateX, hand.CoordinateY, ConfigurationDistance);

                    _handGameObjects.Add(tmpGameObject);
                }

                //TODO: Adjust the Quaternion to Unity coordinate.
                foreach (var face in Camera1.Faces)
                {
                    GameObject tmpGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    string id = string.Format("[{0}]", Guid.NewGuid());

                    tmpGameObject.name = string.Format("Face{0}", id);
                    //                tmpGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    //                tmpGameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    tmpGameObject.transform.parent = GameObject.Find("OmronCamera").transform;
                    tmpGameObject.transform.localPosition = new Vector3(face.LocalPosition.x * (float)_validScaleWidth
                        , face.LocalPosition.y * (float)_validScaleHeight, -ConfigurationDistance);
                    tmpGameObject.transform.localRotation = face.FaceDir.FaceDirRotation;
                    var boxCollider = tmpGameObject.GetComponent<BoxCollider>();
                    boxCollider.size = new Vector3(0.1f, 0.15f, 0.1f);
                    tmpGameObject.GetComponent<MeshRenderer>().enabled = false;

                    DrawLocalCoordinate(_omronCamera, tmpGameObject);
                    DrawGazeVector(_omronCamera, tmpGameObject, face.Gaze.GazeVector);
                    //TODO: Add other functions.

                    _faceGameObjects.Add(tmpGameObject);
                }

                if (Camera1.Image != null)
                    SetImageFormat(Camera1.Image);

                _isOmronCameraCreated = true;
                _isOnCreate = false;
            }

            else if (_isOnCreate && _isOmronCameraCreated)
            {
                //            Debug.LogFormat("Game Objects are deleting.");

                foreach (var face in _faceGameObjects)
                {
                    Destroy(face);
                    _faceGameObjects = new List<GameObject>();
                }

                if (_debugImageGameObject != null)
                    Destroy(_debugImageGameObject);

                _isOmronCameraCreated = false;
            }

        }

        private void SetImageFormat(OmronCameraImage image)
        {
            _debugImageGameObject = new GameObject("ImageForDebug");
            _debugImageGameObject.AddComponent<RawImage>();
            RawImage debugRawImage = _debugImageGameObject.GetComponent<RawImage>();

            _debugImageGameObject.transform.SetParent(_canvas.transform);
            debugRawImage.rectTransform.sizeDelta = new Vector2(1600.0f, 1200.0f);
            debugRawImage.rectTransform.transform.localPosition = Vector3.zero;
            debugRawImage.rectTransform.transform.localScale = Vector3.one * 0.4f;

            switch (SelectedDirection)
            {
                case CameraDirection.Deg0:
                    debugRawImage.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case CameraDirection.Deg90:
                    debugRawImage.transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case CameraDirection.Deg180:
                    debugRawImage.transform.rotation = Quaternion.Euler(0, 0, 180);
                    break;
                case CameraDirection.Deg270:
                    debugRawImage.transform.rotation = Quaternion.Euler(0, 0, 270);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _imageTexture = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);
            //        Debug.LogFormat("Image Texture: {0} * {1}", _imageTexture.width, _imageTexture.height);
            //        Debug.LogFormat("Image : {0}", image.ImageRGBColor32.Length);
            debugRawImage.texture = _imageTexture;

            _imageTexture.SetPixels32(image.ImageRGBColor32);
            _imageTexture.Apply();
        }

        private GameObject DrawLine(string lineName, Vector3 start, Vector3 end, Color color)
        {
            //        Debug.LogFormat("Draw a line.");
            GameObject myLine = new GameObject(lineName);
            myLine.transform.localPosition = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.useWorldSpace = false;

            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = 0.005f;
            lr.endWidth = 0.005f;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            return myLine;
        }

        private void DrawLocalCoordinate(GameObject targetGameObject, GameObject parentGameObject)
        {
            //        Debug.LogFormat("Draw a local coordinate for target object.");

            Color[] coordinateColors = { Color.red, Color.green, Color.blue };

            //        Debug.LogFormat("Parent Position: {0}", parentPos);
            //        Debug.LogFormat("Parent Rotation: {0}", parentRot);

            GameObject coorX = DrawLine("CoorX", new Vector3(0.0f, 0.0f, 0.0f), Vector3.right * 0.2f,
                coordinateColors[0]);
            GameObject coorY = DrawLine("CoorY", new Vector3(0.0f, 0.0f, 0.0f), Vector3.up * 0.2f,
                coordinateColors[1]);
            GameObject coorZ = DrawLine("CoorZ", new Vector3(0.0f, 0.0f, 0.0f), Vector3.forward * 0.2f,
                coordinateColors[2]);

            coorX.transform.parent = parentGameObject.transform;
            coorX.GetComponent<LineRenderer>().transform.localPosition = Vector3.zero;
            //        coorX.GetComponent<LineRenderer>().transform.localPosition = targetGameObject.transform.localPosition;
            coorX.GetComponent<LineRenderer>().transform.localRotation = Quaternion.Euler(Vector3.zero);

            coorY.transform.parent = parentGameObject.transform;
            coorY.GetComponent<LineRenderer>().transform.localPosition = Vector3.zero;
            //        coorY.GetComponent<LineRenderer>().transform.localPosition = targetGameObject.transform.localPosition;
            coorY.GetComponent<LineRenderer>().transform.localRotation = Quaternion.Euler(Vector3.zero);

            coorZ.transform.parent = parentGameObject.transform;
            coorZ.GetComponent<LineRenderer>().transform.localPosition = Vector3.zero;
            //        coorZ.GetComponent<LineRenderer>().transform.localPosition = targetGameObject.transform.localPosition;
            coorZ.GetComponent<LineRenderer>().transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        private void DrawGazeVector(GameObject targetGameObject, GameObject parentGameObject, Quaternion targetVector)
        {
            //        Debug.LogFormat("Draw an eye gaze vector for target object.");

            GameObject gazeVector = DrawLine("GazeVector", Vector3.zero, Vector3.forward * 2.0f, Color.yellow);

            gazeVector.transform.parent = parentGameObject.transform;
            gazeVector.GetComponent<LineRenderer>().transform.localPosition = Vector3.zero;
            //        gazeVector.GetComponent<LineRenderer>().transform.localPosition = targetGameObject.transform.localPosition;
            //gazeVector.GetComponent<LineRenderer>().transform.localRotation = targetVector;
            gazeVector.GetComponent<LineRenderer>().transform.localRotation = targetVector
                                                                              * (Quaternion.Euler(-parentGameObject.transform.localRotation.eulerAngles.x, -parentGameObject.transform.localRotation.eulerAngles.y, -parentGameObject.transform.localRotation.eulerAngles.z));
        }

        public void SendCommand()
        {
            byte[] cmd = CmdFormatter(SelectedCmd, _omronCmdSendDataLength[(int)SelectedCmd]);

            SerialHandlerComponent.SendMessage(cmd);
            SerialHandlerComponent.StartThread(ContinuousMode);
        }

        private byte[] CmdFormatter(OmronCmd cmd, int dataLength)
        {
            byte[] tmpCmdBuffer = new byte[4 + dataLength];

            //Data format: 0xFE CMD LSB MSB
            tmpCmdBuffer[0] = 0xFE;
            tmpCmdBuffer[1] = _omronCmdHex[(int)cmd];
            tmpCmdBuffer[2] = (byte)(dataLength % 256);
            tmpCmdBuffer[3] = (byte)(dataLength / 256);

            //Debug.Log(tmpCmdBuffer.ToString());

            switch (cmd)
            {
                case OmronCmd.SetCamDir:
                    tmpCmdBuffer[4] = (byte)SelectedDirection;
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
                    tmpCmdBuffer[4] = (byte)SelectedBPS;
                    break;
                case OmronCmd.RegisterFace:
                    tmpCmdBuffer[4] = (byte)(RegisterUserID % 256);
                    tmpCmdBuffer[5] = (byte)(RegisterUserID / 256);
                    tmpCmdBuffer[6] = RegisterDataID;
                    break;
                case OmronCmd.DeleteFace:
                    tmpCmdBuffer[4] = (byte)(DeleteUserID % 256);
                    tmpCmdBuffer[5] = (byte)(DeleteUserID / 256);
                    tmpCmdBuffer[6] = DeleterDataID;
                    break;
                case OmronCmd.DeleteUserFace:
                    tmpCmdBuffer[4] = (byte)(DeleteUserID1 % 256);
                    tmpCmdBuffer[5] = (byte)(DeleteUserID1 / 256);
                    break;
                case OmronCmd.ReadUserFaceInfo:
                    tmpCmdBuffer[4] = (byte)(ReadUserID % 256);
                    tmpCmdBuffer[5] = (byte)(ReadUserID / 256);
                    break;
                case OmronCmd.ReadAlbum:
                    //TODO: Read albums from host and write it into device.
                    break;
                default:
                    Debug.LogFormat("Command only. No data sending.");
                    break;
            }

            Debug.Log("Omron Command: " + BitConverter.ToString(tmpCmdBuffer));

            return tmpCmdBuffer;
        }

        private byte[] ExecCmdFormatter()
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
            //        Debug.Log("Execute setting 1: " + Convert.ToString(setting[0], 2));

            if (IsEmotionChecked) setting[1] |= (byte)ExecSetting2.Emotion;
            if (IsFaceRecogChecked) setting[1] |= (byte)ExecSetting2.FaceRecog;
            //        Debug.Log("Execute setting 2: " + Convert.ToString(setting[1], 2));

            setting[2] = (byte)ImageSetting;
            //        Debug.Log("Execute image setting: " + ImageSetting);

            switch (ImageSetting)
            {
                case ExecImageSetting.HalfQVGA:
                    _imageWidth = 160;
                    _imageHeight = 120;
                    break;
                case ExecImageSetting.QVGA:
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

        private byte[] SetThresholdCmdFormatter()
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

        private byte[] SetSizeCmdFormatter()
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


        private int ResponseParser(OmronCmd cmd, byte[] dataStream)
        {
            byte syncCode = dataStream[0];
            if (syncCode != 0xFE) return -1;

            byte resCode = dataStream[1];
            if (resCode != 0x00) return -1;

            int dataLength = BitConverter.ToInt32(dataStream, 2);
            if (dataStream.Length < dataLength + 6) return 0;

            byte[] validData = new byte[dataLength];
            Array.Copy(dataStream, 6, validData, 0, dataLength);
            //        Debug.Log("Valid data length: " + dataLength);
            //        Debug.Log("Full data package: " + BitConverter.ToString(dataStream.Take(dataLength + 6).ToArray()));

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

        private void CmdVersionHandler(byte[] data)
        {
            string versionInfo = Encoding.UTF8.GetString(data.Take(12).ToArray()) +
                                 data[12] + "." + data[13] + "." + data[14] + "." + BitConverter.ToUInt32(data, 15);

            Debug.Log("Version Info: " + versionInfo);
        }

        private void CmdExecHandler(byte[] data)
        {
            //TODO: Still have some problems after changing the image size to zero.
            _isOnCreate = false;
            Camera1 = new OmronCameraComponent(data, _currSettings);

            _isOnCreate = true;
        }

        private void CmdGetCamDirHandler(byte[] data)
        {
            switch ((CameraDirection)data[0])
            {
                case CameraDirection.Deg0:
                    SelectedDirection = CameraDirection.Deg0;
                    break;
                case CameraDirection.Deg90:
                    SelectedDirection = CameraDirection.Deg90;
                    break;
                case CameraDirection.Deg180:
                    SelectedDirection = CameraDirection.Deg180;
                    break;
                case CameraDirection.Deg270:
                    SelectedDirection = CameraDirection.Deg270;
                    break;
            }

            Debug.LogFormat("Current direction setting: {0}", SelectedDirection);
        }

        private void CmdGetThresholdHandler(byte[] data)
        {
            BodyThreshold = BitConverter.ToInt16(data, 0);
            HandThreshold = BitConverter.ToInt16(data, 2);
            FaceThreshold = BitConverter.ToInt16(data, 4);
            FaceRecogThreshold = BitConverter.ToInt16(data, 6);

            Debug.LogFormat("Current Threshold Value - Body: {0}, Hand: {1}, Face: {2}, FaceRecog: {3}"
                , BodyThreshold, HandThreshold, FaceThreshold, FaceRecogThreshold);
        }

        private void CmdGetSizeHandler(byte[] data)
        {
            BodySizeMin = BitConverter.ToInt16(data, 0);
            BodySizeMax = BitConverter.ToInt16(data, 2);
            HandSizeMin = BitConverter.ToInt16(data, 4);
            HandSizeMax = BitConverter.ToInt16(data, 6);
            FaceSizeMin = BitConverter.ToInt16(data, 8);
            FaceSizeMax = BitConverter.ToInt16(data, 10);

            Debug.LogFormat("Current Size Value - Body Min.: {0}, Body Max.: {1}, Hand Min.: {2}, Hand Max.: {3}, Face Min.: {4}, Face Max.: {5}"
                , BodySizeMin, BodySizeMax, HandSizeMin, HandSizeMax, FaceSizeMin, FaceSizeMax);
        }

        private void CmdGetFaceAngleHandler(byte[] data)
        {
            SelectedFaceDirYaw = (FaceDirYaw)data[0];
            SelectedFaceDirRoll = (FaceDirRoll)data[1];

            Debug.LogFormat("Face Direction Setting - Yaw: {0}, Roll: {1}", SelectedFaceDirYaw, SelectedFaceDirRoll);
        }
    }




}
