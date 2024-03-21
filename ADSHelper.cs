using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;
using TwinCAT.TypeSystem;
using TwinCAT.Ads.TypeSystem;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading.Tasks;

namespace ADS_DEMO
{
    public class ADSHelper : IDisposable
    {
        #region 属性字段声明
        /// <summary>
        /// TcAdsClient
        /// </summary>
        private TcAdsClient tcClient;
        /// <summary>
        /// allSymbols
        /// </summary>
        private ReadOnlySymbolCollection allSymbols;
        /// <summary>
        /// allDataTypes
        /// </summary>
        private Dictionary<string, DataType> allDataTypes; //
        //PlcIP
        public string PlcIP { get; private set; }
        //PlcPort
        public int PlcPort { get; private set; }
        //ADSHelper初始化读取标记
        private bool needGetAllDefaults = true;
        #endregion

        #region ADS初始化
        /// <summary>
        /// PLC是否已连接运行
        /// </summary>
        /// <returns></returns>
        public bool IsADSConnected()
        {
            try
            {
                if (tcClient == null)
                {
                    CreateADSConnect(PlcIP, PlcPort);
                }

                if (tcClient.ReadState().AdsState == AdsState.Run || tcClient.ReadState().AdsState == AdsState.Init || tcClient.ReadState().AdsState == AdsState.Start)
                {
                    GetAllDefaults();
                    return true;
                }
                else
                {
                    if (!needGetAllDefaults)
                        needGetAllDefaults = true;
                    return false;
                }
            }
            catch
            {
                if (!needGetAllDefaults)
                    needGetAllDefaults = true;
                return false;
            }
        }
        /// <summary>
        /// 获取PLC allSymbols
        /// </summary>
        private void GetAllDefaults()
        {
            if (needGetAllDefaults || allSymbols == null || allSymbols.Count == 0 || allDataTypes == null || allDataTypes.Count == 0)
            {
                //ADS Client Load symbolic and DataType information 初始化
                ISymbolLoader loader = SymbolLoaderFactory.Create(tcClient, SymbolLoaderSettings.Default);
                allSymbols = loader.Symbols;
                allDataTypes = loader.DataTypes.ToDictionary(i => i.Name, i => (DataType)i);
                if (allSymbols.Count > 0 && allDataTypes.Count > 0)
                    needGetAllDefaults = false;
            }
        }
        /// <summary>
        /// 建立ADS Client连接
        /// </summary>
        public bool CreateADSConnect(string plcIP, int plcPort)
        {
            //ADS Client初始化
            tcClient = new TcAdsClient();
            tcClient.Connect(plcIP, plcPort);
            PlcIP = plcIP;
            PlcPort = plcPort;
            return IsADSConnected();
        }
        #endregion

        #region ADSReadAndWrite

        /// <summary>
        /// 多个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePaths"></param>
        /// <returns>HashTable HashTableList BaseTypeList</returns>
        public object[] ReadObject(List<string> InstancePaths)
        {
            SymbolCollection symbols = GetSymbol(InstancePaths);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbols);

            object[] values = readCommand.Read();
            int count = InstancePaths.Count;
            object[] ret = new object[count];
            Parallel.For(0, values.Count(), i =>
            {
                if (typeof(byte[]) == values[i].GetType())
                {
                    ret[i] = ParsePlcDataToObject((byte[])values[i], (DataType)symbols[i].DataType);
                }
                else
                {

                    ret[i] = values[i];
                }
            });
            return ret;
        }
        /// <summary>
        /// 单个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePath"></param>
        /// <returns>HashTable HashTableList BaseTypeList</returns>
        public object ReadObject(string InstancePath)
        {
            SymbolCollection symbol = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbol);

            object value = readCommand.Read()[0];
            if (typeof(byte[]) == value.GetType())
            {
                return ParsePlcDataToObject((byte[])value, (DataType)symbol[0].DataType);
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 多个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePath"></param>
        /// <returns>Json JsonArray</returns>
        public object[] ReadJson(List<string> InstancePath)
        {
            SymbolCollection symbols = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbols);

            object[] values = readCommand.Read();
            int count = InstancePath.Count;
            object[] ret = new object[count];
            Parallel.For(0, values.Count(), i =>
            {
                if (typeof(byte[]) == values[i].GetType())
                {
                    ret[i] = ParsePlcDataToJson((byte[])values[i], (DataType)symbols[i].DataType);
                }
                else
                {

                    ret[i] = values[i];
                }
            });
            return ret;
        }
        /// <summary>
        /// 单个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePath"></param>
        /// <returns>Json JsonArray</returns>
        public object ReadJson(string InstancePath)
        {
            SymbolCollection symbol = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbol);

