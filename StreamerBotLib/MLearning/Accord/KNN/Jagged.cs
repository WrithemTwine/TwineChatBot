namespace StreamerBotLib.MachineLearning.Accord.KNN
{
    /// <summary>
    ///   Jagged matrices.
    /// </summary>
    /// 
    /// <seealso cref="Matrix"/>
    /// <seealso cref="Vector"/>
    /// 
    public static partial class Jagged
    {
        /// <summary>
        ///   Creates a matrix with all values set to a given value.
        /// </summary>
        /// 
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="values">The initial values for the matrix.</param>
        /// 
        /// <returns>A matrix of the specified size.</returns>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[][] Create<T>(int rows, int columns, params T[] values)
        {
            if (values.Length == 0)
                return Zeros<T>(rows, columns);
            return values.Reshape(rows, columns).ToJagged();
        }

        /// <summary>
        ///   Creates a zero-valued matrix.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the matrix to be created.</typeparam>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// 
        /// <returns>A matrix of the specified size.</returns>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[][] Zeros<T>(int rows, int columns)
        {
            T[][] matrix = new T[rows][];
            for (int i = 0; i < matrix.Length; i++)
                matrix[i] = new T[columns];
            return matrix;
        }

        /// <summary>
        ///   Creates a matrix of one-hot vectors, where all values at each row are 
        ///   zero except for the indicated <paramref name="indices"/>, which is set to one.
        /// </summary>
        /// 
        /// <typeparam name="T">The data type for the matrix.</typeparam>
        /// 
        /// <param name="indices">The rows's dimension which will be marked as one.</param>
        /// <param name="result">The matrix where the one-hot should be marked.</param>
        /// 
        /// <returns>A matrix containing one-hot vectors where only a single position
        /// is one and the others are zero.</returns>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[][] OneHot<T>(int[] indices, T[][] result)
        {
            var one = (T)System.Convert.ChangeType(1, typeof(T));
            for (int i = 0; i < indices.Length; i++)
                result[i][indices[i]] = one;
            return result;
        }
    }


}
