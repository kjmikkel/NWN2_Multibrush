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
	/// Description of FloatPoint.
	/// </summary>
	public class DoublePoint
	{
		public double X;
		public double Y;
		
		public DoublePoint(double X, double Y) {
			this.X = X;
			this.Y = Y;			
		}
		
		public DoublePoint(int X, int Y) {
			this.X = (double)X;
			this.Y = (double)Y;
		}
		
  public override string ToString() 
  {
     return "(" + X + ", " + Y + ")";
  }

		
	}
}
