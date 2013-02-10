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
using System.Text;
using System.Drawing;

namespace SFX.YATT.DAL
{
    public enum ApplyMethod
    {
        Replace,
        Add
    }
    /// <summary>
    /// TRRN Layout in file:
    ///     -> starts at bottom left corner of map, then moves from left to right
    /// </summary>
    public class TRN : IPacket
    {
        public OEIShared.Utils.FourCC FourCC
        {
            get
            {
                return this._fourCC;
            }
        }
        private OEIShared.Utils.FourCC _fourCC = new OEIShared.Utils.FourCC( "NWN2" );
        private ushort _majorFileVersion;
        private ushort _minorFileVersion;
        private int _numPackets;

        private TRRN[,] _TRRNs = null;
        private TRWH _TRWH;
        private System.Collections.Generic.List<ASWM> _ASWMs = new List<ASWM>( 1 );
        private string _name;

        #region Constructor/Destrcutor
        public TRN( System.IO.BinaryReader reader, string name )
        {
            this._name = name;
            this.Load( reader );
        }

        public TRN( NWN2Toolset.NWN2.Data.NWN2GameArea area )
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader( area.TerrainResource.GetStream( false ) );
            this._name = area.Name;
            this.Load( reader );
        }
        #endregion

        #region Misc
        public override string ToString()
        {

            return this._name + "\n\n" + this._TRWH.ToString();
        }
        #endregion

        #region Area Dimensions
        public int Width
        {
            get
            {
                return this._TRWH.Width;
            }
        }

        public int Height
        {
            get
            {
                return this._TRWH.Height;
            }
        }

        public float GetMaxHeight()
        {
            float maxHeight = float.MinValue;
            for ( int x = 0; x < this.Width; x++ )
                for ( int y = 0; y < this.Height; y++ )
                    maxHeight = this._TRRNs[x, y].GetMaxHeight() > maxHeight ? this._TRRNs[x, y].GetMaxHeight() : maxHeight;

            return maxHeight;
        }

        public float GetMinHeight()
        {
            float minHeight = float.MaxValue;
            for ( int x = 0; x < this.Width; x++ )
                for ( int y = 0; y < this.Height; y++ )
                    minHeight = this._TRRNs[x, y].GetMinHeight() < minHeight ? this._TRRNs[x, y].GetMinHeight() : minHeight;

            return minHeight;
        }
        #endregion

        #region Heightmap
        public System.Drawing.Bitmap ExportHeightmapToBitmap()
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( this.Width * 24 + 1, this.Height * 24 + 1 );

