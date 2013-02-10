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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using NWN2Toolset;
using NWN2Toolset.NWN2.NetDisplay;
using NWN2Toolset.NWN2.Views;
using NWN2Toolset.NWN2.Views.Input;
using OEIShared.NetDisplay;
using OEIShared.UI.Input;
using OEIShared.Utils;

namespace Multibrush
{
	/// <summary>
	/// Description of IntervalTimer.
	/// </summary>
	internal class IntervalTimer : Timer
	{
		private brushData data;
		private static InputReceiver inputReciver;
	//	private static Painter painter;
		private bool stop = false;
		private static Hashtable newReceiverTable = new Hashtable();
		private static Hashtable oldReceiverTable = new Hashtable();
		
		// Methods
		public IntervalTimer(brushData data)
		{
			this.data = data;
			base.Interval = 0x7d0;
		}
		
		/// <summary>
		/// If we are to draw a polygon
		/// </summary>
		public void drawPolygon() {
			if (inputReciver != null) {
				debugOut("call polygon");
				inputReciver.drawPolygon();
			}
		}
		
		/// <summary>
		/// If we want to stop using the multibrush
		/// </summary>
		public void stopTexture() {
			debugOut("stop");
			stop = true;
		}
		
		public void startTexture() {
			debugOut("start");
			stop = false;
			this.Start();
		}
		
		/// <summary>
		/// If we want to resume painting
		/// </summary>
		public void resumeTexture() {
			if (stop) {
				debugOut("resume");
				stop = false;
				this.Start();
			}
		}
		
		private static void debugOut(string str) {
			Console.WriteLine(str);
		}
		
		/// <summary>
		/// We are already painting with the multibrush, but we want to change the data
		/// </summary>
		/// <param name="data">The new data we want to use</param>
		public void setNewData(brushData data) {
			this.data = data;
			
			try {
				foreach (NWN2AreaViewer viewer in NWN2ToolsetMainForm.App.GetAllAreaViewers())
				{
					if (newReceiverTable.ContainsKey(viewer)) {
						InputReceiver input = (InputReceiver)newReceiverTable[viewer];
						if (input != null) {
							inputReciver = input;
							inputReciver.changeData(this.data);
						}
					}
				}
			} catch (Exception exception) {
				throw new Exception("Problem found while trying to add new Data: " + exception);
			}
		}
		
		/// <summary>
		/// If we want to go back to painting with the normal listeners - we remove the multibrush and try to restore the normal listeners
		/// </summary>
		public void normalizeListeners() {
			List<NWN2AreaViewer> allAreaViewers = NWN2ToolsetMainForm.App.GetAllAreaViewers();
			foreach (NWN2AreaViewer viewer in allAreaViewers)
			{
				try {
					if (newReceiverTable.ContainsKey(viewer)) {
						InputReceiver receiver = (InputReceiver)newReceiverTable[viewer];
						if (receiver != null) {
							viewer.ElectronPanel.RemoveInputReceiver(receiver);
							newReceiverTable.Remove(viewer);
							inputReciver = receiver;
						}
					}
				} catch (Exception e) {
					throw new Exception("Exception thrown while trying to remove multibrush: " + e);
				}
				
				try {
					if (oldReceiverTable.ContainsKey(viewer)) {
						PaintTerrainInputReceiver oldReceiver = (PaintTerrainInputReceiver)oldReceiverTable[viewer];
						if (oldReceiver != null) {
							viewer.ElectronPanel.AddInputReceiver(oldReceiver);
							oldReceiverTable.Remove(viewer);
						} else {
							addNewReceiver(viewer);
						}
					} else {
						addNewReceiver(viewer);
					}
				} catch (Exception e) {
					throw new Exception("Exception thrown while trying to add the old PaintBrush: " + e);
				}
			}
		}

		/// <summary>
		/// In the case where the old PaintTerrainInputReceiver has been collected by the garbage collector, 
		/// then we try to make a replacement
		/// </summary>
		/// <param name="viewer"></param>
		private void addNewReceiver(NWN2AreaViewer viewer) {
			Console.WriteLine("The retained paintTerrianInputReceiver is null");
			PaintTerrainInputReceiver normalPaint = new PaintTerrainInputReceiver();
			normalPaint.Attach(viewer.ElectronPanel);
			viewer.ElectronPanel.AddInputReceiver(normalPaint);
		}
		
		/// <summary>
		/// Runs constantly in order to whether new Input Receivers are needed to be added
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTick(EventArgs e) {
			
			List<NWN2AreaViewer> allAreaViewers = NWN2ToolsetMainForm.App.GetAllAreaViewers();
			
			foreach (NWN2AreaViewer viewer in allAreaViewers) {
				bool customIn = false;
				
				try {
					foreach (IElectronPanelInputReceiver receiver in viewer.ElectronPanel.Receivers.ToArray()) {
						if (receiver is NWN2Toolset.NWN2.Views.Input.PaintTerrainInputReceiver && !(receiver is InputReceiver)) {
							// We add it here to ensure that there will always be a pointer to the receiver
							oldReceiverTable[viewer] = receiver;
							viewer.ElectronPanel.RemoveInputReceiver(receiver);
							
						} else if (receiver is InputReceiver)  {
							customIn = true;
						}
					}
				} catch (Exception exception) {
					throw new Exception("Exception thrown when going through the electronpanels: " + exception);
				}
				
				if (!customIn) {
					try {
						InputReceiver newInputReciever = new InputReceiver(data);
						inputReciver = newInputReciever;
			//			painter = newInputReciever.painter;
						
						viewer.ElectronPanel.AddInputReceiver(newInputReciever);
						newReceiverTable[viewer] = newInputReciever;
					} catch (Exception exception) {
						throw new Exception("Exception thrown when trying to adding Multibrush: " + exception);
					}
				}
			}
			
			if (!stop) {
				base.OnTick(e);
			} else {
				// Remove the custom receiver and re-add the origional receiver
				this.normalizeListeners();
				// Stop the system
				try {
					this.Stop();
				} catch (Exception exception) {
					throw new Exception("Problem encountered while trying to Stop the intervalTimer: " + exception);
				}
			}
		}
	}
}