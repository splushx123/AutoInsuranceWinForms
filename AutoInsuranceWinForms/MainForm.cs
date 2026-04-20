using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class MainForm : Form
    {
        private readonly UserAccount _user;
        private readonly FlowLayoutPanel _statsPanel = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _tilesPanel = new FlowLayoutPanel();
        public bool ReturnToLogin { get; private set; }

        public MainForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Автострахование - главное окно";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar, Padding = new Padding(18) };
            sidebar.Controls.Add(new Label
            {
                Text = "Auto\nInsurance",
                Dock = DockStyle.Top,
                Height = 86,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White
            });
            var btnLogout = Theme.CreatePrimaryButton("Выход", 210);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Click += delegate { ReturnToLogin = true; Close(); };
            sidebar.Controls.Add(btnLogout);

            var top = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(24, 16, 24, 14), BackColor = Theme.Surface };
            top.Controls.Add(new Label
            {
                Text = "Автоматизация страховой компании по автострахованию",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Theme.Text
            });
            top.Controls.Add(new Label
            {
                Text = "Пользователь: " + _user.FullName + " | Роль: " + RoleTitle(_user.Role),
                Dock = DockStyle.Bottom,
                Height = 26,
                ForeColor = Theme.Muted
            });

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22) };
            _statsPanel.Dock = DockStyle.Top; _statsPanel.Height = 136; _statsPanel.WrapContents = true;
            _tilesPanel.Dock = DockStyle.Fill; _tilesPanel.WrapContents = true; _tilesPanel.AutoScroll = true;
            body.Controls.Add(_tilesPanel); body.Controls.Add(_statsPanel);

            Controls.Add(body); Controls.Add(top); Controls.Add(sidebar);
            Load += delegate { FillStats(); FillTiles(); };
        }

        private string RoleTitle(UserRole role)
        {
            if (role == UserRole.Administrator) return "Администратор";
            if (role == UserRole.Manager) return "Менеджер";
            return "Специалист по урегулированию";
        }

        private void FillStats()
        {
            _statsPanel.Controls.Clear();
            AddStatCard("Клиенты", SafeCount("SELECT COUNT(*) FROM Client").ToString(), Theme.Primary);
            AddStatCard("Автомобили", SafeCount("SELECT COUNT(*) FROM Vehicles").ToString(), Theme.Success);
            AddStatCard("Договоры", SafeCount("SELECT COUNT(*) FROM Contract").ToString(), Theme.Warning);
            AddStatCard("Страховые случаи", SafeCount("SELECT COUNT(*) FROM Insurance_cases").ToString(), Color.FromArgb(56, 96, 178));
            AddStatCard("Выплаты", SafeCount("SELECT COUNT(*) FROM Insurance_payouts").ToString(), Color.FromArgb(126, 87, 194));
        }

        private int SafeCount(string sql)
        {
            try { return Db.Count(sql); } catch { return 0; }
        }

        private void FillTiles()
        {
            _tilesPanel.Controls.Clear();
            AddTile("Клиенты", "Учет физических лиц и их контактных данных.", delegate { OpenModule("Клиенты", new ClientsForm(_user)); }, true);
            AddTile("Автомобили", "VIN, госномер, модель, категория, мощность.", delegate { OpenModule("Автомобили", new VehiclesForm(_user)); }, true);
            AddTile("Договоры", "Оформление полисов ОСАГО, КАСКО, ДСАГО.", delegate { OpenModule("Договоры", new ContractsForm(_user)); }, true);
            AddTile("Страховые случаи", "Регистрация ДТП, угона, повреждений и ущерба.", delegate { OpenModule("Страховые случаи", new InsuranceCasesForm(_user)); }, true);
            AddTile("Выплаты", "Учет страховых выплат по случаям.", delegate { OpenModule("Выплаты", new PayoutsForm(_user)); }, true);
            AddTile("Сотрудники", "Учет сотрудников страховой компании.", delegate { OpenModule("Сотрудники", new EmployeesForm(_user)); }, _user.Role == UserRole.Administrator);
            AddTile("Комиссии", "Просмотр начисленных комиссий.", delegate { OpenModule("Комиссии", new CommissionsForm()); }, _user.Role != UserRole.Adjuster);
            AddTile("Отчеты", "Сводная аналитика по договорам, случаям и выплатам.", delegate { OpenModule("Отчеты", new ReportsForm()); }, true);
        }

        private void AddTile(string title, string description, Action action, bool visible)
        {
            if (!visible) return;
            var card = Theme.CreateCard(); card.Width = 250; card.Height = 155;
            var lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var lblDescription = new Label { Text = description, Dock = DockStyle.Fill, ForeColor = Theme.Muted };
            var btnOpen = Theme.CreatePrimaryButton("Открыть", 110); btnOpen.Dock = DockStyle.Bottom; btnOpen.Click += delegate { action(); };
            card.Controls.Add(btnOpen); card.Controls.Add(lblDescription); card.Controls.Add(lblTitle);
            _tilesPanel.Controls.Add(card);
        }

        private void AddStatCard(string title, string value, Color color)
        {
            var card = Theme.CreateCard(); card.Width = 210; card.Height = 92; card.BackColor = color;
            card.Controls.Add(new Label { Text = value, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter });
            card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 28, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter });
            _statsPanel.Controls.Add(card);
        }

        private void OpenModule(string name, Form form)
        {
            LogService.Log("Открытие модуля", name);
            using (form) form.ShowDialog(this);
            FillStats();
        }
    }
}
