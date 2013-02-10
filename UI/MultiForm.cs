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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using NWN2Toolset.NWN2.Views;

namespace Multibrush.UI
{
	/// <summary>
	/// Description of MultiForm.
	/// </summary>
	public partial class MultiForm : Form
	{
		public brushData data;
		public string saveFormat = ".xml";
		public string filter = "XML files (*.xml)|*.xml";
		public bool stopSystem = false;
		private bool skipReload = false;
		private static MultiForm form;
		
		public const int NUM_TEXTURE_TOTAL = 8;
		public const int NUM_TEXTURE_UNIQE = 6;
		
		private MultiForm() {
			setup();
		}
		
		public static MultiForm makeMultiForm() {
			if (form == null)
				form = new MultiForm();
			
			return form;
		}
		
		public void setData(brushData data) {
			if (data != null) {
				textureList.Items.Clear();
				setValuesToFormControls(data);
				this.data = data;
			}
		}

		private static void setTooltip(System.Windows.Forms.Control button, string str)
		{
			// Create the ToolTip and associate with the Form container.
			ToolTip toolTip = new ToolTip();

			// Set up the delays for the ToolTip.
			toolTip.AutoPopDelay = 3000;
			toolTip.InitialDelay = 1000;
			toolTip.ReshowDelay = 500;
			
			// Force the ToolTip text to be displayed whether or not the form is active.
			toolTip.ShowAlways = true;
			
			// Set up the ToolTip text for the Button and Checkbox.
			toolTip.SetToolTip(button, str);
		}

