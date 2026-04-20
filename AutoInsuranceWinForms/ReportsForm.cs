using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ReportsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly ComboBox _cb = Theme.CreateComboBox(300);
        private readonly Label _lbl = new Label();

        public ReportsForm()
        {
            Theme.StyleForm(this); Text = "Отчеты"; Width = 1200; Height = 700; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(12), BackColor = Theme.Surface };
            _cb.Items.AddRange(new object[] { "Договоры по типам", "Выплаты по страховым случаям", "Активные договоры", "Автомобили по категориям" }); _cb.SelectedIndex = 0; _cb.SelectedIndexChanged += delegate { BuildReport(); };
            var export = Theme.CreateSecondaryButton("Экспорт CSV", 130); export.Click += delegate { Export(); };
            top.Controls.Add(new Label { Text = "Тип отчета:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) }); top.Controls.Add(_cb); top.Controls.Add(export); top.Controls.Add(_lbl);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { BuildReport(); };
        }

        private void BuildReport()
        {
            switch (_cb.SelectedIndex)
            {
                case 0:
                    _grid.DataSource = Db.Query(@"SELECT t.type_name AS [Тип страхования], COUNT(*) AS [Количество договоров] FROM Contract c INNER JOIN insurance_types t ON t.id_type=c.id_type GROUP BY t.type_name ORDER BY [Количество договоров] DESC");
                    _lbl.Text = " Сводка по типам страхования."; break;
                case 1:
                    _grid.DataSource = Db.Query(@"SELECT ic.case_id AS [Страховой случай], ic.brief_description AS [Описание], ic.final_damage AS [Ущерб], ISNULL(SUM(p.payout_amount),0) AS [Выплачено] FROM Insurance_cases ic LEFT JOIN Insurance_payouts p ON p.case_id=ic.case_id GROUP BY ic.case_id, ic.brief_description, ic.final_damage ORDER BY ic.case_id DESC");
                    _lbl.Text = " Выплаты и остаток по страховым случаям."; break;
                case 2:
                    _grid.DataSource = Db.Query(@"SELECT c.id_contract AS [Договор], t.type_name AS [Тип], c.start_date AS [Начало], c.end_date AS [Окончание], c.insurance_amount AS [Сумма], c.VIN AS [VIN] FROM Contract c INNER JOIN insurance_types t ON t.id_type=c.id_type WHERE c.end_date >= CAST(GETDATE() AS DATE) ORDER BY c.end_date");
                    _lbl.Text = " Действующие договоры."; break;
                default:
                    _grid.DataSource = Db.Query(@"SELECT vc.category_name AS [Категория], COUNT(*) AS [Количество автомобилей] FROM Vehicles v INNER JOIN vehicle_categories vc ON vc.id_vehicle_category=v.id_vehicle_category GROUP BY vc.category_name ORDER BY [Количество автомобилей] DESC");
                    _lbl.Text = " Распределение автомобилей по категориям."; break;
            }
        }

        private void Export()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files|*.csv"; sfd.FileName = "report.csv"; if (sfd.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(";", _grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)));
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    sb.AppendLine(string.Join(";", row.Cells.Cast<DataGridViewCell>().Select(c => ((c.Value ?? string.Empty).ToString() ?? string.Empty).Replace(";", ","))));
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Экспорт завершен.");
            }
        }
    }
}
