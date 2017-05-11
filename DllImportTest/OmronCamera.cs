using System;
using System.Collections.Generic;
using System.Linq;

namespace OmronOkaoSTBLib
{
    public class OmronCameraBaseStruct
    {
        public short CoordinateX;
        public short CoordinateY;
        public short Size;
        public short Confidence;

        public OmronCameraBaseStruct(byte[] data)
        {
            CoordinateX = BitConverter.ToInt16(data, 0);
            CoordinateY = BitConverter.ToInt16(data, 2);
            Size = BitConverter.ToInt16(data, 4);
            Confidence = BitConverter.ToInt16(data, 6);

            Console.WriteLine("Coordinate X: {0}, Coordinate Y: {1}, Size: {2}, Confidence: {3}"
                , CoordinateX, CoordinateY, Size, Confidence);
        }
    }

    public class OmronCameraBody : OmronCameraBaseStruct
    {
        public static int DataLength = 8;

        public OmronCameraBody(byte[] data) : base(data) { }
    }

    public class OmronCameraHand : OmronCameraBaseStruct
    {
        public static int DataLength = 8;

        public OmronCameraHand(byte[] data) : base(data) { }
    }

    public class OmronCameraFace : OmronCameraBaseStruct
    {
        public static int DataLength = 8;

        public OmronCameraFaceDir FaceDir;
        public OmronCameraAge Age;
        public OmronCameraGender Gender;
        public OmronCameraGaze Gaze;
        public OmronCameraBlink Blink;
        public OmronCameraEmotion Emotion;
        public OmronCameraFaceRecog FaceRecog;

        public OmronCameraFace(byte[] data) : base(data) { }

    }

    public class OmronCameraFaceDir
    {
        public short FaceDirYawAngle;
        public short FaceDirPitchAngle;
        public short FaceDirRollAngle;
        public short FaceDirConfidence;

        public static int DataLength = 8;

        public OmronCameraFaceDir(byte[] data)
        {
            FaceDirYawAngle = BitConverter.ToInt16(data, 0);
            FaceDirPitchAngle = BitConverter.ToInt16(data, 2);
            FaceDirRollAngle = BitConverter.ToInt16(data, 4);
            FaceDirConfidence = BitConverter.ToInt16(data, 6);

            Console.WriteLine("Face Direction - Yaw: {0}, Pitch: {1}, Roll: {2}, Confidence: {3}"
                , FaceDirYawAngle, FaceDirPitchAngle, FaceDirRollAngle, FaceDirConfidence);
        }
    }

    public class OmronCameraAge
    {
        public sbyte Age;
        public short AgeConfidence;

        public static int DataLength = 3;

        public OmronCameraAge(byte[] data)
        {
            Age = (sbyte)data[0];
            AgeConfidence = BitConverter.ToInt16(data, 1);

            Console.WriteLine("Age: {0}, Confidence: {1}", Age, AgeConfidence);
        }
    }

    public class OmronCameraGender
    {
        public short Gender;
        public short GenderConfidence;

        public static int DataLength = 3;

        public OmronCameraGender(byte[] data)
        {
            Gender = (sbyte)data[0];
            GenderConfidence = BitConverter.ToInt16(data, 1);

            Console.WriteLine("Gender: {0}, Confidence: {1}", Gender, GenderConfidence);
        }
    }

    public class OmronCameraGaze
    {
        public sbyte GazeYawAngle;
        public sbyte GazePitchAngle;

        public static int DataLength = 2;

        public OmronCameraGaze(byte[] data)
        {
            GazeYawAngle = (sbyte)data[0];
            GazePitchAngle = (sbyte)data[1];

            Console.WriteLine("Gaze - Yaw: {0}, Pitch: {1}", GazeYawAngle, GazePitchAngle);
        }
    }

    public class OmronCameraBlink
    {
        public short BlinkLeft;
        public short BlinkRight;

        public static int DataLength = 4;

        public OmronCameraBlink(byte[] data)
        {
            BlinkLeft = BitConverter.ToInt16(data, 0);
            BlinkRight = BitConverter.ToInt16(data, 2);

            Console.WriteLine("Blink - Left: {0}, Right: {1}", BlinkLeft, BlinkRight);
        }
    }

    public class OmronCameraEmotion
    {
        public sbyte EmotionNeutral;
        public sbyte EmotionHappiness;
        public sbyte EmotionSurprise;
        public sbyte EmotionAnger;
        public sbyte EmotionSadness;
        public sbyte EmotionExpression;

        public static int DataLength = 6;

        public OmronCameraEmotion(byte[] data)
        {
            EmotionNeutral = (sbyte)data[0];
            EmotionHappiness = (sbyte)data[1];
            EmotionSurprise = (sbyte)data[2];
            EmotionAnger = (sbyte)data[3];
            EmotionSadness = (sbyte)data[4];
            EmotionExpression = (sbyte)data[5];

            Console.WriteLine("Neutral: {0}, Happiness: {1}, Surprise: {2}, Anger: {3}, Sadness: {4}, Expression: {5}",
                EmotionNeutral, EmotionHappiness, EmotionSurprise, EmotionAnger, EmotionSadness, EmotionExpression);
        }
    }

    public class OmronCameraFaceRecog
    {
        public short FaceRecogUserID;
        public short FaceRecogScore;

        public static int DataLength = 4;

        public OmronCameraFaceRecog(byte[] data)
        {
            FaceRecogUserID = BitConverter.ToInt16(data, 0);
            FaceRecogScore = BitConverter.ToInt16(data, 2);

            Console.WriteLine("Face Recognition - UserID: {0}, Score: {1}", FaceRecogUserID, FaceRecogScore);
        }
    }

