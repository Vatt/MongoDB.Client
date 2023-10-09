using static MongoDB.Client.Tests.Models.DatabaseSeeder;

namespace MongoDB.Client.Tests.Models;

public abstract class SeederBase<T> : ISeeder<T>
{
    public IEnumerable<T> GenerateSeed(SeederOptions options)
    {
        uint count = options.Count;
        uint counter = 0;

        while (options.Infinite || counter < count)
        {
            yield return Create(counter++);
        }
    }

    protected abstract T Create(uint i);
}
