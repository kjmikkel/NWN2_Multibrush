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
using System.Collections.Specialized;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

//using Multibrush.UI;

namespace Multibrush
{
	
	public enum grassOption : int {
		NoGrass,
		Paint
	}
	
	public enum paintmode : int {
		paint,
		eyedrop,
		polygon
	}
	
	public enum colourOption : int {
		Colour,
		NoColour
	}
	/// <summary>
	/// Class that contains the data for each texture
	/// </summary>
	public class textureData {
		public string texture;
		private int presure;
		private int coverage;
		
		// These points contain the interval of change for both the coverage and the presure, in order to make it a bit more random
		public Point varCoverage = new Point(0,0);
		public Point varPresure  = new Point(0,0);
		
		public textureData(string texture) {
			this.texture = texture;
			this.presure = 50;
			this.coverage = 50;
		}
		
		public textureData(string texture, int presure, int coverage, Point vCov, Point vPresure) {
			this.texture = texture;
			this.presure = presure;
			this.coverage = coverage;
			
			this.varCoverage = vCov;
			this.varPresure = vPresure;
		}
		
		public int Presure {
			get {
				return randomChange(this.presure, this.varPresure.X, this.varPresure.Y);
			}
			
			set {
				this.presure = value;
			}
		}
		
		public int getPresureUnmodified() {
			return presure;
		}
		
		
		public int Coverage {
			get {
				return randomChange(this.coverage, this.varCoverage.X, this.varCoverage.Y);
			}
			
			set {
				this.coverage = value;
			}
		}
		
		public int getCoverageUnmodified() {
			return coverage;
		}
		
		/// <summary>
		/// Utility class for finding the
		/// </summary>
		/// <param name="valueBase"></param>
		/// <param name="startChangeValue"></param>
		/// <param name="stopChangeValue"></param>
		/// <returns></returns>
		private static int randomChange(int valueBase, int startChangeValue, int stopChangeValue) {
			Random random = new Random();
			int returnValue = random.Next(startChangeValue, stopChangeValue);
			returnValue += valueBase;
			return Math.Min(100, Math.Max(0, valueBase));
		}
		
		public string printTexture() {
			StringBuilder strBuilder = new StringBuilder();
			strBuilder.Append("\tTexture: " + texture + Environment.NewLine);
			strBuilder.Append("\tCoverage: " + Coverage + Environment.NewLine);
			strBuilder.Append("\tPresure: " + Presure + Environment.NewLine);
						
			if (varCoverage.X != 0 || varCoverage.X != varCoverage.Y) {
				strBuilder.Append("\tVariance of the Coverage: From: " + varCoverage.X + " To: " + varCoverage.Y + Environment.NewLine);
			}
			
			if (varPresure.X != 0 || varPresure.X != varPresure.Y) {
				strBuilder.Append("\tVariance of the Presure: From: " + varPresure.X + " To: " + varPresure.Y + Environment.NewLine);
			}
			
			return strBuilder.ToString();
		}
		
		public override string ToString() {
			return texture;
		}

	}
	
	/// <summary>
	/// Description of brushData.
	/// </summary>
	public class brushData
	{
		//Texture brushes
		public LinkedList<textureData> textures = new LinkedList<textureData>();
		
		public int rounds;
		public bool randomize = false;
		
		// Grass
		public int grassCoverage;
		public int grassDensity;
		
		public int grassSize;
		public int grassSizeVariation;
		public int numSelected;
		
		public grassOption grassOption;
		public string[] grassTexture;
		
		// The brush itself
		public int innerCircle;
		public int outerCircle;
		
		// Colour
		public Color col;
		public colourOption colourOption;
		public int colourCoverage;
		public int colourPresure;
		
		public paintmode mode;
		
		public brushData()
		{
		}
		
		private static textureData readTextureElement(XmlNode node) {
			XmlElement elem = (XmlElement)node;
			
			string texture = elem.InnerXml;
			int coverage = int.Parse(elem.GetAttribute("coverage"));
			int presure = int.Parse(elem.GetAttribute("presure"));
			
			int fromCoverage = ifHasInt(elem, "varCoverageFrom", 0);
			int toCoverage 	 = ifHasInt(elem, "varCoverageTo", 0);
			Point varCoverage = new Point(fromCoverage, toCoverage);
			
			int fromPresure = ifHasInt(elem, "varPresureFrom", 0);
			int toPresure 	= ifHasInt(elem, "varPresureTo", 0);
			Point varPresure = new Point(fromPresure, toPresure);
			
			return new textureData(texture, presure, coverage, varCoverage, varPresure);
		}
		
