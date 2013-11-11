using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SketchPanelLib;

namespace PrimitiveRecognizer
{
    public partial class MainForm : Form
    {
        private SketchPanel sketchPanel;
        private RecognitionManager recManager;
        private DisplayManager disManager;
        public MainForm()
        {
            InitializeComponent();


            sketchPanel = new SketchPanel();
            sketchPanel.Dock = DockStyle.Fill;
            sketchPanel.Name = "Sketch";

            this.Sketch.Controls.Add(sketchPanel);

            recManager = new RecognitionManager(sketchPanel);
            disManager = new DisplayManager(sketchPanel);
        }
        #region File Menu
        private void newSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.Clear();
        }

        private void loadSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Load a Sketch";
            openFileDialog.Filter = "MIT XML sketches (*.xml)|*.xml|" +
                "Microsoft Windows Journal Files (*.jnt)|*.jnt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!System.IO.File.Exists(openFileDialog.FileName))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                sketchPanel.LoadSketch(openFileDialog.FileName);
            }
        }

        private void saveSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "MIT XML Files (*.xml)|*.xml|Canonical Example (*.cxtd)|*.cxtd";
            saveFileDialog.AddExtension = true;

            // Write the XML to a file
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                sketchPanel.SaveSketch(saveFileDialog.FileName);
            }
        }
        #endregion

        #region Tools Menu
        private void recognizePrimitivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            recManager.classifySketch();
            disManager.DisplayClassification();
        }
        private void groupStrokesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            recManager.groupSketch();
            disManager.DisplayGroups();
        }
        #endregion

        #region View Menu
        private void classificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disManager.DisplayClassification();
        }
        #endregion

        #region Test Menu
        private void classifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recManager.testClassifier(sourceFolderDialog.SelectedPath);
        }

        private void grouperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recManager.testGrouper(sourceFolderDialog.SelectedPath);
        }
        #endregion
    }
}