using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Crossword_Generator
{
    public partial class SelectList : Form
    {
        String path;
        public SelectList()
        {
            InitializeComponent();

            // Событие возникающее при закрывании окна
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
        }

        // [Имеющийся]
        private void startButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            CrossGen crossGen = new CrossGen(Application.StartupPath + $"\\Lists\\listOfWords.txt");
            crossGen.Show();
        }

        // (iconOfExplorer)
        private void iconOfExplorer_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.StartupPath + "Crossword generator\\Lists";
            openFileDialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string selectedPath = openFileDialog.FileName;

                if (!(Path.GetExtension(selectedPath).Equals(".txt", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Ошибка! Выберите файл с расширением .txt");
                    return;
                }

                this.Hide();
                CrossGen crossGen = new CrossGen(selectedPath);
                crossGen.Show();
            }
        }


        // [Открыть...]
        public void openButton_Click(object sender, EventArgs e)
        {
            // Инициализируем объект класса OpenFileDialog, задаём фильтр и выбираем путь до файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.StartupPath + "Crossword generator\\Lists";
            openFileDialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
            DialogResult result = openFileDialog.ShowDialog();


            path = openFileDialog.FileName;


            // Если путь успешно выбран
            if (result == DialogResult.OK)
            {
                // Проверка на тип файла
                if (!(Path.GetExtension(path).Equals(".txt")))
                {
                    MessageBox.Show("Ошибка! Выберите файл с расширением .txt");
                    return;
                }

                this.Hide();
                CrossGen crossGen = new CrossGen(path);
                crossGen.Show();
            }
        }

        // [Назад]
        private void backButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainMenu mainMenu = new MainMenu();
            mainMenu.Show();
        }

        // Обработчик события "Закрытие окна"
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}