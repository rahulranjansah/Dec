using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
//Zach B & Rick K

public class DLL<T> : IEnumerable<T>, IList<T>
{


    //Change Dnode visibility
    public class DNode
    {
        public DNode? prev;
        public T? value;
        public DNode? next;

        public DNode(DNode? prev_node, T? value_node, DNode? next_node)
        {
            this.prev = prev_node;
            this.value = value_node;
            this.next = next_node;
        }


    }
    public DNode? head;
    public DNode? tail;
    public int size = 0;
    public DLL()
    {
        this.head = new DNode(null, default, null);
        this.tail = new DNode(null, default, null);

        this.head.next = this.tail;
        this.tail.prev = this.head;

    }
    private void Insert(DNode node, T item)
    {
        DNode new_node = new DNode(node.prev, item, node);
        node.prev.next = new_node;
        node.prev = new_node;
        size++;
    }

    private void Remove(DNode node)
    {
        node.prev.next = node.next;
        node.next.prev = node.prev;
        size--;
    }

    private DNode GetNode(int index)
    {
        if (index < 0 || this.head.next == this.tail) throw new IndexOutOfRangeException(nameof(index));
        DNode CurrentNode = this.head.next;
        while (index >= 1)
        {
            CurrentNode = CurrentNode.next;
            if (CurrentNode.next == null) throw new IndexOutOfRangeException(nameof(index));
            index--;
        }
        return CurrentNode;
    }
    public bool Contains(T item)
    {
        DNode CurrentNode = this.head.next;
        while (CurrentNode.next != null)
        {
            if (EqualityComparer<T>.Default.Equals(CurrentNode.value, item)) return true;
            else CurrentNode = CurrentNode.next;
        }
        return false;
    }

    public int Size()
    {
        return size;
    }

    // public String ToString()
    // {
    //     string ReturnString = "";
    //     DNode CurrentNode = this.head.next;
    //     while (CurrentNode.next != null)
    //     {
    //         ReturnString += $"{CurrentNode.value}, ";
    //         CurrentNode = CurrentNode.next;
    //     }
    //     return ReturnString;
    // }

    public bool Remove(T item)
    {
        DNode CurrentNode = this.head.next;
        while (CurrentNode.next != null)
        {
            if (EqualityComparer<T>.Default.Equals(CurrentNode.value, item))
            {
                Remove(CurrentNode);
                return true;
            }
            CurrentNode = CurrentNode.next;
        }
        return false;


    }

    public T Front()
    {
        if (this.head.next == this.tail) throw new InvalidOperationException("This list is empty");
        else return this.head.next.value;
    }

    public T Back()
    {
        if (this.head.next == this.tail) throw new InvalidOperationException("This list is empty");
        else return this.tail.prev.value;
    }

    public void PushFront(T item)
    {
        Insert(this.head.next, item);
    }

    public void PushBack(T item)
    {
        Insert(this.tail, item);
    }

    public T PopFront()
    {
        if (this.head.next == this.tail) throw new InvalidOperationException("This list is empty");
        T PopValue = this.head.next.value;
        Remove(this.head.next);
        return PopValue;
    }

    public T PopBack()
    {
        if (this.head.next == this.tail) throw new InvalidOperationException("This list is empty");
        T PopValue = this.tail.prev.value;
        Remove(this.tail.prev);
        return PopValue;
    }

    public void Clear()
    {
        this.head.next = this.tail;
        this.tail.prev = this.head;
        size = 0;
    }

    public bool IsEmpty()
    {
        return (this.head.next == this.tail);
    }


    public int Count
    {
        get
        {
            return size;
        }
    }

    public bool IsReadOnly { get { return false; } }


    public void Add(T item)
    {
        PushBack(item);
    }

    public void Insert(int index, T item)
    {
        DNode InsertNode = GetNode(index);
        Insert(InsertNode, item);
    }

    public int IndexOf(T item)
    {
        int Count = 0;
        DNode CurrentNode = this.head.next;
        while (CurrentNode.next != null)
        {
            if (EqualityComparer<T>.Default.Equals(CurrentNode.value, item)) return Count;
            else
            {
                CurrentNode = CurrentNode.next;
                Count++;
            }

        }
        return -1;
    }

    public T this[int index]
    {
        get
        {
            DNode node = GetNode(index);
            return node.value;
        }
        set
        {
            DNode node = GetNode(index);
            node.value = value;
        }
    }

    public void RemoveAt(int index)
    {
        DNode DeleteNode = GetNode(index);
        Remove(DeleteNode);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {

        int Index = 0;
        int Dll_index = 0;
        DNode CurrentNode = this.head.next;
        if (array == null) throw new ArgumentNullException();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException();
        if (array.Length < size - arrayIndex) throw new ArgumentException();
        while (CurrentNode.next != null)
        {
            if (Dll_index >= arrayIndex)
            {
                array[Index] = CurrentNode.value;
                Index++;
            }
            Dll_index++;
            CurrentNode = CurrentNode.next;
        }

    }

    public IEnumerator<T> GetEnumerator()
    {
        DNode CurrentNode = this.head.next;
        while (CurrentNode.next != null) //change all these to != this.tail??
        {
            yield return CurrentNode.value;
            CurrentNode = CurrentNode.next;
        }
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}

