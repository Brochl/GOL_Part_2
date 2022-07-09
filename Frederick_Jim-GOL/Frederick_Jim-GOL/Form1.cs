using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Frederick_Jim_GOL
{
    public partial class Form1 : Form
    {
        #region Variable creation
        bool[,] universe = new bool[20, 20]; // Creating the universe array
        bool[,] sketchPad = new bool[20, 20]; // Creating the sketchPad

        bool hud = true; // true == hud on; false == off;                                       
        bool grid = true; // true == grid on; false == off;
        bool gridx10 = true; // true == gridx10 on; false == off;
        bool neighborCount = true; // true == neighbor counts on; false == off;                 
        bool gameMode = false; // Torodial == true; Finite == false;
        bool timerSwitch = false; // Toggles time on and off
        bool toLoopStopper = true; // use a bool to toggle wether or not we are going to check for a difference in generation

        // Drawing colors
        Color gridColor = Color.Black; // Color of the smaller grid lines                      
        Color gridx10Color = Color.Black; // Color of the larger grid lines                    
        Color cellColor = Color.Black; // Color of the cell when they are alive                
        Color backgroundColor = Color.White; // color of the background                        
        Color hudColor = Color.Red; // Color of the hud in the bottom left hand corner    
        Color deadCell = Color.Red; // Color of number in cell that dies/stays dead next gen   
        Color aliveCell = Color.ForestGreen; // Color of number in cell that lives next gen

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // The Timer class

        // Generation count
        int generations = 0; // Generation count
        int intervals = 20; // Interval count in milliseconds
        int worldX = 30;  // Width of the world
        int worldY = 30; // Height of the world
        int seed = 10;  // Randomizer seed
        int alive = 0; // Keeps track of how many are alive
        int toGenerations = 0; // Generations num for the to method

        string fileName = null; // Name of the last file saved
        string hudPrint = null; // Text displayed by the hud
        #endregion

        #region Mode methods
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0; // alive neighbor count
            int xLen = universe.GetLength(0); // x length
            int yLen = universe.GetLength(1); // y length

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (xOffset == 0 && yOffset == 0) continue; // xOffset and yOffset equal 0

                    if (xCheck < 0) continue; // xCheck less than 0

                    if (yCheck < 0) continue; // yCheck less than 0

                    if (xCheck >= xLen) continue; // xCheck greater than or equal to xLen

                    if (yCheck >= yLen) continue; // yCheck greater than or equal to yLen

                    if (universe[xCheck, yCheck] == true) count++; // if the cell is alive add 1 to count
                }
            }
            return count;
        }

        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0; // alive neighbor count
            int xLen = universe.GetLength(0); // x length
            int yLen = universe.GetLength(1); // y length

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (xOffset == 0 && yOffset == 0) continue; // xOffset & yOffset are equal to 0

                    if (xCheck < 0) xCheck = xLen - 1; // xCheck is less than 0

                    if (yCheck < 0) yCheck = yLen - 1; // yCheck is less than 0

                    if (xCheck >= xLen) xCheck = 0; // if xCheck is greater than or equal to xLen

                    if (yCheck >= yLen) yCheck = 0; // if yCheck is greater than or equal to yLen

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }
        #endregion

        public Form1()
        {
            InitializeComponent();

            #region Load Settings
            hud = Properties.Settings.Default.hud;
            grid = Properties.Settings.Default.grid;
            gridx10 = Properties.Settings.Default.gridx10;
            neighborCount = Properties.Settings.Default.neighborCount;
            gameMode = Properties.Settings.Default.gameMode;
            timerSwitch = Properties.Settings.Default.timerSwitch;

            gridColor = Properties.Settings.Default.gridColor;
            gridx10Color = Properties.Settings.Default.gridx10Color;
            cellColor = Properties.Settings.Default.cellColor;
            backgroundColor = Properties.Settings.Default.backgroundColor;
            hudColor = Properties.Settings.Default.hudColor;
            deadCell = Properties.Settings.Default.deadCell;
            aliveCell = Properties.Settings.Default.aliveCell;

            intervals = Properties.Settings.Default.intervals;
            worldX = Properties.Settings.Default.worldX;
            worldY = Properties.Settings.Default.worldY;
            seed = Properties.Settings.Default.seed;
            #endregion

            // Setup the timer
            timer.Interval = intervals; // set our interval to our intervals variable
            timer.Tick += Timer_Tick;
            timer.Enabled = timerSwitch; // set wether or not the timer is on

            #region Setting Up The Window
            graphicsPanel1.BackColor = backgroundColor; // Setting the background color

            // if the timer is on, turn off the play and next button
            // else, turn off the pause button
            if (timerSwitch == true) { toolStripButton1.Enabled = false; toolStripButton3.Enabled = false; }
            else toolStripButton2.Enabled = false;

            // if neighborCount is true, place a check next to it under the view menu
            if (neighborCount == true) neighborCountToolStripMenuItem.Checked = true;

            // if in toroidal mode place a check on toroidal option under view
            // else you are in finite so place a check on finite option under view
            if (gameMode == true) toroidalToolStripMenuItem.Checked = true;
            else finiteToolStripMenuItem.Checked = true;

            if (grid == true) gridToolStripMenuItem.Checked = true; // if grid is on, place a check on the menu item under view
            if (gridx10 == true) gridX10ToolStripMenuItem.Checked = true; // if gridx10 is on, place a check on the menu item under view

            if (hud == true) HUDToolStripMenuItem.Checked = true; // if hud is on, place a check on the menu item under view

            toolStripStatusLabelIntervals.Text = "Intervals = " + intervals.ToString(); // show how long it takes for the timer to tick
            toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString(); // show the starting seed
            #endregion
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            int count = 0; // neighbor count

            // loop through the universe now to create the next generation
            for (int y = 0; y < universe.GetLength(1); y++)     // Iterate through the universe in the y, top to bottom
            {
                for (int x = 0; x < universe.GetLength(0); x++) // Iterate through the universe in the x, left to right
                {
                    #region Checking Gamemode
                    if (gameMode == true)
                        count = CountNeighborsToroidal(x, y); // Toroidal for gameMode == true
                    else
                        count = CountNeighborsFinite(x, y); // Finite for gameMode == false
                    #endregion

                    #region Game Rules
                    if (universe[x, y] == true)
                    {
                        if (count < 2) { sketchPad[x, y] = false; continue; } // Under-population
                        else if (count > 3) { sketchPad[x, y] = false; continue; } // Over-population
                        else if (count == 2 || count == 3) { sketchPad[x, y] = true; continue; } // Stable population
                    }
                    else if (count == 3) { sketchPad[x, y] = true; continue; } // Reproduction
                    else { sketchPad[x, y] = false; continue; } // Creating a default case as a fall back
                    #endregion
                }
            }
            // copy from the sketchPad to universe
            bool[,] temp = universe;
            universe = sketchPad;
            sketchPad = temp;

            // Increment generation count, then update the status strip
            generations++; 
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

            graphicsPanel1.Invalidate(); // IInvalidate the graphics panel
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
            
            if (generations >= toGenerations && toLoopStopper == false) // if generations is greater than or equal to toGenerations & we are going to a specific generation
            { timer.Enabled = false; toGenerations = 0; toLoopStopper = true; } // turn the timer off, set our destination to 0, & stop looking as we reached our destination
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            alive = 0; // reset our alive varriable to 0

            // Calculate the width and height of each cell in pixels (using a float to avoid weird border gap)
            float cellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0);   // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1); // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y

            Font hudFont = new Font("Aldhabi", 16f);          // A Font for our HUD information

            Pen gridPen = new Pen(gridColor, 1);              // A Pen for drawing the grid lines (color, width)
            Pen gridx10Pen = new Pen(gridx10Color, 3);        // A Pen for drawing the grid lines x10 (color, width)

            Brush cellBrush = new SolidBrush(cellColor);     // A Brush for filling living cells interiors (color)
            Brush deadBrush = new SolidBrush(deadCell);      // A Brush for filling in the number on dead cells
            Brush aliveBrush = new SolidBrush(aliveCell);    // A Brush for filling in the number on alive cells
            Brush hudBrush = new SolidBrush(hudColor);       // A Brush for fillin in the HUD

            StringFormat neighborFormat = new StringFormat();  // Neighbor count alignment, places the number to the center of the cell
            neighborFormat.Alignment = StringAlignment.Center;
            neighborFormat.LineAlignment = StringAlignment.Center;

            StringFormat hudFormat = new StringFormat(); // HUD alignment, places the hud info to the bottom left of the screen
            hudFormat.Alignment = StringAlignment.Near;
            hudFormat.LineAlignment = StringAlignment.Far;

            // Creating the rectangle to display our hud info
            RectangleF HUD = Rectangle.Empty;
            HUD.X = graphicsPanel1.ClientRectangle.X;
            HUD.Y = graphicsPanel1.ClientRectangle.Y;
            HUD.Width = graphicsPanel1.ClientRectangle.Width;
            HUD.Height = graphicsPanel1.ClientRectangle.Height;

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive, incriment alive by 1
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                        alive++;
                    }

                    if (neighborCount == true) // if neighbor count is being displayed
                    {
                        int neighbors = 0; // initializing a temporary neighbor checker

                        if (gameMode == true) // determine which game mode we are in
                            neighbors = CountNeighborsToroidal(x, y); // Toroidal for gameMode == true
                        else
                            neighbors = CountNeighborsFinite(x, y); // Finite for gameMode == false

                        if (neighbors != 0) // if the cell has neighbors
                        {
                            if (universe[x, y] == true && neighbors == 2 || neighbors == 3) // if cell is alive and has 2 or 3 living neighbors use the alive color
                                e.Graphics.DrawString(neighbors.ToString(), graphicsPanel1.Font, aliveBrush, cellRect, neighborFormat);

                            else if (universe[x, y] == false && neighbors == 3) // if cell is dead and has exactly 3 living neighbors use the alive brush
                                e.Graphics.DrawString(neighbors.ToString(), graphicsPanel1.Font, aliveBrush, cellRect, neighborFormat);

                            else    // otherwise use the dead brush
                                e.Graphics.DrawString(neighbors.ToString(), graphicsPanel1.Font, deadBrush, cellRect, neighborFormat);
                        }
                    }
                    // grid is turned on, draw the grid
                    if (grid == true)
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                    // if gridx10 is turned on and its the end of a 10x10 block, draw the x10 grid
                    if (gridx10 == true && x % 10 == 0 && y % 10 == 0)
                        e.Graphics.DrawRectangle(gridx10Pen, cellRect.X, cellRect.Y, cellRect.Width * 10, cellRect.Height * 10);
                }
            }
            toolStripStatusLabelAlive.Text = "Alive = " + alive.ToString(); // Update status strip alive

            if (hud == true) // if hud is turned on, store the following information in hudPrint
            {
                hudPrint = $"Generations = {generations}";      // the generation
                hudPrint = $"{hudPrint}\nCells Alive = {alive}";  // the number of cells alive
                if (!gameMode) hudPrint = $"{hudPrint}\nBoundary Type: Finite"; // if gameMode is false, store the boundary type as finite
                else hudPrint = $"{hudPrint}\nBoundary Type: Toroidal";        // else store the boundary type as toroidal
                hudPrint = $"{hudPrint}\nUniverse Size: Width={worldX}, Height={worldY}"; // finally store the width and height of the world

                e.Graphics.DrawString(hudPrint, hudFont, hudBrush, HUD, hudFormat); // display the information to the user
            }

            #region Disposing of pens and brushes
            gridPen.Dispose();          // as the region states just disposing of the pens and brushes
            gridx10Pen.Dispose();
            cellBrush.Dispose();
            deadBrush.Dispose();
            aliveBrush.Dispose();
            hudBrush.Dispose();
            #endregion
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // After above math is done convert this math to floats to keep mouse clicks clean.
                // Calculate the width and height of each cell in pixels
                float cellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in                                                                                   
                float x = (float)e.X / cellWidth;  // CELL X = MOUSE X / CELL WIDTH
                float y = (float)e.Y / cellHeight; // CELL Y = MOUSE Y / CELL HEIGHT

                // Toggle the cell's state
                universe[(int)x, (int)y] = !universe[(int)x, (int)y];
               
                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }
        #region File Menu Options
        private void New_Click(object sender, EventArgs e) // New Event
        {
            for (int y = 0; y < universe.GetLength(1); y++) // Iterate through the universe in the y, top to bottom
            {
                for (int x = 0; x < universe.GetLength(0); x++) // Iterate through the universe in the x, left to right
                {
                    if (universe[x, y] != false) universe[x, y] = false; // Clean the universe 
                }
            }
            generations = 0; // set generations back to zero

            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString(); // Update status strip generations

            graphicsPanel1.Invalidate(); // Invalidate to repaint everything
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) // Open Event
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                fileName = dlg.FileName; // store the latest file name to use for save function

                // Create a couple variables to calculate the width, height, and y position
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;
                int yPos = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    if (row.StartsWith("!")) continue; // and should be ignored.

                    // If the row is not a comment then it is a row of cells.
                    else if (!row.StartsWith("!")) maxHeight++; // Increment the maxHeight variable for each row read.

                    // Get the length of the current row string
                    maxWidth = row.Length; // and adjust the maxWidth variable if necessary.
                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.
                universe = new bool[maxWidth, maxHeight];
                sketchPad = new bool[maxWidth, maxHeight];

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    if (row.StartsWith("!")) continue; // and should be ignored.

                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos] == 'O') sketchPad[xPos, yPos] = true;

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        else if (row[xPos] == '.') sketchPad[xPos, yPos] = false;
                    }
                    yPos++; // move on to the next row and continue till
                }

                // Close the file.
                reader.Close();

                // copying sketchPad to the universe then invalidating the panel
                bool[,] temp = universe;
                universe = sketchPad;
                sketchPad = temp;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion
    }
}
