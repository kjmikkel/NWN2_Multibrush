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
using System.IO;
using System.Windows.Forms;
using GeometryUtility;
using Microsoft.DirectX;
using NWN2Toolset;
using NWN2Toolset.NWN2.Data;
using NWN2Toolset.NWN2.NetDisplay;
using NWN2Toolset.NWN2.Views;
using OEIShared.NetDisplay;
using OEIShared.OEIMath;
using OEIShared.UI;
using OEIShared.UI.Input;
using PolygonCuttingEar;
using SFX.YATT.DAL;

namespace Multibrush
{
	/// <summary>
	/// The painter method - the class which does the actual painting
	/// A mini dictionary of words used:
	/// 	On-line painting: Painting an area while it is still loaded in the toolset (where the user can see the changes)
	/// 	Off-line painting: Painting an area when it is not seen by the user
	/// </summary>
	public class Painter
	{
		#region Fields
		private NWN2AreaViewer areaViewer = null;
		private ElectronPanel electronPanel = null;
		private brushData data;
		private Random random;
		
		private int newOuter;
		private int newInner;
		
		private int rounds = 10;
		
		private static NWN2TerrainEditorForm TE = NWN2Toolset.NWN2ToolsetMainForm.App.TerrainEditor;

		private static TrackBar barPressure = (TrackBar)TE.Controls.Find("trackBarValue", true)[0];
		private static TrackBar outerRadius = (TrackBar)TE.Controls.Find("trackBarOuterRadius", true)[0];
		private static TrackBar innerRadius = (TrackBar)TE.Controls.Find("trackBarInnerRadius", true)[0];
		
		private static TrackBar grassSize = (TrackBar)TE.Controls.Find("trackBarGrassSize", true)[0];
		private static TrackBar grassSizeVariation = (TrackBar)TE.Controls.Find("trackBarGrassVariation", true)[0];
		
		private static RadioButton opTexture = (RadioButton)TE.Controls.Find("radioButtonTexture", true)[0];
		private static RadioButton opGrass =  (RadioButton)TE.Controls.Find("radioButtonPaintGrass", true)[0];
		
		private static RadioButton opTerrain = (RadioButton)TE.Controls.Find("radioButtonElevation", true)[0];
		private static RadioButton opColour = (RadioButton)TE.Controls.Find("radioButtonColor", true)[0];
		private static Button colourButton = (Button)TE.Controls.Find("buttonColor", true)[0];
		
		private static ListBox grassListBox = (ListBox)TE.Controls.Find("listBoxGrass", true)[0];
		private static ListBox textureListBox = (ListBox)TE.Controls.Find("listBoxTextureNames", true)[0];
		
		private static Button[] textureButtons = new Button[6];

		/*
		private static bool paintClif = true;
		private static int moveConst = 5;
		 */
		
		private const string WAYPOINT_TAG = "MULTIBRUSH_POLYGON_TAG";
		
		private Hashtable pointsAlready = new Hashtable();
		
		private NWN2Toolset.NWN2ToolsetMainForm toolset;
		private NWN2GameModule module;
		#endregion
		
		/// <summary>
		/// The constructor, which is not very important
		/// </summary>
		public Painter() {
		}
		
		/// <summary>
		/// The option to set the electron panel. I cannot set this during the construction, since it must be done while the InputReceiver is attached
		/// </summary>
		/// <param name="electron"></param>
		public void setElectron(ElectronPanel electron) {
			electronPanel = electron;
		}
		
		/// <summary>
		/// This is called when the mousecurser moves around on the area so that we can either
		/// show the circle normally seen, or stop viewing it
		/// </summary>
		/// <param name="panel">The information from the </param>
		public void Move(MousePanel panel) {
			areaViewer = getAreaViewer();
			
			if (!panel.ButtonHeld || areaViewer == null || areaViewer.Area == null) {
				
				SFX.YATT.Tools.Vector<float> coor = findAreaCoor(panel.MouseX, panel.MouseY, areaViewer.Area);

				if (coor != null) {
					showBrush(coor);
				} else {
					stopShowingCircle();
				}
				
			} else {
				stopShowingCircle();
			}
		}
		
