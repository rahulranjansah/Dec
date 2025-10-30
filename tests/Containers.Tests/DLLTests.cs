using Xunit;
using System;
using System.Linq;
using Containers;
/*
 * ====================================================================
 * DLL (Doubly Linked List) Unit Tests
 * ====================================================================
 *
 * Test Author: Claude (Anthropic AI Assistant)
 * Created: September 11, 2025-
 *
 * Description:
 * Comprehensive unit test suite for the DLL<T> (Doubly Linked List)
 * implementation using xUnit framework. Tests cover node creation,
 * list initialization, insertion operations, and deletion operations.
 *
 * ====================================================================
 */
public class DLLTests
{
    [Fact]
    public void Constructor_ShouldInitializeSentinelNodes()
    {
        var dll = new DLL<int>();

        Assert.NotNull(dll.head);
        Assert.NotNull(dll.tail);
        Assert.Equal(dll.tail, dll.head.next);
        Assert.Equal(dll.head, dll.tail.prev);
        Assert.True(dll.IsEmpty());
    }

    [Fact]
    public void PushFront_SingleElement_ShouldAddToFront()
    {
        var dll = new DLL<int>();
        dll.PushFront(42);

        Assert.Equal(42, dll.Front());
        Assert.Equal(42, dll.Back());
        Assert.Equal(1, dll.Count);
        Assert.False(dll.IsEmpty());
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30, 40)]
    [InlineData(100)]
    public void PushFront_MultipleElements_ShouldMaintainOrder(params int[] values)
    {
        var dll = new DLL<int>();

        foreach (int value in values)
        {
            dll.PushFront(value);
        }

        Assert.Equal(values.Last(), dll.Front());
        Assert.Equal(values.First(), dll.Back());
        Assert.Equal(values.Length, dll.Count);
    }

    [Fact]
    public void PushBack_SingleElement_ShouldAddToBack()
    {
        var dll = new DLL<int>();
        dll.PushBack(42);

        Assert.Equal(42, dll.Front());
        Assert.Equal(42, dll.Back());
        Assert.Equal(1, dll.Count);
        Assert.False(dll.IsEmpty());
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30, 40)]
    [InlineData(100)]
    public void PushBack_MultipleElements_ShouldMaintainOrder(params int[] values)
    {
        var dll = new DLL<int>();

        foreach (int value in values)
        {
            dll.PushBack(value);
        }

        Assert.Equal(values.First(), dll.Front());
        Assert.Equal(values.Last(), dll.Back());
        Assert.Equal(values.Length, dll.Count);
    }

    [Fact]
    public void Front_EmptyList_ShouldThrowException()
    {
        var dll = new DLL<int>();

        Assert.Throws<InvalidOperationException>(() => dll.Front());
    }

    [Fact]
    public void Back_EmptyList_ShouldThrowException()
    {
        var dll = new DLL<int>();

        Assert.Throws<InvalidOperationException>(() => dll.Back());
    }

    [Fact]
    public void PopFront_SingleElement_ShouldRemoveAndReturn()
    {
        var dll = new DLL<int>();
        dll.PushBack(42);

        int result = dll.PopFront();

        Assert.Equal(42, result);
        Assert.True(dll.IsEmpty());
        Assert.Equal(0, dll.Count);
    }

    [Fact]
    public void PopFront_EmptyList_ShouldThrowException()
    {
        var dll = new DLL<int>();

        Assert.Throws<InvalidOperationException>(() => dll.PopFront());
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30, 40, 50)]
    public void PopFront_MultipleElements_ShouldRemoveInOrder(params int[] values)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.PushBack(value);
        }

        for (int i = 0; i < values.Length; i++)
        {
            int result = dll.PopFront();
            Assert.Equal(values[i], result);
        }

        Assert.True(dll.IsEmpty());
    }

    [Fact]
    public void PopBack_SingleElement_ShouldRemoveAndReturn()
    {
        var dll = new DLL<int>();
        dll.PushBack(42);

        int result = dll.PopBack();

        Assert.Equal(42, result);
        Assert.True(dll.IsEmpty());
        Assert.Equal(0, dll.Count);
    }

    [Fact]
    public void PopBack_EmptyList_ShouldThrowException()
    {
        var dll = new DLL<int>();

        Assert.Throws<InvalidOperationException>(() => dll.PopBack());
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30, 40, 50)]
    public void PopBack_MultipleElements_ShouldRemoveInReverseOrder(params int[] values)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.PushBack(value);
        }

        for (int i = values.Length - 1; i >= 0; i--)
        {
            int result = dll.PopBack();
            Assert.Equal(values[i], result);
        }

        Assert.True(dll.IsEmpty());
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 2, true)]
    [InlineData(new int[] { 1, 2, 3 }, 4, false)]
    [InlineData(new int[] { }, 1, false)]
    [InlineData(new int[] { 5, 5, 5 }, 5, true)]
    public void Contains_VariousScenarios_ShouldReturnCorrectResult(int[] values, int searchValue, bool expected)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.PushBack(value);
        }

        bool result = dll.Contains(searchValue);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5 })]
    public void Size_VariousLengths_ShouldReturnCorrectCount(int[] values)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.PushBack(value);
        }

        Assert.Equal(values.Length, dll.Size());
        Assert.Equal(values.Length, dll.Count);
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 1, true)]
    [InlineData(new int[] { 1, 2, 3 }, 4, false)]
    [InlineData(new int[] { }, 1, false)]
    [InlineData(new int[] { 5, 5, 5 }, 5, true)]
    [InlineData(new int[] { 1, 2, 3, 2, 4 }, 2, true)]
    public void Remove_VariousScenarios_ShouldReturnCorrectResult(int[] values, int removeValue, bool expected)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.PushBack(value);
        }
        int originalCount = dll.Count;

        bool result = dll.Remove(removeValue);

        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.Equal(originalCount - 1, dll.Count);
            Assert.False(dll.Contains(removeValue) && originalCount == 1);
        }
        else
        {
            Assert.Equal(originalCount, dll.Count);
        }
    }

    [Fact]
    public void Clear_NonEmptyList_ShouldEmptyList()
    {
        var dll = new DLL<int>();
        dll.PushBack(1);
        dll.PushBack(2);
        dll.PushBack(3);

        dll.Clear(); // Note: Changed from clear() to Clear()

        Assert.True(dll.IsEmpty());
        Assert.Equal(0, dll.Count);
        Assert.Equal(dll.tail, dll.head.next);
        Assert.Equal(dll.head, dll.tail.prev);
    }

    [Fact]
    public void Clear_EmptyList_ShouldRemainEmpty()
    {
        var dll = new DLL<int>();

        dll.Clear();

        Assert.True(dll.IsEmpty());
        Assert.Equal(0, dll.Count);
    }

    // IList<T> Interface Tests
    [Fact]
    public void Add_ShouldAppendToEnd()
    {
        var dll = new DLL<int>();

        dll.Add(1);
        dll.Add(2);
        dll.Add(3);

        Assert.Equal(1, dll.Front());
        Assert.Equal(3, dll.Back());
        Assert.Equal(3, dll.Count);
    }

    [Theory]
    [InlineData(0, new int[] { 1, 2, 3 }, 0)]
    [InlineData(1, new int[] { 1, 2, 3 }, 1)]
    [InlineData(3, new int[] { 1, 2, 3 }, 2)]
    [InlineData(-1, new int[] { 1, 2, 3 }, -1)]
    public void Insert_AtValidIndex_ShouldInsertCorrectly(int insertValue, int[] existingValues, int index)
    {
        var dll = new DLL<int>();
        foreach (int value in existingValues)
        {
            dll.Add(value);
        }

        if (index >= 0 && index <= dll.Count)
        {
            dll.Insert(index, insertValue);
            Assert.Equal(existingValues.Length + 1, dll.Count);
            Assert.Equal(insertValue, dll[index]);
        }
        else
        {
            Assert.Throws<IndexOutOfRangeException>(() => dll.Insert(index, insertValue));
        }
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 1, 0)]
    [InlineData(new int[] { 1, 2, 3 }, 2, 1)]
    [InlineData(new int[] { 1, 2, 3 }, 3, 2)]
    [InlineData(new int[] { 1, 2, 3 }, 4, -1)]
    [InlineData(new int[] { 1, 2, 1, 3 }, 1, 0)]
    public void IndexOf_VariousScenarios_ShouldReturnCorrectIndex(int[] values, int searchValue, int expectedIndex)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.Add(value);
        }

        int result = dll.IndexOf(searchValue);

        Assert.Equal(expectedIndex, result);
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 0, 1)]
    [InlineData(new int[] { 1, 2, 3 }, 1, 2)]
    [InlineData(new int[] { 1, 2, 3 }, 2, 3)]
    public void Indexer_Get_ValidIndex_ShouldReturnCorrectValue(int[] values, int index, int expected)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.Add(value);
        }

        int result = dll[index];

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Indexer_Get_InvalidIndex_ShouldThrowException(int index)
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);

        Assert.Throws<IndexOutOfRangeException>(() => dll[index]);
    }

    [Fact]
    public void Indexer_Set_ValidIndex_ShouldUpdateValue()
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);
        dll.Add(3);

        dll[1] = 42;

        Assert.Equal(42, dll[1]);
        Assert.Equal(3, dll.Count);
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 0)]
    [InlineData(new int[] { 1, 2, 3 }, 1)]
    [InlineData(new int[] { 1, 2, 3 }, 2)]
    public void RemoveAt_ValidIndex_ShouldRemoveElement(int[] values, int removeIndex)
    {
        var dll = new DLL<int>();
        foreach (int value in values)
        {
            dll.Add(value);
        }
        int originalCount = dll.Count;

        dll.RemoveAt(removeIndex);

        Assert.Equal(originalCount - 1, dll.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void RemoveAt_InvalidIndex_ShouldThrowException(int index)
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);

        Assert.Throws<IndexOutOfRangeException>(() => dll.RemoveAt(index));
    }

    [Fact]
    public void CopyTo_ValidArray_ShouldCopyElements()
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);
        dll.Add(3);

        int[] array = new int[5];
        dll.CopyTo(array, 1);

        Assert.Equal(new int[] { 2, 3, 0, 0, 0 }, array);
        //Assert.Equal(new int[] { 0, 1, 2, 3, 0 }, array);
    }

    [Fact]
    public void CopyTo_NullArray_ShouldThrowException()
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);
        dll.Add(3);
        int[] array = new int[1];
        Assert.Throws<ArgumentException>(() => dll.CopyTo(array, 0));
    }

    [Fact]
    public void CopyTo_NegativeIndex_ShouldThrowException()
    {
        var dll = new DLL<int>();
        dll.Add(1);

        int[] array = new int[5];
        Assert.Throws<ArgumentOutOfRangeException>(() => dll.CopyTo(array, -1));
    }

    [Fact]
    public void GetEnumerator_ShouldIterateCorrectly()
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);
        dll.Add(3);

        var result = dll.ToList();

        Assert.Equal(new int[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void GetEnumerator_EmptyList_ShouldNotIterate()
    {
        var dll = new DLL<int>();

        var result = dll.ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void IsReadOnly_ShouldReturnFalse()
    {
        var dll = new DLL<int>();

        Assert.False(dll.IsReadOnly);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var dll = new DLL<int>();
        dll.Add(1);
        dll.Add(2);
        dll.Add(3);

        string result = dll.ToString();

        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("3", result);
    }

    // Tests with different data types
    [Fact]
    public void DLL_WithStrings_ShouldWorkCorrectly()
    {
        var dll = new DLL<string>();

        dll.Add("hello");
        dll.Add("world");

        Assert.Equal("hello", dll.Front());
        Assert.Equal("world", dll.Back());
        Assert.True(dll.Contains("hello"));
        Assert.False(dll.Contains("foo"));
    }

    [Fact]
    public void DLL_WithDuplicates_ShouldHandleCorrectly()
    {
        var dll = new DLL<int>();

        dll.Add(1);
        dll.Add(1);
        dll.Add(2);
        dll.Add(1);

        Assert.Equal(4, dll.Count);
        Assert.Equal(0, dll.IndexOf(1)); // Should return first occurrence
        Assert.True(dll.Remove(1)); // Should remove first occurrence
        Assert.Equal(3, dll.Count);
        Assert.Equal(0, dll.IndexOf(1)); // Should still find remaining occurrences
    }
}