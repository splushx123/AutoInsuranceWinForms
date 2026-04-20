using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class PayoutsForm : FormBase
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        public PayoutsForm(UserAccount user)
        {
            Theme.StyleForm(this); Text = "Выплаты"; Width = 1100; Height = 660; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110); var btnEdit = Theme.CreateSecondaryButton("Изменить", 110); var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            btnAdd.Click += delegate { OpenEditor(null); }; btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); }; btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete); Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }
        private void LoadData() { _grid.DataSource = Db.Query("SELECT payout_id AS [Код], case_id AS [Страховой случай], payout_amount AS [Сумма], payout_date AS [Дата выплаты] FROM Insurance_payouts ORDER BY payout_id DESC"); if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false; }
        private void OpenEditor(int? id) { using (var f = new PayoutEditForm(id)) if (f.ShowDialog(this) == DialogResult.OK) LoadData(); }
        private void DeleteSelected() { var id = SelectedId(_grid); if (!id.HasValue) return; if (MessageBox.Show("Удалить выплату?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return; try { Db.Execute("DELETE FROM Insurance_payouts WHERE payout_id=@id", new SqlParameter("@id", id.Value)); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
    }
}
