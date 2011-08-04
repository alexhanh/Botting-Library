namespace Bopycat
{
    partial class Form1
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
			this.labelLeftName = new System.Windows.Forms.Label();
			this.labelRightName = new System.Windows.Forms.Label();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.labelLeftPips = new System.Windows.Forms.Label();
			this.labelRightPips = new System.Windows.Forms.Label();
			this.buttonRoll = new System.Windows.Forms.Button();
			this.buttonDone = new System.Windows.Forms.Button();
			this.buttonUndo = new System.Windows.Forms.Button();
			this.buttonDouble = new System.Windows.Forms.Button();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.buttonSave = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelLeftName
			// 
			this.labelLeftName.AutoSize = true;
			this.labelLeftName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelLeftName.Location = new System.Drawing.Point(212, 507);
			this.labelLeftName.Name = "labelLeftName";
			this.labelLeftName.Size = new System.Drawing.Size(49, 16);
			this.labelLeftName.TabIndex = 1;
			this.labelLeftName.Text = "Name";
			// 
			// labelRightName
			// 
			this.labelRightName.AutoSize = true;
			this.labelRightName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRightName.Location = new System.Drawing.Point(625, 507);
			this.labelRightName.Name = "labelRightName";
			this.labelRightName.Size = new System.Drawing.Size(49, 16);
			this.labelRightName.TabIndex = 2;
			this.labelRightName.Text = "Name";
			this.labelRightName.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1072, 24);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// labelLeftPips
			// 
			this.labelLeftPips.AutoSize = true;
			this.labelLeftPips.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelLeftPips.Location = new System.Drawing.Point(212, 524);
			this.labelLeftPips.Name = "labelLeftPips";
			this.labelLeftPips.Size = new System.Drawing.Size(43, 20);
			this.labelLeftPips.TabIndex = 4;
			this.labelLeftPips.Text = "Pips";
			// 
			// labelRightPips
			// 
			this.labelRightPips.AutoSize = true;
			this.labelRightPips.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRightPips.Location = new System.Drawing.Point(633, 524);
			this.labelRightPips.Name = "labelRightPips";
			this.labelRightPips.Size = new System.Drawing.Size(43, 20);
			this.labelRightPips.TabIndex = 5;
			this.labelRightPips.Text = "Pips";
			this.labelRightPips.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// buttonRoll
			// 
			this.buttonRoll.Enabled = false;
			this.buttonRoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonRoll.Location = new System.Drawing.Point(716, 261);
			this.buttonRoll.Name = "buttonRoll";
			this.buttonRoll.Size = new System.Drawing.Size(70, 50);
			this.buttonRoll.TabIndex = 6;
			this.buttonRoll.Text = "Roll";
			this.buttonRoll.UseVisualStyleBackColor = true;
			this.buttonRoll.Visible = false;
			this.buttonRoll.Click += new System.EventHandler(this.buttonRoll_Click);
			// 
			// buttonDone
			// 
			this.buttonDone.Enabled = false;
			this.buttonDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonDone.Location = new System.Drawing.Point(716, 261);
			this.buttonDone.Name = "buttonDone";
			this.buttonDone.Size = new System.Drawing.Size(70, 50);
			this.buttonDone.TabIndex = 8;
			this.buttonDone.Text = "Done";
			this.buttonDone.UseVisualStyleBackColor = true;
			this.buttonDone.Visible = false;
			this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
			// 
			// buttonUndo
			// 
			this.buttonUndo.Enabled = false;
			this.buttonUndo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonUndo.Location = new System.Drawing.Point(716, 317);
			this.buttonUndo.Name = "buttonUndo";
			this.buttonUndo.Size = new System.Drawing.Size(70, 50);
			this.buttonUndo.TabIndex = 9;
			this.buttonUndo.Text = "Undo";
			this.buttonUndo.UseVisualStyleBackColor = true;
			this.buttonUndo.Visible = false;
			this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
			// 
			// buttonDouble
			// 
			this.buttonDouble.Enabled = false;
			this.buttonDouble.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonDouble.Location = new System.Drawing.Point(12, 261);
			this.buttonDouble.Name = "buttonDouble";
			this.buttonDouble.Size = new System.Drawing.Size(76, 50);
			this.buttonDouble.TabIndex = 10;
			this.buttonDouble.Text = "Double";
			this.buttonDouble.UseVisualStyleBackColor = true;
			this.buttonDouble.Visible = false;
			this.buttonDouble.Click += new System.EventHandler(this.buttonDouble_Click);
			// 
			// textBoxLog
			// 
			this.textBoxLog.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBoxLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxLog.Location = new System.Drawing.Point(834, 108);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.Size = new System.Drawing.Size(226, 322);
			this.textBoxLog.TabIndex = 11;
			this.textBoxLog.TabStop = false;
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(985, 496);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(75, 23);
			this.buttonSave.TabIndex = 12;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1072, 632);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.textBoxLog);
			this.Controls.Add(this.buttonDouble);
			this.Controls.Add(this.buttonRoll);
			this.Controls.Add(this.labelRightPips);
			this.Controls.Add(this.labelLeftPips);
			this.Controls.Add(this.labelRightName);
			this.Controls.Add(this.labelLeftName);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.buttonUndo);
			this.Controls.Add(this.buttonDone);
			this.DoubleBuffered = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLeftName;
        private System.Windows.Forms.Label labelRightName;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Label labelLeftPips;
        private System.Windows.Forms.Label labelRightPips;
		private System.Windows.Forms.Button buttonRoll;
        private System.Windows.Forms.Button buttonDone;
		private System.Windows.Forms.Button buttonUndo;
		private System.Windows.Forms.Button buttonDouble;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.Button buttonSave;
    }
}

