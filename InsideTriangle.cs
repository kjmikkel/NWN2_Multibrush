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
	/// Description of InsideTriangle.
	/// </summary>
	public class InsideTriangle
	{
		DoublePoint[] corners;
		Vector2 vec0;
		Vector2 vec1;
		
		public InsideTriangle(DoublePoint[] corns) {
			corners = corns;
			vec0 = makeVector(corns[0], corns[1]);
			vec1 = makeVector(corns[0], corns[2]);
		}
		
		private Vector2 makeVector(DoublePoint p1, DoublePoint p2) {
			return new Vector2((float)(p2.X - p1.X), (float)(p2.Y - p1.Y));
		}
		
		private static float dotProduct(Vector2 vec0, Vector2 vec1) {
			return vec0.X * vec1.X + vec0.Y * vec1.Y;
		}
				
		public bool insideTriangle(DoublePoint inside) {
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
		
		/*
		Vector2 vec1;
		Vector2 vec2;
		Point p0;
		
		public InsideTriangle(Point v0, Vector2 vec1, Vector2 vec2)
		{
			this.p0 = v0;
			this.vec1 = vec1;
			this.vec2 = vec2;
		}
		
		public bool insideTriangle (Point v) {
			try {
			double a = indValues(v, p0, vec2, vec1);
			double b = -indValues(v, p0, vec1, vec2);
			
			return a > 0 && b > 0 && a + b < 1;
			} catch (Exception e) {
				throw new Exception("inside Triangle: " + e.Message);
			}
		}
		
		private double indValues(Point v, Point v0, Vector2 vec1, Vector2 vec2) {
			double top = determinant(v.X, v.Y, vec1.X, vec1.Y) - determinant(v0.X, v0.Y, vec1.X, vec1.Y);
			double denominator = determinant(vec1.X, vec1.Y, vec2.X, vec2.Y);
			return top / denominator;
		}
		
		private double determinant(double Ux, double Uy, double Vx, double Vy) {
			return Ux * Vy - Uy * Vx;
		}
		*/
	}
}
