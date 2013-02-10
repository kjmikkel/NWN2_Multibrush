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
using System.Windows.Forms;
using System.ComponentModel;

namespace Multibrush
{
	/// <summary>
	/// This part of the code is not really in use, and might be removed in the future
	/// </summary>
	[Serializable]
public class Preferences
{
    // Fields
    private Keys colorPaletteShortcut = Keys.None;
    private bool icones = true;
    private float mvt_HeightMultiplier = 10f;
    private Keys mvt_InnerRadiusShortcut = Keys.None;
    private Keys mvt_OuterRadiusShortcut = Keys.None;
    private int mvt_PressureMultiplier = 10;
    private Keys mvt_PressureShortcut = Keys.None;
    private int mvt_SizeMultiplier = 10;
    private Keys revertPaintMode = Keys.None;
    private Keys smoothModeToggle = Keys.None;
    private float wheel_HeightMetersChange = 0.5f;
    private Keys wheel_InnerRadiusShortcut = Keys.Menu;
    private Keys wheel_OuterRadiusShortcut = Keys.ShiftKey;
    private float wheel_PressurePercentChange = 2f;
    private Keys wheel_PressureShortcut = Keys.ControlKey;
    private int wheel_SizeIncrement = 1;

    // Properties
    [DisplayName("Color Palette Shortcut"), Description("Pressing this shortcut will open up the color palette. [Default : None]"), Browsable(true), Category("Paint Mode Shortcuts"), DefaultValue(0)]
    public Keys ColorPaletteShortcut
    {
        get
        {
            return this.colorPaletteShortcut;
        }
        set
        {
            this.colorPaletteShortcut = value;
        }
    }

    [DisplayName("Icons on toolbars"), Category("Autres"), DefaultValue(true), Description("Replaces text with icons on toolbars. Modifications won't be visible until you restart the Toolset."), Browsable(true)]
    public bool Icones
    {
        get
        {
            return this.icones;
        }
        set
        {
            this.icones = value;
        }
    }

    [Browsable(true), Category("Mouse movements combo"), DisplayName("Multiplier - Size"), DefaultValue(10), Description("Determines how quickly the inner / outer radius of the brush will resize. [Default : 10]")]
    public int Mvt_BrushSizeMultiplier
    {
        get
        {
            return this.mvt_SizeMultiplier;
        }
        set
        {
            this.mvt_SizeMultiplier = value;
        }
    }

    [Category("Mouse movements combo"), DisplayName("Multiplier - Height"), Description("Determines how quickly the height value of the current brush (water or flatten) will increase or decrease. [Default : 10]"), Browsable(true), DefaultValue((float) 10f)]
    public float Mvt_HeightMultiplier
    {
        get
        {
            return this.mvt_HeightMultiplier;
        }
        set
        {
            this.mvt_HeightMultiplier = value;
        }
    }

    [Category("Mouse movements combo"), DisplayName("Shortcut - Inner radius"), Description("Pressing this while moving your mouse will let you modify the inner radius of the current brush. [Default : None]"), DefaultValue(0), Browsable(true)]
    public Keys Mvt_InnerRadiusShortcut
    {
        get
        {
            return this.mvt_InnerRadiusShortcut;
        }
        set
        {
            this.mvt_InnerRadiusShortcut = value;
        }
    }

    [Category("Mouse movements combo"), DefaultValue(0), Browsable(true), DisplayName("Shortcut - Outer radius"), Description("Pressing this while moving your mouse will let you modify the outer radius of the current brush. [Default : None]")]
    public Keys Mvt_OuterRadiusShortcut
    {
        get
        {
            return this.mvt_OuterRadiusShortcut;
        }
        set
        {
            this.mvt_OuterRadiusShortcut = value;
        }
    }

    [DefaultValue(10), DisplayName("Multiplier - Pressure"), Description("Determines how quickly the pressure /density value of the current brush will increase or decrease. [Default : 10]"), Browsable(true), Category("Mouse movements combo")]
    public int Mvt_PressureMultiplier
    {
        get
        {
            return this.mvt_PressureMultiplier;
        }
        set
        {
            this.mvt_PressureMultiplier = value;
        }
    }

