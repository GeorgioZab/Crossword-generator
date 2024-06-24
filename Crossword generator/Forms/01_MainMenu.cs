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

        // [Начать]
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectList selectDB = new SelectList();
            selectDB.Show();
        }

        // [Выход]
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
