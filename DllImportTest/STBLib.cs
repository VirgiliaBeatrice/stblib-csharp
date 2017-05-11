using System;
using System.Runtime.InteropServices;

namespace OmronOkaoSTBLib
{
    class STBLib
    {
        //dumpbin.exe /exports .dll file
        [DllImport("STB.dll", EntryPoint = "#1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetVersion(ref byte pnMajorVersion, ref byte pnMinorVersion);

        [DllImport("STB.dll", EntryPoint = "#2", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr STBCreateHandle(uint unUseFuncFlag);

        [DllImport("STB.dll", EntryPoint = "#3", CallingConvention = CallingConvention.Cdecl)]
        public static extern void STBDeleteHandle(IntPtr hSTB);

        [DllImport("STB.dll", EntryPoint = "#101", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetFrameResult(IntPtr hSTB, ref STBFrameResult stbFrameResult);

        [DllImport("STB.dll", EntryPoint = "#10", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBClearFrameResults(IntPtr hSTB);

        [DllImport("STB.dll", EntryPoint = "#111", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBExecute(IntPtr hSTB);

        [DllImport("STB.dll", EntryPoint = "#112", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetFaces(IntPtr hSTB, ref uint punFaceCount, STBFace[] stface);

        [DllImport("STB.dll", EntryPoint = "#113", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetBodies(IntPtr hSTB, ref uint punBodyCount, STBBody[] stBody);

        [DllImport("STB.dll", EntryPoint = "#301", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetTrRetryCount(IntPtr hSTB, int nMaxRetryCount);

        [DllImport("STB.dll", EntryPoint = "#302", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetTrRetryCount(IntPtr hSTB, ref int pnMaxRetryCount);

        [DllImport("STB.dll", EntryPoint = "#303", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetTrSteadinessParam(IntPtr hSTB, int nPosSteadinessParam, int nSizeSteadinessParam);

        [DllImport("STB.dll", EntryPoint = "#304", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetTrSteadinessParam(IntPtr hSTB, ref int pnPosSteadinessParam, ref int pnSizeSteadinessParam);

        [DllImport("STB.dll", EntryPoint = "#401", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetPeThresholdUse(IntPtr hSTB, int nThreshold);

        [DllImport("STB.dll", EntryPoint = "#402", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetPeThresholdUse(IntPtr hSTB, ref int pnThreshold);

        [DllImport("STB.dll", EntryPoint = "#403", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetPeAngleUse(IntPtr hSTB, int nMinUDAngle, int nMaxUDAngle, int nMinLRAngle, int nMaxLRAngle);

        [DllImport("STB.dll", EntryPoint = "#404", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetPeAngleUse(IntPtr hSTB, ref int pnMinUDAngle, ref int pnMaxUDAngle, ref int pnMinLRAngle, ref int pnMaxLRAngle);

        [DllImport("STB.dll", EntryPoint = "#405", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetPeCompleteFrameCount(IntPtr hSTB, int nFrameCount);

        [DllImport("STB.dll", EntryPoint = "#406", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetPeCompleteFrameCount(IntPtr hSTB, ref int pnFrameCount);

        [DllImport("STB.dll", EntryPoint = "#501", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetFrThresholdUse(IntPtr hSTB, int nThreshold);

        [DllImport("STB.dll", EntryPoint = "#502", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetFrThresholdUse(IntPtr hSTB, ref int pnThreshold);

        [DllImport("STB.dll", EntryPoint = "#503", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetFrAngleUse(IntPtr hSTB, int nMinUDAngle, int nMaxUDAngle, int nMinLRAngle, int nMaxLRAngle);

        [DllImport("STB.dll", EntryPoint = "#504", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetFrAngleUse(IntPtr hSTB, ref int pnMinUDAngle, ref int pnMaxUDAngle, ref int pnMinLRAngle, ref int pnMaxLRAngle);

        [DllImport("STB.dll", EntryPoint = "#505", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetFrCompleteFrameCount(IntPtr hSTB, int nFrameCount);

        [DllImport("STB.dll", EntryPoint = "#506", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetFrCompleteFrameCount(IntPtr hSTB, ref int pnFrameCount);

        [DllImport("STB.dll", EntryPoint = "#507", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBSetFrMinRatio(IntPtr hSTB, int nMinRatio);

        [DllImport("STB.dll", EntryPoint = "#508", CallingConvention = CallingConvention.Cdecl)]
        public static extern int STBGetFrMinRatio(IntPtr hSTB, ref int pnMinRatio);
//
//
        static void Main(string[] args)
        {
            byte majorVersion = 0x00;
            byte minorVersion = 0x00;

            STBGetVersion(ref majorVersion, ref minorVersion);

            Console.Write("Major Version: {0}, Minor Version: {1} \r\n", majorVersion, minorVersion);
            Console.Write("Press any key to stop...");
            Console.ReadKey();

            //Create Handle
//            IntPtr stbHandle = STBCreateHandle((uint)STBExecFlag.FaceTracking);
            //Set parameters
            
            //Set frame result
//            STBFrameResult result = new STBFrameResult()
//            {
//                
//            }; 
            //STBSetFrameResult(stbHandle, )
        }
    }

    //#define STB_FUNC_BD  (0x00000001U)  /* [LSB]bit0: Body Tracking           00000000001     */
    //#define STB_FUNC_DT  (0x00000004U)  /* [LSB]bit2: Face Tracking           00000000100     */
    //#define STB_FUNC_PT  (0x00000008U)  /* [LSB]bit3: Face Direction          00000001000     */
    //#define STB_FUNC_AG  (0x00000010U)  /* [LSB]bit4: Age Estimation          00000010000     */
    //#define STB_FUNC_GN  (0x00000020U)  /* [LSB]bit5: Gender Estimation       00000100000     */
    //#define STB_FUNC_GZ  (0x00000040U)  /* [LSB]bit6: Gaze Estimation         00001000000     */
    //#define STB_FUNC_BL  (0x00000080U)  /* [LSB]bit7: Blink Estimation        00010000000     */
    //#define STB_FUNC_EX  (0x00000100U)  /* [MSB]bit0: Expression Estimation   00100000000     */
    //#define STB_FUNC_FR  (0x00000200U)  /* [MSB]bit1: Face Recognition        01000000000     */
    [Flags]
    public enum STBExecFlag : ushort
    {
        BodyTracking = 0x0001, FaceTracking = 0x0004, FaceDir = 0x0008, Age = 0x0010,
        Gender = 0x0020, Gaze = 0x0040, Blink = 0x0080, Expression = 0x0100, FaceRecog = 0x0200
    }

    public enum STBOkaoExpression
    {
        STBExpressionNeutral,
        STBExpressionHappiness,
        STBExpressionSurprise,
        STBExpressionAnger,
        STBExpressionSadness,
        STBExpressionMax
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBPoint
    {
        public int nX;
        public int nY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultDirection
    {
        public int nLR;
        public int nUD;
        public int nRoll;
        public int nConfidence;

//        public STBFrameResultDirection(byte[] data)
//        {
//            nLR = BitConverter.ToInt16(data, 0);
//            nUD = BitConverter.ToInt16(data, 2);
//            nRoll = BitConverter.ToInt16(data, 4);
//            nConfidence = BitConverter.ToInt16(data, 6);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultAge
    {
        public int nAge;
        public int nConfidence;

//        public STBFrameResultAge(byte[] data)
//        {
//            nAge = (sbyte)data[0];
//            nConfidence = BitConverter.ToInt16(data, 1);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultGender
    {
        public int nGender;
        public int nConfidence;

//        public STBFrameResultGender(byte[] data)
//        {
//            nGender = (sbyte)data[0];
//            nConfidence = BitConverter.ToInt16(data, 1);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultGaze
    {
        public int nLR;
        public int nUD;

//        public STBFrameResultGaze(byte[] data)
//        {
//            nLR = (sbyte)data[0];
//            nUD = (sbyte)data[1];
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultBlink
    {
        public int nLeftEye;
        public int nRightEye;

//        public STBFrameResultBlink(byte[] data)
//        {
//            nLeftEye = BitConverter.ToInt16(data, 0);
//            nRightEye = BitConverter.ToInt16(data, 2);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultExpression
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)STBOkaoExpression.STBExpressionMax)]
        public int[] anScore;
//        public int[] anScore = new int[(int)STBOkaoExpression.STBExpressionMax];
        public int nDegree;

        //        public STBFrameResultExpression(byte[] data)
        //        {
        //            anScore = new int[(int) STBOkaoExpression.STBExpressionMax];
        //
        //            anScore[(int)STBOkaoExpression.STBExpressionNeutral] = (sbyte)data[0];
        //            anScore[(int)STBOkaoExpression.STBExpressionHappiness] = (sbyte)data[1];
        //            anScore[(int)STBOkaoExpression.STBExpressionSurprise] = (sbyte)data[2];
        //            anScore[(int)STBOkaoExpression.STBExpressionAnger] = (sbyte)data[3];
        //            anScore[(int)STBOkaoExpression.STBExpressionSadness] = (sbyte)data[4];
        //            nDegree = (sbyte)data[5];
        //        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultRecognition
    {
        public int nUID;
        public int nScore;

//        public STBFrameResultRecognition(byte[] data)
//        {
//            nUID = BitConverter.ToInt16(data, 0);
//            nScore = BitConverter.ToInt16(data, 2);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultDetection
    {
        public STBPoint center;
        public int nSize;
        public int nConfidence;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultFace
    {
        public STBPoint center;
        public int nSize;
        public int nConfidence;
        public STBFrameResultDirection direction;
        public STBFrameResultAge age;
        public STBFrameResultGender gender;
        public STBFrameResultGaze gaze;
        public STBFrameResultBlink blink;
        public STBFrameResultExpression expression;
        public STBFrameResultRecognition recognition;

//        public STBFrameResultFace(byte[] data)
//        {
//            center = new STBPoint()
//            {
//                nX = BitConverter.ToInt16(data, 0),
//                nY = BitConverter.ToInt16(data, 2)
//            };
//            nSize = BitConverter.ToInt16(data, 4);
//            nConfidence = BitConverter.ToInt16(data, 6);
//        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultBodies
    {
        public int nCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
        public STBFrameResultDetection[] bodies;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResultFaces
    {
        public int nCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
        public STBFrameResultFace[] faces;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFrameResult
    {
        public STBFrameResultBodies bodies;
        public STBFrameResultFaces faces;
    }

    public struct ResultParser
    {
        public static void ResultDetectionParser(byte[] data, ref STBFrameResultDetection resultDetection)
        {
            resultDetection.center.nX = BitConverter.ToInt16(data, 0);
            resultDetection.center.nY = BitConverter.ToInt16(data, 2);
            resultDetection.nSize = BitConverter.ToInt16(data, 4);
            resultDetection.nConfidence = BitConverter.ToInt16(data, 6);
        }

        public static void ResultFaceParser(byte[] data, ref STBFrameResultFace resultFace)
        {
            resultFace.center.nX = BitConverter.ToInt16(data, 0);
            resultFace.center.nY = BitConverter.ToInt16(data, 2);
            resultFace.nSize = BitConverter.ToInt16(data, 4);
            resultFace.nConfidence = BitConverter.ToInt16(data, 6);
        }

        public static void ResultDirectionParser(byte[] data, ref STBFrameResultDirection resultDirection)
        {
            resultDirection.nLR = BitConverter.ToInt16(data, 0);
            resultDirection.nUD = BitConverter.ToInt16(data, 2);
            resultDirection.nRoll = BitConverter.ToInt16(data, 4);
            resultDirection.nConfidence = BitConverter.ToInt16(data, 6);
        }

        public static void ResultAgeParser(byte[] data, ref STBFrameResultAge resultAge)
        {
            resultAge.nAge = (sbyte) data[0];
            resultAge.nConfidence = BitConverter.ToInt16(data, 1);
        }

        public static void ResultGenderParser(byte[] data, ref STBFrameResultGender resultGender)
        {
            resultGender.nGender = (sbyte)data[0];
            resultGender.nConfidence = BitConverter.ToInt16(data, 1);
        }

        public static void ResultGazeParser(byte[] data, ref STBFrameResultGaze resultGaze)
        {
            resultGaze.nLR = (sbyte)data[0];
            resultGaze.nUD = (sbyte)data[1];
        }

        public static void ResultBlinkParser(byte[] data, ref STBFrameResultBlink resultBlink)
        {
            resultBlink.nLeftEye = BitConverter.ToInt16(data, 0);
            resultBlink.nRightEye = BitConverter.ToInt16(data, 2);
        }

        public static void ResultExpressionParser(byte[] data, ref STBFrameResultExpression resultExpression)
        {
            resultExpression.anScore[(int)STBOkaoExpression.STBExpressionNeutral] = (sbyte)data[0];
            resultExpression.anScore[(int)STBOkaoExpression.STBExpressionHappiness] = (sbyte)data[1];
            resultExpression.anScore[(int)STBOkaoExpression.STBExpressionSurprise] = (sbyte)data[2];
            resultExpression.anScore[(int)STBOkaoExpression.STBExpressionAnger] = (sbyte)data[3];
            resultExpression.anScore[(int)STBOkaoExpression.STBExpressionSadness] = (sbyte)data[4];
            resultExpression.nDegree = (sbyte)data[5];
        }

        public static void ResultRecognitionParser(byte[] data, ref STBFrameResultRecognition resultRecognition)
        {
            resultRecognition.nUID = BitConverter.ToInt16(data, 0);
            resultRecognition.nScore = BitConverter.ToInt16(data, 2);
        }
    }

    //OUTPUT data structure

    public enum STBStatus
    {
        STBStatusNoData = -1, STBStatusCalculating = 0,
        STBStatusComplete = 1, STBStatusFixed = 2
    }

    public enum STBExpression
    {
        STBExUnknown = -1,
        STBExNeutral = 0,
        STBExHappiness,
        STBExSurprise,
        STBExAnger,
        STBExSadness,
        STBExMax
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBResult
    {
        public STBStatus status;
        public int conf;
        public int value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBGaze
    {
        public STBStatus status;
        public int conf;
        public int UD;
        public int LR;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBDir
    {
        public STBStatus status;
        public int conf;
        public int yaw;
        public int pitch;
        public int roll;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBBlink
    {
        public STBStatus status;
        public int ratioL;
        public int ratioR;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBPos
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBFace
    {
        public int nDetectID;
        public int nTrackingID;
        public STBPos center;
        public uint nSize;
        public int conf;
        public STBDir direction;
        public STBResult age;
        public STBResult gender;
        public STBGaze gaze;
        public STBBlink blink;
        public STBResult expression;
        public STBResult recognition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STBBody
    {
        public int nDetectID;
        public int nTrackingID;
        public STBPos center;
        public uint nSize;
        public int conf;
    }
}
