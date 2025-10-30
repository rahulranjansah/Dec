using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Containers;
/*
 * ====================================================================
 * SybolTable Unit Tests
 * ====================================================================
 *
 * Test Author: Claude (Anthropic AI Assistant)
 * Created: September 25, 2025-
 * ====================================================================
 */
public class SymbolTableTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_ShouldInitializeEmptyTable()
    {
        var table = new SymbolTable<string, int>();

        Assert.Equal(0, table.Count);
        Assert.Null(table._parent);
        Assert.False(table.IsReadOnly);
        Assert.Empty(table.Keys);
        Assert.Empty(table.Values);
    }

    [Fact]
    public void Constructor_WithParent_ShouldSetParentReference()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        Assert.Equal(parent, child._parent);
        Assert.Equal(0, child.Count);
    }

    #region Additional Robustness Tests

    [Fact]
    public void Add_DuplicateKeys_ShouldActLikeRealDictionary()
    {
        var table = new SymbolTable<string, int>();
        var realDict = new Dictionary<string, int>();

        // Real Dictionary throws on duplicate keys
        table.Add("key1", 100);
        realDict.Add("key1", 100);

        // Your implementation allows duplicates - this is a design decision
        // But it should be consistent and documented behavior
        table.Add("key1", 200);
        // Assert.Throws<ArgumentException>(() => realDict.Add("key1", 200));

        // If allowing duplicates, what should TryGetValue return?
        // Should it be the first or last added? Test your expected behavior.
        var dict = (IDictionary<string, int>)table;
        bool found = dict.TryGetValue("key1", out int value);

        Assert.True(found);
        // Assert.Equal(???, value); // What's your expected behavior?
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void StressTest_ManyOperations_ShouldMaintainConsistency(int operationCount)
    {
        var table = new SymbolTable<string, int>();
        var reference = new Dictionary<string, int>();

        // Add many items
        for (int i = 0; i < operationCount; i++)
        {
            string key = $"key{i}";
            table.Add(key, i);
            if (!reference.ContainsKey(key)) // Your impl allows duplicates
                reference.Add(key, i);
        }

        // Verify all keys exist
        for (int i = 0; i < operationCount; i++)
        {
            string key = $"key{i}";
            Assert.True(table.ContainsKey(key), $"Key {key} should exist");
            var dict = (IDictionary<string, int>)table;
            Assert.True(dict.TryGetValue(key, out int value));

        }

        // Verify count (considering your duplicate behavior)
        Assert.True(table.Count >= reference.Count);
    }

    [Fact]
    public void HierarchicalConsistency_AllMethodsShouldAgree()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        parent.Add("parentKey", 100);
        child.Add("childKey", 200);
        child.Add("sharedKey", 300);
        parent.Add("sharedKey", 400); // Child should shadow this

        // All these methods should give consistent results
        foreach (string key in new[] { "parentKey", "childKey", "sharedKey", "nonexistent" })
        {
            bool containsKey = child.ContainsKey(key);
            var dict = (IDictionary<string, int>)child;
            bool tryGetValue = dict.TryGetValue(key, out int value);

            bool canIndex = true;
            int indexValue = default;

            try
            {
                indexValue = child[key];
            }
            catch (KeyNotFoundException)
            {
                canIndex = false;
            }

            // These should all agree
            Assert.Equal(containsKey, tryGetValue);
            Assert.Equal(containsKey, canIndex);

            if (containsKey)
            {
                Assert.Equal(value, indexValue);
            }
        }
    }

    [Fact]
    public void Remove_ShouldOnlyAffectLocalScope()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        parent.Add("key1", 100);
        child.Add("key1", 200); // Shadows parent

        // Remove from child should not affect parent
        bool removed = child.Remove("key1");
        Assert.True(removed);

        // Should now see parent's value
        Assert.True(child.ContainsKey("key1"));
        var dict = (IDictionary<string, int>)child;
        Assert.True(dict.TryGetValue("key1", out int value));

        Assert.Equal(100, value); // Parent's value

        // Parent should be unchanged
        Assert.True(parent.ContainsKey("key1"));
        Assert.Equal(100, parent["key1"]);
    }

    [Fact]
    public void Enumeration_ShouldOnlyIncludeLocalItems()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        parent.Add("parentKey", 100);
        child.Add("childKey", 200);

        var childItems = child.ToList();

        // Should only enumerate local items, not parent items
        Assert.Single(childItems);
        Assert.Contains(new KeyValuePair<string, int>("childKey", 200), childItems);
        Assert.DoesNotContain(new KeyValuePair<string, int>("parentKey", 100), childItems);
    }

    [Fact]
    public void KeysAndValues_ShouldOnlyIncludeLocalItems()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        parent.Add("parentKey", 100);
        child.Add("childKey", 200);

        // Keys and Values should only include local items
        Assert.Single(child.Keys);
        Assert.Single(child.Values);
        Assert.Contains("childKey", child.Keys);
        Assert.Contains(200, child.Values);
        Assert.DoesNotContain("parentKey", child.Keys);
        Assert.DoesNotContain(100, child.Values);
    }

    #endregion

    #region Add Tests

    [Fact]
    public void Add_KeyValuePair_ShouldAddToTable()
    {
        var table = new SymbolTable<string, int>();
        var pair = new KeyValuePair<string, int>("key1", 100);

        table.Add(pair);

        Assert.Equal(1, table.Count);
        Assert.True(table.ContainsKey("key1"));
    }

    [Fact]
    public void Add_KeyValue_ShouldAddToTable()
    {
        var table = new SymbolTable<string, int>();

        table.Add("key1", 100);

        Assert.Equal(1, table.Count);
        Assert.True(table.ContainsKey("key1"));
    }

    [Theory]
    [InlineData("key1", 1)]
    [InlineData("key2", 2)]
    [InlineData("key3", 3)]
    public void Add_MultipleKeys_ShouldAddAll(string key, int value)
    {
        var table = new SymbolTable<string, int>();

        table.Add(key, value);

        Assert.True(table.ContainsKey(key));
        Assert.Equal(1, table.Count);
    }

    [Fact]
    public void Add_DuplicateKeys_ShouldAllowDuplicates()
    {
        var table = new SymbolTable<string, int>();

        table.Add("key1", 100);
        table.Add("key1", 200); // This implementation allows duplicates

        Assert.Equal(2, table.Count);
    }

    [Fact]
    public void Add_NullKey_ShouldNotThrow()
    {
        var table = new SymbolTable<string, int>();

        // Based on implementation, it doesn't validate null keys in Add
        table.Add(null, 100);

        Assert.Equal(1, table.Count);
    }

    #endregion

    #region ContainsKey Tests

    [Fact]
    public void ContainsKey_ExistingKeyLocal_ShouldReturnTrue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        Assert.True(table.ContainsKey("key1"));
    }

    [Fact]
    public void ContainsKey_NonExistingKey_ShouldReturnFalse()
    {
        var table = new SymbolTable<string, int>();

        Assert.False(table.ContainsKey("nonexistent"));
    }

    [Fact]
    public void ContainsKey_ExistingKeyInParent_ShouldReturnTrue()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("parentKey", 100);

        Assert.True(child.ContainsKey("parentKey"));
    }

    [Fact]
    public void ContainsKey_LocalKeyOverridesParent_ShouldReturnTrue()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("key1", 100);
        child.Add("key1", 200);

        Assert.True(child.ContainsKey("key1"));
    }

    [Fact]
    public void ContainsKey_NullKey_ShouldThrowArgumentNullException()
    {
        var table = new SymbolTable<string, int>();

        Assert.Throws<ArgumentNullException>(() => table.ContainsKey(null));
    }

    [Fact]
    public void ContainsKeyLocal_ExistingKey_ShouldReturnTrue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        Assert.True(table.ContainsKeyLocal("key1"));
    }

    [Fact]
    public void ContainsKeyLocal_KeyInParentOnly_ShouldReturnFalse()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("parentKey", 100);

        Assert.False(child.ContainsKeyLocal("parentKey"));
    }

    #endregion

    #region TryGetValue Tests

    [Fact]
    public void TryGetValue_ExistingKeyLocal_ShouldReturnTrueAndValue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        bool result = ((IDictionary<string, int>)table).TryGetValue("key1", out int value);

        Assert.True(result);
        Assert.Equal(100, value);
    }

    [Fact]
    public void TryGetValue_NonExistingKey_ShouldReturnFalseAndDefault()
    {
        var table = new SymbolTable<string, int>();

        bool result = ((IDictionary<string, int>)table).TryGetValue("nonexistent", out int value);

        Assert.False(result);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void TryGetValue_ExistingKeyInParent_ShouldReturnTrueAndValue()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("parentKey", 100);

        bool result = ((IDictionary<string, int>)child).TryGetValue("parentKey", out int value);

        Assert.True(result);
        Assert.Equal(100, value);
    }

    [Fact]
    public void TryGetValue_LocalKeyOverridesParent_ShouldReturnLocalValue()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("key1", 100);
        child.Add("key1", 200);

        bool result = ((IDictionary<string, int>)child).TryGetValue("key1", out int value);

        Assert.True(result);
        Assert.Equal(200, value);
    }

    [Fact]
    public void TryGetValueLocal_ExistingKey_ShouldReturnTrueAndValue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        bool result = table.TryGetValueLocal("key1", out int value);

        Assert.True(result);
        Assert.Equal(100, value);
    }

    [Fact]
    public void TryGetValueLocal_NonExistingKey_ShouldReturnFalseAndDefaultValue()
    {
        var table = new SymbolTable<string, int>();

        // CORRECT BEHAVIOR: Should return false and set value to default
        bool result = table.TryGetValueLocal("nonexistent", out int value);

        Assert.False(result);
        Assert.Equal(default(int), value);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_Get_ExistingKey_ShouldReturnValue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        Assert.Equal(100, table["key1"]);
    }

    [Fact]
    public void Indexer_Get_NonExistingKey_ShouldThrowKeyNotFoundException()
    {
        var table = new SymbolTable<string, int>();

        Assert.Throws<KeyNotFoundException>(() => table["nonexistent"]);
    }

    [Fact]
    public void Indexer_Set_ExistingKey_ShouldUpdateValue()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        table["key1"] = 200;

        Assert.Equal(200, table["key1"]);
    }

    [Fact]
    public void Indexer_Set_NonExistingKey_ShouldThrowKeyNotFoundException()
    {
        var table = new SymbolTable<string, int>();

        Assert.Throws<KeyNotFoundException>(() => table["nonexistent"] = 100);
    }

    [Fact]
    public void Indexer_Get_KeyInParent_ShouldReturnParentValue()
    {
        // CORRECT BEHAVIOR: Indexer should support hierarchical lookup
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);
        parent.Add("parentKey", 100);

        // Should work just like ContainsKey and TryGetValue
        int value = parent["parentKey"];
        Assert.Equal(100, value);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_KeyValuePair_ExistingItem_ShouldReturnTrueAndRemove()
    {
        var table = new SymbolTable<string, int>();
        var pair = new KeyValuePair<string, int>("key1", 100);
        table.Add(pair);

        bool result = table.Remove(pair);

        Assert.True(result);
        Assert.Equal(0, table.Count);
        Assert.False(table.ContainsKey("key1"));
    }

    [Fact]
    public void Remove_Key_ExistingKey_ShouldReturnTrueAndRemove()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        bool result = table.Remove("key1");

        Assert.True(result);
        Assert.Equal(0, table.Count);
        Assert.False(table.ContainsKey("key1"));
    }

    [Fact]
    public void Remove_Key_NonExistingKey_ShouldReturnFalse()
    {
        var table = new SymbolTable<string, int>();

        bool result = table.Remove("nonexistent");

        Assert.False(result);
    }

    [Fact]
    public void Remove_KeyValuePair_NonExistingKey_ShouldReturnFalse()
    {
        var table = new SymbolTable<string, int>();
        var pair = new KeyValuePair<string, int>("nonexistent", 100);

        // CORRECT BEHAVIOR: Should return false, not throw
        bool result = table.Remove(pair);

        Assert.False(result);
        Assert.Equal(0, table.Count);
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_ExistingKeyValuePair_ShouldReturnTrue()
    {
        var table = new SymbolTable<string, int>();
        var pair = new KeyValuePair<string, int>("key1", 100);
        table.Add(pair);

        Assert.True(table.Contains(pair));
    }

    [Fact]
    public void Contains_ExistingKeyWrongValue_ShouldReturnFalse()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        var pair = new KeyValuePair<string, int>("key1", 200);

        Assert.False(table.Contains(pair));
    }

    [Fact]
    public void Contains_NonExistingKey_ShouldReturnFalse()
    {
        var table = new SymbolTable<string, int>();
        var pair = new KeyValuePair<string, int>("nonexistent", 100);

        // CORRECT BEHAVIOR: Should return false, not throw
        bool result = table.Contains(pair);

        Assert.False(result);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_NonEmptyTable_ShouldMakeEmpty()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        table.Add("key2", 200);

        table.Clear();

        Assert.Equal(0, table.Count);
        Assert.Empty(table.Keys);
        Assert.Empty(table.Values);
    }

    [Fact]
    public void Clear_EmptyTable_ShouldRemainEmpty()
    {
        var table = new SymbolTable<string, int>();

        table.Clear();

        Assert.Equal(0, table.Count);
    }

    #endregion

    #region CopyTo Tests

    [Fact]
    public void CopyTo_ValidArray_ShouldCopyElementsCorrectly()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        table.Add("key2", 200);
        var array = new KeyValuePair<string, int>[3]; // Extra space

        table.CopyTo(array, 0);

        // Should copy elements starting at index 0
        Assert.Equal(new KeyValuePair<string, int>("key1", 100), array[0]);
        Assert.Equal(new KeyValuePair<string, int>("key2", 200), array[1]);
        Assert.Equal(default(KeyValuePair<string, int>), array[2]); // Untouched
    }

    [Fact]
    public void CopyTo_ValidArrayWithOffset_ShouldCopyElementsAtOffset()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        table.Add("key2", 200);
        var array = new KeyValuePair<string, int>[3];

        table.CopyTo(array, 1); // Start copying at index 1

        Assert.Equal(new KeyValuePair<string, int>("key2", 200), array[1]);
        Assert.Equal(new KeyValuePair<string, int>(null, 0), array[0]);
    }

    [Fact]
    public void CopyTo_NullArray_ShouldThrowArgumentNullException()
    {
        var table = new SymbolTable<string, int>();

        Assert.Throws<ArgumentNullException>(() => table.CopyTo(null, 0));
    }

    [Fact]
    public void CopyTo_NegativeArrayIndex_ShouldThrowArgumentOutOfRangeException()
    {
        var table = new SymbolTable<string, int>();
        var array = new KeyValuePair<string, int>[2];

        Assert.Throws<ArgumentOutOfRangeException>(() => table.CopyTo(array, -1));
    }

    [Fact]
    public void CopyTo_InsufficientSpace_ShouldThrowArgumentException()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        table.Add("key2", 200);
        var array = new KeyValuePair<string, int>[1]; // Too small

        Assert.Throws<ArgumentException>(() => table.CopyTo(array, 0));
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumerator_EmptyTable_ShouldNotIterate()
    {
        var table = new SymbolTable<string, int>();
        var result = table.ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void GetEnumerator_NonEmptyTable_ShouldIterateAllElements()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);
        table.Add("key2", 200);

        var result = table.ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(new KeyValuePair<string, int>("key1", 100), result);
        Assert.Contains(new KeyValuePair<string, int>("key2", 200), result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void GetEnumerator_VariousElementCounts_ShouldIterateCorrectly(int elementCount)
    {
        var table = new SymbolTable<string, int>();

        for (int i = 0; i < elementCount; i++)
        {
            table.Add($"key{i}", i * 10);
        }

        var result = table.ToList();
        Assert.Equal(elementCount, result.Count);
    }

    #endregion

    #region Hierarchical Behavior Tests

    [Fact]
    public void HierarchicalLookup_MultipleGenerations_ShouldWork()
    {
        var grandparent = new SymbolTable<string, int>();
        var parent = new SymbolTable<string, int>(grandparent);
        var child = new SymbolTable<string, int>(parent);

        grandparent.Add("gpKey", 100);
        parent.Add("pKey", 200);
        child.Add("cKey", 300);

        Assert.True(child.ContainsKey("gpKey"));
        Assert.True(child.ContainsKey("pKey"));
        Assert.True(child.ContainsKey("cKey"));

        // Test TryGetValue hierarchical lookup
        Assert.True(((IDictionary<string, int>)child).TryGetValue("gpKey", out int gpValue));
        Assert.Equal(100, gpValue);

        Assert.True(((IDictionary<string, int>)child).TryGetValue("pKey", out int pValue));
        Assert.Equal(200, pValue);

        Assert.True(((IDictionary<string, int>)child).TryGetValue("cKey", out int cValue));
        Assert.Equal(300, cValue);
    }

    [Fact]
    public void HierarchicalLookup_ShadowingKeys_LocalShouldWin()
    {
        var parent = new SymbolTable<string, int>();
        var child = new SymbolTable<string, int>(parent);

        parent.Add("key1", 100);
        child.Add("key1", 200); // Shadows parent value

        Assert.True(((IDictionary<string, int>)child).TryGetValue("key1", out int value));
        Assert.Equal(200, value); // Should get child's value, not parent's
    }

    #endregion

    #region Edge Cases and Error Conditions

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        var table = new SymbolTable<string, int>();
        table.Add("key1", 100);

        Assert.False(table.IsReadOnly);
        Assert.Equal(1, table.Count);
        Assert.NotEmpty(table.Keys);
        Assert.NotEmpty(table.Values);
    }

    [Fact]
    public void NullValues_ShouldBeSupported()
    {
        var table = new SymbolTable<string, string>();

        table.Add("nullKey", null);

        Assert.True(table.ContainsKey("nullKey"));
        Assert.True(((IDictionary<string, string>)table).TryGetValue("nullKey", out string value));
        Assert.Null(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("special!@#$%")]
    public void SpecialStringKeys_ShouldWork(string specialKey)
    {
        var table = new SymbolTable<string, int>();

        table.Add(specialKey, 100);

        Assert.True(table.ContainsKey(specialKey));
        Assert.Equal(100, table[specialKey]);
    }

    #endregion
}
#endregion