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
using TwinCAT;

namespace TcAdsHelper
{
    public class AdsHelper : IDisposable
    {
        #region 属性字段声明
        /// <summary>
        /// TcAdsClient
        /// </summary>
        private AdsClient tcClient;
        /// <summary>
        /// allSymbols
        /// </summary>
        private ISymbolCollection<ISymbol> allSymbols;
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
            tcClient = new AdsClient();
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
                        if (type.AdsDataType == AdsDataTypeId.ADST_STRING)
                        {
                            type.Size = match[0].Groups[3] != null ? (int.Parse(match[0].Groups[3].Value) + 1) : type.Size;
                        }
                        else if (type.AdsDataType == AdsDataTypeId.ADST_WSTRING)
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
        private object ParsePlcVariableToJs(byte[] dataBuffer, AdsDataTypeId dataTypeId)
        {
            return Types.types.Find(t => { return t.AdsDataType.Equals(dataTypeId) && t.Size == dataBuffer.Length; }).FromBuffer(dataBuffer);
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
                return (dataType.DataTypeId == AdsDataTypeId.ADST_STRING || dataType.DataTypeId == AdsDataTypeId.ADST_WSTRING) ? "\"" + ParsePlcVariableToJs(dataBuffer, dataType.DataTypeId) + "\"" : ParsePlcVariableToJs(dataBuffer, dataType.DataTypeId);
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
        ~AdsHelper()
        {
            Dispose(disposing: false);
        } 
        #endregion

    }
}

