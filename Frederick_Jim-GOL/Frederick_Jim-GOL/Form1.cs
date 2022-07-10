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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) // Save Event
        {
            StreamWriter writer = new StreamWriter(fileName);

            // Write any comments you want to include first.
            // Prefix all comment strings with an exclamation point.
            // Use WriteLine to write the strings to the file. 
            // It appends a CRLF for you.
            writer.WriteLine($"!{DateTime.Now}");

            // Iterate through the universe one row at a time.
            for (int y = 0; y < universe.GetLength(1); y++)
            {

                String currentRow = string.Empty; // Create a string to represent the current row.

                for (int x = 0; x < universe.GetLength(0); x++)  // Iterate through the current row one cell at a time.
                {
                    if (universe[x, y] == true) currentRow += 'O';        // Alive cells == 0
                    else if (universe[x, y] == false) currentRow += '.';  // Dead cells == .
                }

                // Once the current row has been read through and the 
                // string constructed then write it to the file using WriteLine.
                writer.WriteLine(currentRow);
            }

            // After all rows and columns have been written then close the file.
            writer.Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) // Save As Event
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";


            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine($"!{DateTime.Now}");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {

                    String currentRow = string.Empty; // Create a string to represent the current row.

                    for (int x = 0; x < universe.GetLength(0); x++)  // Iterate through the current row one cell at a time.
                    {
                        if (universe[x, y] == true) currentRow += 'O';         // Alive cells == Os
                        else if (universe[x, y] == false) currentRow += '.';   // Dead cells == .s
                    }

                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    writer.WriteLine(currentRow);
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) // Exit Event
        {
            this.Close();
        }
        #endregion

        #region View Menu Options
        private void HUDToolStripMenuItem_Click(object sender, EventArgs e) // Toggle HUD Event
        {
            if (hud == true) // if it is on turn it off and remove the check
            {
                hud = false;
                HUDToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and place the check
            {
                hud = true;
                HUDToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e) // Togglee Neighbor Count Event
        {
            if (neighborCount == true) // if it is on turn it off and remove the check
            {
                neighborCount = false;
                neighborCountToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it is off turn it on and place the check
            {
                neighborCount = true;
                neighborCountToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e) // Toggle Grid Event
        {
            if (grid == true) // if it is on turn it off and remove the check
            {
                grid = false;
                gridToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and add the check
            {
                grid = true;
                gridToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridX10ToolStripMenuItem_Click(object sender, EventArgs e) // Toggle gridx10 Event
        {
            if (gridx10 == true) // if it is on turn it off and remove the check
            {
                gridx10 = false;
                gridX10ToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and add the check
            {
                gridx10 = true;
                gridX10ToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
        }

        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e) // Toroidal Switch Event
        {
            gameMode = true;
            finiteToolStripMenuItem.Checked = false; // uncheck finite
            toroidalToolStripMenuItem.Checked = true; // check toroidal
            graphicsPanel1.Invalidate(); // Don't forget always repaint
        }

        private void finiteToolStripMenuItem_Click(object sender, EventArgs e) // Finite Switch Event
        {
            gameMode = false;
            toroidalToolStripMenuItem.Checked = false; // uncheck toroidal
            finiteToolStripMenuItem.Checked = true; // check finite
            graphicsPanel1.Invalidate(); // ALWAYS repaint
        }
        #endregion

        #region Run Menu Options
        private void playToolStripMenuItem_Click(object sender, EventArgs e) // Play Event
        {
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = true;
            toolStripButton3.Enabled = false;
            timer.Enabled = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e) // Pause Event
        {
            toolStripButton1.Enabled = true;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = true;
            timer.Enabled = false;
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e) // Next Event
        {
            NextGeneration();
        }

        private void toToolStripMenuItem_Click(object sender, EventArgs e) // To Event
        {
            To dlg = new To();
            dlg.SetGenerationStart(generations);

            if (timer.Enabled != false) timer.Enabled = false;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                toGenerations = dlg.GetGenerationJump(); // set our destination in to Generations
                toLoopStopper = false; // turn off the loop stopper
                timer.Enabled = true; // turn our timer on
            }
        }
        #endregion

        #region Randomize Menu Options
        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e) // From Seed Event
        {
            RandomFromSeed dlg = new RandomFromSeed();
            dlg.SetSeed(seed);

            if (DialogResult.OK == dlg.ShowDialog())
            {
                for (int y = 0; y < universe.GetLength(1); y++) // Iterate through the universe in the y, top to bottom
                {
                    for (int x = 0; x < universe.GetLength(0); x++) // Iterate through the universe in the x, left to right
                    {
                        if (universe[x, y] != false) universe[x, y] = false; // Clean the universe 
                    }
                }

                seed = dlg.GetSeed();             // Getting the seed from the dialog window
                Random rand = new Random(seed);  // Getting a random number generator based on our seed
                int temp = 0;                   // Temporary int to store our random number

                for (int y = 0; y < universe.GetLength(1); y++) // loop through the universe through the y
                {
                    for (int x = 0; x < universe.GetLength(0); x++) // loop through the universe through the x
                    {
                        temp = rand.Next(0, 2); // getting a random number between 0 and 2 to get roughly 1/3 alive

                        if (temp == 0) universe[x, y] = true; // if we roll a 0 this cell lives
                    }
                }

                // reset the generation counter and update script
                generations = 0;
                toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

                toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString(); // show the new seed
                graphicsPanel1.Invalidate();
            }
        }

        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e) // From Current Seed Event
        {
            for (int y = 0; y < universe.GetLength(1); y++) // Iterate through the universe in the y, top to bottom
            {
                for (int x = 0; x < universe.GetLength(0); x++) // Iterate through the universe in the x, left to right
                {
                    if (universe[x, y] != false) universe[x, y] = false; // Clean the universe 
                }
            }

            Random rand = new Random(seed); // creating a random generator using our seed
            int temp = 0;                   // create a temporary int to store our random number

            for (int y = 0; y < universe.GetLength(1); y++) // loop through the universe through the y
            {
                for (int x = 0; x < universe.GetLength(0); x++) // loop through the universe through the x
                {
                    temp = rand.Next(0, 2); // getting a random number between 0 and 2 to get roughly 1/3 alive

                    if (temp == 0) { universe[x, y] = true; alive++; } // if we roll a 0 this cell lives and add one person
                }
            }

            generations = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString(); // Update status strip generations

            toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString(); // show the new seed
            graphicsPanel1.Invalidate();
        }

        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e) // From Time Event
        {
            for (int y = 0; y < universe.GetLength(1); y++) // Iterate through the universe in the y, top to bottom
            {
                for (int x = 0; x < universe.GetLength(0); x++) // Iterate through the universe in the x, left to right
                {
                    if (universe[x, y] != false) universe[x, y] = false; // Clean the universe 
                }
            }

            Random rand = new Random(); // create a random number generator from time
            int temp = 0;              // create a temporary int to store our random number
            seed = 404;               // set seed to 404 as you can't get the seed from time

            for (int y = 0; y < universe.GetLength(1); y++) // loop through the universe through the y
            {
                for (int x = 0; x < universe.GetLength(0); x++) // loop through the universe through the x
                {
                    temp = rand.Next(0, 2); // getting a random number between 0 and 2 to get roughly 1/3 alive

                    if (temp == 0) universe[x, y] = true; // if we roll a 0 this cell lives
                }
            }

            generations = 0;  // set generations to 0
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString(); // Update status strip generations

            toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString(); // show the new seed
            graphicsPanel1.Invalidate();
        }
        #endregion

        #region Settings Menu Options
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e) // Background Color Event
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = backgroundColor;        // setting the selected color to background color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                backgroundColor = dlg.Color;     // set the background color to the selected color
                graphicsPanel1.BackColor = backgroundColor; // apply the color change
                graphicsPanel1.Invalidate();    // invalidate the panel
            }
        }

        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e) // Cell Color Event
        {
            ColorDialog dlg = new ColorDialog();   // create a color dialog box
            dlg.Color = cellColor;                // setting the selected color to the cell color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                cellColor = dlg.Color;        // set the cellColor to the selected color
                graphicsPanel1.Invalidate(); // invalidate the panel
            }
        }

        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e) // Grid Color Event
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = gridColor;          // setting the selected color to the gridColor

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                gridColor = dlg.Color;      // set the grid color to the selected color
                graphicsPanel1.Invalidate(); // invalidate the panel
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) // Options Event
        {
            Options dlg = new Options(); // create an Options dialog

            dlg.SetWidth(worldX);        // set the width to the width of the world
            dlg.SetHeight(worldY);       // set the height to the height of the world
            dlg.SetIntervals(intervals); // set the intervals to the speed of the game

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                worldX = dlg.GetWidth();         // set the world width to the width value
                worldY = dlg.GetHeight();        // set the world height to the height value
                intervals = dlg.GetIntervals();  // set the intervals to the intervals value

                if (universe.GetLength(0) != worldX || universe.GetLength(1) != worldY) // if the universe's width and height has been changed
                {
                    universe = new bool[worldX, worldY];    // recreate the universe with the new size
                    sketchPad = new bool[worldX, worldY];   // recreate the sketchPad with the new size
                    generations = 0;                        // reset generations to 0
                    toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString(); // Update status strip generations
                }

                if (timer.Interval != intervals)     // if the intervals have changed
                {
                    timer.Interval = intervals;      // set the timer to our new intervals
                    toolStripStatusLabelIntervals.Text = "Intervals = " + intervals.ToString(); // update how long it takes for the timer to tick
                }
                graphicsPanel1.Invalidate(); // invalidate the panel
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e) // Reset Event
        {
            // resetting the properties
            Properties.Settings.Default.Reset();

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

            // setting the background color
            graphicsPanel1.BackColor = backgroundColor;

            // update intervals and seed toolstrip
            toolStripStatusLabelIntervals.Text = "Intervals = " + intervals.ToString();
            toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString();

            // then invalidate the panel
            graphicsPanel1.Invalidate();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) // Reload Event
        {
            // reloading the properties
            Properties.Settings.Default.Reload();

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

            // setting the background color
            graphicsPanel1.BackColor = backgroundColor;

            // update intervals and seed toolstrip
            toolStripStatusLabelIntervals.Text = "Intervals = " + intervals.ToString();
            toolStripStatusLabelSeed.Text = "Seed = " + seed.ToString();

            // then invalidate the panel
            graphicsPanel1.Invalidate();
        }
        #endregion

        #region ToolStrip Button Options
        private void newToolStripButton_Click(object sender, EventArgs e) // New Event
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

        private void openToolStripButton_Click(object sender, EventArgs e) // Open Event
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
                        if (row[xPos] == 'O') { sketchPad[xPos, yPos] = true; alive++; }

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        else if (row[xPos] == '.') sketchPad[xPos, yPos] = false;
                    }

                    yPos++; // move on to the next row
                }

                // Close the file.
                reader.Close();

                // copy the sketchPad to universe then invalidate the panel
                bool[,] temp = universe;
                universe = sketchPad;
                sketchPad = temp;
                graphicsPanel1.Invalidate();
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e) // save tool strip button
        {
            StreamWriter writer = new StreamWriter(fileName);

            // Write any comments you want to include first.
            // Prefix all comment strings with an exclamation point.
            // Use WriteLine to write the strings to the file. 
            // It appends a CRLF for you.
            writer.WriteLine($"!{DateTime.Now}");

            // Iterate through the universe one row at a time.
            for (int y = 0; y < universe.GetLength(1); y++)
            {

                String currentRow = string.Empty; // Create a string to represent the current row.

                for (int x = 0; x < universe.GetLength(0); x++)  // Iterate through the current row one cell at a time.
                {
                    if (universe[x, y] == true) currentRow += 'O';        // Alive == Os
                    else if (universe[x, y] == false) currentRow += '.';  // Dead == .s
                }

                // Once the current row has been read through and the 
                // string constructed then write it to the file using WriteLine.
                writer.WriteLine(currentRow);
            }

            // After all rows and columns have been written then close the file.
            writer.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e) // Play Event
        {
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = true;
            toolStripButton3.Enabled = false;
            timer.Enabled = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e) // Pause Event
        {
            toolStripButton1.Enabled = true;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = true;
            timer.Enabled = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e) // Next Event
        {
            NextGeneration();
        }

        #endregion

        #region Context Menu Strip (Color)
        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e) // Background 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = backgroundColor;        // setting the selected color to background color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                backgroundColor = dlg.Color;     // set the background color to the selected color
                graphicsPanel1.BackColor = backgroundColor; // apply the color change
                graphicsPanel1.Invalidate();    // invalidate the panel
            }
        }

        private void gridColorToolStripMenuItem1_Click(object sender, EventArgs e) // Grid 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = gridColor;              // setting the selected color to grid color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                gridColor = dlg.Color;              // set the grid color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }

        private void gridx10ColorToolStripMenuItem_Click(object sender, EventArgs e) // Gridx10 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = gridx10Color;              // setting the selected color to gridx10 color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                gridx10Color = dlg.Color;            // set the grid color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }

        private void hUDToolStripMenuItem2_Click(object sender, EventArgs e) // HUD 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = hudColor;              // setting the selected color to hud color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                hudColor = dlg.Color;            // set the hud color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }

        private void cellColorToolStripMenuItem1_Click(object sender, EventArgs e) // Cell 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = cellColor;              // setting the selected color to cell color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                cellColor = dlg.Color;            // set the cell color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }

        private void aliveNumberToolStripMenuItem_Click(object sender, EventArgs e) // Alive 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = aliveCell;              // setting the selected color to alive color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                aliveCell = dlg.Color;            // set the alive color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }

        private void deadNumberToolStripMenuItem_Click(object sender, EventArgs e) // Dead 
        {
            ColorDialog dlg = new ColorDialog(); // create a color dialog box
            dlg.Color = aliveCell;              // setting the selected color to dead color

            if (DialogResult.OK == dlg.ShowDialog()) // if the result == accept
            {
                aliveCell = dlg.Color;            // set the dead color to the selected color
                graphicsPanel1.Invalidate();        // invalidate the panel
            }
        }
        #endregion

        #region Context Menu Strip (View)
        private void hudToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (hud == true) // if it is on turn it off and remove the check
            {
                hud = false;
                HUDToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and place the check
            {
                hud = true;
                HUDToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void neighborsCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (neighborCount == true) // if it is on turn it off and remove the check
            {
                neighborCount = false;
                neighborCountToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it is off turn it on and place the check
            {
                neighborCount = true;
                neighborCountToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (grid == true) // if it is on turn it off and remove the check
            {
                grid = false;
                gridToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and add the check
            {
                grid = true;
                gridToolStripMenuItem.Checked = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridx10ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (gridx10 == true) // if it is on turn it off and remove the check
            {
                gridx10 = false;
                gridX10ToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
            else // if it was off turn it on and add the check
            {
                gridx10 = true;
                gridX10ToolStripMenuItem.Checked = false;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Context Menu Strip (Gameplay)
        private void toroidalToolStripMenuItem1_Click(object sender, EventArgs e) // Toroidal 
        {
            gameMode = true;                          // set the border mode to toroidal
            finiteToolStripMenuItem.Checked = false; // uncheck finite
            toroidalToolStripMenuItem.Checked = true; // check toroidal
            graphicsPanel1.Invalidate(); // Don't forget always repaint
        }

        private void finiteToolStripMenuItem1_Click(object sender, EventArgs e) // Finite 
        {
            gameMode = false;                           // set the border mode to finite
            toroidalToolStripMenuItem.Checked = false; // uncheck toroidal
            finiteToolStripMenuItem.Checked = true; // check finite
            graphicsPanel1.Invalidate(); // ALWAYS repaint
        }
        #endregion

    }
}