		/// <summary>
		/// Set the data for the painter
		/// </summary>
		/// <param name="data"></param>
		public void setOuterInner(brushData data) {
			this.data = data;
			this.newOuter = data.outerCircle;
			this.newInner = data.innerCircle;
			this.rounds = data.rounds;

			opTexture.Checked = true;
			innerRadius.Value = Math.Min(this.newInner, innerRadius.Maximum);
			outerRadius.Value = Math.Min(this.newOuter, outerRadius.Maximum);
			
			// I set the default texture, and textures
			foreach(object texture in textureListBox.Items) {
				if (texture.ToString() == data.textures.First.Value.ToString()) {
					textureListBox.SelectedItem = texture;
					break;
				}
			}
			
			barPressure.Value = data.textures.First.Value.getPresureUnmodified() * 100;
		}

		private static void debugOut(string str) {
			Console.WriteLine(str);
		}
		
		/// <summary>
		/// The eyeDropCode allows us to find out what kind of textures are used in a megatile
		/// </summary>
		private void eyeDropCode() {
			LinkedList<textureData> textures = new LinkedList<textureData>();

			for(int i = 0; i < 6; i++) {
				Label opTexture = (Label)TE.Controls.Find("labelRadioButtonTexture" + (i + 1), true)[0];
				
				if (opTexture.Text != "") {
					textureData texture = new textureData(opTexture.Text);
					textures.AddFirst(texture);
				}
			}
			
			data.textures = textures;
			Multibrush.PluginContainer.makeMultiForm(data);
		}
		
		/// <summary>
		/// Change the brush data
		/// </summary>
		/// <param name="data"></param>
		public void changeData(brushData data) {
			setOuterInner(data);
		}
		
		/// <summary>
		/// Given the number of points selected, this part allows us to take the selected points and paint them with the selected information
		/// </summary>
		public void drawPolygon() {
			pointsAlready.Clear();
			LinkedList<Pair<double, double>> points = new LinkedList<Pair<double, double>>();
			NWN2AreaViewer view = getAreaViewer();
			NWN2GameArea area = view.Area;
			
			foreach (NWN2Toolset.NWN2.Data.Instances.NWN2WaypointInstance waypoint in area.Waypoints) {
				if (waypoint.Tag.Equals(WAYPOINT_TAG)) {
					Vector3 vector = waypoint.Position;
					points.AddLast(new Pair<double, double>(vector.X, vector.Y));
				}
			}
			
			if (points.Count < 2) {
				debugOut("1 or 0 points");
			} else if (points.Count == 2) {
				debugOut("2 points");
				makeLine(points);
			} else {
				debugOut("3 or more points");
				LinkedList<Triangle> triangles = makeTriangles(points);
				if (triangles != null) {
					paintTriangles(triangles);
				} else {
					debugOut("The triangles are null!");
				}
			}
		}
		
		private void paintTriangles(LinkedList<Triangle> triangles) {
			NWN2Toolset.NWN2.Views.NWN2AreaViewer viewer = getAreaViewer();
			if (viewer != null) {
				NWN2Toolset.NWN2ToolsetMainForm mainForm = NWN2Toolset.NWN2ToolsetMainForm.App;
				NWN2GameArea area = viewer.Area;
				TRN trn = new SFX.YATT.DAL.TRN( new System.IO.BinaryReader( area.TerrainResource.GetStream( false ) ), area.Name );
				
				// The random object we are going to use for all the instances
				Random ran = new Random();

				area.Demand();
				area.RefreshTerrainResource();
				
				System.IO.Stream stream = area.TerrainResource.GetStream( true );
				System.IO.BinaryWriter writer = new System.IO.BinaryWriter( stream );
				
				foreach (Triangle triangle in triangles) {
					trn = ApplyTriangle(area, trn, triangle, triangle.getUpper(), triangle.getLower(), ran);
				}
				
				trn.Save(writer);
				stream.Flush();
				stream.Close();
				
				viewer.SaveContents(true, OEIShared.UI.OEIMessageBoxResult.OK);
				
				mainForm.WaitForPanelsToSave();
				mainForm.CloseViewer(viewer, true);
			}
		}
		