		private static int ifHasInt(XmlElement element, String text, int defaultValue) {
			if (element.HasAttribute(text)) {
				return int.Parse(element.GetAttribute(text));
			} else {
				return defaultValue;
			}
		}
		
		private static XmlElement makeTextureElement(string name, textureData data, XmlDocument xmlDoc) {
			XmlElement textureElem = xmlDoc.CreateElement(name);
			textureElem.InnerXml = data.texture;
			textureElem.SetAttribute("presure", data.getPresureUnmodified().ToString());
			textureElem.SetAttribute("coverage", data.getCoverageUnmodified().ToString());
			
			// The variance
			textureElem.SetAttribute("varCoverageFrom", data.varCoverage.X.ToString());
			textureElem.SetAttribute("varCoverageTo", data.varCoverage.Y.ToString());
			
			textureElem.SetAttribute("varPresureFrom", data.varPresure.X.ToString());
			textureElem.SetAttribute("varPresureTo", data.varPresure.Y.ToString());
			
			return textureElem;
		}
		
		public static brushData importBrush(String filename, int NUM_TEXTURE_TOTAL) {
			brushData	data = new brushData();
			
			
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(filename);
			
			data.textures = new LinkedList<textureData>();
			
			
			for(int index = 1; index <= NUM_TEXTURE_TOTAL; index++) {
				XmlNodeList node = xmlDoc.GetElementsByTagName("texture" + index);
				if (node[0] != null) {
					data.textures.AddLast(readTextureElement(node[0]));
				} else {
					break;
				}
			}
			
			//Grass
			XmlElement grassElem = (XmlElement)xmlDoc.GetElementsByTagName("Grass")[0];
			
			string grassOptionStr = grassElem.GetAttribute("grassOption");
			
			if (grassOptionStr == grassOption.NoGrass.ToString()) {
				data.grassOption = grassOption.NoGrass;
			} else if (grassOptionStr == grassOption.Paint.ToString()) {
				data.grassOption = grassOption.Paint;
			} else {
				throw new Exception("Wrong grass option");
			}
			
			data.grassSize = int.Parse(grassElem.GetAttribute("bladeSize"));
			data.grassSizeVariation = int.Parse(grassElem.GetAttribute("bladeSizeVariation"));
			data.grassCoverage = int.Parse(grassElem.GetAttribute("coverage"));
			data.grassDensity = int.Parse(grassElem.GetAttribute("density"));
			data.grassTexture = new String[3];
			
			
			string val;
			
			for(int index = 0; index < 3; index++) {
				if (grassElem.HasAttribute("grassTexture" + (index + 1))) {
					val = grassElem.GetAttribute("grassTexture" + (index + 1));
					data.grassTexture[index] = val;
				}
			}
			
			//Brush data
			XmlElement brushElem = (XmlElement)xmlDoc.GetElementsByTagName("Brush_Data")[0];
			data.innerCircle = int.Parse(brushElem.GetAttribute("inner"));
			data.outerCircle = int.Parse(brushElem.GetAttribute("outer"));
			
			// Colour
			XmlElement colourElem = (XmlElement)xmlDoc.GetElementsByTagName("Colour")[0];
			data.col = Color.FromArgb(int.Parse(colourElem.InnerXml));
			
			string colourOptionStr = colourElem.GetAttribute("option");
			if (colourOptionStr == colourOption.Colour.ToString()) {
				data.colourOption = colourOption.Colour;
			} else if (colourOptionStr == colourOption.NoColour.ToString()) {
				data.colourOption = colourOption.NoColour;
			}else {
				throw new Exception("Wrong colour option");
			}
			
			data.colourCoverage = int.Parse(colourElem.GetAttribute("coverage"));
			data.colourPresure = int.Parse(colourElem.GetAttribute("presure"));
			
			XmlElement randomizeElement = (XmlElement)xmlDoc.GetElementsByTagName("Random")[0];
			
			if (randomizeElement != null) {
				data.randomize = bool.Parse(randomizeElement.GetAttribute("randomize"));
			}
			return data;
		}
		
