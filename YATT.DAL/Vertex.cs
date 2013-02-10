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
    public class Vertex : IPacket
    {
        public Vertex( System.IO.BinaryReader reader )
        {
            this.Load( reader );
        }

        public Tools.Vector<float> Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
            }
        }

        public Tools.Vector<float> Normal
        {
            get
            {
                return this._normal;
            }
            set
            {
                this._normal = value;
            }
        }

        public System.Drawing.Color Color
        {
            get
            {
                return this._rgba;
            }
            set
            {
                this._rgba = value;
            }
        }

        private Tools.Vector<float> _position;
        private Tools.Vector<float> _normal;
        private System.Drawing.Color _rgba;
        private float[] _data = new float[4];
        public void Load( System.IO.BinaryReader reader )
        {
            this._position = new SFX.YATT.Tools.Vector<float>( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );
            this._normal = new SFX.YATT.Tools.Vector<float>( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );
            byte    b = reader.ReadByte(),
                    g = reader.ReadByte(),
                    r = reader.ReadByte(),
                    a = reader.ReadByte();
            this._rgba = System.Drawing.Color.FromArgb( a, r, g, b );
            this._data[0] = reader.ReadSingle();
            this._data[1] = reader.ReadSingle();
            this._data[2] = reader.ReadSingle();
            this._data[3] = reader.ReadSingle();
        }
        public void Save( System.IO.BinaryWriter writer )
        {
            foreach ( float f in this._position )
                writer.Write( f );

            foreach ( float f in this._normal )
                writer.Write( f );

            writer.Write((byte)this._rgba.B);
            writer.Write((byte)this._rgba.G);
            writer.Write( (byte)this._rgba.R );
            writer.Write((byte)this._rgba.A);           
             
            foreach ( float f in this._data )
                writer.Write( f );
        }

        public override string ToString()
        {
            return string.Format( "[{0:000.00},{1:000.00},{2:000.00}]", this.Position[0], this.Position[1], this.Position[2] );
        }
    }
}
