﻿namespace Sdl.Community.projectAnonymizer.Ui
{
	partial class DecryptSettingsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.decryptPanel = new System.Windows.Forms.TableLayoutPanel();
			this.lockPictureBox = new System.Windows.Forms.PictureBox();
			this.encryptedMessage = new System.Windows.Forms.Label();
			this.decryptionPanel = new System.Windows.Forms.Panel();
			this.encryptionBox = new System.Windows.Forms.TextBox();
			this.messageLbl = new System.Windows.Forms.Label();
			this.decryptPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lockPictureBox)).BeginInit();
			this.decryptionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// decryptPanel
			// 
			this.decryptPanel.AutoSize = true;
			this.decryptPanel.ColumnCount = 3;
			this.decryptPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.decryptPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.decryptPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.decryptPanel.Controls.Add(this.messageLbl, 1, 7);
			this.decryptPanel.Controls.Add(this.decryptionPanel, 1, 5);
			this.decryptPanel.Controls.Add(this.encryptedMessage, 1, 3);
			this.decryptPanel.Controls.Add(this.lockPictureBox, 1, 1);
			this.decryptPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.decryptPanel.Location = new System.Drawing.Point(0, 0);
			this.decryptPanel.Name = "decryptPanel";
			this.decryptPanel.RowCount = 10;
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.decryptPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.decryptPanel.Size = new System.Drawing.Size(717, 434);
			this.decryptPanel.TabIndex = 0;
			// 
			// lockPictureBox
			// 
			this.lockPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lockPictureBox.Image = global::Sdl.Community.projectAnonymizer.PluginResources.lockxxx;
			this.lockPictureBox.Location = new System.Drawing.Point(294, 17);
			this.lockPictureBox.Name = "lockPictureBox";
			this.lockPictureBox.Size = new System.Drawing.Size(129, 132);
			this.lockPictureBox.TabIndex = 9;
			this.lockPictureBox.TabStop = false;
			// 
			// encryptedMessage
			// 
			this.encryptedMessage.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.encryptedMessage.AutoEllipsis = true;
			this.encryptedMessage.AutoSize = true;
			this.encryptedMessage.CausesValidation = false;
			this.encryptedMessage.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.encryptedMessage.Location = new System.Drawing.Point(266, 166);
			this.encryptedMessage.Name = "encryptedMessage";
			this.encryptedMessage.Size = new System.Drawing.Size(184, 25);
			this.encryptedMessage.TabIndex = 10;
			this.encryptedMessage.Text = "Enter encryption key";
			this.encryptedMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// decryptionPanel
			// 
			this.decryptionPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.decryptionPanel.Controls.Add(this.encryptionBox);
			this.decryptionPanel.Location = new System.Drawing.Point(187, 208);
			this.decryptionPanel.Name = "decryptionPanel";
			this.decryptionPanel.Size = new System.Drawing.Size(342, 32);
			this.decryptionPanel.TabIndex = 11;
			// 
			// encryptionBox
			// 
			this.encryptionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.encryptionBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.encryptionBox.Location = new System.Drawing.Point(0, 0);
			this.encryptionBox.Name = "encryptionBox";
			this.encryptionBox.PasswordChar = '*';
			this.encryptionBox.Size = new System.Drawing.Size(342, 29);
			this.encryptionBox.TabIndex = 0;
			// 
			// messageLbl
			// 
			this.messageLbl.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.messageLbl.AutoEllipsis = true;
			this.messageLbl.AutoSize = true;
			this.messageLbl.CausesValidation = false;
			this.messageLbl.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.messageLbl.Location = new System.Drawing.Point(358, 257);
			this.messageLbl.Name = "messageLbl";
			this.messageLbl.Size = new System.Drawing.Size(0, 25);
			this.messageLbl.TabIndex = 12;
			this.messageLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.messageLbl.Visible = false;
			// 
			// DecryptSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.decryptPanel);
			this.Name = "DecryptSettingsControl";
			this.Size = new System.Drawing.Size(717, 434);
			this.decryptPanel.ResumeLayout(false);
			this.decryptPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lockPictureBox)).EndInit();
			this.decryptionPanel.ResumeLayout(false);
			this.decryptionPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel decryptPanel;
		private System.Windows.Forms.PictureBox lockPictureBox;
		private System.Windows.Forms.Label encryptedMessage;
		private System.Windows.Forms.Panel decryptionPanel;
		private System.Windows.Forms.TextBox encryptionBox;
		private System.Windows.Forms.Label messageLbl;
	}
}
