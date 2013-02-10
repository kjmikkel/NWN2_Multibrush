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
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using NWN2Toolset.NWN2.Views;
using OEIShared.UI;
using OEIShared.UI.Input;

namespace Multibrush
{
	/// <summary>
	/// A class used for comparison of points
	/// </summary>
	class ComparePoins : IComparer
	{
		int IComparer.Compare(object a, object b)
		{
			Point p1 = (Point)a;
			Point p2 = (Point)b;
			
			if (p1.X < p2.X) {
				return 1;
			} else if (p1.X == p2.X && p1.Y > p2.Y) {
				return 1;
			} else if (p1.X == p2.X && p1.Y == p2.Y) {
				return 0;
			} else {
				return -1;
			}
		}
	}
	
	/// <summary>
	/// Custom input reciever to make it possible to paint large swaths of the texturebrush.
	/// Much of this class has merely become an outside face to the Painter Class.
	/// </summary>
	public class InputReceiver : IElectronPanelInputReceiver
	{
		// Fields
		private object m_tag = null;
		private ElectronPanel electronPanel = null;
		private Painter painter;
		bool ctrlDown = false;
		
		// Methods
		public InputReceiver(brushData data)
		{
			painter = new Painter();
			painter.setOuterInner(data);
		}
		
		public void drawPolygon() {
			painter.drawPolygon();
		}

		public void changeData(brushData data) {
			painter.changeData(data);
		}
		
		private void debugOut(string str) {
			Console.WriteLine(str);
		}
			
		public virtual void Attach(ElectronPanel cPanel)
		{
			try {
			this.electronPanel = cPanel;
			painter.setElectron(cPanel);
			} catch (Exception e) {
				debugOut("problem in attach: " + e.Message);

				throw new Exception("Problem in attach: " + e.Message);
			}
		}

		public virtual void Detach()
		{
			if (painter != null)
				painter.stopShowingCircle();
			this.electronPanel = null;
		}

		public virtual bool FocusOn(Vector3 cPosition)
		{
			return true;
		}
		
		private TrackBar getTrackbar()
		{
			return null;
		}


		public virtual void OnKeyDown(object oSender, EPKeyEventArgs eArgs)
		{
			ctrlDown = eArgs.Control;
		}

		public virtual void OnKeyPress(object oSender, EPKeyPressEventArgs eArgs)
		{
		}

		public virtual void OnKeyUp(object oSender, EPKeyEventArgs eArgs)
		{
			ctrlDown = false;
		}

		public virtual void OnLostFocus(object oSender, EventArgs eArgs)
		{
		}

		public virtual void OnMouseButtonHeld(object oSender, ref bool bCancel)
		{
			painter.runTextureCode(oSender, ctrlDown);
		}

		public virtual void OnMouseDown(object oSender, EPMouseEventArgs eArgs)
		{
		}
		
		public virtual void OnMouseDrag(MousePanel cPanel, ref bool bCancel)
		{
		}

		public virtual void OnMouseUp(object oSender, EPMouseEventArgs eArgs)
		{
		}
		
		public virtual void OnMouseMove(object oSender, EPMouseEventArgs eArgs)
		{
			if (NWN2AreaViewer.MouseMode == MouseMode.PaintTerrain) {
				MousePanel panel = oSender as MousePanel;
				
				painter.Move(panel);
			}
		}

		public virtual void OnMouseWheel(object oSender, EPMouseEventArgs eArgs)
		{
		}

		public virtual bool ProcessDialogKey(Keys keyData)
		{
			return true;
		}

		public void SmoothModeToggle(EPKeyEventArgs eArgs)
		{
		}

		// Properties
		public virtual object Tag
		{
			get
			{
				return this.m_tag;
			}
			set
			{
				this.m_tag = value;
			}
		}

	}

}