		public void setup() {
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			// Initialize the Terrain
			string a2da = "terrainmaterials";
			string ter = "Terrain";

			brushTextureList.Initialize(a2da, ter, ter);
			
			brushTextureList.ChangeSelected += new TextureBrowser.ChangeSelectedEventHandler(previewTexture);
			previewTexture(null);
			
			// Paints
			setTooltip(numericRounds, "Set the number of paints that you want performed. Remember it will make the " +
			           "squared number of paints");
			
			// Presure:
			texturePresureChanged(null, null);
			
			// Presure change handlers
			presureTrack.ValueChanged += new EventHandler(this.texturePresureChanged);
			setTooltip(presureTrack, "Set the presure of the current texture");
			
			coverageTrack.ValueChanged += new EventHandler(this.UpdateRange);
			setTooltip(coverageTrack, "Set the coverage of the current texture");
			
			UpdateRange(null, null);
			
			// GrassValues
			UpdateGrass(null, null);
			grassCoverageTrackbar.ValueChanged += new EventHandler(this.UpdateGrass);
			setTooltip(grassCoverageTrackbar, "Set the coverage of the grass");
			
			grassDensityTrackbar.ValueChanged += new EventHandler(this.UpdateGrass);
			setTooltip(grassDensityTrackbar, "Set the grass density");
			
			grassTextures.Initialize("grass", "Texture", "Grass");
			grassTextures.multiple();
			
			bladeSize.ValueChanged += new EventHandler(this.secondaryBlade);
			setTooltip(bladeSize, "Set the size of the blades");
			
			bladeSizeVariation.ValueChanged += new EventHandler(this.secondaryBlade);
			setTooltip(bladeSizeVariation, "Set the variation of the blade size");
			
			secondaryBlade(null, null);
			
			paintGrass.Checked = false;
			setTooltip(paintGrass, "Set whether or not you want to have grass painted");
			
			colourButton.BackColor = Color.Black;
			colourButton.Click += new EventHandler(this.colour);
			
			colourCoverage.ValueChanged += new EventHandler(this.colourTrackbarValues);
			setTooltip(colourCoverage, "Set the coverage of the colour");
			
			colourPresure.ValueChanged += new EventHandler(this.colourTrackbarValues);
			setTooltip(colourPresure, "Set the presure of the colour");
			
			paintColour.Checked = false;
			setTooltip(paintColour, "Set whether you want to have colour painted or not");

			colourTrackbarValues(null, null);
			
			// export and import
			exportButton.Click += new EventHandler(exportBrush);
			setTooltip(exportButton, "Export the current settings to a XML file");
			
			importButton.Click += new EventHandler(importBrush);
			setTooltip(importButton, "Import settings from a XML file");
			
			// eyedrop tool
			eyedropTool.Click += new EventHandler(eyedropMethod);
			setTooltip(eyedropTool, "The eyedrop tool allows you to select a terrain region and import its selected textures into this form");
			eyedropTool.DialogResult = DialogResult.OK;
			
			Polygon.Click += new EventHandler(polygonMethod);
			setTooltip(Polygon, "The polygon tool allows you to paint a polygon and let the tool fill it out with textures");
			Polygon.DialogResult = DialogResult.OK;
			
			exit.Click += new EventHandler(exitMethod);
			setTooltip(exit, "Start using the multibrush");
			exit.DialogResult = DialogResult.OK;
			
			stopUsing.Click += new EventHandler(stopMethod);
			setTooltip(stopUsing, "Stop using the multibrush");
			stopUsing.DialogResult = DialogResult.OK;
			
			// Textures list
			setTooltip(textureList, "Textures placed heigher will be painted before textures placed lower");
			
			// Accept button
			acceptBrush.Click += new EventHandler(AcceptBrushClick);
			setTooltip(acceptBrush, "Save the settings for the current brush");
			
			up.Click += new EventHandler(UpClick);
			setTooltip(up, "Move the selected brush up one level in the hierarchy");
			
			down.Click += new EventHandler(DownClick);
			setTooltip(down, "Move the selected brush down one level in the hierarchy");
			
			add.Click += new EventHandler(addBrush);
			setTooltip(add, "Add a new brush");
			
			remove.Click += new EventHandler(removeBrush);
			setTooltip(remove, "Remove the selected brush");
			
			numericRounds.Value = 3;
			numericRounds.Minimum = 1;
			numericRounds.Maximum = 10;
			
			toPresureVar.ValueChanged += new EventHandler(this.varianceChanged);
			fromPresureVar.ValueChanged += new EventHandler(this.varianceChanged);
			toCoverageVar.ValueChanged += new EventHandler(this.varianceChanged);
			fromCoverageVar.ValueChanged += new EventHandler(this.varianceChanged);
			varianceChanged(toPresureVar, null);
			varianceChanged(fromPresureVar, null);
			varianceChanged(toCoverageVar, null);
			varianceChanged(fromCoverageVar, null);
			
			LinkedList<System.Windows.Forms.Control> controls = new LinkedList<Control>();
			
			// Set the tabindex
			
			#region Textures
			controls.AddLast(brushTextureList);
			controls.AddLast(numericRounds);
			controls.AddLast(randomCheck);
			
			// List controls
			controls.AddLast(up);
			controls.AddLast(down);
			controls.AddLast(textureList);
			controls.AddLast(add);
			controls.AddLast(remove);
			
			controls.AddLast(presureTrack);
			controls.AddLast(coverageTrack);
			
			controls.AddLast(acceptBrush);
			
			controls.AddLast(fromPresureVar);
			controls.AddLast(toPresureVar);
			controls.AddLast(fromCoverageVar);
			controls.AddLast(toCoverageVar);
			#endregion
			
			#region Options
			controls.AddLast(exit);
			
			controls.AddLast(importButton);
			controls.AddLast(exportButton);

			controls.AddLast(numInner);
			controls.AddLast(numOuter);
			
			controls.AddLast(eyedropTool);

			controls.AddLast(stopUsing);
			#endregion
			
			#region Colour
			controls.AddLast(colourButton);
			controls.AddLast(paintColour);
			controls.AddLast(colourCoverage);
			controls.AddLast(colourPresure);
			#endregion
			
			#region Brush size
			controls.AddLast(numInner);
			controls.AddLast(numOuter);
			#endregion
			
			#region Grass
			controls.AddLast(grassCoverageTrackbar);
			controls.AddLast(grassDensityTrackbar);
			controls.AddLast(bladeSize);
			controls.AddLast(bladeSizeVariation);
			controls.AddLast(paintGrass);
			controls.AddLast(grassTextures);
			#endregion
			
			int index = 1;
			foreach(System.Windows.Forms.Control control in controls) {
				control.TabIndex = index;
				index++;
			}
			
			
			if (textureList.Items.Count == 0) {
				addBrush(null, null);
			}
			textureList.SelectedIndex = 0;
			
			// Popup with information on how to close the plug-in
			if (Multibrush.PluginContainer.popup) {

				stopMulti stop = stopMulti.makeStopMulti();
				
				stop.reset();
				stop.ShowDialog();
				
				Multibrush.PluginContainer.popup = !stop.dontShow;
			}
		}
		
