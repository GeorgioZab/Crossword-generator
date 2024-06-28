using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Crossword_Generator
{
    public partial class CrossGen : Form
    {
        Random rnd = new Random();

        // Создаём окно подсказок
        Clues clue_window = new Clues();

        // Список слов
        List<id_cells> idCells = new List<id_cells>();

        // Путь до импортируемого списка слов
        String listOfWords_file;

        // Пути до вспомогательных файлов
        public String link_1 = Application.StartupPath + "\\Вспомогательные ссылки\\Руководство пользователя.chm";
        public String link_2 = Application.StartupPath + "\\Вспомогательные ссылки\\Справочная служба.chm";

        public CrossGen()
        {
            InitializeComponent();
        }

        public CrossGen(String path)
        {
            listOfWords_file = path;

            InitializeComponent();

            BuildWordList();


            // Событие возникающее при закрывании окна // Обработчик события можно найти ближе к концу файла в разделе "ВЕРХНЕЕ МЕНЮ"
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
        }

        //
        // ОСНОВНЫЕ МЕТОДЫ АЛГОРИТМА СОЗДАНИЯ КРОССВОРДА
        //

        // Формирование списка слов из файла
        private void BuildWordList()
        {
            if (File.Exists(listOfWords_file))
            {
                idCells.Clear();

                using (StreamReader s = new StreamReader(listOfWords_file))
                {
                    string line;
                    while ((line = s.ReadLine()) != null)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 2)
                        {
                            string word = parts[0].Trim();
                            string clue = parts[1].Trim();
                            idCells.Add(new id_cells(0, 0, "", "", word, clue));
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Список слов не найден: " + listOfWords_file);
            }
        }




        private void CrossGen_Load(object sender, EventArgs e)
        {
            // Создаётся доска
            InitBoard();
            clue_window.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
            clue_window.StartPosition = FormStartPosition.Manual;

            clue_window.Show();
            clue_window.clue_table.AutoResizeColumns();
        }

        private void InitBoard()
        {
            crossword.BackgroundColor = Color.Black;
            crossword.DefaultCellStyle.BackColor = Color.Black;

            for (int i = 0; i < 21; i++)
            {
                crossword.Rows.Add();
            }

            foreach (DataGridViewColumn c in crossword.Columns)
            {
                c.Width = crossword.Width / crossword.Columns.Count;
            }

            foreach (DataGridViewRow r in crossword.Rows)
            {
                r.Height = crossword.Height / crossword.Rows.Count;
            }

            GenerateCrosswordPuzzle();
            crossword.CellValueChanged += board_CellValueChanged;
        }

        public void GenerateCrosswordPuzzle()
        {
            int wordCount = 0;
            int numbersCount = 0;

            while (wordCount < 10 || numbersCount != wordCount)
            {
                crossword.Rows.Clear();
                crossword.Columns.Clear();
                clue_window.clue_table.Rows.Clear();

                for (int i = 0; i < 21; i++)
                {
                    crossword.Columns.Add(new DataGridViewTextBoxColumn());
                    crossword.Rows.Add();
                }

                foreach (DataGridViewColumn c in crossword.Columns)
                {
                    c.Width = crossword.Width / crossword.Columns.Count;
                }

                foreach (DataGridViewRow r in crossword.Rows)
                {
                    r.Height = crossword.Height / crossword.Rows.Count;
                }

                idCells = idCells.OrderBy(x => rnd.Next()).ToList(); // Shuffle word list
                wordCount = 0;
                int wordNumber = 1;

                // Place the first word in a random position
                id_cells firstWord = idCells.First();
                int startRow = rnd.Next(0, crossword.Rows.Count - 1);
                int startCol = rnd.Next(0, crossword.Columns.Count - firstWord.word.Length);
                bool direction = true;
                PlaceWord(startRow, startCol, firstWord.word, direction, wordNumber.ToString());
                clue_window.clue_table.Rows.Add(new String[] { wordNumber.ToString(), direction ? "ГОРИЗОНТАЛЬНО" : "ВЕРТИКАЛЬНО", firstWord.clue });
                wordNumber++;
                wordCount++;

                // List of placed words with their positions and directions
                List<id_cells> placedWords = new List<id_cells> { new id_cells(startCol, startRow, direction ? "ГОРИЗОНТАЛЬНО" : "ВЕРТИКАЛЬНО", wordNumber.ToString(), firstWord.word, firstWord.clue) };

                // Iterate over the remaining words and try to place them
                foreach (id_cells i in idCells.Skip(1))
                {
                    bool placed = false;

                    // Try to place the new word by intersecting with all previously placed words
                    foreach (id_cells placedWord in placedWords)
                    {
                        for (int j = 0; j < placedWord.word.Length; j++)
                        {
                            for (int k = 0; k < i.word.Length; k++)
                            {
                                if (placedWord.word[j] == i.word[k])
                                {
                                    int maxAttempts = 100;
                                    int attempts = 0;

                                    while (attempts < maxAttempts)
                                    {
                                        direction = rnd.Next(2) == 0; // Random direction (horizontal or vertical)
                                        startRow = placedWord.direction == "ГОРИЗОНТАЛЬНО" ? placedWord.Y + j : placedWord.Y - k;
                                        startCol = placedWord.direction == "ГОРИЗОНТАЛЬНО" ? placedWord.X - k : placedWord.X + j;

                                        if (CanPlaceWord(startRow, startCol, i.word.Length, direction, i))
                                        {
                                            PlaceWord(startRow, startCol, i.word, direction, wordNumber.ToString());
                                            clue_window.clue_table.Rows.Add(new String[] { wordNumber.ToString(), direction ? "ГОРИЗОНТАЛЬНО" : "ВЕРТИКАЛЬНО", i.clue });
                                            placedWords.Add(new id_cells(startCol, startRow, direction ? "ГОРИЗОНТАЛЬНО" : "ВЕРТИКАЛЬНО", wordNumber.ToString(), i.word, i.clue));
                                            wordNumber++;
                                            wordCount++;
                                            placed = true;
                                            break;
                                        }
                                        attempts++;
                                    }

                                    if (placed) break;
                                }
                            }
                            if (placed) break;
                        }
                        if (placed) break;
                    }

                    if (!placed)
                    {
                        // Log a message if the word could not be placed
                        Console.WriteLine($"Could not place the word: {i.word}");
                    }
                }

                // Clear the board from unnecessary values
                ClearBoardContent();

                // Check if all numbers are visible
                numbersCount = CheckAllNumbersVisible();
            }
        }


        private int CheckAllNumbersVisible()
        {
            int numbersCount = 0;
            foreach (DataGridViewRow row in crossword.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Style.BackColor == Color.White && int.TryParse(cell.Value?.ToString(), out _))
                    {
                        numbersCount++;
                    }
                }
            }
            return numbersCount;
        }

        private void ClearBoardContent()
        {
            foreach (DataGridViewRow row in crossword.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Style.BackColor == Color.White)
                    {
                        if (cell.Value != null && !int.TryParse(cell.Value.ToString(), out _))
                        {
                            cell.Value = null; // Очистка значения ячейки, если это не число
                        }
                    }
                }
            }
        }

        public bool CanPlaceWord(int startRow, int startCol, int length, bool direction, id_cells word)
        {
            bool hasIntersection = false;

            for (int i = 0; i < length; i++)
            {
                int row = direction ? startRow : startRow + i;
                int col = direction ? startCol + i : startCol;

                if (row >= crossword.Rows.Count || col >= crossword.Columns.Count || row < 0 || col < 0)
                {
                    return false;
                }

                if (crossword[col, row].Style.BackColor != Color.Black && crossword[col, row].Tag != null)
                {
                    if (crossword[col, row].Tag.ToString() == word.word[i].ToString())
                    {
                        hasIntersection = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // Проверка на "ловушки"
                if (crossword[col, row].Value != null && crossword[col, row].Value.ToString() == "*")
                {
                    return false;
                }
            }

            if (!hasIntersection) return false;

            // Проверка пустых ячеек перед и после слова
            if (direction)
            {
                if ((startCol > 0 && crossword[startCol - 1, startRow].Style.BackColor == Color.White) ||
                    (startCol + length < crossword.Columns.Count && crossword[startCol + length, startRow].Style.BackColor == Color.White))
                {
                    return false;
                }
            }
            else
            {
                if ((startRow > 0 && crossword[startCol, startRow - 1].Style.BackColor == Color.White) ||
                    (startRow + length < crossword.Rows.Count && crossword[startCol, startRow + length].Style.BackColor == Color.White))
                {
                    return false;
                }
            }

            return true;
        }


        private void PlaceWord(int startRow, int startCol, string word, bool direction, string number)
        {
            // Размещение слова на доске
            for (int i = 0; i < word.Length; i++)
            {
                int row = direction ? startRow : startRow + i;
                int col = direction ? startCol + i : startCol;
                FormatCell(row, col, word[i].ToString());
            }

            // Размещение номера слова в нужном месте
            if (direction)
            {
                if (startCol - 1 >= 0)
                {
                    FormatCell(startRow, startCol - 1, number);
                }
            }
            else
            {
                if (startRow - 1 >= 0)
                {
                    FormatCell(startRow - 1, startCol, number);
                }
            }

            // Добавление "ловушки" после слова (на одну ячейку дальше)
            int trapRow = direction ? startRow : startRow + word.Length;
            int trapCol = direction ? startCol + word.Length : startCol;

            if (trapRow < crossword.Rows.Count && trapCol < crossword.Columns.Count)
            {
                FormatCell(trapRow, trapCol, "*"); // "Ловушка"
            }
        }

        private void FormatCell(int row, int col, string content)
        {
            DataGridViewCell cell = crossword[col, row];

            int number;
            bool isNumber = int.TryParse(content, out number);

            if (isNumber)
            {
                // Формат для номера слова
                cell.Style.BackColor = Color.White;
                cell.ReadOnly = true;
                cell.Style.SelectionBackColor = Color.White;
                cell.Style.ForeColor = Color.Blue; // Установка цвета для номера слова
                cell.Value = content;
            }

            else if (content == "*")
            {
                // Формат для "ловушки"
                cell.Style.BackColor = Color.Black;
                cell.ReadOnly = true; 
                cell.Style.SelectionBackColor = Color.Cyan;
                cell.Style.ForeColor = Color.Black; 
                cell.Value = content;
            }
            else if (string.IsNullOrEmpty(content))
            {
                // Очистка пустых ячеек
                cell.Style.BackColor = Color.Black;
                cell.ReadOnly = false;
                cell.Style.SelectionBackColor = Color.Cyan;
                cell.Style.ForeColor = Color.Black;
                cell.Value = null;
                cell.Tag = null;
            }
            else if (content.Trim().Length > 0)
            {
                // Формат для буквы
                cell.Style.BackColor = Color.White; 
                cell.ReadOnly = false;
                cell.Style.SelectionBackColor = Color.Cyan;
                cell.Style.ForeColor = Color.Black;
                cell.Value = content.ToUpper();
                cell.Tag = content.ToUpper();
            }
        }


        private void CrossGen_LocationChanged(object sender, EventArgs e)
        {
            clue_window.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
        }

        private void board_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (crossword[e.ColumnIndex, e.RowIndex].Value != null)
            {
                string inputLetter = crossword[e.ColumnIndex, e.RowIndex].Value.ToString().ToUpper();
                string correctLetter = crossword[e.ColumnIndex, e.RowIndex].Tag.ToString().ToUpper();

                if (inputLetter == correctLetter)
                {
                    crossword[e.ColumnIndex, e.RowIndex].Style.ForeColor = Color.DarkGreen;
                }
                else
                {
                    crossword[e.ColumnIndex, e.RowIndex].Style.ForeColor = Color.Red;
                }

                crossword[e.ColumnIndex, e.RowIndex].Value = inputLetter; // Изменение значения ячейки на верхний регистр
            }

            CheckCompletion();
        }

        private void CheckCompletion()
        {
            bool allWordsCorrect = true;

            foreach (DataGridViewRow row in crossword.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Style.BackColor == Color.White && cell.Tag != null)
                    {
                        string inputLetter = cell.Value?.ToString()?.ToUpper();
                        if (inputLetter != cell.Tag.ToString().ToUpper())
                        {
                            allWordsCorrect = false;
                            break;
                        }
                    }
                }
                if (!allWordsCorrect) break;
            }

            if (allWordsCorrect)
                MessageBox.Show("Поздравляем! Вы успешно решили кроссворд!", "Победа", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //
        // ВЕРХНЕЕ МЕНЮ
        //

        // [Открыть список слов]
        private void OpenListOfWords_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String path;

            // Инициализируем объект класса OpenFileDialog, задаём фильтр и выбираем путь до файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.StartupPath + "\\Lists";
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

        // [Показать ответы]
        private void ShowAnswers_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Проход по каждой ячейке сетки кроссвордов
            for (int row = 0; row < crossword.Rows.Count; row++)
            {
                for (int col = 0; col < crossword.Columns.Count; col++)
                {
                    var cell = crossword[col, row];
                    if (cell.Style.BackColor == Color.White && cell.Tag != null)
                    {
                        // Установка для значения ячейки правильной буквы, сохраненной в свойстве Tag
                        cell.Value = cell.Tag.ToString();
                        cell.Style.ForeColor = Color.DarkGreen;
                    }
                }
            }
        }

        // [Руководство пользователя]
        private void UsersGuide_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, link_1);
        }
        private void CrossGen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                Help.ShowHelp(this, link_2);
            }
        }

        // [Авторы]
        private void Authors_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Баязитов Эльмир \r\nembaiazitov@edu.hse.ru\n \nЯкутов Георгий\r\ngaiakutov@edu.hse.ru", "Авторы");
        }

        // [x] Кнопка закрытия окна // Обработчик события
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }

    public class id_cells
    {
        public int X;
        public int Y;
        public String direction;
        public String number;
        public String word;
        public String clue;

        public id_cells(int x, int y, string d, string n, string w, string c)
        {
            this.X = x;
            this.Y = y;
            this.direction = d;
            this.number = n;
            this.word = w;
            this.clue = c;
        }
    }
}