namespace PrimitiveRecognizer
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recognizePrimitivesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupStrokesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classifierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Sketch = new System.Windows.Forms.Panel();
            this.grouperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.testToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(708, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSketchToolStripMenuItem,
            this.loadSketchToolStripMenuItem,
            this.saveSketchToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newSketchToolStripMenuItem
            // 
            this.newSketchToolStripMenuItem.Name = "newSketchToolStripMenuItem";
            this.newSketchToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.newSketchToolStripMenuItem.Text = "New Sketch";
            this.newSketchToolStripMenuItem.Click += new System.EventHandler(this.newSketchToolStripMenuItem_Click);
            // 
            // loadSketchToolStripMenuItem
            // 
            this.loadSketchToolStripMenuItem.Name = "loadSketchToolStripMenuItem";
            this.loadSketchToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.loadSketchToolStripMenuItem.Text = "Load Sketch";
            this.loadSketchToolStripMenuItem.Click += new System.EventHandler(this.loadSketchToolStripMenuItem_Click);
            // 
            // saveSketchToolStripMenuItem
            // 
            this.saveSketchToolStripMenuItem.Name = "saveSketchToolStripMenuItem";
            this.saveSketchToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.saveSketchToolStripMenuItem.Text = "Save Sketch";
            this.saveSketchToolStripMenuItem.Click += new System.EventHandler(this.saveSketchToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.recognizePrimitivesToolStripMenuItem,
            this.groupStrokesToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // recognizePrimitivesToolStripMenuItem
            // 
            this.recognizePrimitivesToolStripMenuItem.Name = "recognizePrimitivesToolStripMenuItem";
            this.recognizePrimitivesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.recognizePrimitivesToolStripMenuItem.Text = "Recognize Primitives";
            this.recognizePrimitivesToolStripMenuItem.Click += new System.EventHandler(this.recognizePrimitivesToolStripMenuItem_Click);
            // 
            // groupStrokesToolStripMenuItem
            // 
            this.groupStrokesToolStripMenuItem.Name = "groupStrokesToolStripMenuItem";
            this.groupStrokesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.groupStrokesToolStripMenuItem.Text = "Group Strokes";
            this.groupStrokesToolStripMenuItem.Click += new System.EventHandler(this.groupStrokesToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.classificationToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // classificationToolStripMenuItem
            // 
            this.classificationToolStripMenuItem.Name = "classificationToolStripMenuItem";
            this.classificationToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.classificationToolStripMenuItem.Text = "Classification";
            this.classificationToolStripMenuItem.Click += new System.EventHandler(this.classificationToolStripMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.classifierToolStripMenuItem,
            this.grouperToolStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.testToolStripMenuItem.Text = "Test";
            // 
            // classifierToolStripMenuItem
            // 
            this.classifierToolStripMenuItem.Name = "classifierToolStripMenuItem";
            this.classifierToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.classifierToolStripMenuItem.Text = "Classifier";
            this.classifierToolStripMenuItem.Click += new System.EventHandler(this.classifierToolStripMenuItem_Click);
            // 
            // Sketch
            // 
            this.Sketch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Sketch.Location = new System.Drawing.Point(0, 24);
            this.Sketch.Name = "Sketch";
            this.Sketch.Size = new System.Drawing.Size(708, 631);
            this.Sketch.TabIndex = 1;
            // 
            // grouperToolStripMenuItem
            // 
            this.grouperToolStripMenuItem.Name = "grouperToolStripMenuItem";
            this.grouperToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.grouperToolStripMenuItem.Text = "Grouper";
            this.grouperToolStripMenuItem.Click += new System.EventHandler(this.grouperToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 655);
            this.Controls.Add(this.Sketch);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSketchToolStripMenuItem;
        private System.Windows.Forms.Panel Sketch;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recognizePrimitivesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classificationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classifierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupStrokesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem grouperToolStripMenuItem;
    }
}