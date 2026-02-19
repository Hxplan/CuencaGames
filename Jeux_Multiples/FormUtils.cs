using System;
using System.Drawing;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public static class FormUtils
    {
        public static void ApplyFullScreen(Form form)
        {
            // Use Sizable to keep the standard window frame (title bar, min/max/close buttons)
            // so notifications are visible and user can resize/minimize.
            form.FormBorderStyle = FormBorderStyle.Sizable; 
            form.WindowState = FormWindowState.Maximized;
            // Ensure proper Z-order behavior
            form.TopMost = false; 
        }

        public static void CenterControl(Control control, Control container)
        {
            control.Location = new Point(
                (container.ClientSize.Width - control.Width) / 2,
                (container.ClientSize.Height - control.Height) / 2
            );
        }

        public static void CenterControlHorizontally(Control control, Control container, int y)
        {
            control.Location = new Point(
                (container.ClientSize.Width - control.Width) / 2,
                y
            );
        }

        public static Button CreateBackButton(Form owner, Action onClick)
        {
            var btn = new Button
            {
                Text = "RETOUR", // Or an icon if preferred
                Size = new Size(100, 36),
                Location = new Point(20, 20),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 50, 70), // Dark grey
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => onClick();
            owner.Controls.Add(btn);
            btn.BringToFront();
            return btn;
        }

        // Helper to check if screen is large (e.g., > 1080p width)
        public static bool IsLargeScreen(Form form)
        {
            return form.ClientSize.Width > 1600;
        }
    }
}
