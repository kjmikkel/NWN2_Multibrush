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
using System.Windows.Forms;

namespace Multibrush
{
	/// <summary>
	/// The that explains how to shut down the multibrush
	/// </summary>
	public partial class stopMulti : Form
	{
		public bool dontShow = false;
		public static stopMulti stop;
		
		private stopMulti()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			close.Click += new EventHandler(clickClose);
		}
		
		/// <summary>
		/// The static constructor
		/// </summary>
		/// <returns>A single instance of the stopMulti form</returns>
		public static stopMulti makeStopMulti() {
			if (stop == null)
				stop = new stopMulti();
			
			return stop;
		}
		
		/// <summary>
		/// The method to reset the form
		/// </summary>
		public void reset() {
			checkShow.Checked = false;
		}
		
		/// <summary>
		/// The method to close the form (which also sets the preference of whether to see
		/// the preference again)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clickClose(object sender, EventArgs e) {
			dontShow = checkShow.Checked;
			this.Close();
		}
	}
}
