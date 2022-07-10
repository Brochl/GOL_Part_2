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
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        #region Get Methods
        public int GetWidth()
        {
            return (int)numericUpDownWidth.Value; // return width value
        }

        public int GetHeight()
        {
            return (int)numericUpDownHeight.Value; // return height value
        }

        public int GetIntervals()
        {
            return (int)numericUpDownMilisecs.Value; // return interval value
        }
        #endregion

        #region Set Methods
        public void SetWidth(int number)
        {
            numericUpDownWidth.Value = number; // set the width value
        }

        public void SetHeight(int number)
        {
            numericUpDownHeight.Value = number; // set the height value
        }

        public void SetIntervals(int number)
        {
            numericUpDownMilisecs.Value = number; // set the interval value
        }
        #endregion
    }
}
