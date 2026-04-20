using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ContractsForm : FormBase
    {
        private readonly UserAccount _user;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };

        public ContractsForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Договоры страхования"; Width = 1250; Height = 680; StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110);
            var btnEdit = Theme.CreateSecondaryButton("Изменить", 110);
            var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            btnAdd.Click += delegate { OpenEditor(null); }; btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); }; btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }

        private void LoadData()
        {
            _grid.DataSource = Db.Query(@"
SELECT c.id_contract AS [Код], t.type_name AS [Тип], c.start_date AS [Начало], c.end_date AS [Окончание], c.insurance_amount AS [Страховая сумма],
       e.last_name + ' ' + e.first_name AS [Сотрудник], c.id_commission AS [Комиссия], c.VIN AS [VIN]
FROM Contract c
LEFT JOIN insurance_types t ON t.id_type = c.id_type
LEFT JOIN Employees e ON e.employee_id = c.employee_id
ORDER BY c.id_contract DESC");
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }

        private void OpenEditor(int? id) { using (var f = new ContractEditForm(id)) if (f.ShowDialog(this) == DialogResult.OK) LoadData(); }

        private void DeleteSelected()
        {
            if (_user.Role == UserRole.Adjuster) { MessageBox.Show("У этой роли нет прав на удаление договоров."); return; }
            var id = SelectedId(_grid); if (!id.HasValue) return;
            if (MessageBox.Show("Удалить договор?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Db.Execute("DELETE FROM Contract WHERE id_contract=@id", new SqlParameter("@id", id.Value)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Не удалось удалить договор.\n" + ex.Message); }
        }
    }
}