		private static Pair< Pair<double, double>, Pair<double, double> > findUpperAndLower(LinkedList<Pair<double, double>> points) {
			double leftTopX = double.MaxValue;
			double leftTopY = double.MaxValue;
			
			double rightBottomX = double.MinValue;
			double rightBottomY = double.MinValue;
			
			foreach(Pair<double, double> p in points) {
				leftTopX = Math.Min(p.X, leftTopX);
				leftTopY = Math.Min(p.Y, leftTopY);
				
				rightBottomX = Math.Max(p.X, rightBottomX);
				rightBottomY = Math.Max(p.Y, rightBottomY);
			}
			
			leftTopX = adjustDown(leftTopX);
			leftTopY = adjustDown(leftTopY);
			
			rightBottomX = adjustUp(rightBottomX);
			rightBottomY = adjustUp(rightBottomY);
			
			Pair<double, double> upper = new Pair<double, double>(leftTopX, leftTopY);
			Pair<double, double> lower = new Pair<double, double>(rightBottomX, rightBottomY);
			return new Pair<Pair<double, double>, Pair<double, double>>(upper, lower);
		}
		
		
		private NWN2AreaViewer getAreaViewer() {
			if (toolset == null)
				toolset = NWN2Toolset.NWN2ToolsetMainForm.App;
			
			if (module == null)
				module = toolset.Module;
			
			return (NWN2Toolset.NWN2.Views.NWN2AreaViewer)toolset.GetActiveViewer();
		}
		
		/// <summary>
		/// Paint a line given the 2 points
		/// </summary>
		/// <param name="points"></param>
		private void makeLine(LinkedList<Pair<double, double>> points) {
			NWN2Toolset.NWN2.Views.NWN2AreaViewer viewer = getAreaViewer();
			NWN2Toolset.NWN2ToolsetMainForm mainForm = NWN2Toolset.NWN2ToolsetMainForm.App;
			NWN2GameArea area = viewer.Area;
			
			TRN trn = new SFX.YATT.DAL.TRN( new System.IO.BinaryReader( area.TerrainResource.GetStream( false ) ), area.Name );
			
			Pair<Pair<double, double>, Pair<double, double>> resultPair = findUpperAndLower(points);
			
			Pair<double, double> upper = resultPair.X;
			Pair<double, double> lower = resultPair.Y;
			
			trn = ApplyLine(area, trn, upper, lower);
			
			System.IO.Stream stream = area.TerrainResource.GetStream( true );
			System.IO.BinaryWriter writer = new System.IO.BinaryWriter( stream );
			
			trn.Save( writer );
			stream.Flush();
			stream.Close();
			areaViewer.SaveContents(true, OEIShared.UI.OEIMessageBoxResult.OK);
			mainForm.WaitForPanelsToSave();
			
			mainForm.CloseViewer(areaViewer, true);

		}
		
		private TRN ApplyLine(NWN2Toolset.NWN2.Data.NWN2GameArea area, TRN trn, Pair<double, double> upper, Pair<double, double> lower)
		{
			if (data.colourOption == colourOption.Colour) {
				trn = ApplyColourLine(trn, upper, lower);
			}
			
			//texturemaps
			trn = ApplyTextureLine(trn, upper, lower);

			return trn;
		}
		
		private TRN ApplyColourLine(TRN trn, Pair<double, double> upper, Pair<double, double> lower) {
			Random ran = new Random();
			int coverage = 0;
			LineSegment linesegment = new LineSegment(upper, lower);
			
			
			foreach (Vertex vertex in trn.Vertices) {
				Pair<double, double> point = new Pair<double, double>(vertex.Position[0], vertex.Position[1]);
				
				if ((upper.X <= point.X && point.X <= lower.X) && (upper.Y <= point.Y && point.Y <= lower.Y)) {
					if (linesegment.distance(point) <= Constants.VertexSpacing + newInner) {
						coverage = ran.Next(1, 100);
						if (coverage <= data.colourCoverage) {
							vertex.Color = data.col;
						}
					}
				}
			}
			
			return trn;
		}
		
		private TRN ApplyTextureLine(TRN trn, Pair<double, double> upper, Pair<double, double> lower) {
			int xStart = (int)Math.Floor((Math.Min(upper.X, lower.X) - this.newInner) / Constants.TextureSpacing);
			int width = (int)(4 * this.newInner * Math.Abs(upper.X - lower.X));
			
			int yStart = (int)Math.Floor((Math.Min(upper.Y, lower.Y) - this.newInner) / Constants.TextureSpacing);
			int height = (int)(4 * this.newInner * Math.Abs(upper.Y - lower.Y));
			
			Rectangle coverRectangle = new Rectangle(xStart, yStart, width, height);
			
			LineSegment lineSegment = new LineSegment(upper, lower);
			foreach (textureData texture in data.textures) {
				int i = 0;
				foreach (DDSGroup ddsGroup in trn.DDSGroups)
				{
					//Check whether the DDSGroup contain parts of the rectangle that surrounds the triangle
					int x = ( i % trn.Width ) * ddsGroup.Width;
					int y = ( ( i - ( i % trn.Width ) ) / trn.Width ) * ddsGroup.Height;
					i++;
					
					ddsGroup.ApplyTextureAsNewChannel(x, y, texture, lineSegment, coverRectangle, this.newInner);
				}
			}
			
			return trn;
		}
		
