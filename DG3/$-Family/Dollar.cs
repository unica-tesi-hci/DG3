/**
 * Extract from The $1 Unistroke Recognizer (C# version)
 *
 *		Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@u.washington.edu
 *
 *		Andrew D. Wilson, Ph.D.
 *		Microsoft Research
 *		One Microsoft Way
 *		Redmond, WA 98052
 *		awilson@microsoft.com
 *
 *		Yang Li, Ph.D.
 *		Department of Computer Science and Engineering
 * 		University of Washington
 *		The Allen Center, Box 352350
 *		Seattle, WA 98195-2840
 * 		yangli@cs.washington.edu
 *
 * The Protractor enhancement was published by Yang Li and programmed here by 
 * Jacob O. Wobbrock.
 *
 *	Li, Y. (2010). Protractor: A fast and accurate gesture 
 *	  recognizer. Proceedings of the ACM Conference on Human 
 *	  Factors in Computing Systems (CHI '10). Atlanta, Georgia
 *	  (April 10-15, 2010). New York: ACM Press, pp. 2169-2172.
 * 
 * The $1 Unistroke Recognizer (C# version) is distributed under the "New BSD License" agreement:
 * 
 * Copyright (c) 2007-2011, Jacob O. Wobbrock, Andrew D. Wilson and Yang Li.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University of Washington nor Microsoft,
 *      nor the names of its contributors may be used to endorse or promote 
 *      products derived from this software without specific prior written
 *      permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock OR Andrew D. Wilson
 * OR Yang Li BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**/
using System;
using System.Collections.Generic;
namespace DG3
{
	class Dollar
	{
		/// <summary>
		/// Vectorize the unistroke according to the algorithm by Yang Li for use in the Protractor extension to $1.
		/// </summary>
		/// <param name="points">The resampled points in the gesture to vectorize.</param>
		/// <returns>A vector of cosine distances.</returns>
		/// <seealso cref="http://yangl.org/protractor/"/>
		public static List<double> Vectorize(Point[] points)
		{
			double sum = 0.0;
			List<double> vector = new List<double>(points.Length * 2);
			for (int i = 0; i < points.Length; i++)
			{
				vector.Add(points[i].X);
				vector.Add(points[i].Y);
				sum += points[i].X * points[i].X + points[i].Y * points[i].Y;
			}
			double magnitude = Math.Sqrt(sum);
			for (int i = 0; i < vector.Count; i++)
			{
				vector[i] /= magnitude;
			}
			return vector;
		}

		/// <summary>
		/// From Protractor by Yang Li, published at CHI 2010. See http://yangl.org/protractor/. 
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static double[] OptimalCosineDistance(List<double> v1, List<double> v2)
		{
			double a = 0.0;
			double b = 0.0;
			for (int i = 0; i < Math.Min(v1.Count, v2.Count); i += 2)
			{
				a += v1[i] * v2[i] + v1[i + 1] * v2[i + 1];
				b += v1[i] * v2[i + 1] - v1[i + 1] * v2[i];
			}
			double angle = Math.Atan(b / a);
			double distance = Math.Acos(a * Math.Cos(angle) + b * Math.Sin(angle));
			return new double[3] { distance, (180 / Math.PI) * angle, 0.0 }; // distance, angle, calls to pathdist
		}
	}
}
