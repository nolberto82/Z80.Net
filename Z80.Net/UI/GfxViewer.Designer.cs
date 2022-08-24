namespace Z80.Net.UI
{
    partial class GfxViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picTiles = new System.Windows.Forms.PictureBox();
            this.picSprites = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picTiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSprites)).BeginInit();
            this.SuspendLayout();
            // 
            // picTiles
            // 
            this.picTiles.Location = new System.Drawing.Point(12, 35);
            this.picTiles.Name = "picTiles";
            this.picTiles.Size = new System.Drawing.Size(256, 256);
            this.picTiles.TabIndex = 0;
            this.picTiles.TabStop = false;
            // 
            // picSprites
            // 
            this.picSprites.Location = new System.Drawing.Point(274, 35);
            this.picSprites.Name = "picSprites";
            this.picSprites.Size = new System.Drawing.Size(256, 256);
            this.picSprites.TabIndex = 1;
            this.picSprites.TabStop = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tiles:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(274, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(256, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sprites:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(16, 301);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // GfxViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 336);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picSprites);
            this.Controls.Add(this.picTiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GfxViewer";
            this.Text = "Graphics Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.picTiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSprites)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picTiles;
        private System.Windows.Forms.PictureBox picSprites;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
    }
}