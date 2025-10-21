namespace homework;
public class BinaryTree<DataT>
{
    protected TreeNode<DataT> Root { get; set; }

    protected class TreeNode<T>
    {
        public TreeNode<T> Left { get; set; }
        public TreeNode<T> Right { get; set; }
        public T Data { get; set; }

        public TreeNode(TreeNode<T> left, T data, TreeNode<T> right)
        {
            Left = left;
            Data = data;
            Right = right;
        }
        public TreeNode(T data) : this(null, data, null) { }
    }
    public class BinaryDigitTree : BinaryTree<int>
    {
        //Methods to be implimented
        public void DivideBy2()
        {
            TreeNode<int> currentNode = Root;
            int shift = 0;
            int temp;
            while (currentNode != null)
            {
                temp = currentNode.Data;    //swaps data in node out with data from previous
                currentNode.Data = shift;
                shift = temp;
                currentNode = currentNode.Left;
            }
        }

        public void DivideByPowerOf2(int input)
        {

            for (int x = input; x > 0; x--)
                DivideBy2();
        }


        public int CalculateBase10() //must be recursive
        {
            return CalculateBase10Helper(0, Root, 1);
        }

        private int CalculateBase10Helper(int total, TreeNode<int> currentNode, int multiplier)
        {
            if (currentNode.Left == null) { return total + currentNode.Data * multiplier; }

            total += currentNode.Data * multiplier;
            return CalculateBase10Helper(total, currentNode.Left, multiplier * 2);

        }
        public void Increment()
        {
            TreeNode<int> currentNode = Root; //perhaps TreeNode<T>, hmm
            while (currentNode.Data != 0)   //this solution accounts for "carrying the 1"
            {
                if (currentNode == null) { throw new OverflowException("Adding 1 Causes Overflow"); }
                currentNode.Data = 0;
                currentNode = currentNode.Left;
            }
            currentNode.Data = 1;
        }
    }
}