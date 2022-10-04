// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Concurrent;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("type an integer...");
        double sum;
        while (!double.TryParse(Console.ReadLine(), out sum))
        {
            Console.WriteLine("can´t parse to double, try again...");
        }
        //creates 100001 numbers from -50000 to 50000 in random order
        var list = TestClass.UniqueRandomDouble(-50000, 50000).ToArray<double>();
        //returns a tuple with the values that meet the required sum
        //list = new double[] { 1, 9, 5, 0, 20, -4, 12, 16, 7 };
        var result = TestClass.SummedTuple(list, sum);
        //writes all the values on the screen
        result.ForEach(x =>
        {
            if(x.Item1 + x.Item2 != sum)
            {
                //if the validation isn´t correct an exception will be throw
                throw new Exception("was calculated incorrectly");
            }
            Console.WriteLine(x.Item1 + "," + x.Item2);
        }
        );
        Console.WriteLine("return to finish");
        Console.ReadLine();

        // uncoment this line and comment every above to benchmark the method
        //var summary = BenchmarkRunner.Run<BenchMarkClass>();

    }

}

public static class TestClass
{
    private const int taskLimit = 4000;

    public static List<Tuple<double, double>> SummedTuple(double[] numsArr, double summed)
    {
        SemaphoreSlim maxThread = new SemaphoreSlim(taskLimit);
        var results = new ConcurrentQueue<Tuple<double, double>>();
        var count = numsArr.Length;
        for (int x = 0; x < count; x++)
        {
            maxThread.Wait();
            Task.Factory.StartNew(() => results.SecondLoop(count, x, numsArr, summed)
            , TaskCreationOptions.LongRunning)
            .ContinueWith((task) => maxThread.Release());
        }
        return results.ToList();
    }

    public static void SecondLoop(this ConcurrentQueue<Tuple<double, double>> Queue, int count, int x, double[] numsArr, double summed)
    {
        for (int y = x; y < count; y++)
        {
            if (x == y) continue;
            Queue.EvaluateAndAsign(numsArr[x], numsArr[y], summed);
        }
    }

    public static void EvaluateAndAsign(this ConcurrentQueue<Tuple<double, double>> Queue, double valueOne, double valueTwo, double summed)
    {
        if ((valueOne + valueTwo) == summed)
        {
            Queue.Enqueue(new Tuple<double, double>(valueOne, valueTwo));
        }
    }

    public static IEnumerable<double> UniqueRandomDouble(int minInclusive, int maxInclusive)
    {
        List<double> candidates = new List<double>();
        for (int i = minInclusive; i <= maxInclusive; i++)
        {
            candidates.Add(i);
        }
        Random rnd = new Random();
        while (candidates.Count > 0)
        {
            int index = rnd.Next(candidates.Count);
            yield return candidates[index];
            candidates.RemoveAt(index);
        }
    }
}

public class BenchMarkClass
{
    private readonly double[] nums = TestClass.UniqueRandomDouble(-50000, 50000).ToArray<double>();

    [Benchmark]
    public void SummedTuple()
    {
        TestClass.SummedTuple(nums, 12);
    }

}


