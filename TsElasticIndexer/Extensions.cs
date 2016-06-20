using System;

namespace TsElasticIndexer
{
    public static class Extensions
    {
        public static int ToEpoch(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1);
            var epochTimeSpan = date - epoch;
            return (int)epochTimeSpan.TotalSeconds;
        }
    }
}
