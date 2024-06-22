using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crossword_generator
{
    public partial class Clues : Form
    {
        public DataGridView clueTable;

        public Clues()
        {
            InitializeComponent();
            clueTable = new DataGridView();
            clueTable.Dock = DockStyle.Fill;
            clueTable.ColumnCount = 3;
            clueTable.Columns[0].Name = "Number";
            clueTable.Columns[1].Name = "Direction";
            clueTable.Columns[2].Name = "Clue";
            this.Controls.Add(clueTable);
        }

        private void Clues_Load(object sender, EventArgs e)
        {

        }
    }
}
