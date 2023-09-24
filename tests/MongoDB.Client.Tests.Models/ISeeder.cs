namespace MongoDB.Client.Tests.Models;

internal interface ISeeder<T>
{
    IEnumerable<T> GenerateSeed(int count);
}
