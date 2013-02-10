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
    /// <summary>
    /// - Height points are 44 bytes apart
    /// - Height points offset from beginning of TRRN by 416 bytes
    /// - These height points are only the terrain model heights - walkmesh seems to be stored seperatly.
    /// </summary>
    class TRRN : IPacket
    {
        public OEIShared.Utils.FourCC FourCC
        {
            get
            {
                return this._fourCC;
            }
        }
        private OEIShared.Utils.FourCC _fourCC = new OEIShared.Utils.FourCC( "TRRN" );
        public int Size
        {
            get
            {
                return this._size;
            }
        }
        private int _size;
        public Vertex[] Vertices
        {
            get
            {
                return this._vertices;
            }
        }
        private Vertex[] _vertices;
        private string _mapPart;
        private float[] _texVectors = new float[18];
        private int _vertexCount;
        private int _triangleCount;
        private Triangle[] _triangles;
        public DDSGroup DDSGroup
        {
            get
            {
                return this._ddsGroup;
            }
        }
        private DDSGroup _ddsGroup;
        private int _unkownDataSize;
        private byte[] _unknownData;

        public TRRN( System.IO.BinaryReader reader )
        {
            this.Load( reader );
        }

        #region HeightAttributes
        public float GetMaxHeight()
        {
            float maxHeight = float.MinValue;
            for ( int i = 0; i < this._vertices.Length; i++ )
                maxHeight = this._vertices[i].Position[2] > maxHeight ? this._vertices[i].Position[2] : maxHeight;

            return maxHeight;
        }

        public float GetMinHeight()
        {
            float minHeight = float.MaxValue;
            for ( int i = 0; i < this._vertices.Length; i++ )
                minHeight = this._vertices[i].Position[2] < minHeight ? this._vertices[i].Position[2] : minHeight;

            return minHeight;
        }

        #endregion

        #region Heightmap
        public void ExportHeightsToBitmap( System.Drawing.Bitmap bitmap, float MaxHeight, float MinHeight )
        {
            float mult = 5.0f / 3.0f;
            float range = ( MaxHeight - MinHeight );

            for ( int i = 0; i < this._vertices.Length; i++ )
            {
                int x = int.MaxValue, y = int.MaxValue, z = int.MaxValue;
                float zf=float.MaxValue;
                Tools.Vector<float> position = this._vertices[i].Position;
                try
                {
                    x = int.Parse( Math.Round( position[0] / mult ).ToString() );
                    y = int.Parse( Math.Round( position[1] / mult ).ToString() );
                    zf = 255.0f * ( position[2] - MinHeight ) / range;
                    z = !float.IsNaN( zf ) ? int.Parse( Math.Round( zf ).ToString() ) : 124;
                    System.Drawing.Color col = System.Drawing.Color.FromArgb( z, z, z );

                    bitmap.SetPixel( x, y, col );
                }
                catch ( System.Exception ex )
                {
                    System.Windows.Forms.MessageBox.Show( "Crapped Out:\n" + ex.Message + "\n" + ex.StackTrace );
                    System.Windows.Forms.MessageBox.Show( string.Format("Position: {0} x={1} y={2} z={3} zf={4}", position, x, y, z, zf ) );
                    throw ex;
                }
            }
        }
        #endregion

        #region Colour Map
        public void ExportColoursToBitmap( System.Drawing.Bitmap bitmap )
        {
            double mult = Constants.VertexSpacing;

            for ( int i = 0; i < this._vertices.Length; i++ )
            {
                int x = int.MaxValue, y = int.MaxValue;
                Tools.Vector<float> position = this._vertices[i].Position;
                try
                {
                    x = int.Parse( Math.Round( position[0] / mult ).ToString() );
                    y = int.Parse( Math.Round( position[1] / mult ).ToString() );
                    System.Drawing.Color col = this._vertices[i].Color;
                    bitmap.SetPixel( x, y, col );
                }
                catch ( System.Exception ex )
                {
                    System.Windows.Forms.MessageBox.Show( "Crapped Out Ixporting Colours:\n" + ex.Message + "\n" + ex.StackTrace );
                    throw ex;
                }
            }
        }
        #endregion

        #region DDS
        public System.Collections.Hashtable ExportDDSs()
        {
            System.Collections.Hashtable ddss = new System.Collections.Hashtable();

            for ( int i = 0; i < this.DDSGroup.TextureNames.Count; i++ )
            {
                if ( i < 3 )
                    ddss.Add(this.DDSGroup.TextureNames[i], this._ddsGroup.dds1.ExportDDSChannelToTexturemap(2 - i));
                else if( i == 3 )
                    ddss.Add(this.DDSGroup.TextureNames[i], this._ddsGroup.dds1.ExportDDSChannelToTexturemap(i));
                else
                    ddss.Add(this.DDSGroup.TextureNames[i], this._ddsGroup.dds2.ExportDDSChannelToTexturemap(2 - (i - 4)));
            }

            return ddss;
        }
        #endregion

        #region Load/Save
        public void Load( System.IO.BinaryReader reader )
        {
            OEIShared.Utils.FourCC cc = new OEIShared.Utils.FourCC( reader.ReadChars( 4 ) );

            if ( cc != this._fourCC )
                throw new ApplicationException( this._fourCC.ToString() + " not found in stream" );

            this._size = reader.ReadInt32();
            long finalPosition = reader.BaseStream.Position + this._size;
            this._mapPart = OEIShared.Utils.CommonUtils.ConvertZeroTerminatedBytesToString( reader.ReadBytes( 128 ) ).TrimEnd( '\0' );

            this._ddsGroup = new DDSGroup(reader);

            for( int i = 0; i < 18; i++ )
                this._texVectors[i] = reader.ReadSingle();

            this._vertexCount = reader.ReadInt32();
            this._triangleCount = reader.ReadInt32();

            this._vertices = new Vertex[this._vertexCount];
            for ( int i = 0; i < this._vertexCount; i++ )
                this._vertices[i] = new Vertex( reader );

            this._triangles = new Triangle[this._triangleCount];
            for ( int i = 0; i < this._triangleCount; i++ )
                this._triangles[i] = new Triangle( reader );

            this._ddsGroup.Load( reader );
            this._ddsGroup.FirstVertex = this._vertices[0];

            this._unkownDataSize = (int)( finalPosition - reader.BaseStream.Position );
            if ( this._unkownDataSize > 0 )
                this._unknownData = reader.ReadBytes( this._unkownDataSize );
        }

        public void Save( System.IO.BinaryWriter writer )
        {
            writer.Write( this.FourCC.IntValue );
            writer.Write( this._size );
            writer.Write( OEIShared.Utils.CommonUtils.ConvertStringToBytes( this._mapPart.PadRight( 0x80, '\0' ) ) );
            this.DDSGroup.SaveTextureNames(writer);
            for ( int i = 0; i < 18; i++ )
                writer.Write( this._texVectors[i] );
            writer.Write( this._vertexCount );
            writer.Write( this._triangleCount );
            for ( int i = 0; i < this._vertexCount; i++ )
                this._vertices[i].Save( writer );
            for ( int i = 0; i < this._triangleCount; i++ )
                this._triangles[i].Save( writer );

            this._ddsGroup.Save( writer );

            if ( this._unkownDataSize > 0 )
                writer.Write( this._unknownData );
        }
        #endregion

        public override string ToString()
        {
            string str = "";
            int i = 0;
            foreach ( Vertex vert in this._vertices )
            {
                str += vert.ToString() + ( i % 25 == 0 && i != 0 ? "\n" : " " );
                i++;
            }

            return str;
        }
    }
}