		/// <summary>
		/// Paint the triangles defined by the points
		/// </summary>
		/// <param name="points"></param>
		private LinkedList<Triangle> makeTriangles(LinkedList<Pair<double, double>> points) {
			if (points == null ||points.Count == 0)
				return null;
			
			CPoint2D[] vertices = new CPoint2D[points.Count];
			LinkedList<Triangle> triangles = new System.Collections.Generic.LinkedList<Triangle>();
			
			int index = 0;
			foreach (Pair<double, double> p in points) {
				vertices[index] = new CPoint2D(p.X, p.Y);
				index++;
			}
			
			CPolygonShape cutPolygon =  new CPolygonShape(vertices);
			cutPolygon.CutEar();
			
			debugOut("Numer of polygons: "  + cutPolygon.NumberOfPolygons);
			
			CPoint2D[] corners;
			
			for(int numPoly = 0; numPoly < cutPolygon.NumberOfPolygons; numPoly++) {
				#region find upper and lower
				corners = cutPolygon.Polygons(numPoly);
				
				Pair<double, double>[] corns = new Pair<double, double>[3];
				for(int cornIndex = 0; cornIndex < 3; cornIndex++) {
					CPoint2D coern = corners[cornIndex];
					corns[cornIndex] = new Pair<double, double>(coern.X, coern.Y);
				}

				Pair<Pair<double, double>, Pair<double, double>> pairResult = findUpperAndLower(new LinkedList<Pair<double, double>>(corns));
				
				Pair<double, double> upper = pairResult.X;
				Pair<double, double> lower = pairResult.Y;
				
				Triangle inside = new Triangle(corns, upper, lower);
				
				debugOut(inside.ToString());
				
				triangles.AddLast(inside);
				#endregion
			}
			return triangles;
		}

		/// <summary>
		/// Adjust the coordinates down to the nearest vertex
		/// </summary>
		/// <param name="coor"></param>
		/// <returns></returns>
		private static double adjustDown(double coor) {
			double diff = coor % Constants.VertexSpacing;
			return Math.Max(coor - diff, 0);
		}
		
		/// <summary>
		/// Adjust the coordiantes up to the nearest vertex
		/// </summary>
		/// <param name="coor"></param>
		/// <returns></returns>
		private static double adjustUp(double coor) {
			double diff = Constants.VertexSpacing - (coor % Constants.VertexSpacing);
			return coor + diff;
		}
		
		/// <summary>
		/// The method that calls the colour and texture methods
		/// </summary>
		/// <param name="area">The area we want to change</param>
		/// <param name="trn">The TRN data that we is going to modify</param>
		/// <param name="triangle">The Triangle we want to paint inside</param>
		/// <param name="upper">The left upper point of the rectangle that contains triangle</param>
		/// <param name="lower">The right lower point of the rectangle that contains triangle</param>
		/// <returns></returns>
		public TRN ApplyTriangle(NWN2Toolset.NWN2.Data.NWN2GameArea area, TRN trn, Triangle triangle, Pair<double, double> upper, Pair<double, double> lower, Random ran)
		{
			if (data.colourOption == colourOption.Colour) {
				trn = ApplyColourTriangle(trn, triangle, upper, lower, ran);
			}
			
			//texturemaps
			trn = ApplyTextureTriangle(trn, triangle, upper, lower, ran);

			return trn;
		}
		