		void debugOut(String str) {
			Console.WriteLine(str);
		}
		
		void varianceChanged(object sender, EventArgs e) {
			NumericUpDown updown = (NumericUpDown)sender;
			
			if (updown == fromPresureVar) {
				if (toPresureVar.Value < updown.Value)
					toPresureVar.Value = updown.Value;
			} else if (updown == toPresureVar) {
				if (updown.Value < fromPresureVar.Value)
					fromPresureVar.Value = updown.Value;
			}
			
			else if (updown == fromCoverageVar) {
				if (toCoverageVar.Value < updown.Value)
					toCoverageVar.Value = updown.Value;
			} else if (updown == toCoverageVar) {
				if (updown.Value < fromCoverageVar.Value)
					fromCoverageVar.Value= updown.Value;
			} else {
				throw new Exception("Wrong sender in varianceChanged");
			}
		}
		
		void TextureListSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!skipReload) {
				textureData texture = (textureData)textureList.SelectedItem;
				if (texture != null) {
					brushTextureList.setListByName(texture.texture);
					presureTrack.Value = texture.getPresureUnmodified();
					coverageTrack.Value = texture.getCoverageUnmodified();
					
					// Variance
					// I need to do this in "reverse" order, to ensure I won't get into any trouble with the minimum and maximum settings
					setVariance(toCoverageVar, texture.varCoverage.Y);
					setVariance(fromCoverageVar, texture.varCoverage.X);
					
					setVariance(toPresureVar, texture.varPresure.Y);
					setVariance(fromPresureVar, texture.varPresure.X);
				}
			}
			skipReload = false;
		}
		
		private static void setVariance(NumericUpDown numpad, int varianceValue) {
			// I need to ensure that the minimum and maximum values are set correctly
			numpad.Value = varianceValue;
			
		}
		
		void stopMethod(object sender, EventArgs e) {
			this.stopSystem = true;
			this.Close();
		}
				
		void exitMethod(object sender, EventArgs e) {
			getDataAndClose(paintmode.paint);
		}
		
		void eyedropMethod(object sender, EventArgs e) {
			getDataAndClose(paintmode.eyedrop);
		}
		
		void polygonMethod(object sender, EventArgs e) {
			getDataAndClose(paintmode.polygon);
		}
		   
		void getDataAndClose(paintmode mode) {
			this.stopSystem = false;
			getValuesFromFormControls();
			this.data.mode = mode;
			this.Close();
		}
		
		void previewTexture(object sender) {
			previewTexture1.BackgroundImage= ((TextureListItem)brushTextureList.SelectedItem).Image;
		}
		
		void colourTrackbarValues(object sender, EventArgs e) {
			colourCoverageLabel.Text = colourCoverage.Value + "%";
			colourPresureLabel.Text = colourPresure.Value + "%";
		}
		
		void secondaryBlade(object sender, EventArgs e) {
			bladeSizeLabel.Text = (((float)bladeSize.Value) / 1000.0f).ToString();
			bladeSizeVariationLabel.Text = (((float)bladeSizeVariation.Value) / 1000.0f).ToString();
		}
		
		void UpdateGrass (object sender, EventArgs e) {
			grassCoverageLabel.Text = grassCoverageTrackbar.Value + "%";
			grassDensityLabel.Text = grassDensityTrackbar.Value + "%";
		}
		
		void UpdateRange(object sender, EventArgs e)
		{
			coverageLabel.Text = "Coverage: " + coverageTrack.Value + "%";
		}
		
		
		void texturePresureChanged(object sender, EventArgs e)
		{
			presureLabel.Text = "Presure: " + presureTrack.Value + "%";
		}
		
		void colour(object sender, EventArgs e) {
			System.Windows.Forms.ColorDialog dia = new System.Windows.Forms.ColorDialog();
			if (dia.ShowDialog() == DialogResult.OK) {
				colourButton.BackColor = dia.Color;
			}
		}
				
		void exportBrush(object sender, EventArgs e) {
			try {
				FileDialog fileDia = new SaveFileDialog();
				fileDia.Filter = filter;
				fileDia.DefaultExt = saveFormat;
				
				if (fileDia.ShowDialog() == DialogResult.OK) {
					getValuesFromFormControls();
					string filename = fileDia.FileName;
					LinkedList<textureData> textures = new System.Collections.Generic.LinkedList<textureData>();
					foreach (textureData tex in textureList.Items) {
						textures.AddLast(tex);
					}
					data.exportBrushData(filename, textures, grassTextures.selectedTextures);
					
				}
			} catch (Exception e2) {
				Console.WriteLine(e2);
			}
		}
		
		void importBrush(object sender, EventArgs e) {
			try {
				FileDialog fileDia = new OpenFileDialog();
				fileDia.Filter = filter;
				fileDia.DefaultExt = saveFormat;

				if (fileDia.ShowDialog() == DialogResult.OK) {
					string filename = fileDia.FileName;
					if (fileDia.CheckFileExists) {
						textureList.Items.Clear();
						data = brushData.importBrush(filename, NUM_TEXTURE_TOTAL);
						
						// All the values have been set - now is the time to set the form itself
						setValuesToFormControls(data);
					} else {
						System.Windows.Forms.MessageBox.Show("File \"" + filename + "\" does not exist. Please choose another one");
					}
				}
			} catch (Exception e2) {
				Console.WriteLine(e2);
			}
		}
		
		void getValuesFromFormControls() {
			try {
				data = new brushData();
				
				//Texture brushes
				foreach(textureData tex in textureList.Items) {
					data.textures.AddLast(tex);
				}
				
				data.rounds = (int)numericRounds.Value;
				// Grass
				data.grassCoverage = grassCoverageTrackbar.Value;
				data.grassDensity = grassDensityTrackbar.Value;
				
				data.grassSize = bladeSize.Value;
				data.grassSizeVariation = bladeSizeVariation.Value;
				data.numSelected = grassTextures.numSelected;
				
				data.grassTexture = grassTextures.selectedTextures;
				
				if (paintGrass.Checked) {
					data.grassOption = grassOption.Paint;
				} else {
					data.grassOption = grassOption.NoGrass;
				}
				
				data.grassTexture = grassTextures.getListOfNames();
				
				// The brush itself
				data.innerCircle = (int)numInner.Value;
				data.outerCircle = (int)numOuter.Value;
				
				// Colour
				data.col = colourButton.BackColor;
				
				if (paintColour.Checked) {
					data.colourOption = colourOption.Colour;
				} else {
					data.colourOption = colourOption.NoColour;
				}
				
				data.colourCoverage = colourCoverage.Value;
				data.colourPresure = colourPresure.Value;
				
				data.randomize = randomCheck.Checked;
				
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
		
		public void setValuesToFormControls(brushData localData) {
			try {
				// Texture
				foreach(textureData tex in localData.textures) {
					textureList.Items.Add(tex);
				}
				textureList.SelectedIndex = 0;
				
				// Grass
				grassCoverageTrackbar.Value = localData.grassCoverage;
				grassDensityTrackbar.Value = localData.grassDensity;
				foreach(string grassTexture in data.grassTexture) {
					grassTextures.setListByName(grassTexture);
				}
				
				bladeSize.Value = localData.grassSize;
				bladeSizeVariation.Value = localData.grassSizeVariation;
				
				if (localData.grassOption == grassOption.NoGrass) {
					paintGrass.Checked = false;
				} else if (localData.grassOption == grassOption.Paint) {
					paintGrass.Checked = true;
				} else {
					throw new Exception("Wrong grass option in init data");
				}
				
				// Outer and inner brush size
				numInner.Value = (decimal)localData.innerCircle;
				numOuter.Value = (decimal)localData.outerCircle;
				
				//Colour
				colourButton.BackColor = localData.col;
				
				colourCoverage.Value = localData.colourCoverage;
				colourPresure.Value = localData.colourPresure;
				
				if (localData.colourOption == colourOption.Colour) {
					paintColour.Checked = true;
				} else if (localData.colourOption == colourOption.NoColour) {
					paintColour.Checked = false;
				} else {
					throw new Exception("Wrong colour option in init data");
				}
				
				randomCheck.Checked = data.randomize;
				
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
		
		private bool canAdd() {
				Hashtable table = new Hashtable();
				
				// Find the number of  uniqe textures
				foreach (textureData tex in textureList.Items) {
					table[tex.ToString()] = 1;
				}
				
				string texName = brushTextureList.SelectedItem.ToString();
				
				// Since the toolset can only handle 6 uniqe textures, we need this check
				return (table.Count < NUM_TEXTURE_UNIQE || table.ContainsKey(texName));
			}
		
		void addBrush(object sender, EventArgs e)
		{
			/* 
			 * There is no theoretical limit on the number of repeat textures, but there needs to be a balance between
			 * the number of textures you can select and the time it will take to paint them all.
			 */
			if (textureList.Items.Count < NUM_TEXTURE_TOTAL) {
				if (canAdd()) {
					textureData texture = new textureData(brushTextureList.SelectedItem.ToString());
					textureList.Items.Add(texture);
					textureList.SelectedItem = texture;
				}
			}
		}
		
		void removeBrush(object sender, EventArgs e)
		{
			if (textureList.Items.Count > 1) {
				int index = textureList.SelectedIndex;
				textureList.Items.Remove(textureList.SelectedItem);
				textureList.SelectedIndex = Math.Min(index, textureList.Items.Count - 1);
			}
		}
		
		void UpClick(object sender, EventArgs e)
		{
			int index = textureList.SelectedIndex;
			object Swap = textureList.SelectedItem;
			if (index > 0) {
				textureList.Items.RemoveAt(index);
				textureList.Items.Insert(index - 1, Swap);
				textureList.SelectedItem = Swap;
			}
		}
		
		void DownClick(object sender, EventArgs e)
		{
			int index = textureList.SelectedIndex;
			int length = textureList.Items.Count - 1;
			object Swap = textureList.SelectedItem;
			if (index > -1 && length > index) {
				textureList.Items.RemoveAt(index);
				textureList.Items.Insert(index + 1, Swap);
				textureList.SelectedItem = Swap;
			}
		}
		
		
		void AcceptBrushClick(object sender, EventArgs e)
		{
			if (canAdd()) {
				int index = textureList.SelectedIndex;
				Point cov = new Point((int)fromCoverageVar.Value, (int)toCoverageVar.Value);
				Point pres = new Point((int)fromPresureVar.Value, (int)toPresureVar.Value);
				
				textureData texture = new textureData(brushTextureList.SelectedItem.ToString(),
				                                      presureTrack.Value,
				                                      coverageTrack.Value,
				                                      cov,
				                                      pres);
				skipReload = true;
				textureList.Items.Remove(textureList.SelectedItem);
				textureList.Items.Insert(index, texture);
				textureList.SelectedIndex = index;
			}
		}
		
	}
}
