using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapInterfaces
{
    public static class Util
    {
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(0, Math.Min(max, value));
        }
    }

    public class Mapper
    {
        public byte[] fileDll;
        public string mapperName;

        public Mapper(byte[] fileDll, string mapperName)
        {
            this.fileDll = fileDll;
            this.mapperName = mapperName;
        }
    }

    public struct JobMap
    {
        public string url;
        public int splitBegin;
        public int splitEnd;

        public JobMap(string url, int splitBegin, int splitEnd)
        {
            this.url = url;
            this.splitBegin = splitBegin;
            this.splitEnd = splitEnd;
        }
    }

    public struct ProgressReport
    {
        public int percentage;
        public double timestamp;
        public double init;
        public string backerUrl;

        public ProgressReport(double init, double timestamp, int percentage, string backerUrl)
        {
            this.init = init;
            this.percentage = percentage;
            this.timestamp = timestamp;
            this.backerUrl = backerUrl;
        }
    }
}