		/// <summary>
		/// The method that colours the inside of the triangle
		/// </summary>
		/// <param name="trn">The TRN data that we is going to modify</param>
		/// <param name="triangle">The Triangle we want to paint inside</param>
		/// <param name="upper">The left upper point of the rectangle that contains triangle</param>
		/// <param name="lower">The right lower point of the rectangle that contains triangle</param>
		/// <returns>The changed TRN data</returns>
		public TRN ApplyColourTriangle(TRN trn, Triangle triangle, Pair<double, double> upper, Pair<double, double> lower, Random ran)
		{
			int coverage = 0;
			
			foreach (Vertex vertex in trn.Vertices) {
				float x = vertex.Position[0];
				float y = vertex.Position[1];
				
				
				if ((upper.X <= x && x <= lower.X) && (upper.Y <= y && y <= lower.Y)) {
					if (triangle.insideTriangle(new Pair<double, double>(x, y))) {
						coverage = ran.Next(1, 100);
						if (coverage <= data.colourCoverage) {
							vertex.Color = data.col;
						}
					}
				}
			}
			return trn;
		}
		
		/// <summary>
		/// The method that uses the texture the inside of the triangle
		/// </summary>
		/// <param name="trn">The TRN data that we is going to modify</param>
		/// <param name="triangle">The Triangle we want to paint inside</param>
		/// <param name="upper">The left upper point of the rectangle that contains triangle</param>
		/// <param name="lower">The right lower point of the rectangle that contains triangle</param>
		/// <returns></returns>
		public TRN ApplyTextureTriangle(TRN trn, Triangle triangle, Pair<double, double> upper, Pair<double, double> lower, Random ran) {
			Rectangle coverRectangle = new Rectangle((int)Math.Floor(upper.X / Constants.TextureSpacing), (int)Math.Floor(upper.Y / Constants.TextureSpacing),
			                                         (int)Math.Ceiling((lower.X - upper.X) / Constants.TextureSpacing), (int)Math.Ceiling((lower.Y - upper.Y) / Constants.TextureSpacing));
			
			foreach (textureData texture in data.textures) {
				int i = 0;
				foreach (DDSGroup ddsGroup in trn.DDSGroups)
				{
					//Check whether the DDSGroup contain parts of the rectangle that surrounds the triangle
					int x = ( i % trn.Width ) * ddsGroup.Width;
					int y = ( ( i - ( i % trn.Width ) ) / trn.Width ) * ddsGroup.Height;
					i++;
					
					ddsGroup.ApplyTextureAsNewChannel(x, y, texture, triangle, coverRectangle, ran);
				}
			}
			return trn;
		}
		
		/// <summary>
		/// Save the area given
		/// </summary>
		/// <param name="area">The area we want to save</param>
		private void SaveAreaTerrain(NWN2AreaViewer area)
		{
			area.SaveTerrain(true);
			area.SaveContents(true,OEIShared.UI.OEIMessageBoxResult .YesToAll );
		}
		
		/// <summary>
		/// An utility to create a new Vector
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		private static Vector2 makeVector(Point p1, Point p2) {
			return new Vector2(p2.X - p1.X, p2.Y - p1.Y);
		}
		
		/// <summary>
		/// This method finds the coordinates that have been clicked on the area (not the coordinates that have been clicked on in the toolset).
		/// </summary>
		/// <param name="X">The X coordinates from the toolset panel</param>
		/// <param name="Y">The Y coordinates from the toolset panel</param>
		/// <param name="area">The area we are working on</param>
		/// <returns></returns>
		private SFX.YATT.Tools.Vector<float> findAreaCoor(int X, int Y, NWN2GameArea area) {
			try {
				NWN2NetDisplayManager manager = NWN2NetDisplayManager.Instance;
				byte[] cMessage = null;
				bool flag = false;
				
				Random ran = new Random();
				
				manager.BeginSynchronizedOperation();
				
				int iMessageID = manager.PlanePick(areaViewer.AreaNetDisplayWindow,
				                                   X,
				                                   Y,
				                                   NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue);
				manager.WaitForMessageSynchronous(NetDisplayMessageType.TerrainPickResults, out cMessage, iMessageID);
				manager.EndSynchronizedOperation();
				
				if (!flag)
				{
					BinaryReader reader = new BinaryReader(new MemoryStream(cMessage));
					if (reader.ReadInt32() != areaViewer.AreaNetDisplayWindow.Scene.ID)
					{
						throw new Exception("Reader Error");
					}
					
					int num = reader.ReadInt32();
					// The coordinates from the mouse
					float valueX = reader.ReadSingle();
					float valueY = reader.ReadSingle();
					float valueZ = reader.ReadSingle();
					
					SFX.YATT.Tools.Vector<float> vec = new SFX.YATT.Tools.Vector<float>(valueX, valueY, valueZ);
					
					return vec;
				} else {
					return null;
				}
			} catch (Exception e) {
				debugOut("Problem with finding the coordinates: " + e.Message);
				throw new Exception("Problem with finding the coordinates: " + e.Message);
			}
		}
		