    [DefaultValue(0), Browsable(true), Description("Pressing this while moving your mouse will let you modify the pressure / density / height value of the current brush. [Default : None]"), Category("Mouse movements combo"), DisplayName("Shortcut - Pressure / Height")]
    public Keys Mvt_PressureShortcut
    {
        get
        {
            return this.mvt_PressureShortcut;
        }
        set
        {
            this.mvt_PressureShortcut = value;
        }
    }

    [DefaultValue(0), Description("Pressing this shortcut will change the brush's sub-mode to it's opposite (i.e. Lower if you were in Raise mode, Erase Water if you were in Paint Water mode, and vice versa, and so on). Extra note : in Color mode, it will revert to white (erase) then back to your last chosen color. [Default : None]"), Category("Paint Mode Shortcuts"), DisplayName("Revert paint mode"), Browsable(true)]
    public Keys RevertPaintMode
    {
        get
        {
            return this.revertPaintMode;
        }
        set
        {
            this.revertPaintMode = value;
        }
    }

    [Browsable(true), DisplayName("Smooth mode shortcut"), Category("Paint Mode Shortcuts"), DefaultValue(0), Description("When in Terrain Paint mode, pressing this shortcut will change the brush's sub-mode to Smooth. Press it again and you're back to the last sub-mode. [Default : None]")]
    public Keys SmoothMode
    {
        get
        {
            return this.smoothModeToggle;
        }
        set
        {
            this.smoothModeToggle = value;
        }
    }

    [Browsable(true), Description("Determines how quickly the inner / outer radius of the brush will resize. Negative values can be used to reverse the wheel's behavior. [Default : 1]"), Category("Mouse wheel combo"), DefaultValue(1), DisplayName("Increment - Size")]
    public int Wheel_BrushSizeIncrement
    {
        get
        {
            return this.wheel_SizeIncrement;
        }
        set
        {
            this.wheel_SizeIncrement = value;
        }
    }

    [Description(". [Default : 0.5]"), Category("Mouse wheel combo"), Browsable(true), DefaultValue((float) 0.5f), DisplayName("Increment - Height (in meters)")]
    public float Wheel_HeightIncrementInMeters
    {
        get
        {
            return this.wheel_HeightMetersChange;
        }
        set
        {
            this.wheel_HeightMetersChange = value;
        }
    }

    [Browsable(true), Category("Mouse wheel combo"), DisplayName("Shortcut - Inner Radius"), DefaultValue(0x12), Description("Pressing this while scrolling your mouse wheel will let you redefine the inner radius of the current brush. [Default : Menu (means Alt. Enter it manually or reset all.)]")]
    public Keys Wheel_InnerRadiusShortcut
    {
        get
        {
            return this.wheel_InnerRadiusShortcut;
        }
        set
        {
            this.wheel_InnerRadiusShortcut = value;
        }
    }

    [Browsable(true), DefaultValue(0x11), DisplayName("Shortcut - Outer Radius"), Category("Mouse wheel combo"), Description("Pressing this while scrolling your mouse wheel will let you modify the outer radius of the current brush. [Default : ControlKey (enter it manually or reset all)]")]
    public Keys Wheel_OuterRadiusShortcut
    {
        get
        {
            return this.wheel_OuterRadiusShortcut;
        }
        set
        {
            this.wheel_OuterRadiusShortcut = value;
        }
    }

    [Category("Mouse wheel combo"), DisplayName("Increment - Pressure (in percent)"), Browsable(true), DefaultValue((float) 2f), Description(". [Default : 2.0]")]
    public float Wheel_PressureIncrementInPercent
    {
        get
        {
            return this.wheel_PressurePercentChange;
        }
        set
        {
            this.wheel_PressurePercentChange = value;
        }
    }

    [DefaultValue(0x10), Description("Pressing this while scrolling your mouse wheel will let you redefine the pressure of the current brush. [Default : ShiftKey (enter it manually or reset all)]"), Browsable(true), Category("Mouse wheel combo"), DisplayName("Shortcut - Pressure / Height")]
    public Keys Wheel_PressureShortcut
    {
        get
        {
            return this.wheel_PressureShortcut;
        }
        set
        {
            this.wheel_PressureShortcut = value;
        }
    }
}


}
