using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwinCAT.Ads;

namespace TcAdsHelper
{
    #region ADSType

    public class Types
    {
        public static List<AdsDataTypeDescription> types = new List<AdsDataTypeDescription>
        {
            new AdsDataTypeDescription
            {
                Names = new List<string> { "WSTRING" },
                AdsDataType = AdsDataTypeId.ADST_WSTRING,
                Size = 162, // 默认大小  
                FromBuffer = buffer => DecodeWString(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "STRING" },
                AdsDataType = AdsDataTypeId.ADST_STRING,
                Size = 81, // 默认大小  
                FromBuffer = buffer => DecodeString(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "BOOL", "BIT", "BIT8" },
                AdsDataType = AdsDataTypeId.ADST_BIT,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeBIT(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "BYTE", "USINT", "BITARR8", "UINT8"},
                AdsDataType = AdsDataTypeId.ADST_UINT8,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT8(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"SINT", "INT8" },
                AdsDataType = AdsDataTypeId.ADST_INT8,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeINT8(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "UINT", "WORD", "BITARR16", "UINT16"},
                AdsDataType = AdsDataTypeId.ADST_UINT16,
                Size = 2, // 默认大小  
                FromBuffer = buffer => DecodeUINT16(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "DINT", "INT32"},
                AdsDataType = AdsDataTypeId.ADST_INT32,
                Size = 4, // 默认大小  
                FromBuffer = buffer => DecodeINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"UDINT", "DWORD", "TIME", "TIME_OF_DAY", "TOD", "BITARR32", "UINT32" },
                AdsDataType = AdsDataTypeId.ADST_UINT32,
                Size = 4, // 默认大小  
                FromBuffer = buffer => DecodeUINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"DATE_AND_TIME", "DT", "DATE" },
                AdsDataType = AdsDataTypeId.ADST_UINT32,
                Size = 4, // 默认大小  
                FromBuffer = buffer => DecodeUINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"LREAL", "DOUBLE" },
                AdsDataType = AdsDataTypeId.ADST_REAL64,
                Size = 8, // 默认大小  
                FromBuffer = buffer => DecodeREAL64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "REAL", "FLOAT"},
                AdsDataType = AdsDataTypeId.ADST_REAL32,
                Size = 4, // 默认大小  
                FromBuffer = buffer => DecodeREAL32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"LWORD", "ULINT", "LTIME", "UINT64" },
                AdsDataType = AdsDataTypeId.ADST_UINT64,
                Size = 8, // 默认大小  
                FromBuffer = buffer => DecodeUINT64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "UINT64" },
                AdsDataType = AdsDataTypeId.ADST_BIGTYPE,
                Size = 8, // 默认大小  
                FromBuffer = buffer => DecodeUINT64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "UINT32" },
                AdsDataType = AdsDataTypeId.ADST_BIGTYPE,
                Size = 4, // 默认大小  
                FromBuffer = buffer => DecodeUINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "LINT", "INT64"},
                AdsDataType = AdsDataTypeId.ADST_INT64,
                Size = 8, // 默认大小  
                FromBuffer = buffer => DecodeINT64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "INT", "INT16"},
                AdsDataType = AdsDataTypeId.ADST_INT16,
                Size = 2, // 默认大小  
                FromBuffer = buffer => DecodeINT16(buffer)
            },
        };

        // 这里可以添加代码来使用上述定义的types列表，例如编码和解码值等。  
        private static string DecodeString(byte[] buffer)
        {
            // 假设我们使用Unicode编码从字节数组解码字符串  
            return buffer == null ? "" : Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }
        private static string DecodeWString(byte[] buffer)
        {
            // 假设我们使用Unicode编码从字节数组解码字符串  
            try
            {
                int al = buffer.Count();
                if (al <= 0)
                {
                    return null;
                }
                string s;
                byte[] Buffer = new byte[al];
                Array.Copy(buffer, 0, Buffer, 0, al);
                string ss = Encoding.Unicode.GetString(Buffer);
                s = cutSubstring(ss, ss.Length);
                return s;
            }
            catch { return null; }
        }
        private static string DecodeBIT(byte[] buffer)
        {
            return BitConverter.ToBoolean(buffer, 0).ToString().ToLower();
        }
        private static byte DecodeUINT8(byte[] buffer)
        {
            return buffer[0];
        }
        private static sbyte DecodeINT8(byte[] buffer)
        {
            return (sbyte)buffer[0];
        }
        private static ushort DecodeUINT16(byte[] buffer)
        {
            return BitConverter.ToUInt16(buffer, 0);
        }
        private static short DecodeINT16(byte[] buffer)
        {
            return BitConverter.ToInt16(buffer, 0);
        }
        private static int DecodeINT32(byte[] buffer)
        {
            return BitConverter.ToInt32(buffer, 0);
        }
        private static uint DecodeUINT32(byte[] buffer)
        {
            return BitConverter.ToUInt32(buffer, 0);
        }
        private static float DecodeREAL32(byte[] buffer)
        {
            return BitConverter.ToSingle(buffer, 0);
        }
        private static double DecodeREAL64(byte[] buffer)
        {
            return BitConverter.ToDouble(buffer, 0);
        }
        private static ulong DecodeUINT64(byte[] buffer)
        {
            return BitConverter.ToUInt64(buffer, 0);
        }
        private static long DecodeINT64(byte[] buffer)
        {
            return BitConverter.ToInt64(buffer, 0);
        }
        private static string cutSubstring(string str, int length)
        {
            if (str == null || str.Length == 0 || length < 0)
            {
                return "";
            }

            byte[] bytes = Encoding.Unicode.GetBytes(str);
            int n = 0;  //  表示当前的字节数
            int i = 0;  //  要截取的字节数
            for (; i < bytes.GetLength(0) && n < length; i++)
            {
                //  偶数位置，如0、2、4等，为UCS2编码中两个字节的第一个字节
                if (i % 2 == 0)
                {
                    if ((bytes[i] == 0 && bytes[i + 1] == 0))
                        break;
                    n++;      //  在UCS2第一个字节时n加1
                }
                else
                {
                    //  当UCS2编码的第二个字节大于0时，该UCS2字符为汉字，一个汉字算两个字节
                    if (bytes[i] > 0)
                    {
                        n++;
                    }
                }
            }
            //  如果i为奇数时，处理成偶数
            if (i % 2 == 1)
            {
                //  该UCS2字符是汉字时，去掉这个截一半的汉字
                if (bytes[i] > 0)
                    i = i - 1;
                //  该UCS2字符是字母或数字，则保留该字符
                else
                    i = i + 1;
            }
            return Encoding.Unicode.GetString(bytes, 0, i);
        }
    }
    #endregion
}

