namespace MongoDB.Client.Tests.Models
{
    public interface ISeeder<T>
    {
        IEnumerable<T> GenerateSeed(SeederOptions options);
    }

    public record struct SeederOptions
    {
        private SeederOptions(uint count, bool lazy, bool infinite)
        {
            Count = count;
            Lazy = lazy;
            Infinite = infinite;
        }

        public uint Count { get; } = 1;
        public bool Lazy { get; }
        public bool Infinite { get; }

        public static SeederOptions Create(uint count)
        {
            return new SeederOptions(count, false, false);
        }

        public static SeederOptions CreateLazy(uint count)
        {
            return new SeederOptions(count, true, false);
        }

        public static SeederOptions CreateInfinite()
        {
            return new SeederOptions(0, true, true);
        }
    }
}