            object value = readCommand.Read()[0];
            if (typeof(byte[]) == value.GetType())
            {
                return ParsePlcDataToJson((byte[])value, (DataType)symbol[0].DataType);
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 多个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePath"></param>
        /// <returns>JsonStr JsonArrayStr</returns>
        public object[] ReadJsonStr(List<string> InstancePath)
        {
            SymbolCollection symbols = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbols);

            object[] values = readCommand.Read();
            int count = InstancePath.Count;
            object[] ret = new object[count];
            Parallel.For(0, values.Count(), i =>
            {
                if (typeof(byte[]) == values[i].GetType())
                {
                    ret[i] = ParsePlcDataToJsonString((byte[])values[i], (DataType)symbols[i].DataType);
                }
                else
                {

                    ret[i] = values[i];
                }
            });
            return ret;
        }
        /// <summary>
        /// 单个PLC Struct、FB、VI等变量读取
        /// </summary>
        /// <param name="InstancePath"></param>
        /// <returns>JsonStr JsonArrayStr</returns>
        public object ReadJsonStr(string InstancePath)
        {
            SymbolCollection symbol = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbol);

            object value = readCommand.Read()[0];
            if (typeof(byte[]) == value.GetType())
            {
                return ParsePlcDataToJsonString((byte[])value, (DataType)symbol[0].DataType);
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 单个PLC变量写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="InstancePath"></param>
        /// <param name="value"></param>
        /// <returns>写入结果</returns>
        public bool SumWrite<T>(string InstancePath, T value)
        {
            // Sum Command Write
            object[] writeValues = new object[] { value };
            SumSymbolWrite writeCommand = new SumSymbolWrite(tcClient, GetSymbol(InstancePath));
            writeCommand.Write(writeValues);
            return writeCommand.Succeeded;
        }
        /// <summary>
        /// 多个PLC变量写入
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns>写入结果</returns>
        public bool SumWrite(Dictionary<string, object> keyValuePairs)
        {
            List<string> Symbols = keyValuePairs.Keys.ToList();
            object[] Values = keyValuePairs.Values.ToArray();
            SumSymbolWrite writeCommand = new SumSymbolWrite(tcClient, GetSymbol(Symbols));
            writeCommand.Write(Values);
            return writeCommand.Succeeded;
        }
        public object[] Read(List<string> InstancePaths)
        {
            SymbolCollection symbols = GetSymbol(InstancePaths);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbols);
            return readCommand.Read();
        }
        public object Read(string InstancePath)
        {
            SymbolCollection symbol = GetSymbol(InstancePath);
            SumSymbolRead readCommand = new SumSymbolRead(tcClient, symbol);
            return readCommand.Read()[0];
        }
        private SymbolCollection GetSymbol(List<string> Symbols)
        {
            SymbolCollection symbolCollection = new SymbolCollection();

            foreach (string symbol in Symbols)
            {
                symbolCollection.Add(allSymbols[symbol]);
            }

            return symbolCollection;
        }
        private SymbolCollection GetSymbol(string Symbols)
        {
            SymbolCollection symbolCollection = new SymbolCollection();

            symbolCollection.Add(allSymbols[Symbols]);

            return symbolCollection;
        }
        #endregion

