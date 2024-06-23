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
        Clues clue_window = new Clues();
        List<id_cells> idCells = new List<id_cells>();
        Random rnd = new Random();
        String puzzle_file;
        public String link_1 = Application.StartupPath + "\\Вспомогательные ссылки\\Руководство пользователя.chm";
        public String link_2 = Application.StartupPath + "\\Вспомогательные ссылки\\Справочная служба.chm";

        public CrossGen()
        {
            puzzle_file = Application.StartupPath + $"\\DataBase\\listOfWords.txt";

            InitializeComponent();

            BuildWordList();
        }

        private void exitButton(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BuildWordList()
        {
            if (File.Exists(puzzle_file))
            {
                String line = "";
                using (StreamReader s = new StreamReader(puzzle_file))
                {
                    line = s.ReadLine();
                    while ((line = s.ReadLine()) != null)
                    {
                        String[] l = line.Split('|');
                        idCells.Add(new id_cells(Int32.Parse(l[0]), Int32.Parse(l[1]), l[2], l[3], l[4], l[5]));
                    }
                }
            }
            else
                MessageBox.Show("Список слов не найден: " + puzzle_file);
        }

        private void CrossGen_Load(object sender, EventArgs e)
        {
            InitBoard();
            clue_window.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
            clue_window.StartPosition = FormStartPosition.Manual;

            clue_window.Show();
            clue_window.clue_table.AutoResizeColumns();
        }

        private void InitBoard()
        {
            board.BackgroundColor = Color.Black;
            board.DefaultCellStyle.BackColor = Color.Black;

            for (int i = 0; i < 21; i++)
            {
                board.Rows.Add();
            }

            foreach (DataGridViewColumn c in board.Columns)
            {
                c.Width = board.Width / board.Columns.Count;
            }

            foreach (DataGridViewRow r in board.Rows)
            {
                r.Height = board.Height / board.Rows.Count;
            }

            GenerateCrosswordPuzzle();
            board.CellValueChanged += board_CellValueChanged;
        }

        private void GenerateCrosswordPuzzle()
        {
            idCells = idCells.OrderBy(x => rnd.Next()).ToList();  // Shuffle the list
            int wordNumber = 1;
            id_cells firstWord = idCells.First();
            int startRow = board.Rows.Count / 2;
            int startCol = board.Columns.Count / 2;
            bool direction = true; // Default direction is horizontal
            PlaceWord(startRow, startCol, firstWord.word, direction, wordNumber.ToString());
            clue_window.clue_table.Rows.Add(new String[] { wordNumber.ToString(), "ГОРИЗОНТАЛЬНО", firstWord.clue });
            wordNumber++;

            List<id_cells> placedWords = new List<id_cells> { new id_cells(startCol, startRow, "ГОРИЗОНТАЛЬНО", wordNumber.ToString(), firstWord.word, firstWord.clue) };

            foreach (id_cells i in idCells.Skip(1))
            {
                bool placed = false;

                foreach (id_cells placedWord in placedWords)
                {
                    for (int j = 0; j < placedWord.word.Length; j++)
                    {
                        for (int k = 0; k < i.word.Length; k++)
                        {
                            if (placedWord.word[j] == i.word[k])
                            {
                                // Try to place vertically
                                if (CanPlaceWord(placedWord.Y + j - k, placedWord.X, i.word.Length, false, i))
                                {
                                    PlaceWord(placedWord.Y + j - k, placedWord.X, i.word, false, wordNumber.ToString());
                                    clue_window.clue_table.Rows.Add(new String[] { wordNumber.ToString(), "ВЕРТИКАЛЬНО", i.clue });
                                    wordNumber++;
                                    placedWords.Add(new id_cells(placedWord.X, placedWord.Y + j - k, "ВЕРТИКАЛЬНО", wordNumber.ToString(), i.word, i.clue));
                                    placed = true;
                                    break;
                                }
                                // Try to place horizontally
                                else if (CanPlaceWord(placedWord.Y, placedWord.X + j - k, i.word.Length, true, i))
                                {
                                    PlaceWord(placedWord.Y, placedWord.X + j - k, i.word, true, wordNumber.ToString());
                                    clue_window.clue_table.Rows.Add(new String[] { wordNumber.ToString(), "ГОРИЗОНТАЛЬНО", i.clue });
                                    wordNumber++;
                                    placedWords.Add(new id_cells(placedWord.X + j - k, placedWord.Y, "ГОРИЗОНТАЛЬНО", wordNumber.ToString(), i.word, i.clue));
                                    placed = true;
                                    break;
                                }
                            }
                        }
                        if (placed) break;
                    }
                    if (placed) break;
                }
                if (!placed)
                {
                    // Log message if word couldn't be placed
                    Console.WriteLine($"Couldn't place word: {i.word}");
                }
            }

            // Очистка доски от лишних значений
            ClearBoardContent();
        }

        private void ClearBoardContent()
        {
            foreach (DataGridViewRow row in board.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Style.BackColor == Color.White)
                    {
                        if (cell.Value != null && !int.TryParse(cell.Value.ToString(), out _))
                        {
                            cell.Value = null; // Clear the cell value if it's not a number
                        }
                    }
                }
            }
        }

        private bool CanPlaceWord(int startRow, int startCol, int length, bool direction, id_cells word)
        {
            for (int i = 0; i < length; i++)
            {
                int row = direction ? startRow : startRow + i;
                int col = direction ? startCol + i : startCol;

                if (row >= board.Rows.Count || col >= board.Columns.Count || row < 0 || col < 0)
                {
                    return false;
                }

                if (board[col, row].Style.BackColor != Color.Black && board[col, row].Tag != null && board[col, row].Tag.ToString() != word.word[i].ToString())
                {
                    return false;
                }

                // Additional check for word intersections
                if (board[col, row].Tag != null && board[col, row].Tag.ToString() != word.word[i].ToString())
                {
                    return false;
                }
            }

            // Ensure there is at least one empty cell before and after the word
            if (direction)
            {
                if ((startCol > 0 && board[startCol - 1, startRow].Style.BackColor == Color.White) ||
                    (startCol + length < board.Columns.Count && board[startCol + length, startRow].Style.BackColor == Color.White))
                {
                    return false;
                }
            }
            else
            {
                if ((startRow > 0 && board[startCol, startRow - 1].Style.BackColor == Color.White) ||
                    (startRow + length < board.Rows.Count && board[startCol, startRow + length].Style.BackColor == Color.White))
                {
                    return false;
                }
            }

            return true;
        }

        private void PlaceWord(int startRow, int startCol, string word, bool direction, string number)
        {
            for (int i = 0; i < word.Length; i++)
            {
                int row = direction ? startRow : startRow + i;
                int col = direction ? startCol + i : startCol;
                FormatCell(row, col, word[i].ToString());
            }

            // Place word number
            int numRow = direction ? startRow : startRow - 1;
            int numCol = direction ? startCol - 1 : startCol;
            if (numRow >= 0 && numCol >= 0 && numRow < board.Rows.Count && numCol < board.Columns.Count)
            {
                FormatCell(numRow, numCol, number); // FormatCell now handles placing numbers
            }
        }


        private void FormatCell(int row, int col, string content)
        {
            DataGridViewCell cell = board[col, row];

            // Check if the content is a number (indicating word number)
            int number;
            bool isNumber = int.TryParse(content, out number);

            if (isNumber)
            {
                // Format for word number
                cell.Style.BackColor = Color.White;
                cell.ReadOnly = true;
                cell.Style.SelectionBackColor = Color.White;
                cell.Style.ForeColor = Color.Blue; // Set color for word number
                cell.Value = content;
            }
            else if (content != null && content.Trim().Length > 0)
            {
                // Format for letter
                cell.Style.BackColor = Color.White;
                cell.ReadOnly = false;
                cell.Style.SelectionBackColor = Color.Cyan;
                cell.Style.ForeColor = Color.Black;
                cell.Value = content.ToUpper(); // Ensure letters are uppercase
                cell.Tag = content.ToUpper(); // Store the correct letter in Tag
            }
            else
            {
                // Clear empty cells
                cell.Style.BackColor = Color.Black; // Set background to black
                cell.ReadOnly = false; // Allow editing for empty cells
                cell.Style.SelectionBackColor = Color.Cyan;
                cell.Style.ForeColor = Color.Black;
                cell.Value = null; // Clear cell value
                cell.Tag = null; // Clear cell tag
            }
        }


        private void CrossGen_LocationChanged(object sender, EventArgs e)
        {
            clue_window.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
        }

        private void board_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (board[e.ColumnIndex, e.RowIndex].Value != null)
            {
                string inputLetter = board[e.ColumnIndex, e.RowIndex].Value.ToString().ToUpper(); // Convert input to uppercase
                string correctLetter = board[e.ColumnIndex, e.RowIndex].Tag.ToString().ToUpper(); // Ensure correct letter is uppercase

                if (inputLetter == correctLetter)
                {
                    board[e.ColumnIndex, e.RowIndex].Style.ForeColor = Color.DarkGreen;
                }
                else
                {
                    board[e.ColumnIndex, e.RowIndex].Style.ForeColor = Color.Red;
                }

                board[e.ColumnIndex, e.RowIndex].Value = inputLetter; // Update the cell value to uppercase
            }

            CheckCompletion(); // Check game completion after each cell value change
        }

        private void CheckCompletion()
        {
            bool allWordsCorrect = true;

            foreach (DataGridViewRow row in board.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Style.BackColor == Color.White && cell.Tag != null) // Check only white cells with tags
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
            {
                MessageBox.Show("Поздравляем! Вы успешно решили кроссворд!", "Победа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close(); // Close the crossword form upon victory
            }
        }



        private void openPuzzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Puzzle Files|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                puzzle_file = ofd.FileName;

                board.Rows.Clear();
                clue_window.clue_table.Rows.Clear();
                idCells.Clear();

                BuildWordList();
                InitBoard();
            }
        }

        private void CrossGen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                Help.ShowHelp(this, link_2);
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Gosha and Almir", "Creators program");
        }

        private void HlpButton_Click_1(object sender, EventArgs e)
        {
            Help.ShowHelp(this, link_1);
        }

        private void usersGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, link_1);
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
