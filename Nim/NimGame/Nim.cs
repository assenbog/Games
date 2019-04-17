using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NimGame
{
    public class Nim
    {
        public NimMove Move(List<int> rowItems)
        {
            const int maxLength = 8;

            // Represent each integer as a binary string padded to a predefined max length
            var binaryStringRepresentation = rowItems.Select(p => Convert.ToString(p, 2).PadLeft(maxLength, '0'))
                                                     .ToList();

            // Convert the binary strings to binary (0,1) integer arrays
            var binaryIntegerRepresentation = binaryStringRepresentation.Select(p => p.ToCharArray()
                                                                                      .Select(q => Convert.ToInt32(q.ToString()))
                                                                                      .ToArray())
                                                                        .ToList();

            var sumString = new StringBuilder();

            // Add each column Mod 2 and store as a string
            for (var i = 0; i < maxLength; i++)
            {
                sumString.Append((binaryIntegerRepresentation.Sum(p => p[i]) % 2).ToString());            
            }

            // Convert the result to a number. This is the number of items that have to be removed to restore the balance
            var mod2Balance = Convert.ToInt32(sumString.ToString(), 2);

            var xorValues = rowItems.Select(p => p ^ mod2Balance).ToList();

            var minXorValue = xorValues.Min();

            int rowToDeleteFrom = xorValues.IndexOf(minXorValue);

            var itemsToDelete = rowItems[rowToDeleteFrom] - minXorValue;
            
            // No winning move available; delete one item from the row containing the most items
            if (itemsToDelete == 0)
            {
                var maxItems = rowItems.Max();

                rowToDeleteFrom = rowItems.IndexOf(maxItems);
                itemsToDelete = 1;
            }

            return new NimMove { Row = rowToDeleteFrom, Items = itemsToDelete };
        }
    }
}
