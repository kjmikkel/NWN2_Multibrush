/***********************************************************************************
 * YATT - NWN2 Terrain Tool plugin allowing importing of heightmaps, colourmaps, etc
 * Copyright (C) 2006 Simon Noble ( sidefxboy(at)gmail(dot)com )

 * YATT is free software; you can redistribute it and/or modify it under the terms of
 * the GNU General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or (at your option) any later version.

 * YATT is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
 * PURPOSE. See the GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License along with YATT;
 * if not, write to the Free Software Foundation, Inc., 59 Temple Place,
 * Suite 330, Boston, MA 02111-1307 USA
 
 * In addition, as a special exception, the copyright holders give permission to link
 * the code of portions of YATT with the NWN2 Toolset libraries under certain
 * conditions as described in each individual source file, and distribute linked
 * combinations including the two.
 
 * You must obey the GNU General Public License in all respects for all of the code used
 * other than that related to the NWN2 Toolset Libraries.  If you modify file(s) with
 * this exception, you may extend this exception to your version of the file(s), but you
 * are not obligated to do so.  If you do not wish to do so, delete this exception
 * statement from your version.  If you delete this exception statement from all source
 * files in the program, then also delete it here.
 ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Multibrush;

namespace SFX.YATT.DAL
{
	public enum TextureOverflowAction
	{
		Merge,
		PreferLast,
		Nothing
	}

	/// <summary>
	/// Encamsulates 2 DDSs and makes normalising them easier
	/// </summary>
	public class DDSGroup : DAL.IPacket
	{
		public List<string> TextureNames
		{
			get
			{
				return this._textureNames;
			}
		}
		private List<string> _textureNames = new List<string>();
		public DDS dds1
		{
			get
			{
				return this._dds1;
			}
		}
		private DDS _dds1 = null;
		public DDS dds2
		{
			get
			{
				return this._dds2;
			}
		}
		private DDS _dds2 = null;

		public TextureOverflowAction OverflowAction
		{
			get
			{
				return this._overflowAction;
			}
			set
			{
				this._overflowAction = value;
			}
		}
		private TextureOverflowAction _overflowAction = TextureOverflowAction.Merge;
		public Vertex FirstVertex
		{
			get
			{
				return this._firstVertex;
			}
			set
			{
				this._firstVertex = value;
			}
		}
		private Vertex _firstVertex;

		public DDSGroup( System.IO.BinaryReader reader )
		{

			this.LoadTextureNames( reader );
		}

		#region Load/Save
		public void Load(System.IO.BinaryReader reader)
		{
			this._dds1 = new DDS( reader );
			this._dds2 = new DDS( reader );
		}

		public void Save( System.IO.BinaryWriter writer )
		{
			this.Normalise();

			this._dds1.Save( writer );
			this._dds2.Save( writer );
		}

		public void LoadTextureNames(System.IO.BinaryReader reader)
		{
			for (int i = 0; i < 6; i++)
			{
				// check if it's not empty
				string textureName = OEIShared.Utils.CommonUtils.ConvertZeroTerminatedBytesToString(reader.ReadBytes(32)).TrimEnd('\0');
				if (textureName != "")
					this._textureNames.Add(textureName);
			}
		}

		public void SaveTextureNames(System.IO.BinaryWriter writer)
		{
			// write texturenames, blanks for ones that are not present
			for (int i = 0; i < 6; i++)
			{
				if (i < this._textureNames.Count)
					writer.Write(OEIShared.Utils.CommonUtils.ConvertStringToBytes(this._textureNames[i].PadRight(0x20, '\0')));
				else
					writer.Write(OEIShared.Utils.CommonUtils.ConvertStringToBytes("".PadRight(0x20, '\0')));
			}
		}
		#endregion

		private void Normalise()
		{
			if ( this.OverflowAction == TextureOverflowAction.Nothing )
				return;
			else if ( this.OverflowAction == TextureOverflowAction.Merge )
			{
				for ( int x = 0; x < this._dds1.Width; x++ )
				{
					for ( int y = 0; y < this._dds1.Height; y++ )
					{
						double totalTexVal = 0;

						for ( int j = 0; j < 8; j++ )
							totalTexVal += this[x, y, j];

						for ( int j = 0; j < 8; j++ )
						{
							if ( ( totalTexVal / 255.0 ) > 0 )
							{
								double newVal = this[x, y, j] / ( totalTexVal / 255.0 );
								this[x, y, j] = byte.Parse( Math.Round( newVal).ToString() );
							}
						}
					}
				}
			}
			else
			{
				for ( int x = 0; x < this._dds1.Width; x++ )
				{
					for ( int y = 0; y < this._dds1.Height; y++ )
					{
						double totalTexVal = 0;
						int lastTexIndex = 0;

						for ( int j = 0; j < 8; j++ )
							lastTexIndex = ( this[x, y, j] > 0 ? j : lastTexIndex );

						for ( int j = 0; j < 8; j++ )
							if( j != lastTexIndex )
								totalTexVal += this[x, y, j];

						double scaleVal = this[x, y, lastTexIndex] < 255 ? totalTexVal / ( 255.0 - this[x, y, lastTexIndex] ) : 0.0;
						for ( int j = 0; j < 8; j++ )
						{
							if ( (byte)scaleVal > 0 && j != lastTexIndex )
							{
								double newVal = this[x, y, j] / scaleVal;
								newVal = ( newVal > 255.0 ? 255.0 : newVal );
								this[x, y, j] = byte.Parse( Math.Round( newVal ).ToString() );
							}
							else if ( j == lastTexIndex )
								this[x, y, j] = this[x, y, j];
							else
								this[x, y, j] = 0;
						}
					}
				}
			}
		}

		public int Width
		{
			get
			{
				return this._dds1.Width;
			}
		}

		public int Height
		{
			get
			{
				return this._dds1.Height;
			}
		}

		public void ApplyTextureAsNewChannel(int x, int y, textureData texture, Multibrush.Triangle inside, Rectangle coverRectangle, Random ran) {
			Rectangle rec = new Rectangle(x, y, this.Width, this.Height);
			
			int channel = 0;
			int coverage = 0;
			bool firstCheck = true;
			
			double presure = (float)texture.Presure / 100.0;
			
			if (coverRectangle.IntersectsWith(rec)) {
				for(int xCoor = x; xCoor < x + this.Width ; xCoor++) {
					double adjustedXCoor = xCoor * Constants.TextureSpacing;
					
					for(int yCoor = y; yCoor < y + this.Height; yCoor++) {
						double adjustedYCoor = yCoor * Constants.TextureSpacing;
						
						if (inside.insideTriangle(new Pair<double, double>(adjustedXCoor, adjustedYCoor))) {
							coverage = ran.Next(1, 100);
							if (coverage <= texture.Coverage) {
								
								if (firstCheck) {
									firstCheck = false;
									
									if (this.TextureNames.Count >= 6)
										return;
									
									this.TextureNames.Add(texture.ToString());
									channel = this.TextureNames.Count - 1;
								}
								
								try {
									this[xCoor % this.Width, yCoor % this.Height, channel] = (byte)(presure * 255f);
								} catch (Exception e) {
									throw new Exception("DDSGroup error: X:" + xCoor + ", Y:" + yCoor +" Channel: " + channel + ". Error Message: " + e.Message);
								}
							}
						}
					}
				}
			}
		}
		
		public void ApplyTextureAsNewChannel(int x, int y, textureData texture, Multibrush.LineSegment linesegment, Rectangle coverRectangle, double inner) {
			Rectangle rec = new Rectangle(x, y, this.Width, this.Height);
			Random ran = new Random();
			
			int channel = 0;
			int coverage = 0;
			bool firstCheck = true;
			bool painted = false;
					
			double presure = (float)texture.Presure / 100.0;
			
			if (coverRectangle.IntersectsWith(rec)) {
				for(int xCoor = x; xCoor < x + this.Width ; xCoor++) {
					double adjustedXCoor = xCoor * Constants.TextureSpacing;
					painted = false;
					
					for(int yCoor = y; yCoor < y + this.Height; yCoor++) {
						double adjustedYCoor = yCoor * Constants.TextureSpacing;
						
						Pair<double, double> point = new Multibrush.Pair<double, double>(adjustedXCoor, adjustedYCoor);
						
						if (linesegment.distance(point) <= inner) {
							coverage = ran.Next(1, 100);
							if (coverage <= texture.Coverage) {
								
								if (firstCheck) {
									firstCheck = false;
									painted = true;
									
									if (this.TextureNames.Count >= 6)
										return;
									
									this.TextureNames.Add(texture.ToString());
									channel = this.TextureNames.Count - 1;
								} 
								
								try {
									this[xCoor % this.Width, yCoor % this.Height, channel] = (byte)(presure * 255f);
								} catch (Exception e) {
									throw new Exception("DDSGroup error: X:" + xCoor + ", Y:" + yCoor +" Channel: " + channel + ". Error Message: " + e.Message);
								}
							}
						/* Since we are going over this in a linear fashion, we if we ever paint something, and then stop painting,
			  				then we can be sure that we do not need to paint any more
			 			*/
						} 
						else if (painted) {
							break;
						}
					}
				}
			}
		}

		public byte this[int x, int y, int channel]
		{
			get
			{
				if ( channel < 3 )
					return this._dds1[x, y, 2 - channel];
				else if ( channel == 3 )
					return this._dds1[x, y, channel];
				else if ( channel < 7 )
					return this._dds2[x, y, 2 - ( channel - 4 )];
				else
					return this._dds2[x, y, channel - 4];
			}
			set
			{
				if ( channel < 3 )
					this._dds1[x, y, 2 - channel] = value;
				else if ( channel == 3 )
					this._dds1[x, y, channel] = value;
				else if ( channel < 7 )
					this._dds2[x, y, 2 - ( channel - 4 )] = value;
				else
					this._dds2[x, y, channel - 4] = value;
			}
		}
	}
}