        #region PLCDataConvert
        private object ParsePlcVariableToJs(byte[] dataBuffer, DataType dataType)
        {
            if (dataType == null) return null;
            else
            {
                //1.DataTypeId
                AdsDataTypeDescription type = Types.types.Find(t => { return t.AdsDataType.Equals(dataType.DataTypeId); });
                if (type == null)
                {
                    //2.name
                    string name = dataType.Name.Trim();
                    type = Types.types.Find(t => { return t.Names.Contains(name); });
                    if (type == null)
                    {
                        //3.正则name
                        MatchCollection match = null;
                        type = Types.types.Find(t =>
                        {
                            Regex re = new Regex(@"^(" + string.Join("|", t.Names) + ")([\\[\\(](.*)[\\)\\]])*$", RegexOptions.IgnoreCase);
                            match = re.Matches(name);
                            return match != null;
                        });
                        //4.返回
                        if (type == null)
                            return null;
                        if (type.AdsDataType == AdsDatatypeId.ADST_STRING)
                        {
                            type.Size = match[0].Groups[3] != null ? (int.Parse(match[0].Groups[3].Value) + 1) : type.Size;
                        }
                        else if (type.AdsDataType == AdsDatatypeId.ADST_WSTRING)
                        {
                            type.Size = match[0].Groups[3] != null ? (int.Parse(match[0].Groups[3].Value) * 2 + 2) : type.Size;
                        }
                    }
                }
                else
                {
                    type.Size = dataType.Size;
                }

                return type.FromBuffer(dataBuffer);
            }
        }
        private object ParsePlcVariableToJs(byte[] dataBuffer, AdsDatatypeId dataTypeId)
        {
            return Types.types.Find(t => { return t.AdsDataType.Equals(dataTypeId); }).FromBuffer(dataBuffer);
        }
        private object ParsePlcDataToJson(byte[] dataBuffer, DataType datatype)
        {
            if (datatype.Category == DataTypeCategory.Array)
            {
                return JArray.Parse(ParsePlcDataToJsonString(dataBuffer, datatype).ToString());
            }else
            return JObject.Parse(ParsePlcDataToJsonString(dataBuffer, datatype).ToString()); ;
        }
        private object ParsePlcDataToObject(byte[] dataBuffer, DataType dataType)
        {
            //Struct
            if (dataType.Category == DataTypeCategory.Struct)
            {
                Hashtable hash = new Hashtable();
                StructType st = (StructType)dataType;
                int count = st.AllMembers.Count;
                if (count > 0)
                {
                    int offset;
                    int size;
                    string name;
                    DataType idataType = null;
                    object o;
                    for (int i = 0; i < count; i++)
                    {
                        offset = st.AllMembers[i].ByteOffset;
                        size = st.AllMembers[i].ByteSize;
                        name = st.AllMembers[i].InstanceName;
                        idataType = allDataTypes[st.AllMembers[i].DataType.Name];
                        o = ParsePlcDataToObject(dataBuffer.Skip(offset).Take(size).ToArray(), idataType);
                        //hash
                        hash.Add(name, o);
                    }
                    return hash;
                }
                return hash;
            }//StructArray
            else if (dataType.Category == DataTypeCategory.Array)
            {
                List<object> list = new List<object>();
                ArrayType ArraySymbol = (ArrayType)dataType;
                int count = ArraySymbol.Dimensions[0].ElementCount;
                int lower = ArraySymbol.Dimensions[0].LowerBound;
                int upper = lower + count - 1;
                if (count > 0)
                {
                    int size;
                    DataType idataType = null;
                    object o;
                    for (int i = lower; i <= upper; i++)
                    {
                        size = ArraySymbol.ElementSize;
                        if (idataType is null)
                        {
                            idataType = allDataTypes[ArraySymbol.ElementTypeName]; ;
                        }
                        o = ParsePlcDataToObject(dataBuffer.Take(size).ToArray(), idataType);
                        dataBuffer = dataBuffer.Skip(size).ToArray();
                        //list
                        list.Add(o);
                    }
                    return list;
                }
                return list;
            }
            else
            {
                //adstypes
                return ParsePlcVariableToJs(dataBuffer, dataType.DataTypeId);
            }
        }
        private object ParsePlcDataToJsonString(byte[] dataBuffer, DataType dataType)
        {
            //Struct
            if (dataType.Category == DataTypeCategory.Struct)
            {
                StringBuilder sb = new StringBuilder("{");
                StructType st = (StructType)dataType;
                int count = st.AllMembers.Count;
                if (count > 0)
                {
                    DataType idataType = null;
                    for (int i = 0; i < count; i++)
                    {
                        idataType = allDataTypes[st.AllMembers[i].DataType.Name];
                        sb.Append("\"");
                        sb.Append(st.AllMembers[i].InstanceName);
                        sb.Append("\":");
                        sb.Append(ParsePlcDataToJsonString(dataBuffer.Skip(st.AllMembers[i].ByteOffset).Take(st.AllMembers[i].ByteSize).ToArray(), idataType));
                        sb.Append(",");
                    }
                    return string.Join("", sb.Remove(sb.Length - 1, 1).Append("}"));
                }
                return string.Join("", sb.Append("}"));
            }//StructArray
            else if (dataType.Category == DataTypeCategory.Array)
            {
                StringBuilder sb = new StringBuilder("[");
                ArrayType ArraySymbol = (ArrayType)dataType;
                int count = ArraySymbol.Dimensions[0].ElementCount;
                int lower = ArraySymbol.Dimensions[0].LowerBound;
                int upper = lower + count - 1;
                if (count > 0)
                {
                    DataType idataType = null;
                    for (int i = lower; i <= upper; i++)
                    {
                        if (idataType is null)
                        {
                            idataType = allDataTypes[ArraySymbol.ElementTypeName];
                        }
                        sb.Append(ParsePlcDataToJsonString(dataBuffer.Take(ArraySymbol.ElementSize).ToArray(), idataType));
                        sb.Append(",");
                        dataBuffer = dataBuffer.Skip(ArraySymbol.ElementSize).ToArray();
                    }
                    return string.Join("", sb.Remove(sb.Length - 1, 1).Append("]"));
                }
                return string.Join("", sb.Append("]"));
            }
            else
            {
                //adstypes
                return (dataType.DataTypeId == AdsDatatypeId.ADST_STRING || dataType.DataTypeId == AdsDatatypeId.ADST_WSTRING) ? "\"" + ParsePlcVariableToJs(dataBuffer, dataType.DataTypeId) + "\"" : ParsePlcVariableToJs(dataBuffer, dataType.DataTypeId);
            }
        }
        #endregion


