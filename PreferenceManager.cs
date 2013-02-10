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

namespace Multibrush
{
	/// <summary>
	/// This part of the code is not really in use, and might be removed in the future
	/// </summary>
	public class PreferenceManager
	{
    // Fields
    private Preferences m_pref;
    private static PreferenceManager me = null;

    // Methods
    private PreferenceManager()
    {
    }

    // Properties
    public static Preferences PluginPreferences
    {
        get
        {
            if (me == null)
            {
                me = new PreferenceManager();
                me.m_pref = new Preferences();
            }
            return me.m_pref;
        }
        set
        {
            if (me == null)
            {
                me = new PreferenceManager();
            }
            me.m_pref = value;
        }
    }
}
	}