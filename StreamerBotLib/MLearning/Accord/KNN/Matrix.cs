// Accord Math Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//



using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamerBotLib.MachineLearning.Accord.KNN
{
    /// <summary>
    ///   Matrix major order. The default is to use C-style Row-Major order.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   In computing, row-major order and column-major order describe methods for arranging 
    ///   multidimensional arrays in linear storage such as memory. In row-major order, consecutive 
    ///   elements of the rows of the array are contiguous in memory; in column-major order, 
    ///   consecutive elements of the columns are contiguous. Array layout is critical for correctly
    ///   passing arrays between programs written in different languages. It is also important for 
    ///   performance when traversing an array because accessing array elements that are contiguous 
    ///   in memory is usually faster than accessing elements which are not, due to caching. In some
    ///   media such as tape or NAND flash memory, accessing sequentially is orders of magnitude faster 
    ///   than nonsequential access.</para>
    /// 
    ///   <para>
    ///     References:
    ///     <list type="bullet">
    ///       <item><description>
    ///         <a href="https://en.wikipedia.org/wiki/Row-major_order">
    ///         Wikipedia contributors. "Row-major order." Wikipedia, The Free Encyclopedia. Wikipedia, 
    ///         The Free Encyclopedia, 13 Feb. 2016. Web. 22 Mar. 2016.</a>
    ///       </description></item>
    ///     </list>
    ///   </para>
    /// </remarks>
    /// 
    public enum MatrixOrder
    {
        /// <summary>
        ///   Row-major order (C, C++, C#, SAS, Pascal, NumPy default).
        /// </summary>
        CRowMajor = 1,

        /// <summary>
        ///   Column-major oder (Fotran, MATLAB, R).
        /// </summary>
        /// 
        FortranColumnMajor = 0,

        /// <summary>
        ///   Default (Row-Major, C/C++/C# order).
        /// </summary>
        /// 
        Default = CRowMajor
    }
    public static class Matrix
    {
        /// <summary>
        ///   Gets the index of the maximum element in a matrix across a given dimension.
        /// </summary>
        /// 
        public static int[] ArgMax<T>(this T[][] matrix, int dimension)
            where T : IComparable<T>
        {
            int s = GetLength(matrix, dimension);
            var values = new T[s];
            var indices = new int[s];
            Max(matrix, dimension, indices, values);
            return indices;
        }

        internal static int GetLength<T>(T[][] values, int dimension)
        {
            if (dimension == 1)
                return values.Length;
            return values[0].Length;
        }

        /// <summary>
        ///   Gets the maximum values across one dimension of a matrix.
        /// </summary>
        /// 
        public static T[] Max<T>(this T[][] matrix, int dimension, int[] indices, T[] result)
            where T : IComparable<T>
        {
            if (dimension == 1) // Search down columns
            {
                matrix.GetColumn(0, result: result);
                for (int j = 0; j < matrix.Length; j++)
                {
                    for (int i = 0; i < matrix[j].Length; i++)
                    {
                        if (matrix[j][i].CompareTo(result[j]) > 0)
                        {
                            result[j] = matrix[j][i];
                            indices[j] = i;
                        }
                    }
                }
            }
            else
            {
                matrix.GetRow(0, result: result);
                for (int j = 0; j < result.Length; j++)
                {
                    for (int i = 0; i < matrix.Length; i++)
                    {
                        if (matrix[i][j].CompareTo(result[j]) > 0)
                        {
                            result[j] = matrix[i][j];
                            indices[j] = i;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///   Gets the number of columns in a jagged matrix.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
        /// <param name="matrix">The matrix whose number of columns must be computed.</param>
        /// 
        /// <returns>The number of columns in the matrix.</returns>
        /// 
        public static int Columns<T>(this T[][] matrix)
        {
            if (matrix.Length == 0)
                return 0;
            return matrix[0].Length;
        }

        /// <summary>
        ///   Gets a column vector from a matrix.
        /// </summary>
        /// 
        public static T[] GetColumn<T>(this T[][] m, int index, T[] result = null)
        {
            if (result == null)
                result = new T[m.Length];

            index = Matrix.index(index, m.Columns());
            for (int i = 0; i < result.Length; i++)
                result[i] = m[i][index];

            return result;
        }

        /// <summary>
        ///   Gets a row vector from a matrix.
        /// </summary>
        /// 
        public static T[] GetRow<T>(this T[][] m, int index, T[] result = null)
        {
            index = Matrix.index(index, m.Rows());

            if (result == null)
            {
                return (T[])m[index].Clone();
            }
            else
            {
                m[index].CopyTo(result, 0);
                return result;
            }
        }

        /// <summary>
        ///   Gets the number of rows in a jagged matrix.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
        /// <param name="matrix">The matrix whose number of rows must be computed.</param>
        /// 
        /// <returns>The number of rows in the matrix.</returns>
        /// 
        public static int Rows<T>(this T[][] matrix)
        {
            return matrix.Length;
        }

        /// <summary>
        ///   Transforms a vector into a matrix of given dimensions.
        /// </summary>
        /// 
        public static T[,] Reshape<T>(this T[] array, int rows, int cols, MatrixOrder order = MatrixOrder.Default)
        {
            return Reshape(array, rows, cols, new T[rows, cols], order);
        }

        /// <summary>
        ///   Transforms a vector into a matrix of given dimensions.
        /// </summary>
        /// 
        public static T[,] Reshape<T>(this T[] array, int rows, int cols, T[,] result, MatrixOrder order = MatrixOrder.Default)
        {
            if (order == MatrixOrder.CRowMajor)
            {
                int k = 0;
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[i, j] = array[k++];
            }
            else
            {
                int k = 0;
                for (int j = 0; j < cols; j++)
                    for (int i = 0; i < rows; i++)
                        result[i, j] = array[k++];
            }

            return result;
        }

        /// <summary>
        ///   Converts a multidimensional array into a jagged array.
        /// </summary>
        /// 
        public static T[][] ToJagged<T>(this T[,] matrix, bool transpose = false)
        {
            T[][] array;

            if (transpose)
            {
                int cols = matrix.GetLength(1);

                array = new T[cols][];
                for (int i = 0; i < cols; i++)
                    array[i] = matrix.GetColumn(i);
            }
            else
            {
                int rows = matrix.GetLength(0);

                array = new T[rows][];
                for (int i = 0; i < rows; i++)
                    array[i] = matrix.GetRow(i);
            }

            return array;
        }

        /// <summary>
        ///   Gets a row vector from a matrix.
        /// </summary>
        ///
        public static T[] GetRow<T>(this T[,] m, int index, T[] result = null)
        {
            if (result == null)
                result = new T[m.GetLength(1)];

            index = Matrix.index(index, m.Rows());
            for (int i = 0; i < result.Length; i++)
                result[i] = m[index, i];

            return result;

        }

        /// <summary>
        ///   Gets the number of rows in a multidimensional matrix.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
        /// <param name="matrix">The matrix whose number of rows must be computed.</param>
        /// 
        /// <returns>The number of rows in the matrix.</returns>
        /// 
        public static int Rows<T>(this T[,] matrix)
        {
            return matrix.GetLength(0);
        }

        /// <summary>
        ///   Gets a column vector from a matrix.
        /// </summary>
        /// 
        public static T[] GetColumn<T>(this T[,] m, int index, T[] result = null)
        {
            if (result == null)
                result = new T[m.Rows()];

            index = Matrix.index(index, m.Columns());
            for (int i = 0; i < result.Length; i++)
                result[i] = m[i, index];

            return result;
        }

        /// <summary>
        ///   Gets the number of columns in a multidimensional matrix.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
        /// <param name="matrix">The matrix whose number of columns must be computed.</param>
        /// 
        /// <returns>The number of columns in the matrix.</returns>
        /// 
        public static int Columns<T>(this T[,] matrix)
        {
            return matrix.GetLength(1);
        }

        private static int index(int end, int length)
        {
            if (end < 0)
                end = length + end;
            return end;
        }

        /// <summary>
        ///   Gets the number of distinct values 
        ///   present in each column of a matrix.
        /// </summary>
        /// 
        public static int DistinctCount<T>(this T[] values)
        {
            return values.Distinct().Length;
        }

        /// <summary>
        ///   Retrieves only distinct values contained in an array.
        /// </summary>
        /// 
        /// <param name="values">The array.</param>
        /// 
        /// <returns>An array containing only the distinct values in <paramref name="values"/>.</returns>
        /// 
        public static T[] Distinct<T>(this T[] values)
        {
            var set = new HashSet<T>(values);

            return set.ToArray();
        }

        /// <summary>
        ///   Retrieves the bottom <c>count</c> values of an array.
        /// </summary>
        /// 
        public static int[] Bottom<T>(this T[] values, int count, bool inPlace = false)
            where T : IComparable<T>
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count",
                "The number of elements to be selected must be positive.");
            }

            if (count == 0)
                return new int[0];

            if (count > values.Length)
                return Range(0, values.Length);

            T[] work = (inPlace) ? values : (T[])values.Clone();
            int[] idx = Range(values.Length);
            work.NthElement(idx, 0, work.Length, count, asc: true);
            StreamerBotLib.MachineLearning.Accord.KNN.Sort.Insertion(work, idx, 0, count, asc: true);
            return idx.First(count);
        }

        /// <summary>
        ///   Returns a copy of an array in reversed order.
        /// </summary>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] First<T>(this T[] values, int count)
        {
            var r = new T[count];
            for (int i = 0; i < r.Length; i++)
                r[i] = values[i];
            return r;
        }

        /// <summary>
        ///   Creates a range vector (like NumPy's arange function).
        /// </summary>
        /// 
        /// <param name="n">The exclusive upper bound of the range.</param>
        ///
        /// <remarks>
        /// <para>
        ///   The Range methods should be equivalent to NumPy's np.arange method, with one
        ///   single difference: when the intervals are inverted (i.e. a > b) and the step
        ///   size is negative, the framework still iterates over the range backwards, as 
        ///   if the step was negative.</para>
        /// <para>
        ///   This function never includes the upper bound of the range. For methods
        ///   that include it, please see the <see cref="Interval(int, int)"/> methods.</para>  
        /// </remarks>
        ///
        /// <seealso cref="Interval(int, int)"/>
        ///
        public static int[] Range(int n)
        {
            int[] r = new int[(int)n];
            for (int i = 0; i < r.Length; i++)
                r[i] = (int)i;
            return r;
        }

        /// <summary>
        ///   Creates a range vector (like NumPy's arange function).
        /// </summary>
        ///
        /// <param name="a">The inclusive lower bound of the range.</param>
        /// <param name="b">The exclusive upper bound of the range.</param>
        ///
        /// <remarks>
        /// <para>
        ///   The Range methods should be equivalent to NumPy's np.arange method, with one
        ///   single difference: when the intervals are inverted (i.e. a > b) and the step
        ///   size is negative, the framework still iterates over the range backwards, as 
        ///   if the step was negative.</para>
        /// <para>
        ///   This function never includes the upper bound of the range. For methods
        ///   that include it, please see the <see cref="Interval(int, int)"/> methods.</para>  
        /// </remarks>
        ///
        /// <seealso cref="Interval(int, int)"/>
        ///
        public static int[] Range(int a, int b)
        {
            if (a == b)
                return new int[] { };

            int[] r;

            if (b > a)
            {
                r = new int[(int)(b - a)];
                for (int i = 0; i < r.Length; i++)
                    r[i] = (int)(a++);
            }
            else
            {
                r = new int[(int)(a - b)];
                for (int i = 0; i < r.Length; i++)
                    r[i] = (int)(a--);
            }

            return r;
        }

        /// <summary>
        ///   Returns a subvector extracted from the current vector.
        /// </summary>
        /// 
        /// <param name="source">The vector to return the subvector from.</param>
        /// <param name="indexes">Array of indices.</param>
        /// <param name="inPlace">True to return the results in place, changing the
        ///   original <paramref name="source"/> vector; false otherwise.</param>
        /// 
        public static T[] Get<T>(this T[] source, int[] indexes, bool inPlace = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (indexes == null)
                throw new ArgumentNullException("indexes");

            if (inPlace && source.Length != indexes.Length)
                throw new DimensionMismatchException("Source and indexes arrays must have the same dimension for in-place operations.");

            var destination = new T[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                int j = indexes[i];
                if (j >= 0)
                    destination[i] = source[j];
                else
                    destination[i] = source[source.Length + j];
            }

            if (inPlace)
            {
                for (int i = 0; i < destination.Length; i++)
                    source[i] = destination[i];
            }

            return destination;
        }

        /// <summary>
        ///   Swaps two elements in an array, given their indices.
        /// </summary>
        /// 
        /// <param name="array">The array whose elements will be swapped.</param>
        /// <param name="a">The index of the first element to be swapped.</param>
        /// <param name="b">The index of the second element to be swapped.</param>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Swap<T>(this T[] array, int a, int b)
        {
            T aux = array[a];
            array[a] = array[b];
            array[b] = aux;
        }

        /// <summary>
        ///   Gets the maximum element in a vector.
        /// </summary>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ArgMax<T>(this T[] values)
            where T : IComparable<T>
        {
            int imax = 0;
            T max = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i].CompareTo(max) > 0)
                {
                    max = values[i];
                    imax = i;
                }
            }

            return imax;
        }
    }
}
