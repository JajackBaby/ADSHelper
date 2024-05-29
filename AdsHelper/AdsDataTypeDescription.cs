using System;
using System.Collections.Generic;
using TwinCAT.Ads;

namespace TcAdsHelper
{
    #region ADSType

    public class AdsDataTypeDescription
    {
        public List<string> Names { get; set; }
        public AdsDataTypeId AdsDataType { get; set; }
        public int Size { get; set; }
        public Func<byte[], object> FromBuffer { get; set; }
    }
    #endregion
}

