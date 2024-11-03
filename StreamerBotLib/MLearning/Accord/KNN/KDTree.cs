namespace StreamerBotLib.MLearning.Accord.KNN
{
    // Accord Machine Learning Library
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


    /// <summary>
    ///   Convenience class for k-dimensional tree static methods. To 
    ///   create a new KDTree, specify the generic parameter as in
    ///   <see cref="KDTree{T}"/>.
    /// </summary>
    /// 
    /// <remarks>
    ///   Please check the documentation page for <see cref="KDTree{T}"/>
    ///   for examples, usage and actual remarks about kd-trees.
    /// </remarks>
    /// 
    /// <seealso cref="KDTree{T}"/>
    /// <seealso cref="SPTree"/>
    /// <seealso cref="VPTree"/>
    /// 
    [Serializable]
    public class KDTree : KDTreeBase<KDTreeNode>
    {
        /// <summary>
        ///   Creates a new <see cref="KDTree"/>.
        /// </summary>
        /// 
        /// <param name="dimensions">The number of dimensions in the tree.</param>
        /// 
        public KDTree(int dimensions)
            : base(dimensions)
        {
        }

        /// <summary>
        ///   Creates a new k-dimensional tree from the given points.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the value to be stored.</typeparam>
        /// 
        /// <param name="points">The points to be added to the tree.</param>
        /// <param name="values">The corresponding values at each data point.</param>
        /// <param name="distance">The distance function to use.</param>
        /// <param name="inPlace">Whether the given <paramref name="points"/> vector
        ///   can be ordered in place. Passing true will change the original order of
        ///   the vector. If set to false, all operations will be performed on an extra
        ///   copy of the vector.</param>
        /// 
        /// <returns>A <see cref="KDTree{T}"/> populated with the given data points.</returns>
        /// 
        public static KDTree<T> FromData<T>(double[][] points, T[] values,
            IMetric<double[]> distance, bool inPlace = false)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (distance == null)
            {
                throw new ArgumentNullException("distance");
            }

            int leaves;

            var root = KDTree<T>.CreateRoot(points, values, inPlace, out leaves);

            return new KDTree<T>(points[0].Length, root, points.Length, leaves)
            {
                Distance = distance,
            };
        }

    }
}
