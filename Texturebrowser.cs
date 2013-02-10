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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using NWN2Toolset.NWN2.Views;
using OEIShared.IO.TwoDA;
using OEIShared.Utils;

namespace Multibrush
{
	/// <summary>
	/// The TextureBrowser is my own custom implementation of the texture browser found in the toolset. It was an attempt to avoid some strange DirectX 
	/// errors that I was getting (and getting in some custom code I felt I needed)
	/// The errors haven't stopped comming, but at least I now have my own custom texture browser and some experiance in writing Windows forms
	/// </summary>
	public class TextureBrowser : System.Windows.Forms.UserControl
	{
		#region Fields
		private System.ComponentModel.IContainer containter;

		public delegate void ChangeSelectedEventHandler(object sender);
		public event ChangeSelectedEventHandler ChangeSelected; // event handler for range changed
		
		public int numSelected = 0;
		public string[] selectedTextures;
		
		private System.Windows.Forms.ListBox list;
		#endregion
		
		/// <summary>
		/// The constructor
		/// </summary>
		public TextureBrowser()
		{
			InitializeComponent();
			
			//Perhaps this is not good
			containter = new Container();
			
			ComponentResourceManager manager = new ComponentResourceManager(typeof(NWN2TextureBrowser));

			manager.ApplyResources(list, "list");
			this.list.DrawMode = DrawMode.OwnerDrawFixed;
			manager.ApplyResources(list, "list");
		}
		
		private void InitializeComponent()
		{
			this.list = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			
			// list		 
			this.list.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.list.FormattingEnabled = true;
			this.list.Location = new System.Drawing.Point(0, 0);
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(230, 220);
			this.list.Sorted = true;
			this.list.TabIndex = 0;
			this.list.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.draw);
			this.list.SelectedIndexChanged += new EventHandler(OnChangeSelected);
			
			// TextureBrowser
			this.Controls.Add(this.list);
			this.DoubleBuffered = true;
			this.Name = "TextureBrowser";
			this.Size = new System.Drawing.Size(230, 220);
			this.Load += new System.EventHandler(this.OnLoad);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			this.Resize += new System.EventHandler(this.OnResize);
			this.SizeChanged += new System.EventHandler(this.OnSizeChanged);
			this.ResumeLayout(false);
		}
			
		/// <summary>
		/// The methods call when the class is loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLoad(object sender, System.EventArgs e)
		{
			// use double buffering
			SetStyle(ControlStyles.DoubleBuffer,true);
			SetStyle(ControlStyles.AllPaintingInWmPaint,true);
			SetStyle(ControlStyles.UserPaint,true);
			
		}
		
		/// <summary>
		/// The method called when draw is invoked, here used to ensure that both the images and the text is properly formatted
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="args1"></param>
		private void draw(object obj, DrawItemEventArgs args1)
		{
			args1.DrawBackground();
			if (!CommonUtils.DesignMode)
			{
				TextureListItem item = this.list.Items[args1.Index] as TextureListItem;
				if (item.Image != null)
				{
					Rectangle imageRec = new Rectangle(args1.Bounds.Left + 2, args1.Bounds.Top + 2, 0x40, 0x40);
					args1.Graphics.DrawImage(item.Image, imageRec);
				}
				
				Rectangle stringRec = new Rectangle(args1.Bounds.Left + 0x44,
				                                    args1.Bounds.Bottom - 0x10,
				                                    args1.Bounds.Width - 0x45,
				                                    args1.Bounds.Height);
				args1.Graphics.DrawString(item.Text, this.Font, SystemBrushes.WindowText, stringRec);
				args1.DrawFocusRectangle();
			}
			
		}

		/// <summary>
		/// This method makes it possible to select multiple items from the browser. It is only used for the grassbrowser
		/// </summary>
		public void multiple() {
			this.list.SelectionMode = SelectionMode.MultiSimple;
			this.list.Sorted = true;
			selectedTextures = new string[3];
			this.list.SelectedIndexChanged += new EventHandler(this.GrassSelection);
			GrassSelection(null, null);

		}
		