		/// <summary>
		/// The method that prepares to call the Method that does the actual paiting
		/// This include finding the coordinates that are to be painted, in what order, and the size of the outer and inner brush
		/// </summary>
		/// <param name="coor">The coordiantes we are going to start from</param>
		/// <param name="modType">The type of painting we are to do</param>
		/// <param name="localTexture">The texture we are going to paint</param>
		private void texturePreperation(SFX.YATT.Tools.Vector<float> coor, TerrainModificationType modType, textureData localTexture) {
			
			float valueX = coor[0];
			float valueY = coor[1];
			float valueZ = coor[2];
			
			int radius = (data.outerCircle + data.innerCircle) / 2;

			double roundModifier = 2 - (rounds / 11);
			
			float localOuter = (float)((newOuter / rounds) * roundModifier);
			float localInner = (float)((newInner / rounds) * roundModifier);
			
			opTexture.Checked = true;
			outerRadius.Value = Math.Min((int)localOuter, outerRadius.Maximum);
			innerRadius.Value = Math.Min((int)localInner, innerRadius.Maximum);

			float baseCoordinatesX = Math.Max(0, valueX - (radius / 2));
			float baseCoordinatesY = Math.Max(0, valueY - (radius / 2));
			
			float step = (radius / rounds);
			
			float coorX;
			float coorY;
			
			int ran = 0;
			
			// Make sure we have a new random number
			random = new Random(System.DateTime.Now.Millisecond);
			System.Collections.Generic.List<Pair<float, float>> coordinates = new System.Collections.Generic.List<Pair<float, float>>();
			
			for(int index1 = 1; index1 <= rounds; index1++ ) {
				coorX = baseCoordinatesX + (step * index1);
				
				for (int index2 = 1; index2 <= rounds; index2++) {
					coorY = baseCoordinatesY + (step * index2);
					
					coordinates.Add(new Pair<float, float>(coorX, coorY));
				}
			}
			
			int coverage = 0;
			string textureStr = localTexture.texture;
			int col = Color.White.ToArgb();
			
			if (modType == TerrainModificationType.Texture) {
				// CoverageChange must be bigger than 0 and less than 100
				coverage = localTexture.Coverage;
				
				// Set the presure of the current texture
				barPressure.Value = localTexture.Presure * 100;
				
			} else if (modType == TerrainModificationType.Grass) {
				coverage = data.grassCoverage;
				
			} else if (modType == TerrainModificationType.Color) {
				coverage = data.colourCoverage;
				col = data.col.ToArgb();
				
			} else {
				throw new Exception("Wrong Terrain Modification Type");
			}
			
			foreach (Pair<float, float> point in coordinates) {
				
				coorX = point.X;
				coorY = point.Y;
				
				ran = random.Next(1, 100);
				
				NWN2NetDisplayManager.Instance.BeginSynchronizedOperation();
				if (ran <= coverage) {
					paintTexture(textureStr, modType, col, localInner, localOuter, coorX, coorY, valueZ);
				}
				NWN2NetDisplayManager.Instance.EndSynchronizedOperation();
			}
		}
		
