using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisPositionInfo
{
    public class SingleAxisInfo
    {
        public SingleAxisInfo(string axisname, double position, int speed=100, int rank = 1)
        {
            this.AxisName = axisname;
            this.Position = position;
            this.Speed = speed;
            this.Rank = rank;
        }
        /// <summary>
        /// the axis name 
        /// </summary>
        public string AxisName { get;private set; }

        /// <summary>
        /// the axis absolute position
        /// </summary>
        public double Position { get; private set; }

        /// <summary>
        /// the axis speed move to the position
        /// </summary>
        public int Speed { get; private set; } 

        /// <summary>
        /// the axis move index
        /// </summary>
        public int Rank { get; private set; } 
    }
    public class PointInfo
    {
        string pointName = string.Empty;
        string aliger = string.Empty;
        List<SingleAxisInfo> axisInfos = new List<SingleAxisInfo>();
        public List<SingleAxisInfo> AxisInfos
        {
            get { return axisInfos; }
            set { axisInfos = value; }
        }
        /// <summary>
        /// 点位名称
        /// </summary>
        public string PointName
        {
            get { return pointName; }
            set { pointName = value; }
        }
        /// <summary>
        /// 所在逻辑组
        /// </summary>
        public string Aliger
        {
            get { return aliger; }
            set { aliger = value; }
        }
        public void Save(string Path,string filename)
        {
            if(!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            AxisInfos = AxisInfos.OrderBy(item => item.Rank).ToList<SingleAxisInfo>();
            string serializeInfo = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            File.WriteAllText(Path + $"\\{pointName}.json", serializeInfo);
        }
    }
}
