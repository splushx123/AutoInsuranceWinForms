using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ContractEditForm : Form
    {
        private readonly int? _id;
        private readonly ComboBox _type = Theme.CreateComboBox(220);
        private readonly DateTimePicker _start = Theme.CreateDatePicker(220);
        private readonly DateTimePicker _end = Theme.CreateDatePicker(220);
        private readonly NumericUpDown _amount = Theme.CreateNumeric(220, 100000000);
        private readonly ComboBox _employee = Theme.CreateComboBox(220);
        private readonly NumericUpDown _commission = Theme.CreateNumeric(220, 1000000, 0);
        private readonly ComboBox _vin = Theme.CreateComboBox(220);

        public ContractEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение договора" : "Добавление договора"; Width = 620; Height = 420; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "Тип страхования", _type); AddField(table, "Дата начала", _start); AddField(table, "Дата окончания", _end); AddField(table, "Страховая сумма", _amount); AddField(table, "Сотрудник", _employee); AddField(table, "Код комиссии", _commission); AddField(table, "Автомобиль", _vin);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120); btnSave.Click += delegate { SaveData(); };
            var btnCancel = Theme.CreateSecondaryButton("Отмена", 120); btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(btnSave); buttons.Controls.Add(btnCancel); Controls.Add(table); Controls.Add(buttons);
            FillCombos(); if (id.HasValue) LoadData();
        }

        private void FillCombos()
        {
            LookupService.Fill(_type, "SELECT id_type, type_name FROM insurance_types ORDER BY type_name", "id_type", "type_name");
            LookupService.Fill(_employee, "SELECT employee_id, last_name + ' ' + first_name AS fio FROM Employees ORDER BY last_name, first_name", "employee_id", "fio");
            LookupService.Fill(_vin, "SELECT VIN, VIN + ' | ' + license_plate AS title FROM Vehicles ORDER BY VIN", "VIN", "title");
        }

        private void AddField(TableLayoutPanel t, string name, Control control)
        {
            int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); t.Controls.Add(new Label { Text = name, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r); t.Controls.Add(control, 1, r);
        }

        private void LoadData()
        {
            var dt = Db.Query("SELECT * FROM Contract WHERE id_contract=@id", new SqlParameter("@id", _id.Value)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0];
            _type.SelectedValue = Convert.ToInt32(r["id_type"]); _start.Value = Convert.ToDateTime(r["start_date"]); _end.Value = Convert.ToDateTime(r["end_date"]); _amount.Value = Convert.ToDecimal(r["insurance_amount"]);
            _employee.SelectedValue = Convert.ToInt32(r["employee_id"]); _commission.Value = Convert.ToDecimal(r["id_commission"]); _vin.SelectedValue = r["VIN"].ToString();
        }

        private void SaveData()
        {
            try
            {
                if (_id.HasValue)
                {
                    Db.Execute(@"UPDATE Contract SET id_type=@type, start_date=@start, end_date=@end, insurance_amount=@amount, employee_id=@employee, id_commission=@commission, VIN=@vin WHERE id_contract=@id",
                        new SqlParameter("@type", _type.SelectedValue), new SqlParameter("@start", _start.Value.Date), new SqlParameter("@end", _end.Value.Date), new SqlParameter("@amount", _amount.Value), new SqlParameter("@employee", _employee.SelectedValue), new SqlParameter("@commission", Convert.ToInt32(_commission.Value)), new SqlParameter("@vin", _vin.SelectedValue), new SqlParameter("@id", _id.Value));
                }
                else
                {
                    Db.Execute(@"INSERT INTO Contract(id_contract,id_type,start_date,end_date,insurance_amount,employee_id,id_commission,VIN)
VALUES(@id,@type,@start,@end,@amount,@employee,@commission,@vin)",
                        new SqlParameter("@id", Db.NextId("Contract", "id_contract")), new SqlParameter("@type", _type.SelectedValue), new SqlParameter("@start", _start.Value.Date), new SqlParameter("@end", _end.Value.Date), new SqlParameter("@amount", _amount.Value), new SqlParameter("@employee", _employee.SelectedValue), new SqlParameter("@commission", Convert.ToInt32(_commission.Value)), new SqlParameter("@vin", _vin.SelectedValue));
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения договора.\n" + ex.Message); }
        }
    }
}
