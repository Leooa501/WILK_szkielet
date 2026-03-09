using ClosedXML.Excel;
using WILK.Models;

namespace WILK.Views
{
    public partial class MultipleListsView : Form
    {
        public readonly List<FileEntry> _files = new List<FileEntry>();

        private ListBox? _fileListBox;
        private TextBox? _fileContentTextBox;
        private Label? _previewLabel;
        private Button? _buttonAdd;
        private Button? _buttonRemove;
        private Button? _buttonOk;
        private ToolTip? _toolTip;

        public MultipleListsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Multiple Lists View";
            this.Size = new Size(600, 400);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 220,
                IsSplitterFixed = false,
                Panel1MinSize = 220,
                Panel2MinSize = 100
            };

            _toolTip = new ToolTip();

            var leftPanel = new Panel { Dock = DockStyle.Fill };
            _fileListBox = new ListBox { Dock = DockStyle.Fill };
            _fileListBox.SelectedIndexChanged += FileListBox_SelectedIndexChanged;
            _fileListBox.MouseMove += FileListBox_MouseMove;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };
            _buttonAdd = new Button { Text = "Add file..." };
            _buttonAdd.Click += ButtonAdd_Click;
            _buttonRemove = new Button { Text = "Remove" };
            _buttonRemove.Click += ButtonRemove_Click;
            buttonPanel.Controls.Add(_buttonAdd);
            buttonPanel.Controls.Add(_buttonRemove);

            leftPanel.Controls.Add(_fileListBox);
            leftPanel.Controls.Add(buttonPanel);
            split.Panel1.Controls.Add(leftPanel);

            var rightTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _previewLabel = new Label
            {
                Text = "Podgląd",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            rightTable.Controls.Add(_previewLabel, 0, 0);

            _fileContentTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                WordWrap = false,
                Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            rightTable.Controls.Add(_fileContentTextBox, 0, 1);
            split.Panel2.Controls.Add(rightTable);

            // add split container to the form (was missing)
            this.Controls.Add(split);

            // bottom panel containing OK button
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };
            _buttonOk = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            btnFlow.Controls.Add(_buttonOk);
            bottomPanel.Controls.Add(btnFlow);
            this.Controls.Add(bottomPanel);

            this.AcceptButton = _buttonOk;
        }

        private void FileListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_fileListBox == null || _fileContentTextBox == null)
                return;

            var entry = _fileListBox.SelectedItem as FileEntry;
            _fileContentTextBox.Text = entry?.Content ?? string.Empty;
        }

        private void FileListBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_fileListBox == null || _toolTip == null)
                return;

            int idx = _fileListBox.IndexFromPoint(e.Location);
            if (idx >= 0 && idx < _fileListBox.Items.Count)
            {
                var entry = _fileListBox.Items[idx] as FileEntry;
                if (entry != null)
                {
                    using var g = _fileListBox.CreateGraphics();
                    var width = (int)g.MeasureString(entry.FileName, _fileListBox.Font).Width;
                    if (width > _fileListBox.ClientSize.Width)
                    {
                        _toolTip.SetToolTip(_fileListBox, entry.FileName);
                        return;
                    }
                }
            }
            _toolTip.SetToolTip(_fileListBox, string.Empty);
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select file to add",
                Filter = "All files (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                AddFileToList(ofd.FileName);
            }
        }

        private void ButtonRemove_Click(object? sender, EventArgs e)
        {
            if (_fileListBox == null)
                return;

            var entry = _fileListBox.SelectedItem as FileEntry;
            if (entry == null)
                return;

            RemoveFile(entry);
        }

        private void AddFileToList(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return;

                if (_files.Any(f => string.Equals(f.FileName, path, StringComparison.OrdinalIgnoreCase)))
                    return;

                string content;
                var ext = Path.GetExtension(path)?.ToLowerInvariant();
                if (ext == ".xlsx" || ext == ".xls")
                {
                    content = GenerateExcelPreview(path);
                }
                else
                {
                    content = File.ReadAllText(path);
                }

                var entry = new FileEntry(path, content);
                _files.Add(entry);
                _fileListBox?.Items.Add(entry);
            }catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to add file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public IReadOnlyList<FileEntry> Files => _files.AsReadOnly();

        public void RemoveFile(FileEntry entry)
        {
            if (entry == null)
                return;

            _files.Remove(entry);
            _fileListBox?.Items.Remove(entry);
            if (_fileContentTextBox != null && _fileListBox?.SelectedItem == null)
                _fileContentTextBox.Text = string.Empty;
        }

        public bool RemoveFileByName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var entry = _files.FirstOrDefault(f =>
                string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
                return false;

            RemoveFile(entry);
            return true;
        }

        private string GenerateExcelPreview(string path)
        {
            try
            {
                using var wb = new XLWorkbook(path);
                var ws = wb.Worksheets.First();

                var rows = ws.RowsUsed().Select(r => r.Cells().Select(c => c.GetString()).ToArray()).ToList();
                if (rows.Count == 0)
                    return string.Empty;

                int cols = rows.Max(r => r.Length);
                var colWidths = new int[cols];
                for (int i = 0; i < cols; i++)
                {
                    foreach (var row in rows)
                    {
                        if (i < row.Length)
                        {
                            colWidths[i] = Math.Max(colWidths[i], row[i].Length);
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                foreach (var row in rows)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        string cell = i < row.Length ? row[i] : string.Empty;
                        sb.Append(cell.PadRight(colWidths[i] + 2));
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch
            {
                return File.ReadAllText(path);
            }
        }
    }
}