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

namespace SFX.YATT.DAL
{
    public class DDS : DAL.IPacket
    {
        public OEIShared.Utils.FourCC FourCC
        {
            get
            {
                return this._fourCC;
            }
        }

        private byte[] _unknownHeader;
        private OEIShared.Utils.FourCC _fourCC = new OEIShared.Utils.FourCC( "DDS" + (char)0x20 );
        private int _unknown0;
        private int _unknown1;
        public int Width
        {
            get
            {
                return this._width;
            }
        }
        private int _width;
        public int Height
        {
            get
            {
                return this._height;
            }
        }
        private int _height;
        private byte[] _unknown3;
        internal byte[] RawTextureData
        {
            get
            {
                return this._textureData;
            }
        }

        private byte[] _textureData;

        public DDS( System.IO.BinaryReader reader )
        {
            this.Load( reader );
        }

        public void Load( System.IO.BinaryReader reader )
        {
            this._unknownHeader = reader.ReadBytes( 4 );

            OEIShared.Utils.FourCC cc = new OEIShared.Utils.FourCC( reader.ReadChars( 4 ) );

            if ( cc != this._fourCC )
                throw new ApplicationException( 
                    string.Format( "{0} not found in stream.\nActual: ({1})\nStream Position: {2}", 
                    this._fourCC, cc.IntValue, reader.BaseStream.Position - 4 ) );

            this._unknown0 = reader.ReadInt32();
            this._unknown1 = reader.ReadInt32();
            this._width = reader.ReadInt32();
            this._height = reader.ReadInt32();
            this._unknown3 = reader.ReadBytes( 108 );
            this._textureData = reader.ReadBytes( this._width * this._height * 4 );

        }

        public void Save( System.IO.BinaryWriter writer )
        {
            writer.Write( this._unknownHeader );
            writer.Write( this._fourCC.IntValue );
            writer.Write( this._unknown0 );
            writer.Write( this._unknown1 );
            writer.Write( this._width );
            writer.Write( this._height );
            writer.Write( this._unknown3 );
            writer.Write( this._textureData );
        }

        public System.Drawing.Bitmap ExportDDSChannelToTexturemap( int channel )
        {
            System.Drawing.Bitmap texturemap = new System.Drawing.Bitmap( this._width, this._height );
            for ( int i = channel; i < this._textureData.Length; i += 4 )
            {
                int x = -1, y = -1;
                try
                {
                    int textureAmount = this._textureData[i];
                    x = ( i / 4 ) % this._width;
                    y = this._height - (int)Math.Floor( ( i / 4 ) / (double)this._width ) - 1;
                    texturemap.SetPixel( x, y, System.Drawing.Color.FromArgb( textureAmount, textureAmount, textureAmount ) );
                }
                catch ( Exception ex )
                {
                    System.Windows.Forms.MessageBox.Show( string.Format( "i={0} x={1} y={2}", i, x, y ) );
                    throw ex;
                }
            }

            return texturemap;
        }

        public byte this[int x, int y, int channel]
        {
            get
            {
                return this._textureData[x * 4 + y * this._width * 4 + channel];
            }
            set
            {
                this._textureData[x * 4 + y * this._width * 4 + channel] = value;
            }
        }
    }
}
