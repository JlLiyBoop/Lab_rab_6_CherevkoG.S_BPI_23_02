using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_rab_2_6__CherevkoG.S_BPI_23_02.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_rab_2_6__CherevkoG.S_BPI_23_02.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private int[] _originalArray;

        private CancellationTokenSource _bubbleCts;
        private CancellationTokenSource _quickCts;
        private CancellationTokenSource _insertionCts;
        private CancellationTokenSource _heapCts;

        [ObservableProperty] private int _arraySize = 10000;
        [ObservableProperty] private string _originalArrayString;

        [ObservableProperty] private string _bubbleSortResult;
        [ObservableProperty] private string _quickSortResult;
        [ObservableProperty] private string _insertionSortResult;
        [ObservableProperty] private string _heapSortResult;

        [ObservableProperty] private string _totalComparisons = "Общее число сравнений: 0";

        [ObservableProperty] private double _bubbleProgress;
        [ObservableProperty] private double _quickProgress;
        [ObservableProperty] private double _insertionProgress;
        [ObservableProperty] private double _heapProgress;

        [ObservableProperty] private string _bubblePreview;
        [ObservableProperty] private string _quickPreview;
        [ObservableProperty] private string _insertionPreview;
        [ObservableProperty] private string _heapPreview;

        public IAsyncRelayCommand GenerateArrayCommand { get; }
        public IAsyncRelayCommand BubbleSortCommand { get; }
        public IAsyncRelayCommand QuickSortCommand { get; }
        public IAsyncRelayCommand InsertionSortCommand { get; }
        public IAsyncRelayCommand HeapSortCommand { get; }
        public IAsyncRelayCommand SortAllCommand { get; }

        public IRelayCommand CancelAllCommand { get; }
        public IRelayCommand CancelBubbleCommand { get; }
        public IRelayCommand CancelQuickCommand { get; }
        public IRelayCommand CancelInsertionCommand { get; }
        public IRelayCommand CancelHeapCommand { get; }

        public MainViewModel()
        {
            _sorter = new ArraySorter();

            GenerateArrayCommand = new AsyncRelayCommand(GenerateArrayAsync);

            BubbleSortCommand = new AsyncRelayCommand(BubbleSortAsync, () => _originalArray != null);
            QuickSortCommand = new AsyncRelayCommand(QuickSortAsync, () => _originalArray != null);
            InsertionSortCommand = new AsyncRelayCommand(InsertionSortAsync, () => _originalArray != null);
            HeapSortCommand = new AsyncRelayCommand(HeapSortAsync, () => _originalArray != null);
            SortAllCommand = new AsyncRelayCommand(SortAllAsync, () => _originalArray != null);

            CancelAllCommand = new RelayCommand(CancelAll);

            CancelBubbleCommand = new RelayCommand(() => _bubbleCts?.Cancel());
            CancelQuickCommand = new RelayCommand(() => _quickCts?.Cancel());
            CancelInsertionCommand = new RelayCommand(() => _insertionCts?.Cancel());
            CancelHeapCommand = new RelayCommand(() => _heapCts?.Cancel());
        }

        private async Task GenerateArrayAsync()
        {
            await Task.Delay(100);

            _originalArray = _sorter.GenerateRandomArray(ArraySize);

            OriginalArrayString = "Исходный массив: " + FormatArray(_originalArray);
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            HeapSortCommand.NotifyCanExecuteChanged();
            SortAllCommand.NotifyCanExecuteChanged();

            ResetAll();
        }

        private async Task BubbleSortAsync()
        {
            ResetBubble();

            _bubbleCts = new CancellationTokenSource();
            var progress = new Progress<double>(p => BubbleProgress = p);

            BubbleSortResult = "Сортируется...";

            try
            {
                var result = await _sorter.BubbleSortAsync(
                    _originalArray,
                    progress,
                    _bubbleCts.Token);
                BubblePreview = "Результат: " + FormatArray(result.SortedArray);
                BubbleSortResult =
                    $"Пузырьковая: {FormatArray(result.SortedArray)}, " +
                    $"время: {result.ElapsedMilliseconds:F2} мс, " +
                    $"сравнений: {result.Comparisons}";

                UpdateTotal();
            }
            catch (OperationCanceledException)
            {
                ResetBubble();
            }
        }

        private async Task QuickSortAsync()
        {
            ResetQuick();

            _quickCts = new CancellationTokenSource();
            var progress = new Progress<double>(p => QuickProgress = p);

            QuickSortResult = "Сортируется...";

            try
            {
                var result = await _sorter.QuickSortAsync(
                    _originalArray,
                    progress,
                    _quickCts.Token);
                QuickPreview = "Результат: " + FormatArray(result.SortedArray);
                QuickSortResult =
                    $"Быстрая: {FormatArray(result.SortedArray)}, " +
                    $"время: {result.ElapsedMilliseconds:F2} мс, " +
                    $"сравнений: {result.Comparisons}";

                UpdateTotal();
            }
            catch (OperationCanceledException)
            {
                ResetQuick();
            }
        }

        private async Task InsertionSortAsync()
        {
            ResetInsertion();

            _insertionCts = new CancellationTokenSource();
            var progress = new Progress<double>(p => InsertionProgress = p);

            InsertionSortResult = "Сортируется...";

            try
            {
                var result = await _sorter.InsertionSortAsync(
                    _originalArray,
                    progress,
                    _insertionCts.Token);
                InsertionPreview = "Результат: " + FormatArray(result.SortedArray);
                InsertionSortResult =
                    $"Вставками: {FormatArray(result.SortedArray)}, " +
                    $"время: {result.ElapsedMilliseconds:F2} мс, " +
                    $"сравнений: {result.Comparisons}";

                UpdateTotal();
            }
            catch (OperationCanceledException)
            {
                ResetInsertion();
            }
        }

        private async Task HeapSortAsync()
        {
            ResetHeap();

            _heapCts = new CancellationTokenSource();
            var progress = new Progress<double>(p => HeapProgress = p);

            HeapSortResult = "Сортируется...";

            try
            {
                var result = await _sorter.HeapSortAsync(
                    _originalArray,
                    progress,
                    _heapCts.Token);
                HeapPreview = "Результат: " + FormatArray(result.SortedArray);
                HeapSortResult =
                    $"Пирамидальная: {FormatArray(result.SortedArray)}, " +
                    $"время: {result.ElapsedMilliseconds:F2} мс, " +
                    $"сравнений: {result.Comparisons}";

                UpdateTotal();
            }
            catch (OperationCanceledException)
            {
                ResetHeap();
            }
        }

        private void CancelAll()
        {
            _bubbleCts?.Cancel();
            _quickCts?.Cancel();
            _insertionCts?.Cancel();
            _heapCts?.Cancel();

            ResetAll();
        }

        private void ResetBubble()
        {
            BubbleProgress = 0;
            BubbleSortResult = null;
        }

        private void ResetQuick()
        {
            QuickProgress = 0;
            QuickSortResult = null;
        }

        private void ResetInsertion()
        {
            InsertionProgress = 0;
            InsertionSortResult = null;
        }

        private void ResetHeap()
        {
            HeapProgress = 0;
            HeapSortResult = null;
        }

        private void ResetAll()
        {
            ResetBubble();
            ResetQuick();
            ResetInsertion();
            ResetHeap();

            TotalComparisons = "Общее число сравнений: 0";
        }
        private async Task SortAllAsync()
        {
            var bubbleTask = BubbleSortAsync();
            var quickTask = QuickSortAsync();
            var insertionTask = InsertionSortAsync();
            var heapTask = HeapSortAsync();

            await Task.WhenAll(bubbleTask, quickTask, insertionTask, heapTask);
        }

        private void UpdateTotal()
        {
            TotalComparisons =
                $"Общее число сравнений: {_sorter.TotalComparisons}";
        }

        private string FormatArray(int[] arr)
        {
            if (arr == null || arr.Length == 0) return "пусто";
            int take = Math.Min(arr.Length, 10);
            var elements = arr.Take(take);
            string baseStr = string.Join(", ", elements);
            return arr.Length > 10 ? $"{baseStr}..." : baseStr;
        }
    }
}