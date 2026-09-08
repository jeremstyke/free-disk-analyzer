namespace FreeDiskAnalyzer.Core.Internal;

/// <summary>
/// Keeps only the top N largest items seen so far, without ever holding
/// every scanned item in memory. Items are buffered up to a capacity, then
/// trimmed down to <c>topCount</c> once the buffer is full. This keeps
/// memory bounded even when scanning a drive with millions of files.
/// </summary>
internal sealed class TopNTracker<T>
{
    private readonly List<T> _items = new();
    private readonly int _bufferCapacity;
    private readonly int _topCount;
    private readonly Comparison<T> _ascendingComparison;

    /// <param name="bufferCapacity">How many items to accumulate before trimming.</param>
    /// <param name="topCount">How many items to keep after trimming.</param>
    /// <param name="ascendingComparison">Comparison where a larger value means "should be kept" (e.g. by size ascending).</param>
    public TopNTracker(int bufferCapacity, int topCount, Comparison<T> ascendingComparison)
    {
        _bufferCapacity = Math.Max(bufferCapacity, topCount);
        _topCount = topCount;
        _ascendingComparison = ascendingComparison;
    }

    public void Offer(T item)
    {
        _items.Add(item);
        if (_items.Count >= _bufferCapacity)
        {
            Trim();
        }
    }

    private void Trim()
    {
        _items.Sort((a, b) => _ascendingComparison(b, a)); // descending
        if (_items.Count > _topCount)
        {
            _items.RemoveRange(_topCount, _items.Count - _topCount);
        }
    }

    public IReadOnlyList<T> GetTopDescending()
    {
        Trim();
        return _items.ToList();
    }
}
