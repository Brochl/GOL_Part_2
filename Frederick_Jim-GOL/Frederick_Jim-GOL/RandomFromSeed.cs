using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Frederick_Jim_GOL
{
    public partial class RandomFromSeed : Form
    {
        public RandomFromSeed()
        {
            InitializeComponent();
        }

        public int GetSeed()
        {
            return (int)numericUpDown1.Value; // return the number the user has chosen
        }

        public void SetSeed(int number)
        {
            numericUpDown1.Value = number; // sets the number in the numericUpDown to the seed
        }

        private void button1_Click(object sender, EventArgs e) // Randomize button event
        {
            Random rand = new Random(); // create a random number generator
            int temp = rand.Next(0, 2147483647); // get a random seed

            numericUpDown1.Value = temp; // update the number in the numericUpDown to the random number
            numericUpDown1.Invalidate(); // invalidate numericUpDown
        }
    }
}
