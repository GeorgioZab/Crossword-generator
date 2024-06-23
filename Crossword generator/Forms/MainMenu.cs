using System;
using System.Windows.Forms;

namespace Crossword_Generator
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void MainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {

        }

        // [Начать]
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectDB selectDB = new SelectDB();
            selectDB.Show();
        }

        // [Выход]
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
