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
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;

using NWN2Toolset;
using NWN2Toolset.NWN2.NetDisplay;
using NWN2Toolset.NWN2.Views;
using NWN2Toolset.Plugins;
using TD.SandBar;

namespace Multibrush
{
	public class PluginContainer : INWN2Plugin
	{
		#region Fields
		
		private static IntervalTimer m_timer;

		//   public SerializableDictionary<string, string> subPreferences = new SerializableDictionary<string, string>();
		private const String name = "Multibrush";
		private MenuButtonItem m_cMenuItem;
		private Dictionary<string, ToolBarDef> AllToolbars;
		
		private static ButtonItem startStop;
		
		private const String startStr = " is Active";
		private const String stopStr = " is Inactive";
		private static String tool = "Brush";
		
		private static ButtonItem edit;
		
		private static ButtonItem drawPolygon;
		
		public static bool popup = true;
		private MultibrushPref preferences = new MultibrushPref();
		#endregion
		
		#region Toolbar
		private enum ToolBarInitialPos
		{
			Top,
			Bottom,
			Left,
			Right
		};

		private List<ToolBarDef> GetAllToolbars()
		{
			List<ToolBarDef> list = new List<ToolBarDef>();
			foreach (ToolBarDef def in this.AllToolbars.Values)
			{
				list.Add(def);
			}
			return list;
		}

		public struct ToolBarDef
		{
			public TD.SandBar.ToolBar toolBar;
			public string NWNToolsetDockName;
		};

		private void buildToolbars()
		{
			AllToolbars = new Dictionary<string, ToolBarDef>();
			CreateToolBar(name, ToolBarInitialPos.Top);
			
			startStop = new ButtonItem();
			edit = new ButtonItem();
			drawPolygon = new ButtonItem();
				
			startStop.Activate += new EventHandler(toogleStartStop);
			setStartStopTitle(false);
			
			edit.Activate += new EventHandler(callMakeMultiForm);
			edit.Text = "Brush Settings";
			edit.ToolTipText = "Set the settings for the brush";
			
			drawPolygon.Activate += new EventHandler(drawPolygonMethod);
			drawPolygon.Text = "Draw polygon";
			drawPolygon.ToolTipText = "Draw the textures on the selected polygon";
			drawPolygon.Enabled = false;
			
			AddButtonToToolbar(name, startStop);
			AddButtonToToolbar(name, edit);
			AddButtonToToolbar(name, drawPolygon);
		}
		
		
		/// <summary>
		/// A utility method to set the correct text and tooltip for the star/stop button
		/// </summary>
		/// <param name="start">If start is true we set the start text, otherwise we set the stop text</param>
		public static void setStartStopTitle(bool start) {
			if (start) {
				startStop.Text = tool + startStr;
				startStop.ToolTipText = "Start using the " + name;
				
			} else {
				startStop.Text = tool + stopStr;
				startStop.ToolTipText = "Stop using the " + name;
				
				
			}
		}
		
		/// <summary>
		/// When we press the start/stop button we must set it to be the correct value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void toogleStartStop(object sender, EventArgs e) {
			if (m_timer != null) {
				if (startStop.Text != tool + startStr) {
					
					setStartStopTitle(true);
					m_timer.resumeTexture();
				} else {
					
					setStartStopTitle(false);
					m_timer.stopTexture();
				}
			} else {
				setStartStopTitle(true);
				callMakeMultiForm(null, null);
			}
		}
		
		/// <summary>
		/// When we go into polygon mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void drawPolygonMethod(object sender, EventArgs e) {
			// In both cases we must call setStartStopTitle with true, so it might as well go outside
			setStartStopTitle(true);
			