    public class OmronCamera
    {
        public byte[] RawData;
        public sbyte HandCount;

        public List<OmronCameraHand> Hands = new List<OmronCameraHand>();
//        public OmronCameraImage Image;

        public STBFrameResult FrameResult;

        public OmronCamera(byte[] data, byte[] settings)
        {
            FrameResult = new STBFrameResult()
            {
                bodies = new STBFrameResultBodies(),
                faces = new STBFrameResultFaces()
            };

            FrameResult.bodies.bodies = new STBFrameResultDetection[35];
            FrameResult.faces.faces = new STBFrameResultFace[35];
            RawData = data;

            FrameResult.bodies.nCount = (sbyte)RawData[0];
            HandCount = (sbyte)RawData[1];
            FrameResult.faces.nCount = (sbyte)RawData[2];
            int ptrIndex = 4;

            Console.WriteLine("Body: {0}  Hand: {1}  Face: {2}", FrameResult.bodies.nCount, HandCount, FrameResult.faces.nCount);

            for (int i = 0; i < FrameResult.bodies.nCount; i++)
            {
                ResultParser.ResultDetectionParser(data.Skip(ptrIndex).Take(8).ToArray(),
                    ref FrameResult.bodies.bodies[i]);
//                FrameResult.bodies.bodies[i] =
//                    new STBFrameResultDetection(data.Skip(ptrIndex).Take(8).ToArray());
                ptrIndex += 8;
            }

            for (int i = 0; i < HandCount; i++)
            {
                Hands.Add(new OmronCameraHand(data.Skip(ptrIndex).Take(OmronCameraHand.DataLength).ToArray()));
                ptrIndex += OmronCameraHand.DataLength;
            }

            for (int i = 0; i < FrameResult.faces.nCount; i++)
            {
                if ((settings[0] & (byte)OmronManager.ExecSetting1.Face) != 0x00)
                {
                    ResultParser.ResultFaceParser(data.Skip(ptrIndex).Take(8).ToArray(), ref FrameResult.faces.faces[i]);
//                    FrameResult.faces.faces[i] = 
//                        new STBFrameResultFace(data.Skip(ptrIndex).Take(8).ToArray());
                    ptrIndex += 8;
                }

                if ((settings[0] & (byte)OmronManager.ExecSetting1.FaceDir) != 0x00)
                {
                    ResultParser.ResultDirectionParser(data.Skip(ptrIndex).Take(8).ToArray(), ref FrameResult.faces.faces[i].direction);
//                    FrameResult.faces.faces[i].direction = 
//                        new STBFrameResultDirection(data.Skip(ptrIndex).Take(8).ToArray());
                    ptrIndex += 8;
                }

                if ((settings[0] & (byte)OmronManager.ExecSetting1.Age) != 0x00)
                {
                    ResultParser.ResultAgeParser(data.Skip(ptrIndex).Take(3).ToArray(), ref FrameResult.faces.faces[i].age);
//                    FrameResult.faces.faces[i].age =
//                        new STBFrameResultAge(data.Skip(ptrIndex).Take(3).ToArray());
                    ptrIndex += 3;
                }

                if ((settings[0] & (byte)OmronManager.ExecSetting1.Gender) != 0x00)
                {
                    ResultParser.ResultGenderParser(data.Skip(ptrIndex).Take(3).ToArray(), ref FrameResult.faces.faces[i].gender);
//                    FrameResult.faces.faces[i].gender = 
//                        new STBFrameResultGender(data.Skip(ptrIndex).Take(3).ToArray());
                    ptrIndex += 3;
                }

                if ((settings[0] & (byte)OmronManager.ExecSetting1.Gaze) != 0x00)
                {
                    ResultParser.ResultGazeParser(data.Skip(ptrIndex).Take(2).ToArray(), ref FrameResult.faces.faces[i].gaze);
//                    FrameResult.faces.faces[i].gaze = 
//                        new STBFrameResultGaze(data.Skip(ptrIndex).Take(2).ToArray());
                    ptrIndex += 2;
                }

                if ((settings[0] & (byte)OmronManager.ExecSetting1.Blink) != 0x00)
                {
                    ResultParser.ResultBlinkParser(data.Skip(ptrIndex).Take(4).ToArray(), ref FrameResult.faces.faces[i].blink);
//                    FrameResult.faces.faces[i].blink = 
//                        new STBFrameResultBlink(data.Skip(ptrIndex).Take(4).ToArray());
                    ptrIndex += 4;
                }

                if ((settings[1] & (byte)OmronManager.ExecSetting2.Emotion) != 0x00)
                {
                    ResultParser.ResultExpressionParser(data.Skip(ptrIndex).Take(6).ToArray(),  ref FrameResult.faces.faces[i].expression);
//                    FrameResult.faces.faces[i].expression = 
//                        new STBFrameResultExpression(data.Skip(ptrIndex).Take(6).ToArray());
                    ptrIndex += 6;
                }

                if ((settings[1] & (byte)OmronManager.ExecSetting2.FaceRecog) != 0x00)
                {
                    ResultParser.ResultRecognitionParser(data.Skip(ptrIndex).Take(4).ToArray(), ref FrameResult.faces.faces[i].recognition);
//                    FrameResult.faces.faces[i].recognition = 
//                        new STBFrameResultRecognition(data.Skip(ptrIndex).Take(4).ToArray());
                    ptrIndex += 4;
                }
            }
        }
    }
}
