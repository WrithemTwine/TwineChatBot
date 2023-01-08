using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.MachineLearning.Accord.KNN
{
    public static class Vector
    {
        /// <summary>
        ///   Creates a one-hot vector, where all values are zero except for the indicated
        ///   <paramref name="index"/>, which is set to one.
        /// </summary>
        /// 
        /// <typeparam name="T">The data type for the vector.</typeparam>
        /// 
        /// <param name="index">The vector's dimension which will be marked as one.</param>
        /// <param name="result">The vector where the one-hot should be marked.</param>
        /// 
        /// <returns>A one-hot vector where only a single position is one and the others are zero.</returns>
        /// 
#if NET45 || NET46 || NET462 || NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] OneHot<T>(int index, T[] result)
        {
            var one = (T)System.Convert.ChangeType(1, typeof(T));
            result[index] = one;
            return result;
        }
    }
}