        #region 资源释放

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    tcClient.Dispose();
                }
                disposed = true;
            }
        }
        ~ADSHelper()
        {
            Dispose(disposing: false);
        } 
        #endregion

    }
    #region ADSType

    public class AdsDataTypeDescription
    {
        public List<string> Names { get; set; }
        public AdsDatatypeId AdsDataType { get; set; }
        public int Size { get; set; }
        public Func<byte[], object> FromBuffer { get; set; }
    }

    public class Types
    {
        public static List<AdsDataTypeDescription> types = new List<AdsDataTypeDescription>
        {
            new AdsDataTypeDescription
            {
                Names = new List<string> { "WSTRING" },
                AdsDataType = AdsDatatypeId.ADST_WSTRING,
                Size = 162, // 默认大小  
                FromBuffer = buffer => DecodeWString(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "STRING" },
                AdsDataType = AdsDatatypeId.ADST_STRING,
                Size = 81, // 默认大小  
                FromBuffer = buffer => DecodeString(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "BOOL", "BIT", "BIT8" },
                AdsDataType = AdsDatatypeId.ADST_BIT,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeBIT(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "BYTE", "USINT", "BITARR8", "UINT8"},
                AdsDataType = AdsDatatypeId.ADST_UINT8,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT8(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"SINT", "INT8" },
                AdsDataType = AdsDatatypeId.ADST_INT8,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeINT8(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "UINT", "WORD", "BITARR16", "UINT16"},
                AdsDataType = AdsDatatypeId.ADST_UINT16,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT16(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "DINT", "INT32"},
                AdsDataType = AdsDatatypeId.ADST_INT32,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"UDINT", "DWORD", "TIME", "TIME_OF_DAY", "TOD", "BITARR32", "UINT32" },
                AdsDataType = AdsDatatypeId.ADST_UINT32,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"DATE_AND_TIME", "DT", "DATE" },
                AdsDataType = AdsDatatypeId.ADST_UINT32,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"LREAL", "DOUBLE" },
                AdsDataType = AdsDatatypeId.ADST_REAL64,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeREAL64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "REAL", "FLOAT"},
                AdsDataType = AdsDatatypeId.ADST_REAL32,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeREAL32(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> {"LWORD", "ULINT", "LTIME", "UINT64" },
                AdsDataType = AdsDatatypeId.ADST_UINT64,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeUINT64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "LINT", "INT64"},
                AdsDataType = AdsDatatypeId.ADST_INT64,
                Size = 1, // 默认大小  
                FromBuffer = buffer => DecodeINT64(buffer)
            },
            new AdsDataTypeDescription
            {
                Names = new List<string> { "INT", "INT16"},
                AdsDataType = AdsDatatypeId.ADST_INT16,
                Size = 1, // 默认大小  
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

