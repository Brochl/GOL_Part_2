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
    public partial class To : Form
    {
        public To()
        {
            InitializeComponent();
        }

        public int GetGenerationJump()
        {
            return (int)numericUpDown1.Value;
        }

        public void SetGenerationStart(int number)
        {
            numericUpDown1.Value = number;
        }
    }
}
