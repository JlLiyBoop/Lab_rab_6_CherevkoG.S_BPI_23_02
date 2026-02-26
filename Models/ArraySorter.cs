using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_rab_2_6__CherevkoG.S_BPI_23_02.Models
{
    public class SortResult
    {
        public int[] SortedArray { get; set; }
        public long Comparisons { get; set; }
        public double ElapsedMilliseconds { get; set; }
    }

    public class ArraySorter
    {
        private long _totalComparisons;
        private readonly object _locker = new object();

        public long TotalComparisons => _totalComparisons;

        public int[] GenerateRandomArray(int size)
        {
            Random rand = new Random();
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(1000);
            return array;
        }

        private int[] CopyArray(int[] source)
        {
            int[] copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        public Task<SortResult> BubbleSortAsync(int[] originalArray, IProgress<double> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();
                int n = array.Length;

                for (int i = 0; i < n - 1; i++)
                {
                    token.ThrowIfCancellationRequested();
                    for (int j = 0; j < n - 1 - i; j++)
                    {
                        comparisons++;
                        if (array[j] > array[j + 1])
                            (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    }
                    progress?.Report((double)(i + 1) / n * 100);
                }
                watch.Stop();
                lock (_locker) _totalComparisons += comparisons;
                return new SortResult { SortedArray = array, Comparisons = comparisons, ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds };
            }, token);
        }

        public Task<SortResult> InsertionSortAsync(int[] originalArray, IProgress<double> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();
                int n = array.Length;

                for (int i = 1; i < n; i++)
                {
                    token.ThrowIfCancellationRequested();
                    int key = array[i];
                    int j = i - 1;
                    while (j >= 0 && array[j] > key)
                    {
                        comparisons++;
                        array[j + 1] = array[j];
                        j--;
                    }
                    comparisons++;
                    array[j + 1] = key;
                    progress?.Report((double)(i + 1) / n * 100);
                }
                watch.Stop();
                lock (_locker) _totalComparisons += comparisons;
                return new SortResult { SortedArray = array, Comparisons = comparisons, ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds };
            }, token);
        }

        public Task<SortResult> HeapSortAsync(int[] originalArray, IProgress<double> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();
                int n = array.Length;
                double lastReportedProgress = -1;

                for (int i = n / 2 - 1; i >= 0; i--)
                    Heapify(array, n, i, ref comparisons, token);

                for (int i = n - 1; i > 0; i--)
                {
                    token.ThrowIfCancellationRequested();
                    (array[0], array[i]) = (array[i], array[0]);
                    Heapify(array, i, 0, ref comparisons, token);

                    double currentProgress = (double)(n - i) / n * 100;
                    if (currentProgress - lastReportedProgress >= 1.0)
                    {
                        progress?.Report(currentProgress);
                        lastReportedProgress = currentProgress;
                        Thread.Sleep(1);
                    }
                }

                watch.Stop();
                lock (_locker) _totalComparisons += comparisons;
                return new SortResult { SortedArray = array, Comparisons = comparisons, ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds };
            }, token);
        }

        private void Heapify(int[] arr, int n, int i, ref long comparisons, CancellationToken token)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n)
            {
                comparisons++;
                if (arr[left] > arr[largest]) largest = left;
            }
            if (right < n)
            {
                comparisons++;
                if (arr[right] > arr[largest]) largest = right;
            }
            if (largest != i)
            {
                (arr[i], arr[largest]) = (arr[largest], arr[i]);
                Heapify(arr, n, largest, ref comparisons, token);
            }
        }

        public Task<SortResult> QuickSortAsync(int[] originalArray, IProgress<double> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                long processed = 0;
                double lastReportedProgress = -1;
                var watch = Stopwatch.StartNew();

                QuickSortRecursive(array, 0, array.Length - 1, ref comparisons, ref processed, ref lastReportedProgress, progress, token, array.Length);

                watch.Stop();
                lock (_locker) _totalComparisons += comparisons;
                return new SortResult { SortedArray = array, Comparisons = comparisons, ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds };
            }, token);
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons, ref long processed, ref double lastReported, IProgress<double> progress, CancellationToken token, int total)
        {
            token.ThrowIfCancellationRequested();

            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right, ref comparisons, ref processed);

                double currentProgress = (double)processed / total * 100;
                if (currentProgress - lastReported >= 1.0)
                {
                    progress?.Report(currentProgress);
                    lastReported = currentProgress;
                    Thread.Sleep(1);
                }

                QuickSortRecursive(arr, left, pivotIndex - 1, ref comparisons, ref processed, ref lastReported, progress, token, total);
                QuickSortRecursive(arr, pivotIndex + 1, right, ref comparisons, ref processed, ref lastReported, progress, token, total);
            }
        }

        private int Partition(int[] arr, int left, int right, ref long comparisons, ref long processed)
        {
            int pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                comparisons++;
                processed++;
                if (arr[j] < pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            processed++;
            return i + 1;
        }
    }
}