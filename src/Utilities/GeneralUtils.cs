using System;
using System.Collections.Generic;

namespace Utilities;

/**
* A comprehensive utility library providing various helper methods for common programming tasks.
* Includes functions for string manipulation, validation, mathematical operations, and collection processing.
*
* Bugs: None known
*
* @author Rahul & Reza
* @Tool for commenting: DeepSeek-V3 June 2024
* @Sep 02 2025
*/
public static class GeneralUtils
{
    #region Collection Operations

    /// <summary>
    /// Determines whether an array contains a specific item.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    /// <param name="array">The array to search</param>
    /// <param name="item">The item to locate</param>
    /// <returns>true if the item is found in the array; otherwise, false</returns>
    public static bool Contains<T>(T[] array, T item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(array[i], item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns a list containing only unique items from the given list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to process</param>
    /// <returns>A new list containing only unique items</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input list is null</exception>
    public static List<T> GetUniqueItems<T>(List<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list), "List cannot be null.");

        var result = new List<T>();
        foreach (var item in list)
        {
            if (!result.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// Returns all items from an array that are duplicated more than once.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    /// <param name="array">The array to search for duplicates</param>
    /// <returns>An array containing duplicate items</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input array is null</exception>
    public static T[] Duplicates<T>(T[] array) where T : notnull
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");

        var counts = new Dictionary<T, int>();
        var result = new List<T>();

        foreach (var item in array)
        {
            if (counts.ContainsKey(item))
            {
                counts[item]++;
            }
            else
            {
                counts[item] = 1;
            }
        }

        foreach (var kvp in counts)
        {
            if (kvp.Value > 1)
            {
                result.Add(kvp.Key);
            }
        }

        return result.ToArray();
    }

    #endregion

    #region String Operations

    /// <summary>
    /// Converts space-separated words to camelCase format.
    /// </summary>
    /// <param name="s">The input string to convert</param>
    /// <returns>The camelCase formatted string</returns>
    public static string ToCamelCase(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return "";

        // Split and remove empty entries to handle multiple spaces
        string[] words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return "";

        string result = words[0].ToLower();

        for (int i = 1; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }

        return result;
    }

    /// <summary>
    /// Counts the number of occurrences of a character in a string.
    /// </summary>
    /// <param name="s">The string to search</param>
    /// <param name="c">The character to count</param>
    /// <returns>The number of occurrences of the character</returns>
    public static int CountOccurances(string s, char c)
    {
        if (string.IsNullOrEmpty(s))
            return 0;

        int count = 0;
        foreach (char character in s)
        {
            if (character == c)
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Checks if a password meets strength requirements.
    /// </summary>
    /// <param name="pwd">The password to validate</param>
    /// <returns>true if the password is strong; otherwise, false</returns>
    public static bool IsPasswordStrong(string pwd)
    {
        if (string.IsNullOrEmpty(pwd))
            return false;

        if (pwd.Length < 8)
        {
            return false;
        }

        bool hasLower = false;
        bool hasUpper = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char c in pwd)
        {
            if (char.IsLower(c))
            {
                hasLower = true;
            }
            else if (char.IsUpper(c))
            {
                hasUpper = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else
            {
                hasSpecial = true;
            }
        }

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }

    /// <summary>
    /// Validates if a string represents a valid variable name (all lowercase).
    /// </summary>
    /// <param name="name">The variable name to validate</param>
    /// <returns>true if the variable name is valid; otherwise, false</returns>
    public static bool IsValidVariable(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return name == name.ToLower();
    }

    /// <summary>
    /// Validates if a string represents a supported mathematical operator.
    /// </summary>
    /// <param name="op">The operator to validate</param>
    /// <returns>true if the operator is valid; otherwise, false</returns>
    public static bool IsValidOperator(string op)
    {
        if (string.IsNullOrEmpty(op))
            return false;

        List<string> ops = new List<string> { "+", "-", "*", "/", "//", "%", "**" };
        return ops.Contains(op);
    }

    #endregion

    #region Mathematical Operations

    /// <summary>
    /// Calculates the average value of an array of integers.
    /// </summary>
    /// <param name="numbers">The array of integers</param>
    /// <returns>The average value as a double</returns>
    /// <exception cref="ArgumentException">Thrown when the array is null or empty</exception>
    public static double CalculateAverage(int[] numbers)
    {
        if (numbers == null || numbers.Length == 0)
        {
            throw new ArgumentException("Array cannot be null or empty.");
        }

        int sum = 0;
        foreach (int num in numbers)
        {
            sum += num;
        }

        return (double)sum / numbers.Length;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Generates indentation string for a given level.
    /// </summary>
    /// <param name="level">The indentation level</param>
    /// <returns>A string of spaces representing the indentation</returns>
    public static string GetIndentation(int level)
    {
        if (level <= 0)
            return "";

        return new string(' ', level * 4);
    }

    #endregion

}