		/// <summary>
		/// The method which takes care of the on-line painting
		/// </summary>
		/// <param name="texture">The name of the texture we are going to paint</param>
		/// <param name="type">The type of painting we are doing</param>
		/// <param name="colour">The colour we are painting with</param>
		/// <param name="inner">The size of the inner circle</param>
		/// <param name="outer">The size of the outer circle</param>
		/// <param name="X">X coordinate</param>
		/// <param name="Y">Y coordinate</param>
		/// <param name="Z">Z coordinate</param>
		private void paintTexture(string texture, TerrainModificationType type, int colour, float inner, float outer,
		                          float X, float Y, float Z) {
			try {
				
				Vector3 vec = new Vector3(X, Y, Z);
				Vector3 vec2 = new Vector3(X, Y, Z);
				NWN2NetDisplayManager.Instance.TerrainBrush(areaViewer.AreaNetDisplayWindow.Scene, 1, 0, vec,
				                                            inner,
				                                            outer,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue2,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainBrushColor,
				                                            colour,
				                                            texture,
				                                            type);
				
				NWN2NetDisplayManager.Instance.TerrainModify(areaViewer.AreaNetDisplayWindow.Scene,
				                                             type,
				                                             NWN2ToolsetMainForm.App.TerrainEditor.TextureMode);
				
				byte[] buffer2 = null;
				bool bCancel = false;
				
				NWN2NetDisplayManager.Instance.BeginSynchronizedOperation();
				
				int num6 = NWN2NetDisplayManager.Instance.TerrainSelection(areaViewer.AreaNetDisplayWindow.Scene, vec2);
				NWN2NetDisplayManager.Instance.WaitForMessageSynchronous(NetDisplayMessageType.TerrainSelectResults, out buffer2, num6);
				NWN2NetDisplayManager.Instance.EndSynchronizedOperation();
				
				if (buffer2 != null)
				{
					NWN2ToolsetMainForm.App.TerrainEditor.HandleSelectTerrain(buffer2, ref bCancel);
				}
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
		
		/// <summary>
		/// The code to show the at the given coordiantes.
		/// The colour of the circle is different based on the type we are working with
		/// </summary>
		/// <param name="coor">The coordinates the circle is to be shown at</param>
		private void showBrush(SFX.YATT.Tools.Vector<float> coor)
		{
			Color col;
			
			if (data.mode == paintmode.paint) {
				col = Color.Black;
			} else if (data.mode == paintmode.eyedrop) {
				col = Color.Blue;
			} else {
				col = Color.Red;
			}
			
			Vector3 vec = new Vector3(coor[0], coor[1], coor[2]);
			
			NWN2NetDisplayManager.Instance.TerrainBrush(this.electronPanel.NDWindow.Scene,
			                                            1,
			                                            1,
			                                            vec,
			                                            (float)newInner,
			                                            (float)newOuter,
			                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue,
			                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue2,
			                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainBrushColor,
			                                            col.ToArgb(),
			                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainBrushTexture,
			                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainMode);
			byte[] buffer2 = null;
			bool bCancel = false;
			if ((NWN2ToolsetMainForm.App.TerrainEditor.TerrainMode != TerrainModificationType.Water) && (NWN2ToolsetMainForm.App.TerrainEditor.TerrainMode != TerrainModificationType.NoWater))
			{
				NWN2NetDisplayManager.Instance.BeginSynchronizedOperation();
				int num6 = NWN2NetDisplayManager.Instance.TerrainSelection(this.electronPanel.NDWindow.Scene, vec);
				NWN2NetDisplayManager.Instance.WaitForMessageSynchronous(NetDisplayMessageType.TerrainSelectResults, out buffer2, num6);
				NWN2NetDisplayManager.Instance.EndSynchronizedOperation();
				if (buffer2 != null)
				{
					NWN2ToolsetMainForm.App.TerrainEditor.HandleSelectTerrain(buffer2, ref bCancel);
				}
			}

		}
		
		/// <summary>
		/// The code to stop showing the circle
		/// </summary>
		public void stopShowingCircle() {
			if ((this.electronPanel != null) && (this.electronPanel.NDWindow != null) && (this.electronPanel.NDWindow.Scene != null))
			{
				
				NWN2NetDisplayManager.Instance.TerrainBrush(this.electronPanel.NDWindow.Scene,
				                                            1,
				                                            1,
				                                            new Vector3(0f, 0f, 0f),
				                                            0,
				                                            0,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainValue2,
				                                            data.col.ToArgb(),
				                                            NWN2ToolsetMainForm.App.TerrainEditor.CursorColor,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainBrushTexture,
				                                            NWN2ToolsetMainForm.App.TerrainEditor.TerrainMode);
			}
		}
		
		/// <summary>
		/// The code called when the multibrush is activated. The painter should only work when we are in paint mode and the
		/// CTRL button is not held down (otherwise it becomes inconvinient for the user to move around).
		/// </summary>
		/// <param name="oSender"></param>
		/// <param name="ctrlDown">Whether the CTRL button is held down</param>
		public void runTextureCode(object oSender, bool ctrlDown) {
			MousePanel cPanel = oSender as MousePanel;
			if (cPanel.ButtonHeld && cPanel.LeftButton && NWN2AreaViewer.MouseMode == MouseMode.PaintTerrain && !ctrlDown){
				if (data.mode == paintmode.paint) {
					TerrianCode(cPanel.MouseX, cPanel.MouseY);
				} else if (data.mode == paintmode.eyedrop) {
					eyeDropCode();
				} else {
					makePoint(cPanel.MouseX, cPanel.MouseY);
				}
				setOuterInner(this.data);
			}
		}
		
		/// <summary>
		/// Make a new point (only if it is not already there)
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		private void makePoint(int X, int Y) {
			SFX.YATT.Tools.Vector<float> coor = findAreaCoor(X, Y, areaViewer.Area);
			
			if (coor != null) {
				Pair<double, double> p = new Pair<double, double>(coor[0], coor[1]);
				Point id = new Point((int)Math.Floor(coor[0]), (int)Math.Floor(coor[1]));
				if (!pointsAlready.ContainsKey(id)) {
					pointsAlready[id] = "1";
					
					// Get the current area
					NWN2Toolset.NWN2.Views.NWN2AreaViewer viewer = getAreaViewer();
					NWN2Toolset.NWN2ToolsetMainForm mainForm = NWN2Toolset.NWN2ToolsetMainForm.App;
					NWN2GameArea area = viewer.Area;
					
					// Make waypoint
					NWN2Toolset.NWN2.Data.Instances.NWN2WaypointInstance wayPoint = new NWN2Toolset.NWN2.Data.Instances.NWN2WaypointInstance();
					wayPoint.Tag = WAYPOINT_TAG;
					wayPoint.Position = new Vector3(coor[0], coor[1], coor[2]);
					area.AddInstance(wayPoint);
					
					debugOut("Point: " + p + " added");
					
				}
			}
		}
		
		/// <summary>
		/// The code for painting on-line3
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		private void TerrianCode(int X, int Y)
		{
			try {
				areaViewer = getAreaViewer();
				SFX.YATT.Tools.Vector<float> coor = findAreaCoor(X, Y, areaViewer.Area);
				
				Random ran = new Random();
				
				if (coor != null) {
					NWN2ToolsetMainForm.App.TerrainEditor.SetBrushType(NWN2TerrainEditorForm.BrushType.StandardTerrain);
					

					// Textures:
					opTexture.Checked = true;
					if (data.randomize) {
						ran = new Random(System.DateTime.Now.Millisecond);
						
						ArrayList texData = new ArrayList(data.textures.Count);
						texData.AddRange(data.textures);
						
						while(texData.Count > 0) {
							int num = ran.Next(0, texData.Count - 1);
							textureData texture = (textureData)texData[num];
							texturePreperation(coor, TerrainModificationType.Texture, texture);
							texData.RemoveAt(num);
						}
					} else {

						LinkedListNode<textureData> node = data.textures.Last;
						while (node != null) {
							DateTime dateTime = DateTime.Now;

							double start = ((double)dateTime.Ticks) / 1000000.0;
							texturePreperation(coor, TerrainModificationType.Texture, node.Value);
							dateTime = DateTime.Now;
							node = node.Previous;
						}
					}
					
					if (data.grassOption == grassOption.Paint) {
						opGrass.Checked = true;
						
						barPressure.Value = data.grassDensity * 100;
						grassSize.Value = (data.grassSize / 30);
						grassSizeVariation.Value = (data.grassSizeVariation / 10);

						object[] objList = new object[grassListBox.Items.Count];
						grassListBox.Items.CopyTo(objList, 0);
						
						int index = 0;
						
						grassListBox.SelectedIndices.Clear();
						
						foreach(TextureListItem textureItem in objList) {
							string textStr = textureItem.Text;
							if (textStr == data.grassTexture[0] ||
							    textStr == data.grassTexture[1] ||
							    textStr == data.grassTexture[2]) {
								grassListBox.SelectedItem = textureItem;
								index++;
								
								if (index == 3)
									break;
							}
						}
						
						NWN2ToolsetMainForm.App.TerrainEditor.SetBrushType(NWN2TerrainEditorForm.BrushType.FloraGrass);
						texturePreperation(coor, TerrainModificationType.Grass, data.textures.First.Value);
					}
					
					
					if (data.colourOption == colourOption.Colour) {
						opTerrain.Checked = true;
						opColour.Checked = true;
						colourButton.BackColor = data.col;
						barPressure.Value = data.colourPresure * 100;
						texturePreperation(coor, TerrainModificationType.Color, data.textures.First.Value);
					}
					
				}
				
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
	}
}
