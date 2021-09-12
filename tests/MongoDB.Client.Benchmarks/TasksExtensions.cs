namespace MongoDB.Client.Benchmarks
{
    public static class TasksExtensions
    {
        public static async Task WhenAllExt<T>(this Task<T>[] tasks)
        {
            if (tasks.Length == 1)
            {
                await tasks[0];
                return;
            }
            if (tasks.Length == 2)
            {
                await tasks[0];
                await tasks[1];
                return;
            }

            await Task.WhenAll(tasks);
        }
    }
}