namespace RedisFusion.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RedisFusionOutputCache : Attribute
    {
        public int durationInSeconds { get; }

        public RedisFusionOutputCache(int DurationInSeconds = 0)
        {
            durationInSeconds = DurationInSeconds;
        }
    }
}
