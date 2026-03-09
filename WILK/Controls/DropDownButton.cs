using System.Drawing.Text;

namespace WILK.Controls
{
    public class DropDownButton : Button
    {
        private ContextMenuStrip? _contextMenu;
        private const int DropdownZoneWidth = 28;
        public int HoverZone = 0;
        public string _text;
        private EventHandler? _clickHandler;

        public DropDownButton()
        {
            _text = "";
            this.MouseDown += Button_MouseDown;
            this.MouseMove += Button_MouseMove;
            this.MouseLeave += Button_MouseLeave;
            this.Paint += Button_Paint;
        }

        public void SetText(string text)
        {
            _text = text;
            this.Invalidate();
        }

        public void SetOnClick(EventHandler handler)
        {
            _clickHandler = handler;
        }

        public void SetContextMenu(ContextMenuStrip contextMenu)
        {
            _contextMenu = contextMenu;
        }

        private void Button_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var btn = this;
            var rect = btn.ClientRectangle;
            
            // Określ strefy: główna i strzałki
            int separatorX = rect.Width - DropdownZoneWidth;
            var mainZone = new Rectangle(1, 1, separatorX - 1, rect.Height - 2);
            var dropdownZone = new Rectangle(separatorX, 1, DropdownZoneWidth - 1, rect.Height - 2);

            Color normalColor = SystemColors.Control;
            Color hoverColor = Color.FromArgb(225, 235, 245);
            Color pressedColor = Color.FromArgb(200, 215, 230);

            // Rysuj tło przycisku
            if (HoverZone == 1)
            {
                using var mainBrush = new SolidBrush(hoverColor);
                g.FillRectangle(mainBrush, mainZone);
            }

            // Rysuj tło strefy strzałki
            if (HoverZone == 2)
            {
                using var dropdownBrush = new SolidBrush(hoverColor);
                g.FillRectangle(dropdownBrush, dropdownZone);
            }

            // Rysuj separator
            using (var darkPen = new Pen(SystemColors.ControlDark, 1))
            using (var lightPen = new Pen(SystemColors.ControlLightLight, 1))
            {
                g.DrawLine(darkPen, separatorX, 4, separatorX, rect.Height - 5);
                g.DrawLine(lightPen, separatorX + 1, 4, separatorX + 1, rect.Height - 5);
            }

            // Rysuj strzałkę
            int arrowX = separatorX + (DropdownZoneWidth / 2);
            int arrowY = rect.Height / 2;
            var arrowPoints = new Point[]
            {
                new Point(arrowX - 4, arrowY - 2),
                new Point(arrowX + 4, arrowY - 2),
                new Point(arrowX, arrowY + 3)
            };
            using (var arrowBrush = new SolidBrush(SystemColors.ControlText))
            {
                g.FillPolygon(arrowBrush, arrowPoints);
            }

            // Rysuj tekst
            var textRect = new Rectangle(6, 0, separatorX - 8, rect.Height);
            TextRenderer.DrawText(g, _text, btn.Font, textRect, btn.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void Button_MouseMove(object? sender, MouseEventArgs e)
        {
            int separatorX = this.Width - DropdownZoneWidth;
            int newZone = e.X >= separatorX ? 2 : 1;

            if (newZone != HoverZone)
            {
                HoverZone = newZone;
                this.Invalidate();
            }
        }

        private void Button_MouseLeave(object? sender, EventArgs e)
        {
            HoverZone = 0;
            this.Invalidate();
        }

        private void Button_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            // Sprawdź, czy kliknięto w strefę strzałki
            bool clickedDropdownZone = e.X >= this.Width - DropdownZoneWidth;

            if (clickedDropdownZone && _contextMenu != null)
            {
                // Pokaż menu pod przyciskiem
                _contextMenu.Show(this, new Point(this.Width - _contextMenu.Width, this.Height));
            }
            else
            {
                // Normalne wykonianie
                this._clickHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}