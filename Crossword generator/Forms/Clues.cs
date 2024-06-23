using System.Windows.Forms;

namespace Crossword_Generator
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
            clueTable.Columns[0].Name = "N";
            clueTable.Columns[1].Name = "Direction";
            clueTable.Columns[2].Name = "Clue";
            this.Controls.Add(clueTable);

            // Событие возникающее при закрывании окна
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
        }

        // Обработчик события
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void clue_table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
