
namespace Frederick_Jim_GOL
{
    partial class RandomFromSeed
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
            this.button1 = new System.Windows.Forms.Button();
            this.RFSOK = new System.Windows.Forms.Button();
            this.RFSCancel = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(195, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 25);
            this.button1.TabIndex = 0;
            this.button1.Text = "Randomize";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // RFSOK
            // 
            this.RFSOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.RFSOK.Location = new System.Drawing.Point(68, 63);
            this.RFSOK.Name = "RFSOK";
            this.RFSOK.Size = new System.Drawing.Size(73, 23);
            this.RFSOK.TabIndex = 1;
            this.RFSOK.Text = "OK";
            this.RFSOK.UseVisualStyleBackColor = true;
            // 
            // RFSCancel
            // 
            this.RFSCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.RFSCancel.Location = new System.Drawing.Point(147, 63);
            this.RFSCancel.Name = "RFSCancel";
            this.RFSCancel.Size = new System.Drawing.Size(75, 23);
            this.RFSCancel.TabIndex = 2;
            this.RFSCancel.Text = "Cancel";
            this.RFSCancel.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(60, 24);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(129, 20);
            this.numericUpDown1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Seed: ";
            // 
            // RandomFromSeed
            // 
            this.AcceptButton = this.RFSOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.RFSCancel;
            this.ClientSize = new System.Drawing.Size(291, 98);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.RFSCancel);
            this.Controls.Add(this.RFSOK);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RandomFromSeed";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Random From Seed";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button RFSOK;
        private System.Windows.Forms.Button RFSCancel;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
    }
}