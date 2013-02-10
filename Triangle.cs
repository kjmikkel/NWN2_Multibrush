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
using System.Drawing;
using Microsoft.DirectX;

namespace Multibrush
{
	/// <summary>
	/// This class is desined is meant to define a simple triangle. Its primary function is to find out whether a point
	/// is inside the triangle or not
	/// </summary>
	public class Triangle
	{
		// The corners
		public Pair<double, double>[] corners;
		
		// The vectors that define triangle from corner[0]
		Vector2 vec0;
		Vector2 vec1;
		
		Pair<double, double> upper;
		Pair<double, double> lower;
	
		public Triangle(Pair<double, double>[] corns, Pair<double, double> upper, Pair<double, double> lower) {
			corners = corns;
			vec0 = makeVector(corns[0], corns[1]);
			vec1 = makeVector(corns[0], corns[2]);
			this.upper = upper;
			this.lower = lower;
		}
		
		/// <summary>
		/// Utility class to make a 2d Vector 
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		private static Vector2 makeVector(Pair<double, double> p1, Pair<double, double> p2) {
			return new Vector2((float)(p2.X - p1.X), (float)(p2.Y - p1.Y));
		}
		
		/// <summary>
		/// Utility class for making a dotProduct
		/// </summary>
		/// <param name="vec0"></param>
		/// <param name="vec1"></param>
		/// <returns></returns>
		private static float dotProduct(Vector2 vec0, Vector2 vec1) {
			return vec0.X * vec1.X + vec0.Y * vec1.Y;
		}
				
		
		/// <summary>
		///  The main guts of the application. Given a DoublePoint the application 
		/// calculates whether it is inside the triangle or not.
		/// I used the second solution found on http://www.blackpawn.com/texts/pointinpoly/default.html to 
		/// figure out to do this
		/// </summary>
		/// <param name="inside">The Doublepoint that we want to check is inside the triangle or not</param>
		/// <returns></returns>
		public bool insideTriangle(Pair<double, double> inside) {
			Vector2 vec2 = makeVector(corners[0], inside);
			
			float dot00 = dotProduct(vec0, vec0);
			float dot01 = dotProduct(vec0, vec1);
			float dot02 = dotProduct(vec0, vec2);
			float dot11 = dotProduct(vec1, vec1);
			float dot12 = dotProduct(vec1, vec2);
			
			double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
			double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
			double v = (dot00 * dot12 - dot01 * dot02) * invDenom;
			
			return (u > 0.0) && (v > 0.0) && (u + v < 1);		
		}
		
		public Pair<double, double> getLower() {
			return lower;
		}
		
		public Pair<double, double> getUpper() {
			return upper;
		}
		
		public override string ToString() {
			return "Vector 1: " + vec0 + System.Environment.NewLine + "Vector 2: " + vec1 + System.Environment.NewLine;
		}
	}
}
