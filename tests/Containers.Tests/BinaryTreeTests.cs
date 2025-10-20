using System;
using Xunit;

namespace homework
{
    // You don't actually use T for the digit tree; keep the generic outer shell to match your API.
    public class BinaryTree<T>
    {
        public sealed class BinaryDigitTree
        {
            // Doubly-linked list of bits, MSB ... LSB.
            private sealed class Node
            {
                public int Bit;          // 0 or 1
                public Node? Prev;       // toward MSB
                public Node? Next;       // toward LSB
                public Node(int bit) { Bit = bit; }
            }

            private Node _msb;           // head
            private Node _lsb;           // tail

            public BinaryDigitTree()
            {
                // Represent zero as a single 0 node so we always have a valid structure.
                _msb = _lsb = new Node(0);
            }

            /// <summary>Add 1 to the binary number.</summary>
            public void Increment()
            {
                // Start at LSB and propagate carry through consecutive 1s.
                var n = _lsb;
                while (n is not null && n.Bit == 1)
                {
                    n.Bit = 0;
                    n = n.Prev;
                }

                if (n is null)
                {
                    // Ran off the MSB: need a new leading 1.
                    var newHead = new Node(1) { Next = _msb };
                    _msb.Prev = newHead;
                    _msb = newHead;
                }
                else
                {
                    n.Bit = 1;
                }

                TrimLeadingZeros();
            }

            /// <summary>Divide by 2 (right shift by one bit).</summary>
            public void DivideBy2()
            {
                // Drop the least significant bit. Always leave at least one node.
                if (_msb == _lsb)
                {
                    // Single node -> becomes 0.
                    _msb.Bit = 0;
                    return;
                }

                _lsb = _lsb.Prev!;
                _lsb.Next = null;

                TrimLeadingZeros();
            }

            /// <summary>Divide by 2^power (right shift by 'power' bits).</summary>
            public void DivideByPowerOf2(int power)
            {
                if (power < 0) throw new ArgumentOutOfRangeException(nameof(power));
                for (int i = 0; i < power; i++)
                    DivideBy2();
            }

            /// <summary>Return the decimal value of the current bits.</summary>
            public int CalculateBase10()
            {
                int total = 0;
                int multiplier = 1;

                // Walk from LSB to MSB, doubling the multiplier each step.
                for (var n = _lsb; n is not null; n = n.Prev)
                {
                    if (n.Bit == 1) total += multiplier;
                    // Guard against undefined shift behavior if multiplier would overflow int.
                    if (multiplier <= (int.MaxValue >> 1))
                        multiplier <<= 1;
                    else
                        throw new OverflowException("Value does not fit in 32-bit signed integer.");
                }

                return total;
            }

            /// <summary>Remove redundant leading zeros; always keep at least one node.</summary>
            private void TrimLeadingZeros()
            {
                while (_msb != _lsb && _msb.Bit == 0)
                {
                    _msb = _msb.Next!;
                    _msb.Prev = null;
                }
            }
        }
    }
}
