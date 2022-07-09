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

        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = true; // start timer running
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {


            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }
    }
}
