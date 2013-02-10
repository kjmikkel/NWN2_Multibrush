/* 
 * This file is part of Multibrush.
 * Multibrush is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Multibrush is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with Multibrush.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Multibrush
{
	/// <summary>
	/// Description of LineSegment.
	/// </summary>
	public class LineSegment
	{
		
		Pair<double, double> upper;
		Pair<double, double> lower;
		
		public LineSegment(Pair<double, double> upper, Pair<double, double> lower)
		{
			this.upper = upper;
			this.lower = lower;
		}
		
		/// <summary>
		/// Find the distance between a point and the line segment
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		/// 
		
		private static double lengthSub(Pair<double, double> vector) {
			return vector.X * vector.X + vector.Y * vector.Y;
		}
		
		private static double length(Pair<double, double> vector) {
			return Math.Sqrt(lengthSub(vector));
		}
		
		public double distance(Pair<double, double> point) {
			// Solution found at http://www.codeguru.com/forum/printthread.php?t=194400
		
			Pair<double, double> AC = makeVector(lower, point);
			Pair<double, double> BC = makeVector(upper, point);
			Pair<double, double> AB = makeVector(lower, upper);
			
			double r_numerator = LineSegment.dot(AC, AB);			
			double r_denomenator = LineSegment.lengthSub(AB);
			
			double r = r_numerator / r_denomenator;
			
			Pair<double, double>  P = new Multibrush.Pair<double, double>(lower.X + r * AB.X, 
			                                                              lower.Y + r * AB.Y);		
			double s = (AC.Y * AB.X - AC.X * AB.Y) / r_denomenator;
			
			double distanceLine = Math.Abs(s) * Math.Sqrt(r_denomenator);
			
			// If we lie within the partition the linesegment creates, then we just return the already calculated distance
			if ( (r >= 0) && (r <= 1) )
			{
				return distanceLine;
			}
			else
			{
				// If that is not the case, then we need to find out whether the point is closest to A or B
				double dist1 = LineSegment.length(AC);
				double dist2 = LineSegment.length(BC);
				
				if (dist1 < dist2)
				{					
					return dist1;
				}
				else
				{
					return dist2;
				}
			}
			
		}

		private static Pair<double, double> makeVector(Pair<double, double> point1, Pair<double, double> point2) {
			double X = point1.X - point2.X;
			double Y = point1.Y - point2.Y;
			
			return new Pair<double, double>(X, Y);
		}
		
		private static double dot(Pair<double, double> A, Pair<double, double> B) {
			return A.X * B.X + A.Y * B.Y;
		}
		
		

	}
}
