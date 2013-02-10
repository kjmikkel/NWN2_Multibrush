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

/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 08-06-2010
 * Time: 15:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Multibrush
{
	partial class stopMulti
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to 
/// 		this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(stopMulti));
			this.checkShow = new System.Windows.Forms.CheckBox();
			this.close = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// checkShow
			// 
			this.checkShow.Location = new System.Drawing.Point(12, 90);
			this.checkShow.Name = "checkShow";
			this.checkShow.Size = new System.Drawing.Size(249, 44);
			this.checkShow.TabIndex = 0;
			this.checkShow.Text = "Don\'t show this dialog again (can be changed in the preferences in the toolset\'s " +
			"options menu)";
			this.checkShow.UseVisualStyleBackColor = true;
			// 
			// close
			// 
			this.close.Location = new System.Drawing.Point(97, 147);
			this.close.Name = "close";
			this.close.Size = new System.Drawing.Size(75, 23);
			this.close.TabIndex = 1;
			this.close.Text = "Close";
			this.close.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(249, 69);
			this.label1.TabIndex = 2;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// stopMulti
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(273, 182);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.close);
			this.Controls.Add(this.checkShow);
			this.MaximizeBox = false;
			this.Name = "stopMulti";
			this.Text = "How to stop the Multibrush";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.CheckBox checkShow;
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Label label1;
	}
}
