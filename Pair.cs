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
	/// The Pair class contains 1 instance of each of the classes T and S
	/// </summary>
	public class Pair<T, S>
	{
		public T X;
		public S Y;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="X">An instance of the class T</param>
		/// <param name="Y">An instance of the class S</param>
		public Pair(T X, S Y) {
			this.X = X;
			this.Y = Y;			
		}
				
  public override string ToString() 
  {
     return "(" + X + ", " + Y + ")";
  }

		
	}
}
