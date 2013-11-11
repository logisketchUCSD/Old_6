using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using ZedGraph;

namespace IOTrain
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private ZedGraph.ZedGraphControl zg1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private string filename;
		private ArrayList list1;
		private ArrayList list2;
		private ArrayList list3;
		private ArrayList list4;

		private ArrayList wirenames;
		private Wire[] wires;
		/*
		private PointPairList list5;
		private PointPairList list6;
		private PointPairList list7;
		private PointPairList list8;
		private PointPairList list9;
		private PointPairList list10;
		*/
		public Form1(ArrayList list1, ArrayList list2, ArrayList list3,
			ArrayList list4, string filename, ArrayList wirenames, Wire[] wires)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.filename = filename;
			this.list1 = list1;
			this.list2 = list2;
			this.list3 = list3;
			this.list4 = list4;
			/*
			this.list5 = list5;
			this.list6 = list6;
			this.list7 = list7;
			this.list8 = list8;
			this.list9 = list9;
			this.list10 = list10;
			*/
			this.wirenames = wirenames;
			this.wires = wires;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zg1 = new ZedGraph.ZedGraphControl();
			this.SuspendLayout();
			// 
			// zg1
			// 
			this.zg1.Location = new System.Drawing.Point(8, 8);
			this.zg1.Name = "zg1";
			this.zg1.ScrollGrace = 0;
			this.zg1.ScrollMaxX = 0;
			this.zg1.ScrollMaxY = 0;
			this.zg1.ScrollMaxY2 = 0;
			this.zg1.ScrollMinX = 0;
			this.zg1.ScrollMinY = 0;
			this.zg1.ScrollMinY2 = 0;
			this.zg1.Size = new System.Drawing.Size(352, 248);
			this.zg1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(368, 262);
			this.Controls.Add(this.zg1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void Form1_Load(object sender, System.EventArgs e)
		{
			CreateChart( zg1 ,filename);
			SetSize();
		}

		// Call this method from the Form_Load method, passing your ZedGraphControl
		public void CreateChart( ZedGraphControl zgc, string filename)
		{
			GraphPane myPane = zgc.GraphPane;

			// Set the title and axis labels
			myPane.Title.Text = filename;
			myPane.XAxis.Title.Text = "X Axis";
			myPane.YAxis.Title.Text = "Y Axis";
			myPane.Legend.IsVisible = false;

			for (int i=0; i<list1.Count; i++)
			{
				PointPairList temppts = (PointPairList)list1[i];
				LineItem myCurve = myPane.AddCurve( "Wires", 
					temppts, Color.Red, SymbolType.Diamond );

				// Add a text item to label the highlighted range
				TextObj text = new TextObj( (string)wirenames[i], (double)wires[i].P1.X, -(double)wires[i].P1.Y, CoordType.AxisXYScale,
					AlignH.Right, AlignV.Center );
				text.FontSpec.FontColor = Color.Black;
				text.FontSpec.Fill.IsVisible = false;
				text.FontSpec.Border.IsVisible = false;
				text.FontSpec.IsBold = true;
				text.FontSpec.IsItalic = true;
				myPane.GraphObjList.Add( text );

			}
			for (int i=0; i<list2.Count; i++)
			{
				PointPairList temppts = (PointPairList)list2[i];
				LineItem myCurve1 = myPane.AddCurve( "Gates",
					temppts, Color.Green, SymbolType.Diamond );
			}
			
			// Generate a blue curve with circle
			// symbols, and "Piper" in the legend
			for (int i=0; i<list3.Count; i++)
			{
				PointPairList temppts = (PointPairList)list3[i];
				LineItem myCurve2 = myPane.AddCurve( "End Points",
					temppts, Color.Blue, SymbolType.Circle );
			}
			
			for (int i=0; i<list4.Count; i++)
			{
				PointPairList temppts = (PointPairList)list4[i];
				LineItem myCurve3 = myPane.AddCurve( "Labels",
					temppts, Color.Orange, SymbolType.Circle );
			}

			/*
			LineItem myCurve4 = myPane.AddCurve( "Right-Box",
				list5, Color.Blue, SymbolType.Circle );

			LineItem myCurve5 = myPane.AddCurve( "Bottom-Box",
				list6, Color.Blue, SymbolType.Circle );

			LineItem myCurve6 = myPane.AddCurve( "TL-Box",
				list7, Color.Orange, SymbolType.Circle );

			LineItem myCurve7 = myPane.AddCurve( "TR-Box",
				list8, Color.Orange, SymbolType.Circle );

			LineItem myCurve8 = myPane.AddCurve( "BL-Box",
				list9, Color.Orange, SymbolType.Circle );

			LineItem myCurve9 = myPane.AddCurve( "BR-Box",
				list10, Color.Orange, SymbolType.Circle );
  */  
			
			// Calculate the Axis Scale Ranges
			zgc.AxisChange();
		}

		private void Form1_Resize(object sender, System.EventArgs e)
		{
			SetSize();
		}

		private void SetSize()
		{
			zg1.Location = new Point( 10, 10 );
			// Leave a small margin around the outside of the control
			zg1.Size = new Size( this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20 );
		}

	}
}
