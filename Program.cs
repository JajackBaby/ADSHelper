using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_DEMO
{
    class Program
    {
        static void Main(string[] args)
        {
            ADSHelper ads = new ADSHelper();
            if (ads.CreateADSConnect("IP", 851))
            {
                //多变量读取 
                List<string> InstancePaths = new List<string>();
                InstancePaths.Add("InstancePath1");
                InstancePaths.Add("InstancePath2");
                InstancePaths.Add("InstancePath3");
                InstancePaths.Add("InstancePath4");
                InstancePaths.Add("InstancePath5");
                //读取输出格式支持Object Json JsonStr 
                object[] o1 = ads.ReadJson(InstancePaths);
                object[] o2 = ads.ReadJsonStr(InstancePaths);
                object[] o3 = ads.ReadObject(InstancePaths);

                //单变量读取 读取输出格式支持Object Json JsonStr
                object o4 = ads.ReadJson("InstancePath");
                object o5 = ads.ReadJsonStr("InstancePath");
                object o6 = ads.ReadObject("InstancePath");

                //多变量写入 返回写入结果
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                keyValuePairs.Add("InstancePath1", "value");
                keyValuePairs.Add("InstancePath2", 1);
                keyValuePairs.Add("InstancePath3", true);
                keyValuePairs.Add("InstancePath4", 1.23);
                ads.SumWrite(keyValuePairs);

                //单变量写入 返回写入结果
                ads.SumWrite("InstancePath", "value");

                //释放ADSConnect
                ads.Dispose();
            }


        }
    }
}
