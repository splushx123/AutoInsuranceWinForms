using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class InsuranceCasesForm : FormBase
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        public InsuranceCasesForm(UserAccount user)
        {
            Theme.StyleForm(this); Text = "Страховые случаи"; Width = 1200; Height = 680; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = CreateTopPanel();
            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110); var btnEdit = Theme.CreateSecondaryButton("Изменить", 110); var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            btnAdd.Click += delegate { OpenEditor(null); }; btnEdit.Click += delegate { var id = SelectedId(_grid); if (id.HasValue) OpenEditor(id.Value); }; btnDelete.Click += delegate { DeleteSelected(); };
            top.Controls.Add(btnAdd); top.Controls.Add(btnEdit); top.Controls.Add(btnDelete); Controls.Add(_grid); Controls.Add(top); Load += delegate { LoadData(); };
        }
        private void LoadData()
        {
            _grid.DataSource = Db.Query(@"SELECT case_id AS [Код], id_contract AS [Договор], brief_description AS [Описание], final_damage AS [Ущерб], guilty_person AS [Виновное лицо] FROM Insurance_cases ORDER BY case_id DESC");
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }
        private void OpenEditor(int? id) { using (var f = new InsuranceCaseEditForm(id)) if (f.ShowDialog(this) == DialogResult.OK) LoadData(); }
        private void DeleteSelected() { var id = SelectedId(_grid); if (!id.HasValue) return; if (MessageBox.Show("Удалить страховой случай?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return; try { Db.Execute("DELETE FROM Insurance_cases WHERE case_id=@id", new SqlParameter("@id", id.Value)); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
    }
}
