using System;
using System.Windows.Forms;

namespace Crossword_Generator
{
    public partial class SelectDB : Form
    {
        // Путь до импортируемого списка слов
        public String listOfWords_file;
        public SelectDB()
        {
            InitializeComponent();
        }

        private void SelectDB_Load(object sender, EventArgs e)
        {

        }

        // [Существующий]
        private void startButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            CrossGen crossGen = new CrossGen();
            crossGen.Show();
        }

        // [Открыть...]
        private void button1_Click(object sender, EventArgs e)
        {

        }

        // [Назад]
        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainMenu mainMenu = new MainMenu();
            mainMenu.Show();
        }
    }
}
