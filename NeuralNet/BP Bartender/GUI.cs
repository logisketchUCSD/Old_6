using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using NeuralNets;

namespace BP_Bartender
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form : System.Windows.Forms.Form
	{
		/// <summary>
		/// Number of total ingredients
		/// </summary>
		private const int NUM_INGREDIENTS = 28;
		private const int NUM_FEATURES = 34;
		
		/// <summary>
		/// Number of garnishes (within the ingredients)
		/// </summary>
		private const int GARNISHES = 6;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem3;

		private NeuralNets.BackProp bpNetwork;
		private System.Windows.Forms.TabControl ingrTab;
		private System.Windows.Forms.TabPage hardAlcoholTab;
		private System.Windows.Forms.TabPage alcoholicMixersTab;
		private System.Windows.Forms.TabPage nonalcoholicMixersTab;
		private System.Windows.Forms.TabPage juicesTab;
		private System.Windows.Forms.TabPage garnishTab;
		private System.Windows.Forms.NumericUpDown tequilaIndex;
		private System.Windows.Forms.NumericUpDown whiskeyIndex;
		private System.Windows.Forms.NumericUpDown ginIndex;
		private System.Windows.Forms.NumericUpDown rumIndex;
		private System.Windows.Forms.NumericUpDown vodkaIndex;
		private System.Windows.Forms.NumericUpDown coffeeLiqueurIndex;
		private System.Windows.Forms.NumericUpDown baileysIndex;
		private System.Windows.Forms.NumericUpDown cremeMintheIndex;
		private System.Windows.Forms.NumericUpDown tripleSecIndex;
		private System.Windows.Forms.NumericUpDown dryVermouthIndex;
		private System.Windows.Forms.NumericUpDown sweetVermouthIndex;
		private System.Windows.Forms.NumericUpDown sourIndex;
		private System.Windows.Forms.NumericUpDown gingerAleIndex;
		private System.Windows.Forms.NumericUpDown tonicIndex;
		private System.Windows.Forms.NumericUpDown clubSodaIndex;
		private System.Windows.Forms.NumericUpDown cokeIndex;
		private System.Windows.Forms.NumericUpDown creamIndex;
		private System.Windows.Forms.NumericUpDown grenadineIndex;
		private System.Windows.Forms.NumericUpDown grapefruitIndex;
		private System.Windows.Forms.NumericUpDown cranberryIndex;
		private System.Windows.Forms.NumericUpDown pineappleIndex;
		private System.Windows.Forms.NumericUpDown orangeIndex;
		private System.Windows.Forms.NumericUpDown bittersIndex;
		private System.Windows.Forms.NumericUpDown oliveIndex;
		private System.Windows.Forms.NumericUpDown cherryIndex;
		private System.Windows.Forms.NumericUpDown lemonIndex;
		private System.Windows.Forms.NumericUpDown limeIndex;
		private System.Windows.Forms.NumericUpDown mintIndex;
		private System.Windows.Forms.Label tequilaLabel;
		private System.Windows.Forms.Label whiskeyLabel;
		private System.Windows.Forms.Label ginLabel;
		private System.Windows.Forms.Label rumLabel;
		private System.Windows.Forms.Label vodkaLabel;
		private System.Windows.Forms.Label coffeeLiqueurLabel;
		private System.Windows.Forms.Label baileysLabel;
		private System.Windows.Forms.Label cremeMintheLabel;
		private System.Windows.Forms.Label tripleSecLabel;
		private System.Windows.Forms.Label dryVermouthLabel;
		private System.Windows.Forms.Label sweetVermouthLabel;
		private System.Windows.Forms.Label sourLabel;
		private System.Windows.Forms.Label gingerAleLabel;
		private System.Windows.Forms.Label tonicLabel;
		private System.Windows.Forms.Label clubSodaLabel;
		private System.Windows.Forms.Label cokeLabel;
		private System.Windows.Forms.Label creamLabel;
		private System.Windows.Forms.Label grenadineLabel;
		private System.Windows.Forms.Label grapefruitLabel;
		private System.Windows.Forms.Label cranberryLabel;
		private System.Windows.Forms.Label pineappleLabel;
		private System.Windows.Forms.Label orangeLabel;
		private System.Windows.Forms.Label bittersLabel;
		private System.Windows.Forms.Label oliveLabel;
		private System.Windows.Forms.Label cherryLabel;
		private System.Windows.Forms.Label lemonLabel;
		private System.Windows.Forms.Label limeLabel;
		private System.Windows.Forms.Label mintLabel;
		private System.Windows.Forms.Label ratingLabel;
		private System.Windows.Forms.TextBox recipeTextBox;
		private System.Windows.Forms.MenuItem quitMenuItem;

		/// <summary>
		/// Drink vector to input into the neural network
		/// </summary>
		private double[] drinkVector;
		
		/// <summary>
		/// Names of all ingredients
		/// </summary>
		private string[] ingredientNames;
		private System.Windows.Forms.MenuItem loadMenuItem;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.Button clearRecipeBtn;

		/// <summary>
		/// Array containing all of the ingredient's numericUpDown boxes
		/// </summary>
		private System.Windows.Forms.NumericUpDown[] ingredientNumericBoxes;

		/// <summary>
		/// Ingredient enum (just incase)
		/// </summary>
		private enum Ingredients
		{
			// Hard alcohols
			Vodka = 0, Rum = 1, Gin	= 2, Whiskey = 3, Tequila = 4,
			// Alcoholic mixers
			SweetVermouth = 5, DryVermouth = 6, TripleSec = 7, CremeDeMinthe = 8, 
			Baileys = 9, CoffeeLiqueur = 10,
			// Non-alcoholic mixers
			Coke = 11, ClubSoda = 12, Tonic = 13, GingerAle = 14, Sour = 15,
			// Juices, etc.
			Orange = 16, Pineapple = 17, Cranberry = 18, Grapefruit = 19, Grenadine = 20, Cream = 21,
			// Garnish
			Mint = 22, Lime = 23, Lemon = 24, Cherry = 25, Olive = 26, Bitters = 27
		}


		public Form()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			
			// Set up our back-propagation
			this.bpNetwork = new NeuralNets.BackProp("cocktails.bp");

			// Initialize the drink vector
			this.drinkVector = new double[NUM_FEATURES];
			for (int i = 0; i < NUM_FEATURES; i++)
				this.drinkVector[i] = 0.0;
			
			// Initialize the names of the ingredients
			this.ingredientNames = new string[NUM_INGREDIENTS] {"Vodka", "Rum", "Gin", "Whiskey", "Tequila", 
				"Sweet Vermouth", "Dry Vermouth", "Triple Sec", "Crème De Minthe", "Bailey\'s Irish Cream", "Coffee Liqueur",
				"Coke", "Club Soda", "Tonic", "Ginger Ale", "Sour", "Orange Juice", "Pineapple Juice", 
				"Cranberry Juice", "Grapefruit Juice", "Grenadine", "Cream", "Mint Leaves", "Lime", "Lemon",
				"Cherry", "Olive", "Dash of Bitters"};
			
			// Initialize the ingredient's numeric boxes, or what updates the drink vector
			this.ingredientNumericBoxes = new System.Windows.Forms.NumericUpDown[NUM_INGREDIENTS]
				{vodkaIndex, rumIndex, ginIndex, whiskeyIndex, tequilaIndex, sweetVermouthIndex, dryVermouthIndex,
				 tripleSecIndex, cremeMintheIndex, baileysIndex, coffeeLiqueurIndex, cokeIndex, clubSodaIndex,
				 tonicIndex, gingerAleIndex, sourIndex, orangeIndex, pineappleIndex, cranberryIndex, grapefruitIndex,
				 grenadineIndex, creamIndex, mintIndex, limeIndex, lemonIndex, cherryIndex, oliveIndex, bittersIndex};

			// Sets the ingredient numeric boxes to update other properties when their value is changed
			for (int i = 0; i < NUM_INGREDIENTS; i++)
			{
				this.ingredientNumericBoxes[i].ValueChanged += 
					new System.EventHandler(this.drinkIndex_ValueChanged);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.loadMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.quitMenuItem = new System.Windows.Forms.MenuItem();
			this.clearRecipeBtn = new System.Windows.Forms.Button();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.ingrTab = new System.Windows.Forms.TabControl();
			this.hardAlcoholTab = new System.Windows.Forms.TabPage();
			this.tequilaLabel = new System.Windows.Forms.Label();
			this.tequilaIndex = new System.Windows.Forms.NumericUpDown();
			this.whiskeyLabel = new System.Windows.Forms.Label();
			this.whiskeyIndex = new System.Windows.Forms.NumericUpDown();
			this.ginLabel = new System.Windows.Forms.Label();
			this.ginIndex = new System.Windows.Forms.NumericUpDown();
			this.rumLabel = new System.Windows.Forms.Label();
			this.rumIndex = new System.Windows.Forms.NumericUpDown();
			this.vodkaLabel = new System.Windows.Forms.Label();
			this.vodkaIndex = new System.Windows.Forms.NumericUpDown();
			this.alcoholicMixersTab = new System.Windows.Forms.TabPage();
			this.coffeeLiqueurIndex = new System.Windows.Forms.NumericUpDown();
			this.coffeeLiqueurLabel = new System.Windows.Forms.Label();
			this.baileysLabel = new System.Windows.Forms.Label();
			this.baileysIndex = new System.Windows.Forms.NumericUpDown();
			this.cremeMintheLabel = new System.Windows.Forms.Label();
			this.cremeMintheIndex = new System.Windows.Forms.NumericUpDown();
			this.tripleSecLabel = new System.Windows.Forms.Label();
			this.tripleSecIndex = new System.Windows.Forms.NumericUpDown();
			this.dryVermouthLabel = new System.Windows.Forms.Label();
			this.dryVermouthIndex = new System.Windows.Forms.NumericUpDown();
			this.sweetVermouthLabel = new System.Windows.Forms.Label();
			this.sweetVermouthIndex = new System.Windows.Forms.NumericUpDown();
			this.nonalcoholicMixersTab = new System.Windows.Forms.TabPage();
			this.sourLabel = new System.Windows.Forms.Label();
			this.sourIndex = new System.Windows.Forms.NumericUpDown();
			this.gingerAleLabel = new System.Windows.Forms.Label();
			this.gingerAleIndex = new System.Windows.Forms.NumericUpDown();
			this.tonicLabel = new System.Windows.Forms.Label();
			this.tonicIndex = new System.Windows.Forms.NumericUpDown();
			this.clubSodaLabel = new System.Windows.Forms.Label();
			this.clubSodaIndex = new System.Windows.Forms.NumericUpDown();
			this.cokeLabel = new System.Windows.Forms.Label();
			this.cokeIndex = new System.Windows.Forms.NumericUpDown();
			this.juicesTab = new System.Windows.Forms.TabPage();
			this.creamIndex = new System.Windows.Forms.NumericUpDown();
			this.creamLabel = new System.Windows.Forms.Label();
			this.grenadineLabel = new System.Windows.Forms.Label();
			this.grenadineIndex = new System.Windows.Forms.NumericUpDown();
			this.grapefruitLabel = new System.Windows.Forms.Label();
			this.grapefruitIndex = new System.Windows.Forms.NumericUpDown();
			this.cranberryLabel = new System.Windows.Forms.Label();
			this.cranberryIndex = new System.Windows.Forms.NumericUpDown();
			this.pineappleLabel = new System.Windows.Forms.Label();
			this.pineappleIndex = new System.Windows.Forms.NumericUpDown();
			this.orangeLabel = new System.Windows.Forms.Label();
			this.orangeIndex = new System.Windows.Forms.NumericUpDown();
			this.garnishTab = new System.Windows.Forms.TabPage();
			this.bittersIndex = new System.Windows.Forms.NumericUpDown();
			this.bittersLabel = new System.Windows.Forms.Label();
			this.oliveLabel = new System.Windows.Forms.Label();
			this.oliveIndex = new System.Windows.Forms.NumericUpDown();
			this.cherryLabel = new System.Windows.Forms.Label();
			this.cherryIndex = new System.Windows.Forms.NumericUpDown();
			this.lemonLabel = new System.Windows.Forms.Label();
			this.lemonIndex = new System.Windows.Forms.NumericUpDown();
			this.limeLabel = new System.Windows.Forms.Label();
			this.limeIndex = new System.Windows.Forms.NumericUpDown();
			this.mintLabel = new System.Windows.Forms.Label();
			this.mintIndex = new System.Windows.Forms.NumericUpDown();
			this.recipeTextBox = new System.Windows.Forms.TextBox();
			this.ratingLabel = new System.Windows.Forms.Label();
			this.ingrTab.SuspendLayout();
			this.hardAlcoholTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tequilaIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.whiskeyIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ginIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rumIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.vodkaIndex)).BeginInit();
			this.alcoholicMixersTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.coffeeLiqueurIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.baileysIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cremeMintheIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tripleSecIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dryVermouthIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sweetVermouthIndex)).BeginInit();
			this.nonalcoholicMixersTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sourIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gingerAleIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tonicIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.clubSodaIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cokeIndex)).BeginInit();
			this.juicesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.creamIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grenadineIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grapefruitIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cranberryIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pineappleIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.orangeIndex)).BeginInit();
			this.garnishTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bittersIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.oliveIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cherryIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lemonIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.limeIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mintIndex)).BeginInit();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.loadMenuItem,
																					  this.menuItem3,
																					  this.quitMenuItem});
			this.menuItem1.Text = "File";
			// 
			// loadMenuItem
			// 
			this.loadMenuItem.Index = 0;
			this.loadMenuItem.Text = "Load BackProp Network";
			this.loadMenuItem.Click += new System.EventHandler(this.loadMenuItem_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "-";
			// 
			// quitMenuItem
			// 
			this.quitMenuItem.Index = 2;
			this.quitMenuItem.Text = "Quit";
			this.quitMenuItem.Click += new System.EventHandler(this.quitMenuItem_Click);
			// 
			// clearRecipeBtn
			// 
			this.clearRecipeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.clearRecipeBtn.Location = new System.Drawing.Point(504, 160);
			this.clearRecipeBtn.Name = "clearRecipeBtn";
			this.clearRecipeBtn.Size = new System.Drawing.Size(264, 40);
			this.clearRecipeBtn.TabIndex = 4;
			this.clearRecipeBtn.Text = "Clear Recipe";
			this.clearRecipeBtn.Click += new System.EventHandler(this.clearRecipeBtn_Click);
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 453);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(792, 22);
			this.statusBar.SizingGrip = false;
			this.statusBar.TabIndex = 5;
			// 
			// ingrTab
			// 
			this.ingrTab.Controls.Add(this.hardAlcoholTab);
			this.ingrTab.Controls.Add(this.alcoholicMixersTab);
			this.ingrTab.Controls.Add(this.nonalcoholicMixersTab);
			this.ingrTab.Controls.Add(this.juicesTab);
			this.ingrTab.Controls.Add(this.garnishTab);
			this.ingrTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ingrTab.Location = new System.Drawing.Point(16, 240);
			this.ingrTab.Name = "ingrTab";
			this.ingrTab.SelectedIndex = 0;
			this.ingrTab.Size = new System.Drawing.Size(760, 192);
			this.ingrTab.TabIndex = 22;
			// 
			// hardAlcoholTab
			// 
			this.hardAlcoholTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hardAlcoholTab.Controls.Add(this.tequilaLabel);
			this.hardAlcoholTab.Controls.Add(this.tequilaIndex);
			this.hardAlcoholTab.Controls.Add(this.whiskeyLabel);
			this.hardAlcoholTab.Controls.Add(this.whiskeyIndex);
			this.hardAlcoholTab.Controls.Add(this.ginLabel);
			this.hardAlcoholTab.Controls.Add(this.ginIndex);
			this.hardAlcoholTab.Controls.Add(this.rumLabel);
			this.hardAlcoholTab.Controls.Add(this.rumIndex);
			this.hardAlcoholTab.Controls.Add(this.vodkaLabel);
			this.hardAlcoholTab.Controls.Add(this.vodkaIndex);
			this.hardAlcoholTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.hardAlcoholTab.Location = new System.Drawing.Point(4, 24);
			this.hardAlcoholTab.Name = "hardAlcoholTab";
			this.hardAlcoholTab.Size = new System.Drawing.Size(752, 164);
			this.hardAlcoholTab.TabIndex = 0;
			this.hardAlcoholTab.Text = "Hard Alcohols";
			// 
			// tequilaLabel
			// 
			this.tequilaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tequilaLabel.Location = new System.Drawing.Point(280, 96);
			this.tequilaLabel.Name = "tequilaLabel";
			this.tequilaLabel.Size = new System.Drawing.Size(112, 24);
			this.tequilaLabel.TabIndex = 63;
			this.tequilaLabel.Text = "Tequila";
			this.tequilaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tequilaIndex
			// 
			this.tequilaIndex.DecimalPlaces = 2;
			this.tequilaIndex.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   65536});
			this.tequilaIndex.Location = new System.Drawing.Point(416, 96);
			this.tequilaIndex.Name = "tequilaIndex";
			this.tequilaIndex.Size = new System.Drawing.Size(64, 21);
			this.tequilaIndex.TabIndex = 62;
			// 
			// whiskeyLabel
			// 
			this.whiskeyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.whiskeyLabel.Location = new System.Drawing.Point(32, 96);
			this.whiskeyLabel.Name = "whiskeyLabel";
			this.whiskeyLabel.Size = new System.Drawing.Size(104, 24);
			this.whiskeyLabel.TabIndex = 61;
			this.whiskeyLabel.Text = "Whiskey";
			this.whiskeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// whiskeyIndex
			// 
			this.whiskeyIndex.DecimalPlaces = 2;
			this.whiskeyIndex.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   65536});
			this.whiskeyIndex.Location = new System.Drawing.Point(160, 96);
			this.whiskeyIndex.Name = "whiskeyIndex";
			this.whiskeyIndex.Size = new System.Drawing.Size(64, 21);
			this.whiskeyIndex.TabIndex = 60;
			// 
			// ginLabel
			// 
			this.ginLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ginLabel.Location = new System.Drawing.Point(560, 40);
			this.ginLabel.Name = "ginLabel";
			this.ginLabel.Size = new System.Drawing.Size(80, 24);
			this.ginLabel.TabIndex = 59;
			this.ginLabel.Text = "Gin";
			this.ginLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ginIndex
			// 
			this.ginIndex.DecimalPlaces = 2;
			this.ginIndex.Increment = new System.Decimal(new int[] {
																	   5,
																	   0,
																	   0,
																	   65536});
			this.ginIndex.Location = new System.Drawing.Point(664, 40);
			this.ginIndex.Name = "ginIndex";
			this.ginIndex.Size = new System.Drawing.Size(64, 21);
			this.ginIndex.TabIndex = 58;
			// 
			// rumLabel
			// 
			this.rumLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.rumLabel.Location = new System.Drawing.Point(288, 40);
			this.rumLabel.Name = "rumLabel";
			this.rumLabel.Size = new System.Drawing.Size(104, 24);
			this.rumLabel.TabIndex = 57;
			this.rumLabel.Text = "Rum";
			this.rumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// rumIndex
			// 
			this.rumIndex.DecimalPlaces = 2;
			this.rumIndex.Increment = new System.Decimal(new int[] {
																	   5,
																	   0,
																	   0,
																	   65536});
			this.rumIndex.Location = new System.Drawing.Point(416, 40);
			this.rumIndex.Name = "rumIndex";
			this.rumIndex.Size = new System.Drawing.Size(64, 21);
			this.rumIndex.TabIndex = 56;
			// 
			// vodkaLabel
			// 
			this.vodkaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.vodkaLabel.Location = new System.Drawing.Point(32, 40);
			this.vodkaLabel.Name = "vodkaLabel";
			this.vodkaLabel.Size = new System.Drawing.Size(104, 24);
			this.vodkaLabel.TabIndex = 55;
			this.vodkaLabel.Text = "Vodka";
			this.vodkaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// vodkaIndex
			// 
			this.vodkaIndex.DecimalPlaces = 2;
			this.vodkaIndex.Increment = new System.Decimal(new int[] {
																		 5,
																		 0,
																		 0,
																		 65536});
			this.vodkaIndex.Location = new System.Drawing.Point(160, 40);
			this.vodkaIndex.Name = "vodkaIndex";
			this.vodkaIndex.Size = new System.Drawing.Size(64, 21);
			this.vodkaIndex.TabIndex = 54;
			// 
			// alcoholicMixersTab
			// 
			this.alcoholicMixersTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.alcoholicMixersTab.Controls.Add(this.coffeeLiqueurIndex);
			this.alcoholicMixersTab.Controls.Add(this.coffeeLiqueurLabel);
			this.alcoholicMixersTab.Controls.Add(this.baileysLabel);
			this.alcoholicMixersTab.Controls.Add(this.baileysIndex);
			this.alcoholicMixersTab.Controls.Add(this.cremeMintheLabel);
			this.alcoholicMixersTab.Controls.Add(this.cremeMintheIndex);
			this.alcoholicMixersTab.Controls.Add(this.tripleSecLabel);
			this.alcoholicMixersTab.Controls.Add(this.tripleSecIndex);
			this.alcoholicMixersTab.Controls.Add(this.dryVermouthLabel);
			this.alcoholicMixersTab.Controls.Add(this.dryVermouthIndex);
			this.alcoholicMixersTab.Controls.Add(this.sweetVermouthLabel);
			this.alcoholicMixersTab.Controls.Add(this.sweetVermouthIndex);
			this.alcoholicMixersTab.Location = new System.Drawing.Point(4, 24);
			this.alcoholicMixersTab.Name = "alcoholicMixersTab";
			this.alcoholicMixersTab.Size = new System.Drawing.Size(752, 164);
			this.alcoholicMixersTab.TabIndex = 1;
			this.alcoholicMixersTab.Text = "Alcoholic Mixers";
			// 
			// coffeeLiqueurIndex
			// 
			this.coffeeLiqueurIndex.DecimalPlaces = 2;
			this.coffeeLiqueurIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.coffeeLiqueurIndex.Increment = new System.Decimal(new int[] {
																				 5,
																				 0,
																				 0,
																				 65536});
			this.coffeeLiqueurIndex.Location = new System.Drawing.Point(664, 96);
			this.coffeeLiqueurIndex.Name = "coffeeLiqueurIndex";
			this.coffeeLiqueurIndex.Size = new System.Drawing.Size(64, 21);
			this.coffeeLiqueurIndex.TabIndex = 43;
			// 
			// coffeeLiqueurLabel
			// 
			this.coffeeLiqueurLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.coffeeLiqueurLabel.Location = new System.Drawing.Point(528, 96);
			this.coffeeLiqueurLabel.Name = "coffeeLiqueurLabel";
			this.coffeeLiqueurLabel.Size = new System.Drawing.Size(112, 24);
			this.coffeeLiqueurLabel.TabIndex = 42;
			this.coffeeLiqueurLabel.Text = "Coffee Liqueur";
			this.coffeeLiqueurLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// baileysLabel
			// 
			this.baileysLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.baileysLabel.Location = new System.Drawing.Point(248, 96);
			this.baileysLabel.Name = "baileysLabel";
			this.baileysLabel.Size = new System.Drawing.Size(144, 24);
			this.baileysLabel.TabIndex = 41;
			this.baileysLabel.Text = "Bailey\'s Irish Cream";
			this.baileysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// baileysIndex
			// 
			this.baileysIndex.DecimalPlaces = 2;
			this.baileysIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.baileysIndex.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   65536});
			this.baileysIndex.Location = new System.Drawing.Point(416, 96);
			this.baileysIndex.Name = "baileysIndex";
			this.baileysIndex.Size = new System.Drawing.Size(64, 21);
			this.baileysIndex.TabIndex = 40;
			// 
			// cremeMintheLabel
			// 
			this.cremeMintheLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cremeMintheLabel.Location = new System.Drawing.Point(16, 96);
			this.cremeMintheLabel.Name = "cremeMintheLabel";
			this.cremeMintheLabel.Size = new System.Drawing.Size(120, 24);
			this.cremeMintheLabel.TabIndex = 39;
			this.cremeMintheLabel.Text = "Crème de Minthe";
			this.cremeMintheLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cremeMintheIndex
			// 
			this.cremeMintheIndex.DecimalPlaces = 2;
			this.cremeMintheIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cremeMintheIndex.Increment = new System.Decimal(new int[] {
																			   5,
																			   0,
																			   0,
																			   65536});
			this.cremeMintheIndex.Location = new System.Drawing.Point(160, 96);
			this.cremeMintheIndex.Name = "cremeMintheIndex";
			this.cremeMintheIndex.Size = new System.Drawing.Size(64, 21);
			this.cremeMintheIndex.TabIndex = 38;
			// 
			// tripleSecLabel
			// 
			this.tripleSecLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tripleSecLabel.Location = new System.Drawing.Point(544, 40);
			this.tripleSecLabel.Name = "tripleSecLabel";
			this.tripleSecLabel.Size = new System.Drawing.Size(96, 24);
			this.tripleSecLabel.TabIndex = 37;
			this.tripleSecLabel.Text = "Triple Sec";
			this.tripleSecLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tripleSecIndex
			// 
			this.tripleSecIndex.DecimalPlaces = 2;
			this.tripleSecIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tripleSecIndex.Increment = new System.Decimal(new int[] {
																			 5,
																			 0,
																			 0,
																			 65536});
			this.tripleSecIndex.Location = new System.Drawing.Point(664, 40);
			this.tripleSecIndex.Name = "tripleSecIndex";
			this.tripleSecIndex.Size = new System.Drawing.Size(64, 21);
			this.tripleSecIndex.TabIndex = 36;
			// 
			// dryVermouthLabel
			// 
			this.dryVermouthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dryVermouthLabel.Location = new System.Drawing.Point(280, 40);
			this.dryVermouthLabel.Name = "dryVermouthLabel";
			this.dryVermouthLabel.Size = new System.Drawing.Size(112, 24);
			this.dryVermouthLabel.TabIndex = 35;
			this.dryVermouthLabel.Text = "Dry Vermouth";
			this.dryVermouthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// dryVermouthIndex
			// 
			this.dryVermouthIndex.DecimalPlaces = 2;
			this.dryVermouthIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dryVermouthIndex.Increment = new System.Decimal(new int[] {
																			   5,
																			   0,
																			   0,
																			   65536});
			this.dryVermouthIndex.Location = new System.Drawing.Point(416, 40);
			this.dryVermouthIndex.Name = "dryVermouthIndex";
			this.dryVermouthIndex.Size = new System.Drawing.Size(64, 21);
			this.dryVermouthIndex.TabIndex = 34;
			// 
			// sweetVermouthLabel
			// 
			this.sweetVermouthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.sweetVermouthLabel.Location = new System.Drawing.Point(16, 40);
			this.sweetVermouthLabel.Name = "sweetVermouthLabel";
			this.sweetVermouthLabel.Size = new System.Drawing.Size(120, 24);
			this.sweetVermouthLabel.TabIndex = 33;
			this.sweetVermouthLabel.Text = "Sweet Vermouth";
			this.sweetVermouthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// sweetVermouthIndex
			// 
			this.sweetVermouthIndex.DecimalPlaces = 2;
			this.sweetVermouthIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.sweetVermouthIndex.Increment = new System.Decimal(new int[] {
																				 5,
																				 0,
																				 0,
																				 65536});
			this.sweetVermouthIndex.Location = new System.Drawing.Point(160, 40);
			this.sweetVermouthIndex.Name = "sweetVermouthIndex";
			this.sweetVermouthIndex.Size = new System.Drawing.Size(64, 21);
			this.sweetVermouthIndex.TabIndex = 32;
			// 
			// nonalcoholicMixersTab
			// 
			this.nonalcoholicMixersTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nonalcoholicMixersTab.Controls.Add(this.sourLabel);
			this.nonalcoholicMixersTab.Controls.Add(this.sourIndex);
			this.nonalcoholicMixersTab.Controls.Add(this.gingerAleLabel);
			this.nonalcoholicMixersTab.Controls.Add(this.gingerAleIndex);
			this.nonalcoholicMixersTab.Controls.Add(this.tonicLabel);
			this.nonalcoholicMixersTab.Controls.Add(this.tonicIndex);
			this.nonalcoholicMixersTab.Controls.Add(this.clubSodaLabel);
			this.nonalcoholicMixersTab.Controls.Add(this.clubSodaIndex);
			this.nonalcoholicMixersTab.Controls.Add(this.cokeLabel);
			this.nonalcoholicMixersTab.Controls.Add(this.cokeIndex);
			this.nonalcoholicMixersTab.Location = new System.Drawing.Point(4, 24);
			this.nonalcoholicMixersTab.Name = "nonalcoholicMixersTab";
			this.nonalcoholicMixersTab.Size = new System.Drawing.Size(752, 164);
			this.nonalcoholicMixersTab.TabIndex = 2;
			this.nonalcoholicMixersTab.Text = "Non-alcoholic Mixers";
			// 
			// sourLabel
			// 
			this.sourLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.sourLabel.Location = new System.Drawing.Point(280, 96);
			this.sourLabel.Name = "sourLabel";
			this.sourLabel.Size = new System.Drawing.Size(112, 24);
			this.sourLabel.TabIndex = 53;
			this.sourLabel.Text = "Sour";
			this.sourLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// sourIndex
			// 
			this.sourIndex.DecimalPlaces = 2;
			this.sourIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.sourIndex.Increment = new System.Decimal(new int[] {
																		5,
																		0,
																		0,
																		65536});
			this.sourIndex.Location = new System.Drawing.Point(416, 96);
			this.sourIndex.Name = "sourIndex";
			this.sourIndex.Size = new System.Drawing.Size(64, 21);
			this.sourIndex.TabIndex = 52;
			// 
			// gingerAleLabel
			// 
			this.gingerAleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.gingerAleLabel.Location = new System.Drawing.Point(32, 96);
			this.gingerAleLabel.Name = "gingerAleLabel";
			this.gingerAleLabel.Size = new System.Drawing.Size(104, 24);
			this.gingerAleLabel.TabIndex = 51;
			this.gingerAleLabel.Text = "Ginger Ale";
			this.gingerAleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gingerAleIndex
			// 
			this.gingerAleIndex.DecimalPlaces = 2;
			this.gingerAleIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.gingerAleIndex.Increment = new System.Decimal(new int[] {
																			 5,
																			 0,
																			 0,
																			 65536});
			this.gingerAleIndex.Location = new System.Drawing.Point(160, 96);
			this.gingerAleIndex.Name = "gingerAleIndex";
			this.gingerAleIndex.Size = new System.Drawing.Size(64, 21);
			this.gingerAleIndex.TabIndex = 50;
			// 
			// tonicLabel
			// 
			this.tonicLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tonicLabel.Location = new System.Drawing.Point(560, 40);
			this.tonicLabel.Name = "tonicLabel";
			this.tonicLabel.Size = new System.Drawing.Size(80, 24);
			this.tonicLabel.TabIndex = 49;
			this.tonicLabel.Text = "Tonic";
			this.tonicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tonicIndex
			// 
			this.tonicIndex.DecimalPlaces = 2;
			this.tonicIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tonicIndex.Increment = new System.Decimal(new int[] {
																		 5,
																		 0,
																		 0,
																		 65536});
			this.tonicIndex.Location = new System.Drawing.Point(664, 40);
			this.tonicIndex.Name = "tonicIndex";
			this.tonicIndex.Size = new System.Drawing.Size(64, 21);
			this.tonicIndex.TabIndex = 48;
			// 
			// clubSodaLabel
			// 
			this.clubSodaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.clubSodaLabel.Location = new System.Drawing.Point(288, 40);
			this.clubSodaLabel.Name = "clubSodaLabel";
			this.clubSodaLabel.Size = new System.Drawing.Size(104, 24);
			this.clubSodaLabel.TabIndex = 47;
			this.clubSodaLabel.Text = "Club Soda";
			this.clubSodaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// clubSodaIndex
			// 
			this.clubSodaIndex.DecimalPlaces = 2;
			this.clubSodaIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.clubSodaIndex.Increment = new System.Decimal(new int[] {
																			5,
																			0,
																			0,
																			65536});
			this.clubSodaIndex.Location = new System.Drawing.Point(416, 40);
			this.clubSodaIndex.Name = "clubSodaIndex";
			this.clubSodaIndex.Size = new System.Drawing.Size(64, 21);
			this.clubSodaIndex.TabIndex = 46;
			// 
			// cokeLabel
			// 
			this.cokeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cokeLabel.Location = new System.Drawing.Point(32, 40);
			this.cokeLabel.Name = "cokeLabel";
			this.cokeLabel.Size = new System.Drawing.Size(104, 24);
			this.cokeLabel.TabIndex = 45;
			this.cokeLabel.Text = "Coke";
			this.cokeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cokeIndex
			// 
			this.cokeIndex.DecimalPlaces = 2;
			this.cokeIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cokeIndex.Increment = new System.Decimal(new int[] {
																		5,
																		0,
																		0,
																		65536});
			this.cokeIndex.Location = new System.Drawing.Point(160, 40);
			this.cokeIndex.Name = "cokeIndex";
			this.cokeIndex.Size = new System.Drawing.Size(64, 21);
			this.cokeIndex.TabIndex = 44;
			// 
			// juicesTab
			// 
			this.juicesTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.juicesTab.Controls.Add(this.creamIndex);
			this.juicesTab.Controls.Add(this.creamLabel);
			this.juicesTab.Controls.Add(this.grenadineLabel);
			this.juicesTab.Controls.Add(this.grenadineIndex);
			this.juicesTab.Controls.Add(this.grapefruitLabel);
			this.juicesTab.Controls.Add(this.grapefruitIndex);
			this.juicesTab.Controls.Add(this.cranberryLabel);
			this.juicesTab.Controls.Add(this.cranberryIndex);
			this.juicesTab.Controls.Add(this.pineappleLabel);
			this.juicesTab.Controls.Add(this.pineappleIndex);
			this.juicesTab.Controls.Add(this.orangeLabel);
			this.juicesTab.Controls.Add(this.orangeIndex);
			this.juicesTab.Location = new System.Drawing.Point(4, 24);
			this.juicesTab.Name = "juicesTab";
			this.juicesTab.Size = new System.Drawing.Size(752, 164);
			this.juicesTab.TabIndex = 3;
			this.juicesTab.Text = "Juices, etc.";
			// 
			// creamIndex
			// 
			this.creamIndex.DecimalPlaces = 2;
			this.creamIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.creamIndex.Increment = new System.Decimal(new int[] {
																		 5,
																		 0,
																		 0,
																		 65536});
			this.creamIndex.Location = new System.Drawing.Point(664, 96);
			this.creamIndex.Name = "creamIndex";
			this.creamIndex.Size = new System.Drawing.Size(64, 21);
			this.creamIndex.TabIndex = 55;
			// 
			// creamLabel
			// 
			this.creamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.creamLabel.Location = new System.Drawing.Point(512, 96);
			this.creamLabel.Name = "creamLabel";
			this.creamLabel.Size = new System.Drawing.Size(128, 24);
			this.creamLabel.TabIndex = 54;
			this.creamLabel.Text = "Cream";
			this.creamLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// grenadineLabel
			// 
			this.grenadineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.grenadineLabel.Location = new System.Drawing.Point(248, 96);
			this.grenadineLabel.Name = "grenadineLabel";
			this.grenadineLabel.Size = new System.Drawing.Size(144, 24);
			this.grenadineLabel.TabIndex = 53;
			this.grenadineLabel.Text = "Grenadine";
			this.grenadineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// grenadineIndex
			// 
			this.grenadineIndex.DecimalPlaces = 2;
			this.grenadineIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.grenadineIndex.Increment = new System.Decimal(new int[] {
																			 5,
																			 0,
																			 0,
																			 65536});
			this.grenadineIndex.Location = new System.Drawing.Point(416, 96);
			this.grenadineIndex.Name = "grenadineIndex";
			this.grenadineIndex.Size = new System.Drawing.Size(64, 21);
			this.grenadineIndex.TabIndex = 52;
			// 
			// grapefruitLabel
			// 
			this.grapefruitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.grapefruitLabel.Location = new System.Drawing.Point(16, 96);
			this.grapefruitLabel.Name = "grapefruitLabel";
			this.grapefruitLabel.Size = new System.Drawing.Size(120, 24);
			this.grapefruitLabel.TabIndex = 51;
			this.grapefruitLabel.Text = "Grapefruit Juice";
			this.grapefruitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// grapefruitIndex
			// 
			this.grapefruitIndex.DecimalPlaces = 2;
			this.grapefruitIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.grapefruitIndex.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  65536});
			this.grapefruitIndex.Location = new System.Drawing.Point(160, 96);
			this.grapefruitIndex.Name = "grapefruitIndex";
			this.grapefruitIndex.Size = new System.Drawing.Size(64, 21);
			this.grapefruitIndex.TabIndex = 50;
			// 
			// cranberryLabel
			// 
			this.cranberryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cranberryLabel.Location = new System.Drawing.Point(512, 40);
			this.cranberryLabel.Name = "cranberryLabel";
			this.cranberryLabel.Size = new System.Drawing.Size(128, 24);
			this.cranberryLabel.TabIndex = 49;
			this.cranberryLabel.Text = "Cranberry Juice";
			this.cranberryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cranberryIndex
			// 
			this.cranberryIndex.DecimalPlaces = 2;
			this.cranberryIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cranberryIndex.Increment = new System.Decimal(new int[] {
																			 5,
																			 0,
																			 0,
																			 65536});
			this.cranberryIndex.Location = new System.Drawing.Point(664, 40);
			this.cranberryIndex.Name = "cranberryIndex";
			this.cranberryIndex.Size = new System.Drawing.Size(64, 21);
			this.cranberryIndex.TabIndex = 48;
			// 
			// pineappleLabel
			// 
			this.pineappleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.pineappleLabel.Location = new System.Drawing.Point(248, 40);
			this.pineappleLabel.Name = "pineappleLabel";
			this.pineappleLabel.Size = new System.Drawing.Size(144, 24);
			this.pineappleLabel.TabIndex = 47;
			this.pineappleLabel.Text = "Pineapple Juice";
			this.pineappleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pineappleIndex
			// 
			this.pineappleIndex.DecimalPlaces = 2;
			this.pineappleIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.pineappleIndex.Increment = new System.Decimal(new int[] {
																			 5,
																			 0,
																			 0,
																			 65536});
			this.pineappleIndex.Location = new System.Drawing.Point(416, 40);
			this.pineappleIndex.Name = "pineappleIndex";
			this.pineappleIndex.Size = new System.Drawing.Size(64, 21);
			this.pineappleIndex.TabIndex = 46;
			// 
			// orangeLabel
			// 
			this.orangeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.orangeLabel.Location = new System.Drawing.Point(16, 40);
			this.orangeLabel.Name = "orangeLabel";
			this.orangeLabel.Size = new System.Drawing.Size(120, 24);
			this.orangeLabel.TabIndex = 45;
			this.orangeLabel.Text = "Orange Juice";
			this.orangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// orangeIndex
			// 
			this.orangeIndex.DecimalPlaces = 2;
			this.orangeIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.orangeIndex.Increment = new System.Decimal(new int[] {
																		  5,
																		  0,
																		  0,
																		  65536});
			this.orangeIndex.Location = new System.Drawing.Point(160, 40);
			this.orangeIndex.Name = "orangeIndex";
			this.orangeIndex.Size = new System.Drawing.Size(64, 21);
			this.orangeIndex.TabIndex = 44;
			// 
			// garnishTab
			// 
			this.garnishTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.garnishTab.Controls.Add(this.bittersIndex);
			this.garnishTab.Controls.Add(this.bittersLabel);
			this.garnishTab.Controls.Add(this.oliveLabel);
			this.garnishTab.Controls.Add(this.oliveIndex);
			this.garnishTab.Controls.Add(this.cherryLabel);
			this.garnishTab.Controls.Add(this.cherryIndex);
			this.garnishTab.Controls.Add(this.lemonLabel);
			this.garnishTab.Controls.Add(this.lemonIndex);
			this.garnishTab.Controls.Add(this.limeLabel);
			this.garnishTab.Controls.Add(this.limeIndex);
			this.garnishTab.Controls.Add(this.mintLabel);
			this.garnishTab.Controls.Add(this.mintIndex);
			this.garnishTab.Location = new System.Drawing.Point(4, 24);
			this.garnishTab.Name = "garnishTab";
			this.garnishTab.Size = new System.Drawing.Size(752, 164);
			this.garnishTab.TabIndex = 4;
			this.garnishTab.Text = "Flavoring & Garnishes";
			// 
			// bittersIndex
			// 
			this.bittersIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.bittersIndex.Location = new System.Drawing.Point(664, 96);
			this.bittersIndex.Name = "bittersIndex";
			this.bittersIndex.Size = new System.Drawing.Size(64, 21);
			this.bittersIndex.TabIndex = 67;
			// 
			// bittersLabel
			// 
			this.bittersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.bittersLabel.Location = new System.Drawing.Point(512, 96);
			this.bittersLabel.Name = "bittersLabel";
			this.bittersLabel.Size = new System.Drawing.Size(128, 24);
			this.bittersLabel.TabIndex = 66;
			this.bittersLabel.Text = "Bitters";
			this.bittersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// oliveLabel
			// 
			this.oliveLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.oliveLabel.Location = new System.Drawing.Point(248, 96);
			this.oliveLabel.Name = "oliveLabel";
			this.oliveLabel.Size = new System.Drawing.Size(144, 24);
			this.oliveLabel.TabIndex = 65;
			this.oliveLabel.Text = "Olive";
			this.oliveLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// oliveIndex
			// 
			this.oliveIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.oliveIndex.Location = new System.Drawing.Point(416, 96);
			this.oliveIndex.Name = "oliveIndex";
			this.oliveIndex.Size = new System.Drawing.Size(64, 21);
			this.oliveIndex.TabIndex = 64;
			// 
			// cherryLabel
			// 
			this.cherryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cherryLabel.Location = new System.Drawing.Point(16, 96);
			this.cherryLabel.Name = "cherryLabel";
			this.cherryLabel.Size = new System.Drawing.Size(120, 24);
			this.cherryLabel.TabIndex = 63;
			this.cherryLabel.Text = "Cherry";
			this.cherryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cherryIndex
			// 
			this.cherryIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cherryIndex.Location = new System.Drawing.Point(160, 96);
			this.cherryIndex.Name = "cherryIndex";
			this.cherryIndex.Size = new System.Drawing.Size(64, 21);
			this.cherryIndex.TabIndex = 62;
			// 
			// lemonLabel
			// 
			this.lemonLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lemonLabel.Location = new System.Drawing.Point(512, 40);
			this.lemonLabel.Name = "lemonLabel";
			this.lemonLabel.Size = new System.Drawing.Size(128, 24);
			this.lemonLabel.TabIndex = 61;
			this.lemonLabel.Text = "Lemon";
			this.lemonLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lemonIndex
			// 
			this.lemonIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lemonIndex.Location = new System.Drawing.Point(664, 40);
			this.lemonIndex.Name = "lemonIndex";
			this.lemonIndex.Size = new System.Drawing.Size(64, 21);
			this.lemonIndex.TabIndex = 60;
			// 
			// limeLabel
			// 
			this.limeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.limeLabel.Location = new System.Drawing.Point(248, 40);
			this.limeLabel.Name = "limeLabel";
			this.limeLabel.Size = new System.Drawing.Size(144, 24);
			this.limeLabel.TabIndex = 59;
			this.limeLabel.Text = "Lime";
			this.limeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// limeIndex
			// 
			this.limeIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.limeIndex.Location = new System.Drawing.Point(416, 40);
			this.limeIndex.Name = "limeIndex";
			this.limeIndex.Size = new System.Drawing.Size(64, 21);
			this.limeIndex.TabIndex = 58;
			// 
			// mintLabel
			// 
			this.mintLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.mintLabel.Location = new System.Drawing.Point(16, 40);
			this.mintLabel.Name = "mintLabel";
			this.mintLabel.Size = new System.Drawing.Size(120, 24);
			this.mintLabel.TabIndex = 57;
			this.mintLabel.Text = "Mint";
			this.mintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// mintIndex
			// 
			this.mintIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.mintIndex.Location = new System.Drawing.Point(160, 40);
			this.mintIndex.Name = "mintIndex";
			this.mintIndex.Size = new System.Drawing.Size(64, 21);
			this.mintIndex.TabIndex = 56;
			// 
			// recipeTextBox
			// 
			this.recipeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.recipeTextBox.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.recipeTextBox.Location = new System.Drawing.Point(24, 24);
			this.recipeTextBox.Multiline = true;
			this.recipeTextBox.Name = "recipeTextBox";
			this.recipeTextBox.ReadOnly = true;
			this.recipeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.recipeTextBox.Size = new System.Drawing.Size(440, 176);
			this.recipeTextBox.TabIndex = 23;
			this.recipeTextBox.Text = "No current recipe";
			// 
			// ratingLabel
			// 
			this.ratingLabel.BackColor = System.Drawing.SystemColors.Window;
			this.ratingLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ratingLabel.Font = new System.Drawing.Font("Trebuchet MS", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ratingLabel.Location = new System.Drawing.Point(504, 24);
			this.ratingLabel.Name = "ratingLabel";
			this.ratingLabel.Size = new System.Drawing.Size(264, 104);
			this.ratingLabel.TabIndex = 24;
			this.ratingLabel.Text = "Rating: ";
			this.ratingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Form
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(792, 475);
			this.Controls.Add(this.ratingLabel);
			this.Controls.Add(this.recipeTextBox);
			this.Controls.Add(this.ingrTab);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.clearRecipeBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximumSize = new System.Drawing.Size(800, 528);
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(800, 528);
			this.Name = "Form";
			this.Text = "BP Bartender";
			this.ingrTab.ResumeLayout(false);
			this.hardAlcoholTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tequilaIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.whiskeyIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ginIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rumIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.vodkaIndex)).EndInit();
			this.alcoholicMixersTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.coffeeLiqueurIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.baileysIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cremeMintheIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tripleSecIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dryVermouthIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sweetVermouthIndex)).EndInit();
			this.nonalcoholicMixersTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sourIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gingerAleIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tonicIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.clubSodaIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cokeIndex)).EndInit();
			this.juicesTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.creamIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grenadineIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grapefruitIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cranberryIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pineappleIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.orangeIndex)).EndInit();
			this.garnishTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.bittersIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.oliveIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cherryIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lemonIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.limeIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.mintIndex)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form());
		}

		#region MENU ITEMS

		/// <summary>
		/// Loads a backpropagation neural network
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void loadMenuItem_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fdlg = new OpenFileDialog();
			fdlg.Title = "Open Backpropagation Network File" ;
			fdlg.Filter = "Backpropagation file (*.bp)|*.bp" ;
			
			if (fdlg.ShowDialog() == DialogResult.OK)
			{
				this.bpNetwork = new NeuralNets.BackProp(fdlg.FileName);
			}
		}


		/// <summary>
		/// Quits the application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void quitMenuItem_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.Application.Exit();
		}

		#endregion
		
		#region UPDATE METHODS
		
		/// <summary>
		/// Update the rating for a drink.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clearRecipeBtn_Click(object sender, System.EventArgs e)
		{
			for (int i = 0; i < NUM_INGREDIENTS; i++)
			{
				this.ingredientNumericBoxes[i].Value = Convert.ToDecimal(0.0);
			}	

			updateRecipeTextBox();
			updateRating();
		}

		
		/// <summary>
		/// Updates the input drink's vector
		/// </summary>
		private void updateDrinkVector()
		{
			for (int i = 0; i < NUM_INGREDIENTS; i++)
			{
				this.drinkVector[i] = Convert.ToDouble(this.ingredientNumericBoxes[i].Value);
			}
		}

		
		/// <summary>
		/// Update the drink's recipe in a text box.
		/// Provides a easy-to-see version of the drink the user is creating.
		/// </summary>
		private void updateRecipeTextBox()
		{
			this.recipeTextBox.Text = "";
			
			for (int i = 0; i < NUM_INGREDIENTS; i++)
			{
				if (this.drinkVector[i] > 0)
				{
					if (i < NUM_INGREDIENTS - GARNISHES)
					{
						this.recipeTextBox.Text += (this.ingredientNumericBoxes[i].Value
							+ " oz " + this.ingredientNames[i] + System.Environment.NewLine);
					}
					else
					{
						this.recipeTextBox.Text += (this.ingredientNumericBoxes[i].Value
							+ " " + this.ingredientNames[i] + System.Environment.NewLine);
					}
				}
			}
		}
		
		
		/// <summary>
		/// Update the rating of the drink by running it through the current neural network
		/// </summary>
		private void updateRating()
		{
			// Proportionalize the drink vector and update proportions accordingly
			this.drinkVector = propDrink(this.drinkVector);
			this.drinkVector = updateFeatures(this.drinkVector);

			double[] output = bpNetwork.Run(new ArrayList(this.drinkVector));
			ratingLabel.Text = "Rating: " + output[0].ToString("#0.000");
		}
		

		/// <summary>
		/// Updates the drink vector and written recipe.
		/// Also automatically updates the rating if the proper checkbox is checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void drinkIndex_ValueChanged(object sender, System.EventArgs e)
		{
			updateDrinkVector();
			updateRecipeTextBox();
			updateRating();
		}

		#endregion
	
		#region INPUT HANDLING
		
		/// <summary>
		/// Parses a input string
		/// </summary>
		/// <param name="input">Input string to parse</param>
		/// <returns>The parsed, string array of data</returns>
		private string[] parseInput(string input)
		{
			// Remove whitespace from the string
			char[] sepr = new char[2] {' ', '\t'};
			string[] parsed = input.Split(sepr);

			// More removing of whitespace
			ArrayList tmp_input = new ArrayList();
			foreach (string s in parsed)
			{
				if (s != "")
					tmp_input.Add(s);
			}

			parsed = (string[])tmp_input.ToArray(typeof(string));

			return parsed;
		}

		
		/// <summary>
		/// Short for "proportionalize drink"
		/// This will take a drink vector we have and change them from ounces into proportions.
		/// If something is 1.5 oz gin, 1.5 oz tonic it will now be 0.5 and 0.5.
		/// 
		/// The last 6 ingredients are important because they are discrete (Mint, Lime, Lemon, Cherry, Olive, Bitters).
		/// For these, we calculate the total proportion of the volume drinks first, and then for each discrete
		/// ingredient we treat it as being 0.05 of the volume. Each of these ingredients is important and
		/// enhances the flavor, so for now I believe 5% of a drink is a worthy amount. 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private double[] propDrink(double[] input)
		{
			const double enhancerWt = 0.05;

			double[] propInput = (double[])input.Clone();

			int volIngrs = NUM_INGREDIENTS - 6;
			double totalVol = 0.0;

			// Get the total volume of ingredients
			for (int i = 0; i < volIngrs; i++)
			{
				totalVol += (double)input[i];
			}

			// Get the number of enhancers
			int numEnhancers = 0;
			for (int i = volIngrs; i < NUM_INGREDIENTS; i++)
			{
				numEnhancers += Convert.ToInt32(input[i]);
			}

			// Increase the volume based on the number of enhancers
			totalVol = totalVol + (numEnhancers * enhancerWt * totalVol);
			
			// Add the proportionalized ingredients
			for (int i = 0; i < volIngrs; i++)
			{
				propInput[i] = input[i] / totalVol;
			}

			// Add the proportionalized enhancers
			for (int i = volIngrs; i < NUM_INGREDIENTS; i++)
			{
				if (Convert.ToInt32(input[i]) > 0)
					propInput[i] = enhancerWt;
				else
					propInput[i] = 0.00;
			}

			return propInput;
		}

		
		/// <summary>
		/// Updates more features to the input:
		///  * Proportion of alcohol in the drink [0,1]
		///  * Number of alcohols 
		///  * Number of mixers
		///  * Number of ingredients
		/// </summary>
		/// <param name="input"></param>
		double[] updateFeatures(double[] input)
		{
			const int NUM_HARD_ALCS = 5;
			const int NUM_ALC_MIXERS = 6;
			const int NUM_NONALC_MIXERS = 11;
			const int NUM_GARNISHES = 6;
			
			// Get proportions
			double propHardAlc = 0.0;
			double propAlcMixers = 0.0;
			double propNonAlcMixers = 0.0;
			
			// Get numeric values
			double numAlcIngrs = 0.0;
			double numNonAlcIngrs = 0.0;
			double numIngrs = 0.0;
			
			// Hard alcohol
			for (int i = 0; i < NUM_HARD_ALCS; i++)
			{
				double currInput = Convert.ToDouble(input[i]);
				
				if (currInput > 0)
				{
					propHardAlc += currInput;
					numAlcIngrs++;
					numIngrs++;
				}
			}

			// Alcoholic mixers
			for (int i = NUM_HARD_ALCS; i < NUM_HARD_ALCS + NUM_ALC_MIXERS; i++)
			{
				double currInput = Convert.ToDouble(input[i]);
				
				if (currInput > 0)
				{
					propAlcMixers += currInput;
					numAlcIngrs++;
					numIngrs++;
				}
			}

			// Non-alcoholic mixers	
			for (int i = NUM_HARD_ALCS + NUM_ALC_MIXERS; i < NUM_HARD_ALCS + NUM_ALC_MIXERS + NUM_NONALC_MIXERS; i++)
			{
				double currInput = Convert.ToDouble(input[i]);
				
				if (currInput > 0)
				{
					propNonAlcMixers += currInput;
					numNonAlcIngrs++;
					numIngrs++;
				}
			}

			// Garnishes
			for (int i = NUM_HARD_ALCS + NUM_ALC_MIXERS + NUM_NONALC_MIXERS; i < NUM_INGREDIENTS; i++)
			{
				double currInput = Convert.ToDouble(input[i]);
				
				if (currInput > 0)
				{
					numNonAlcIngrs++;
					numIngrs++;
				}
			}

			// Add the proportion of hard alcohol
			input[NUM_INGREDIENTS] = propHardAlc;

			// Add the proportion of alcoholic mixers
			input[NUM_INGREDIENTS + 1] = propAlcMixers;

			// Add the proportion of non-alcoholic mixers
			input[NUM_INGREDIENTS + 2] = propNonAlcMixers;

			// Add the number of alcoholic ingredients
			input[NUM_INGREDIENTS + 3] = numAlcIngrs;

			// Add the number of non-alcoholic ingredients
			input[NUM_INGREDIENTS + 4] = numNonAlcIngrs;

			// Add the number of ingredients
			input[NUM_INGREDIENTS + 5] = numIngrs;

			return input;
		}

		#endregion	
	}
}