            for ( int x = 0; x < this.Width; x++ )
            {
                for ( int y = 0; y < this.Height; y++ )
                {
                    this._TRRNs[x, y].ExportHeightsToBitmap( bitmap, this.GetMaxHeight(), this.GetMinHeight() );
                }
            }
            bitmap.RotateFlip( System.Drawing.RotateFlipType.RotateNoneFlipY );
            return bitmap;
        }
        #endregion

        #region Colour Map
        public System.Drawing.Bitmap ExportVertexColoursToBitmap()
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( this.Width * 24 + 1, this.Height * 24 + 1 );

            for ( int x = 0; x < this.Width; x++ )
                for ( int y = 0; y < this.Height; y++ )
                    this._TRRNs[x, y].ExportColoursToBitmap( bitmap );
            bitmap.RotateFlip( System.Drawing.RotateFlipType.RotateNoneFlipY );
            return bitmap;
        }
        #endregion

        #region DDS
        public System.Collections.Hashtable ExportTexturemaps()
        {
            System.Collections.Hashtable ddss = new System.Collections.Hashtable();
            // create a blank bitmap for each texture present
            for ( int j = 0; j < this.Height; j++ )
            {
                for ( int i = 0; i < this.Width; i++ )
                {
                    foreach ( string textureName in this._TRRNs[i, j].DDSGroup.TextureNames )
                    {
                        // if it already exists, do nothing
                        if ( !ddss.ContainsKey( textureName ) )
                        {
                            ddss.Add( textureName, new System.Drawing.Bitmap( this.Width * 128, this.Height * 128 ) );
                            // default to black
                            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage( (System.Drawing.Bitmap)ddss[textureName] );
                            graphics.FillRectangle( new System.Drawing.SolidBrush( 
                                System.Drawing.Color.Black ), 
                                new System.Drawing.Rectangle( 0, 0, this.Width * 128, this.Height * 128  ) );
                        }
                    }
                }
            }

            // get each megatile chunk and update the bitmaps with it
            for ( int j = 0; j < this.Height; j++ )
            {
                for ( int i = 0; i < this.Width; i++ )
                {
                    System.Collections.Hashtable megatileTexturemaps = this._TRRNs[i, j].ExportDDSs();
                    foreach ( string textureName in megatileTexturemaps.Keys )
                    {
                        System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage( (System.Drawing.Bitmap)ddss[textureName] );
                        int x = i * 128;
                        int y = ( (System.Drawing.Bitmap)ddss[textureName] ).Height - 1 - ( 128 + j * 128 );
                        System.Drawing.Rectangle destRect = new System.Drawing.Rectangle( x, y, 128, 128 );
                        graphics.DrawImage( (System.Drawing.Bitmap)megatileTexturemaps[textureName], destRect );
                    }
                }
            }

            return ddss;
        }
        #endregion

        #region Load/Save
        /// <summary>
        /// Load the TRN direct from a binary reader stream.
        /// Stream must be aligned with the beginning of a TRN
        /// </summary>
        /// <param name="reader"></param>
        public void Load( System.IO.BinaryReader reader )
        {
            OEIShared.Utils.FourCC cc = new OEIShared.Utils.FourCC( reader.ReadChars( 4 ) );

            if ( cc != this._fourCC )
                throw new ApplicationException( this._fourCC.ToString() + " not found in stream" );

            this._majorFileVersion = reader.ReadUInt16();
            this._minorFileVersion = reader.ReadUInt16();
            this._numPackets = reader.ReadInt32();

            PacketPointer[] packetPointers = new PacketPointer[this._numPackets];
            for ( int i = 0; i < this._numPackets; i++ )
            {
                packetPointers[i].Type = new OEIShared.Utils.FourCC( reader.ReadChars( 4 ) );
                packetPointers[i].Address = reader.ReadInt32();
            }

            int curTRRNIndex = 0;
            int curTRRNX = 0;
            int curTRRNY = 0;
            // use the above info to find the TRWH
            foreach( PacketPointer ptr in packetPointers )
            {
                if ( ptr.Type == new OEIShared.Utils.FourCC( "TRWH" ) )
                {
                    if ( this._TRWH == null )
                    {
                        reader.BaseStream.Position = ptr.Address;
                        this._TRWH = new TRWH( reader );
                        this._TRRNs = new TRRN[ this.Width, this.Height ];
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show( "Ignoring Additional TRWH" );
                    }
                }
                else if ( ptr.Type == new OEIShared.Utils.FourCC( "TRRN" ) )
                {
                    if ( this._TRWH == null )
                        throw new ApplicationException( "TRRN found but no TRWH. Can't load TRRN." );
                    else
                    {                       
                        curTRRNX = curTRRNIndex % Width;
                        curTRRNY = int.Parse( Math.Floor( curTRRNIndex / (double)Width ).ToString() );
                        curTRRNIndex++;

                        reader.BaseStream.Position = ptr.Address;
                        this._TRRNs[curTRRNX, curTRRNY] = new TRRN( reader );
                        this._vertices.AddRange(this._TRRNs[curTRRNX, curTRRNY].Vertices);
                        this._ddsGroups.Add(this._TRRNs[curTRRNX, curTRRNY].DDSGroup);
                    }
                }
                else if ( ptr.Type == new OEIShared.Utils.FourCC( "ASWM" ) )
                {
                    reader.BaseStream.Position = ptr.Address;
                    this._ASWMs.Add( new ASWM( reader ) );
                }
            }
        }

        public void Save( System.IO.BinaryWriter writer )
        {
            writer.Write( this._fourCC.IntValue );
            writer.Write( this._majorFileVersion );
            writer.Write( this._minorFileVersion );
            writer.Write( this._numPackets );
            writer.Write( this._TRWH.FourCC.IntValue );
            int addr = 12 + 8 * ( 2 + Width * Height );
            writer.Write( addr );
            addr += 20;
            for ( int y = 0; y < Height; y++ )
            {
                for ( int x = 0; x < Width; x++ )
                {
                    writer.Write( this._TRRNs[0, 0].FourCC.IntValue );
                    writer.Write( addr );
                    addr += this._TRRNs[x, y].Size + 8;
                }
            }
            writer.Write( this._ASWMs[0].FourCC.IntValue );
            writer.Write( addr);
            this._TRWH.Save( writer );
            for ( int y = 0; y < Height; y++ )
                for ( int x = 0; x < Width; x++ )
                    this._TRRNs[x, y].Save( writer );
            foreach( ASWM aswm in this._ASWMs )
                aswm.Save( writer );
        }
        #endregion

        #region Vertices
        public List<Vertex> Vertices
        {
            get
            {
                return this._vertices;
            }
        }
        private List<Vertex> _vertices = new List<Vertex>();
        #endregion

        #region DDSGroups
        public List<DDSGroup> DDSGroups
        {
            get
            {
                return this._ddsGroups;
            }
        }
        private List<DDSGroup> _ddsGroups = new List<DDSGroup>();
        #endregion
    }    
}
