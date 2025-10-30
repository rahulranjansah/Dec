using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
// Comments added by Claude (Anthropic) for improved code readability

/// <summary>
/// A hierarchical symbol table that supports parent-child relationships for scope resolution
/// </summary>
///
namespace Containers
{
   public class SymbolTable<TKey, TValue> : IDictionary<TKey, TValue> //make parent proerty
{
    // Parallel data structures to maintain key-value pairs
    private DLL<TKey> _keys;
    private DLL<TValue> _values;
    private int _size;
    public SymbolTable<TKey, TValue> _parent;

    public SymbolTable()
    {
        this._keys = new DLL<TKey>();
        this._values = new DLL<TValue>();
        this._size = 0;
        this._parent = null;
    }

    public SymbolTable(SymbolTable<TKey, TValue> parent)
    {
        this._parent = parent;
        this._keys = new DLL<TKey>();
        this._values = new DLL<TValue>();
        this._size = 0;
    }

    /// <summary>
    /// Implements hierarchical lookup: searches local table first, then parent chain
    /// </summary>
    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
    {
        TValue LocalValue = default(TValue);

        // First check local scope
        if (this.TryGetValueLocal(key, out LocalValue))
        {
            value = LocalValue;
            return true;
        }

        // If not found locally, recurse up the parent chain
        if (this._parent != null)
        {
            return ((IDictionary<TKey, TValue>)this._parent).TryGetValue(key, out value);
        }

        // Key not found in any scope
        value = default(TValue);
        return false;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null)
            throw new ArgumentNullException(nameof(item), "Key cannot be null");

        if (this.ContainsKeyLocal(item.Key))
            throw new ArgumentException($"Key '{item.Key}' already exists in the symbol table"); //rethink

        // Add to parallel data structures and maintain size
        this._keys.Add(item.Key);
        this._values.Add(item.Value);
        this._size++;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) //Needs to Check Parent, look at contains(T item) for reference
    {
        int index = this._keys.IndexOf(item.Key);
        if (index == -1) return false;
        TValue value = this._values[index];
        return index != -1 && value.Equals(item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int Index = 0;
        if (array == null) throw new ArgumentNullException();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException();
        if (array.Length < this._size - arrayIndex) throw new ArgumentException();

        // Copy key-value pairs from parallel structures to array
        while (Index < this._size)
        {
            if (Index >= arrayIndex)
            {
                array[Index] = new KeyValuePair<TKey, TValue>(this._keys[Index], this._values[Index]);
                Index++;
            }
            else Index = Index + 1;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int index = this._keys.IndexOf(item.Key);
        if (index == -1) return false;

        // Remove from both parallel structures at same index
        this._keys.RemoveAt(index);
        this._values.RemoveAt(index);
        this._size--;
        return true;
    }

    /// <summary>
    /// Iterator that yields key-value pairs from parallel data structures
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() //use getenum from symbol tables and just step once for O(1).
    {
        int index = 0;
        while (index < this._size)
        {
            yield return new KeyValuePair<TKey, TValue>(this._keys[index], this._values[index]);
            index++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(TKey key, TValue value) //check for duplicate keys!!
    {
        this._keys.Add(key);
        this._values.Add(value);
        this._size++;
    }

    /// <summary>
    /// Hierarchical key search: checks local scope first, then parent chain
    /// </summary>
    public bool ContainsKey(TKey key)
    {
        if (this._parent == null)
        {
            return ContainsKeyLocal(key);
        }
        else if (ContainsKeyLocal(key))
        {
            return true;
        }
        // Recursively search parent scopes
        return this._parent.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        int index = this._keys.IndexOf(key);
        if (index != -1)
        {
            // Remove from both parallel structures and update size
            this._keys.RemoveAt(index);
            this._values.RemoveAt(index);
            this._size--;
            return true;
        }
        else return false;
    }

    public void Clear()
    {
        this._keys.Clear();
        this._values.Clear();
        this._size = 0;
    }

    public ICollection<TKey> Keys => this._keys; //properties got at the top by constructors, like instance vars

    public ICollection<TValue> Values => this._values;

    public int Count => this._size;
    public bool IsReadOnly => false;

    /// <summary>
    /// Indexer with hierarchical lookup for get, local-only for set
    /// </summary>
    public TValue this[TKey key]
    {
        get
        {
            TValue value;
            // Uses hierarchical TryGetValue for lookup
            if (((IDictionary<TKey, TValue>)this).TryGetValue(key, out value))
            {
                return value;
            }
            throw new KeyNotFoundException($"Key '{key}' not found");
        }

        set
        {
            // Set operation only works on local scope
            int index = this._keys.IndexOf(key);
            if (index == -1) throw new KeyNotFoundException($"Key '{key}' not found");
            this._values[index] = value;
        }
    }

    /// <summary>
    /// Local-only key search (does not check parent scopes)
    /// </summary>
    public bool ContainsKeyLocal(TKey key)
    {
        if (key == null) throw new ArgumentNullException("Null key");
        return this._keys.IndexOf(key) != -1;
    }

    /// <summary>
    /// Local-only value lookup (does not check parent scopes)
    /// </summary>
    public bool TryGetValueLocal(TKey key, out TValue value)
    {
        int index = this._keys.IndexOf(key);
        if (index != -1)
        {
            // Key found at this index in both parallel structures
            value = this._values[index];
            return true;
        }
        value = default(TValue);
        return false;
    }
}
}