		public void exportBrushData(String filename, LinkedList<textureData> textureList, String[] grassTextures) {
			XmlDocument xmlDoc = new XmlDocument();
			
			// Write down the XML declaration
			XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0","utf-8",null);
			
			// Create the root element
			XmlElement rootNode  = xmlDoc.CreateElement("MultiBrush_Information");
			xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
			xmlDoc.AppendChild(rootNode);
			
			// Textures
			XmlElement textures = xmlDoc.CreateElement("Textures");
			int loopIndex = 1;
			foreach(textureData texture in textureList) {
				textures.AppendChild(makeTextureElement("texture" + loopIndex, texture, xmlDoc));
				loopIndex++;
			}
			rootNode.AppendChild(textures);
			
			//Grass
			XmlElement grass = xmlDoc.CreateElement("Grass");
			grass.SetAttribute("coverage", grassCoverage.ToString());
			grass.SetAttribute("density", grassDensity.ToString());
			grass.SetAttribute("bladeSize", grassSize.ToString());
			grass.SetAttribute("bladeSizeVariation", grassSizeVariation.ToString());
			grass.SetAttribute("grassOption", grassOption.ToString());
			
			int index = 1;
			foreach (string textureStr in grassTextures) {
				grass.SetAttribute("grassTexture" + index, textureStr);
				index++;
			}
			
			rootNode.AppendChild(grass);
			
			XmlElement brushSizeData = xmlDoc.CreateElement("Brush_Data");
			brushSizeData.SetAttribute("inner", innerCircle.ToString());
			brushSizeData.SetAttribute("outer", outerCircle.ToString());
			rootNode.AppendChild(brushSizeData);
			
			XmlElement colourData = xmlDoc.CreateElement("Colour");
			
			colourData.InnerXml = col.ToArgb().ToString();
			colourData.SetAttribute("option", colourOption.ToString());
			colourData.SetAttribute("coverage", colourCoverage.ToString());
			colourData.SetAttribute("presure", colourPresure.ToString());
			rootNode.AppendChild(colourData);
			
			XmlElement randomData = xmlDoc.CreateElement("Random");
			randomData.SetAttribute("randomize", randomize.ToString());
			rootNode.AppendChild(randomData);
			
			xmlDoc.Save(filename);
		}
		
		public override string ToString() {
			StringBuilder strBuilder = new StringBuilder();
			// Textures
			int numTexture = 1;
			foreach(Multibrush.textureData tex in textures) {
				strBuilder.Append("Texture " + numTexture + ":" + Environment.NewLine);
				strBuilder.Append(tex.printTexture());
				strBuilder.Append(Environment.NewLine);
				numTexture++;
			}
			strBuilder.Append(Environment.NewLine);
			
			// Grass
			if (grassOption == Multibrush.grassOption.Paint) {
				strBuilder.Append("Grass:" + Environment.NewLine);
				strBuilder.Append("\tGrass Coverage: " + grassCoverage + Environment.NewLine);
				strBuilder.Append("\tGrass Density: " + grassDensity + Environment.NewLine);
				strBuilder.Append("\tGrass Size: " + grassSize + Environment.NewLine);
				strBuilder.Append("\tGrass Veriation: " + grassSizeVariation + Environment.NewLine);
				
				strBuilder.Append("\n");
				int grassNum = 1;
				foreach (String gTex in grassTexture) {
					strBuilder.Append("\tGrass Texture " + grassNum + ": " + gTex + Environment.NewLine);
					grassNum++;
				}
			}
			
			// Colour
			if (this.colourOption == colourOption.Colour) {
				if (col.IsNamedColor) {
					strBuilder.Append("Colour: " + col.Name + Environment.NewLine);
				} else {
					strBuilder.Append("Colour (rgb): " + col.ToArgb() + Environment.NewLine);
				}
				strBuilder.Append("Colour coverage: " + colourCoverage + Environment.NewLine);
				strBuilder.Append("Colour presure: " + colourPresure + Environment.NewLine);
			}
			return strBuilder.ToString();
		}

	}
}