		/// <summary>
		/// When the selection of the grass texture is changed
		/// It records the grass textures selected, and makes sure that only up to 3 grass textures can be selected at any one time
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GrassSelection(object sender, EventArgs e) {
			try {

				if (this.list.SelectedItems.Count > 3)
				{
					/* 
 					 * We go through every item in the grass texture list and check whether it is contained in the
					 * selected textures
					 */
					
					LinkedList<String> selectedTextures = new LinkedList<String>(this.selectedTextures);
					
					for (int i = 0; i < this.list.SelectedIndices.Count; i++)
					{
				//		bool flag = false;
						TextureListItem item = this.list.Items[this.list.SelectedIndices[i]] as TextureListItem;
						
						/*
						for (int j = 0; j < this.numSelected; j++)
						{
							if (this.selectedTextures[j] == item.ToString())
							{
								flag = true;
								break;
							}
						} */
						
						if (! selectedTextures.Contains(item.ToString()))
						{
							this.list.SetSelected(this.list.SelectedIndices[i], false);
						}
					}
				}
				else
				{
					string str = "";
					for (int k = 0; k < this.list.SelectedItems.Count; k++)
					{
						TextureListItem item2 = this.list.SelectedItems[k] as TextureListItem;
						str = str + item2.ToString() + ", ";
						this.selectedTextures[k] = item2.ToString();
					}
					this.numSelected = this.list.SelectedItems.Count;
				}
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
		
		/// <summary>
		/// The automaticly called dispose message
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (containter != null))
			{
				containter.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// The method to load the terrain information (icons and string)
		/// </summary>
		/// <param name="type1"></param>
		/// <param name="type2"></param>
		/// <param name="textureOrGrass">The path to either the textures or the grass</param>
		public void Initialize(string type1, string type2,string textureOrGrass) {
			string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Neverwinter Nights 2\\NWN2Toolset\\");
			
			TwoDAColumn column = TwoDAManager.Instance.Get(type1).Columns[type2];
			
			for (int j = 0; j < column.Count; j++) {
				if (column.IsPopulatedValue(j))
				{
					Bitmap bitmap3 = null;
					string str1 = OEIShared.IO.ResourceManager.Instance.BaseDirectory + "/NWN2Toolset/" + textureOrGrass + "/" + column[j] + ".bmp";
					string str2 = OEIShared.IO.ResourceManager.Instance.BaseDirectory + "/NWN2Toolset_X1/" + textureOrGrass + "/" + column[j] + ".bmp";
					string str3 = OEIShared.IO.ResourceManager.Instance.BaseDirectory + "/NWN2Toolset_X2/" + textureOrGrass + "/" + column[j] + ".bmp";
					
					string strToUse = Path.Combine(str + type1, column[j] + ".bmp");
					
					if (!File.Exists(strToUse))
					{
						strToUse = str3;
					}
					
					if (!File.Exists(strToUse))
					{
						strToUse = str2;
					}
					
					if (!File.Exists(strToUse))
					{
						strToUse = str1;
					}
					try
					{
						Bitmap bitmap4 = new Bitmap(strToUse);
						bitmap3 = bitmap4.Clone() as Bitmap;
						bitmap4.Dispose();
					}
					catch (Exception)
					{
						MessageBox.Show("Error Terrain Icon Not Found");
					}
					TextureListItem item2 = new TextureListItem(bitmap3, column[j], new TwoDAReference("terrainmaterials", "STR_REF", true, j));
					list.ItemHeight = bitmap3.Height;
					list.Items.Add(item2);
				}
			}
			
			if (list.Items.Count > 0) {
				list.SelectedItem = list.Items[0];
			}
		}

		/// <summary>
		/// When the size of the container changes, then it must be updated
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSizeChanged(object sender, System.EventArgs e)
		{
			Invalidate(true);
			Update();
		}
		
		/// <summary>
		/// When the size of the container changes, then it must be updatd
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnResize(object sender, System.EventArgs e)
		{
			Invalidate(true);
		}
		
		/// <summary>
		/// I have no special update for repainting the container
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
		}
		
		#region Properties
		
		public int SelectedIndex
		{
			set
			{
				list.SelectedIndex = value;
			}
			
			get {
				return list.SelectedIndex;
			}
		}
		
		public ListBox.SelectedIndexCollection SelectedIndicies
		{
			set
			{
				foreach(int index in value) {
					list.SelectedIndex = index;
				}

			}
			
			get {
				return list.SelectedIndices;
			}
			
		}
		
		public object SelectedItem
		{
			get
			{
				return this.list.SelectedItem;
				
			}
		}
		#endregion
		
		/// <summary>
		/// Set the selected items in the texture browser based on the names instead of the indexes
		/// </summary>
		/// <param name="name">The name we want to have set</param>
		public void setListByName(string name) {
			int index = 0;
			foreach (TextureListItem item in list.Items) {
				if (item.ToString() == name) {
					list.SelectedIndex = index;
					break;
				}
				index++;
			}
		}
		
		/// <summary>
		/// Get the selected textures
		/// </summary>
		/// <returns>An array of strings that contain the selected names</returns>
		public string[] getListOfNames() {
			return selectedTextures;
		}
		
		/// <summary>
		/// When the what is selected changes, s
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnChangeSelected(object sender ,EventArgs e)
		{
			if(ChangeSelected != null )
				ChangeSelected(this);
		}
	}
}