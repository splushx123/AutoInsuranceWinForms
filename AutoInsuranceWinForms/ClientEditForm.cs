using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ClientEditForm : Form
    {
        private readonly int? _id;
        private readonly TextBox _lastName = Theme.CreateTextBox(220);
        private readonly TextBox _firstName = Theme.CreateTextBox(220);
        private readonly TextBox _middleName = Theme.CreateTextBox(220);
        private readonly DateTimePicker _birthDate = Theme.CreateDatePicker(220);
        private readonly TextBox _passportSeries = Theme.CreateTextBox(220);
        private readonly TextBox _passportNumber = Theme.CreateTextBox(220);
        private readonly TextBox _inn = Theme.CreateTextBox(220);
        private readonly TextBox _driverSeries = Theme.CreateTextBox(220);
        private readonly TextBox _phone = Theme.CreateTextBox(220);
        private readonly TextBox _email = Theme.CreateTextBox(220);

        public ClientEditForm(int? id)
        {
            _id = id;
            Theme.StyleForm(this);
            Text = id.HasValue ? "Изменение клиента" : "Добавление клиента";
            Width = 650; Height = 540; StartPosition = FormStartPosition.CenterParent;

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoScroll = true };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "Фамилия", _lastName); AddField(table, "Имя", _firstName); AddField(table, "Отчество", _middleName);
            AddField(table, "Дата рождения", _birthDate); AddField(table, "Серия паспорта", _passportSeries); AddField(table, "Номер паспорта", _passportNumber);
            AddField(table, "ИНН", _inn); AddField(table, "Серия ВУ", _driverSeries); AddField(table, "Телефон", _phone); AddField(table, "E-mail", _email);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120);
            var btnCancel = Theme.CreateSecondaryButton("Отмена", 120);
            btnSave.Click += delegate { SaveData(); };
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(btnSave); buttons.Controls.Add(btnCancel);
            Controls.Add(table); Controls.Add(buttons);
            if (id.HasValue) LoadData();
        }

        private void AddField(TableLayoutPanel t, string name, Control control)
        {
            int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            t.Controls.Add(new Label { Text = name, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r);
            t.Controls.Add(control, 1, r);
        }

        private void LoadData()
        {
            var dt = Db.Query("SELECT * FROM Client WHERE id_client=@id", new SqlParameter("@id", _id.Value));
            if (dt.Rows.Count == 0) return;
            DataRow r = dt.Rows[0];
            _lastName.Text = r["last_name"].ToString(); _firstName.Text = r["first_name"].ToString(); _middleName.Text = r["middle_name"].ToString();
            if (r["birth_date"] != DBNull.Value) _birthDate.Value = Convert.ToDateTime(r["birth_date"]);
            _passportSeries.Text = r["passport_series"].ToString(); _passportNumber.Text = r["passport_number"].ToString(); _inn.Text = r["inn"].ToString();
            _driverSeries.Text = r["drivers_license_series"].ToString(); _phone.Text = r["phone"].ToString(); _email.Text = r["email"].ToString();
        }

        private void SaveData()
        {
            try
            {
                if (_id.HasValue)
                {
                    Db.Execute(@"UPDATE Client SET last_name=@last_name, first_name=@first_name, middle_name=@middle_name, birth_date=@birth_date,
passport_series=@passport_series, passport_number=@passport_number, inn=@inn, drivers_license_series=@drivers_license_series,
phone=@phone, email=@email WHERE id_client=@id",
                        new SqlParameter("@last_name", _lastName.Text.Trim()), new SqlParameter("@first_name", _firstName.Text.Trim()),
                        new SqlParameter("@middle_name", _middleName.Text.Trim()), new SqlParameter("@birth_date", _birthDate.Value.Date),
                        new SqlParameter("@passport_series", _passportSeries.Text.Trim()), new SqlParameter("@passport_number", _passportNumber.Text.Trim()),
                        new SqlParameter("@inn", _inn.Text.Trim()), new SqlParameter("@drivers_license_series", _driverSeries.Text.Trim()),
                        new SqlParameter("@phone", _phone.Text.Trim()), new SqlParameter("@email", _email.Text.Trim()), new SqlParameter("@id", _id.Value));
                }
                else
                {
                    Db.Execute(@"INSERT INTO Client(id_client,last_name,first_name,middle_name,birth_date,passport_series,passport_number,inn,drivers_license_series,phone,email)
VALUES(@id,@last_name,@first_name,@middle_name,@birth_date,@passport_series,@passport_number,@inn,@drivers_license_series,@phone,@email)",
                        new SqlParameter("@id", Db.NextId("Client", "id_client")), new SqlParameter("@last_name", _lastName.Text.Trim()), new SqlParameter("@first_name", _firstName.Text.Trim()),
                        new SqlParameter("@middle_name", _middleName.Text.Trim()), new SqlParameter("@birth_date", _birthDate.Value.Date), new SqlParameter("@passport_series", _passportSeries.Text.Trim()),
                        new SqlParameter("@passport_number", _passportNumber.Text.Trim()), new SqlParameter("@inn", _inn.Text.Trim()), new SqlParameter("@drivers_license_series", _driverSeries.Text.Trim()),
                        new SqlParameter("@phone", _phone.Text.Trim()), new SqlParameter("@email", _email.Text.Trim()));
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения клиента.\n" + ex.Message); }
        }
    }
}