			if (m_timer != null) {
				m_timer.drawPolygon();
			} else {
				callMakeMultiForm(null, null);
			}
		}
		
		
		private string GetDockNameFromPosEnum(ToolBarInitialPos pos)
		{
			switch (pos)
			{
				case ToolBarInitialPos.Top:
				default:
					return "topSandBarDock";
				case ToolBarInitialPos.Bottom:
					return "bottomSandBarDock";
				case ToolBarInitialPos.Left:
					return "leftSandBarDock";
				case ToolBarInitialPos.Right:
					return "rightSandBarDock";
			}
		}

		private bool AddButtonToToolbar(string toolbarName, ButtonItem buttonToAdd)
		{
			if (this.AllToolbars.ContainsKey(toolbarName))
			{
				this.AllToolbars[toolbarName].toolBar.Items.Add(buttonToAdd);
				return true;
			}
			return false;
		}

		private void CreateToolBar(string name, ToolBarInitialPos initialPos)
		{
			TD.SandBar.ToolBar temp = new TD.SandBar.ToolBar();
			temp.Name = name;
			temp.Overflow = ToolBarOverflow.Hide;
			temp.AllowHorizontalDock = true;
			temp.AllowRightToLeft = true;
			temp.AllowVerticalDock = true;
			temp.Closable = false;
			temp.Movable = true;
			temp.Tearable = true;
			temp.DockLine = 2;

			ToolBarDef tbd = new ToolBarDef();
			tbd.NWNToolsetDockName = GetDockNameFromPosEnum(initialPos);
			tbd.toolBar = temp;

			AllToolbars.Add(name, tbd);
		}
		#endregion

		#region Plug-in Essential
		public string DisplayName
		{
			get { return name; }
		}

		public string MenuName
		{
			get { return name; }
		}

		public string Name
		{
			get { return name; }
		}

		public MenuButtonItem PluginMenuItem {
			get
			{
				return m_cMenuItem;
			}
		}

		public object Preferences
		{
            get
                {
                MultibrushPref prefs = new MultibrushPref();
                prefs.closePopup = popup;
                return prefs;
                }
            set
                {
                MultibrushPref prefs = (MultibrushPref)value;
                popup = prefs.closePopup;
                }
		}
		
		private void HandlePluginLaunch(object sender, EventArgs e)
		{
			// Create the toolbar
			try {
				List<ToolBarDef> allToolbars = this.GetAllToolbars();
				for (int i = 0; i < allToolbars.Count; i++)
				{
					for (int j = 0; j < NWN2ToolsetMainForm.App.Controls.Count; j++)
					{
						if (NWN2ToolsetMainForm.App.Controls[j].GetType() == typeof(ToolBarContainer))
						{
							ToolBarContainer container = (ToolBarContainer)NWN2ToolsetMainForm.App.Controls[j];
							if (container.Name == allToolbars[i].NWNToolsetDockName)
							{
								container.Controls.Add(allToolbars[i].toolBar);
								break;
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				System.Windows.Forms.MessageBox.Show(exception.Message);
			}
		//	callMakeMultiForm(null, null);			
		}
		
		private static void callMakeMultiForm(object sender, EventArgs e) {
			makeMultiForm(null);
		}
		
		/// <summary>
		/// A utility method to handle the launch of the multiform. 
		/// If the data is different than null, then it becomes the default data for the multiform
		/// </summary>
		/// <param name="data"></param>
		public static void makeMultiForm(brushData data) {
		
			UI.MultiForm form = UI.MultiForm.makeMultiForm();
		
			if (data != null)
				form.setData(data);
			
			if (DialogResult.OK == form.ShowDialog() && form.data != null) {
					
				drawPolygon.Enabled = false;
				if (form.data.mode == paintmode.paint) {
					debugOut("brush!");
					tool = "Brush";
				} else if (form.data.mode == paintmode.eyedrop) {
					debugOut("eyedrop");
					tool = "Eyedrop tool";
				} else if (form.data.mode == paintmode.polygon) {
					debugOut("Polygon");
					tool = "Polygon paint";
					drawPolygon.Enabled = true;
				} else {
					throw new Exception("Error wrong paint mode");
				}
			//	setStartStopTitle(true);
				
				if (!form.stopSystem) {
					if (m_timer != null) {
						m_timer.setNewData(form.data);
						m_timer.resumeTexture();	
					} else {
						m_timer = new IntervalTimer(form.data);
						m_timer.startTexture();
					} 
					setStartStopTitle(true);
				}
				else {
					
					if (m_timer != null) {
						m_timer.stopTexture();
						setStartStopTitle(false);
					}
				}
			}
			
			if (form.data == null) {
				setStartStopTitle(false);
			}
		}

		private static void debugOut(string str) {
				Console.WriteLine(str);
		}
		
		public void Startup(INWN2PluginHost cHost)
		{
			m_cMenuItem = cHost.GetMenuForPlugin(this);
			m_cMenuItem.Activate += new EventHandler(this.HandlePluginLaunch);
			
			buildToolbars();
			
			//    NWN2ToolsetMainForm.App.KeyPreview = true;
			//   NWN2ToolsetMainForm.App.KeyDown += new System.Windows.Forms.KeyEventHandler(NWN2BrushSaver);
		}

		public void Shutdown(INWN2PluginHost cHost)
		{
		}

		public void Load(INWN2PluginHost cHost)
		{
		}

		public void Unload(INWN2PluginHost cHost)
		{
			UI.MultiForm form = UI.MultiForm.makeMultiForm();
			form.Dispose();
			
			m_cMenuItem.Dispose();
			m_cMenuItem = null;		
		}
		#endregion

		#region Preference
        public class MultibrushPref
            {
          	
        	bool popup = true;
        	
        	[Category("Popup help"), DisplayName("Close information"), Browsable(true), Description("Decide whether the popup with information on how to stop the Multibrush "   	                                                                                       + "should be shown when the settings form for the Multibrush is started")]
            public bool closePopup
                {
                get { return popup; }
                set { popup = value; }
                }

            }
		#endregion
	}
}

