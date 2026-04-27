using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
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
        private readonly DateTimePicker _commissionPaymentDate = Theme.CreateDatePicker(220);

        private readonly TextBox _clientLastName = Theme.CreateTextBox(220);
        private readonly TextBox _clientFirstName = Theme.CreateTextBox(220);
        private readonly TextBox _clientMiddleName = Theme.CreateTextBox(220);
        private readonly DateTimePicker _clientBirthDate = Theme.CreateDatePicker(220);
        private readonly TextBox _clientPassportSeries = Theme.CreateTextBox(220);
        private readonly TextBox _clientPassportNumber = Theme.CreateTextBox(220);
        private readonly TextBox _clientInn = Theme.CreateTextBox(220);
        private readonly TextBox _clientDriverSeries = Theme.CreateTextBox(220);
        private readonly TextBox _clientPhone = Theme.CreateTextBox(220);
        private readonly TextBox _clientEmail = Theme.CreateTextBox(220);

        private readonly TextBox _vehicleVin = Theme.CreateTextBox(220);
        private readonly TextBox _vehiclePlate = Theme.CreateTextBox(220);
        private readonly ComboBox _vehicleBrand = Theme.CreateComboBox(220);
        private readonly ComboBox _vehicleModel = Theme.CreateComboBox(220);
        private readonly ComboBox _vehicleCategory = Theme.CreateComboBox(220);
        private readonly NumericUpDown _vehiclePower = Theme.CreateNumeric(220, 5000);
        private readonly TextBox _vehiclePtsSeries = Theme.CreateTextBox(220);
        private readonly TextBox _vehiclePtsNumber = Theme.CreateTextBox(220);

        public ContractEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение договора" : "Добавление договора"; Width = 780; Height = 700; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoScroll = true };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddSection(table, "Договор");
            AddField(table, "Тип страхования", _type); AddField(table, "Дата начала", _start); AddField(table, "Дата окончания", _end); AddField(table, "Страховая сумма", _amount); AddField(table, "Сотрудник", _employee);

            if (id.HasValue)
            {
                AddField(table, "Код комиссии", _commission);
                AddField(table, "Автомобиль", _vin);
            }
            else
            {
                AddField(table, "Дата выплаты комиссии", _commissionPaymentDate);

                AddSection(table, "Клиент");
                AddField(table, "Фамилия", _clientLastName);
                AddField(table, "Имя", _clientFirstName);
                AddField(table, "Отчество", _clientMiddleName);
                AddField(table, "Дата рождения", _clientBirthDate);
                AddField(table, "Серия паспорта", _clientPassportSeries);
                AddField(table, "Номер паспорта", _clientPassportNumber);
                AddField(table, "ИНН", _clientInn);
                AddField(table, "Серия ВУ", _clientDriverSeries);
                AddField(table, "Телефон", _clientPhone);
                AddField(table, "E-mail", _clientEmail);

                AddSection(table, "Автомобиль");
                AddField(table, "VIN", _vehicleVin);
                AddField(table, "Госномер", _vehiclePlate);
                AddField(table, "Марка", _vehicleBrand);
                AddField(table, "Модель", _vehicleModel);
                AddField(table, "Категория ТС", _vehicleCategory);
                AddField(table, "Мощность", _vehiclePower);
                AddField(table, "Серия ПТС", _vehiclePtsSeries);
                AddField(table, "Номер ПТС", _vehiclePtsNumber);
            }
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120); btnSave.Click += delegate { SaveData(); };
            var btnCancel = Theme.CreateSecondaryButton("Отмена", 120); btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(btnSave); buttons.Controls.Add(btnCancel); Controls.Add(table); Controls.Add(buttons);
            ConfigureInputRules();
            FillCombos(); if (id.HasValue) LoadData();
        }

        private void ConfigureInputRules()
        {
            _clientPassportSeries.MaxLength = 4;
            _clientPassportNumber.MaxLength = 6;
            _clientInn.MaxLength = 12;
            _clientDriverSeries.MaxLength = 4;
            _clientPhone.MaxLength = 18;
            _vehicleVin.MaxLength = 17;
            _vehiclePlate.MaxLength = 9;

            _vehicleVin.CharacterCasing = CharacterCasing.Upper;
            _vehiclePlate.CharacterCasing = CharacterCasing.Upper;

            _clientPassportSeries.KeyPress += DigitsOnlyKeyPress;
            _clientPassportNumber.KeyPress += DigitsOnlyKeyPress;
            _clientInn.KeyPress += DigitsOnlyKeyPress;
            _clientDriverSeries.KeyPress += DigitsOnlyKeyPress;
            _vehicleVin.KeyPress += VinKeyPress;
        }

        private void FillCombos()
        {
            LookupService.Fill(_type, "SELECT id_type, type_name FROM insurance_types ORDER BY type_name", "id_type", "type_name");
            LookupService.Fill(_employee, "SELECT employee_id, last_name + ' ' + first_name AS fio FROM Employees ORDER BY last_name, first_name", "employee_id", "fio");
            LookupService.Fill(_vin, "SELECT VIN, VIN + ' | ' + license_plate AS title FROM Vehicles ORDER BY VIN", "VIN", "title");
            LookupService.Fill(_vehicleBrand, "SELECT id_brand, brand_name FROM car_brands ORDER BY brand_name", "id_brand", "brand_name");
            LookupService.Fill(_vehicleModel, "SELECT id_model, model_name FROM car_models ORDER BY model_name", "id_model", "model_name");
            LookupService.Fill(_vehicleCategory, "SELECT id_vehicle_category, category_name FROM vehicle_categories ORDER BY category_name", "id_vehicle_category", "category_name");
        }

        private void AddField(TableLayoutPanel t, string name, Control control)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            t.Controls.Add(new Label { Text = name, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 4, 0, 4);
            t.Controls.Add(control, 1, r);
        }

        private void AddSection(TableLayoutPanel t, string title)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            var lbl = new Label { Text = title, AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) };
            t.Controls.Add(lbl, 0, r);
            t.SetColumnSpan(lbl, 2);
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
                if (!ValidateCommonFields()) return;

                if (_id.HasValue)
                {
                    Db.Execute(@"UPDATE Contract SET id_type=@type, start_date=@start, end_date=@end, insurance_amount=@amount, employee_id=@employee, id_commission=@commission, VIN=@vin WHERE id_contract=@id",
                        new SqlParameter("@type", _type.SelectedValue), new SqlParameter("@start", _start.Value.Date), new SqlParameter("@end", _end.Value.Date), new SqlParameter("@amount", _amount.Value), new SqlParameter("@employee", _employee.SelectedValue), new SqlParameter("@commission", Convert.ToInt32(_commission.Value)), new SqlParameter("@vin", _vin.SelectedValue), new SqlParameter("@id", _id.Value));
                }
                else
                {
                    if (!ValidateAddModeFields()) return;
                    string vin = _vehicleVin.Text.Trim().ToUpperInvariant();

                    using (var connection = new SqlConnection(Db.ConnectionString))
                    {
                        connection.Open();
                        using (var tx = connection.BeginTransaction())
                        {
                            try
                            {
                                int clientId = NextId(connection, tx, "Client", "id_client");
                                int commissionId = NextId(connection, tx, "commissions", "commission_id");
                                int contractId = NextId(connection, tx, "Contract", "id_contract");

                                if (ExistsInTransaction(connection, tx, "SELECT COUNT(1) FROM Vehicles WHERE VIN=@vin", new SqlParameter("@vin", vin)))
                                    throw new Exception("Автомобиль с таким VIN уже существует.");

                                ExecuteInTransaction(connection, tx, @"INSERT INTO Client(id_client,last_name,first_name,middle_name,birth_date,passport_series,passport_number,inn,drivers_license_series,phone,email)
VALUES(@id,@last_name,@first_name,@middle_name,@birth_date,@passport_series,@passport_number,@inn,@drivers_license_series,@phone,@email)",
                                    new SqlParameter("@id", clientId),
                                    new SqlParameter("@last_name", _clientLastName.Text.Trim()),
                                    new SqlParameter("@first_name", _clientFirstName.Text.Trim()),
                                    new SqlParameter("@middle_name", _clientMiddleName.Text.Trim()),
                                    new SqlParameter("@birth_date", _clientBirthDate.Value.Date),
                                    new SqlParameter("@passport_series", _clientPassportSeries.Text.Trim()),
                                    new SqlParameter("@passport_number", _clientPassportNumber.Text.Trim()),
                                    new SqlParameter("@inn", _clientInn.Text.Trim()),
                                    new SqlParameter("@drivers_license_series", _clientDriverSeries.Text.Trim()),
                                    new SqlParameter("@phone", _clientPhone.Text.Trim()),
                                    new SqlParameter("@email", _clientEmail.Text.Trim()));

                                ExecuteInTransaction(connection, tx, @"INSERT INTO Vehicles(VIN,license_plate,id_brand,id_model,id_vehicle_category,engine_power,pts_series,pts_number,id_client)
VALUES(@vin,@plate,@brand,@model,@cat,@power,@pts_series,@pts_number,@client)",
                                    new SqlParameter("@vin", vin),
                                    new SqlParameter("@plate", _vehiclePlate.Text.Trim()),
                                    new SqlParameter("@brand", _vehicleBrand.SelectedValue),
                                    new SqlParameter("@model", _vehicleModel.SelectedValue),
                                    new SqlParameter("@cat", _vehicleCategory.SelectedValue),
                                    new SqlParameter("@power", _vehiclePower.Value),
                                    new SqlParameter("@pts_series", _vehiclePtsSeries.Text.Trim()),
                                    new SqlParameter("@pts_number", _vehiclePtsNumber.Text.Trim()),
                                    new SqlParameter("@client", clientId));

                                ExecuteInTransaction(connection, tx, "INSERT INTO commissions(commission_id,payment_date) VALUES(@id,@date)",
                                    new SqlParameter("@id", commissionId),
                                    new SqlParameter("@date", _commissionPaymentDate.Value.Date));

                                ExecuteInTransaction(connection, tx, @"INSERT INTO Contract(id_contract,id_type,start_date,end_date,insurance_amount,employee_id,id_commission,VIN)
VALUES(@id,@type,@start,@end,@amount,@employee,@commission,@vin)",
                                    new SqlParameter("@id", contractId),
                                    new SqlParameter("@type", _type.SelectedValue),
                                    new SqlParameter("@start", _start.Value.Date),
                                    new SqlParameter("@end", _end.Value.Date),
                                    new SqlParameter("@amount", _amount.Value),
                                    new SqlParameter("@employee", _employee.SelectedValue),
                                    new SqlParameter("@commission", commissionId),
                                    new SqlParameter("@vin", vin));

                                tx.Commit();
                            }
                            catch
                            {
                                tx.Rollback();
                                throw;
                            }
                        }
                    }
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения договора.\n" + ex.Message); }
        }

        private bool ValidateCommonFields()
        {
            if (_type.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип страхования.");
                return false;
            }
            if (_employee.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника.");
                return false;
            }
            if (_end.Value.Date < _start.Value.Date)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.");
                return false;
            }
            if (_amount.Value <= 0)
            {
                MessageBox.Show("Страховая сумма должна быть больше 0.");
                return false;
            }
            if (_id.HasValue && _vin.SelectedValue == null)
            {
                MessageBox.Show("Выберите автомобиль.");
                return false;
            }
            if (_id.HasValue && _commission.Value <= 0)
            {
                MessageBox.Show("Код комиссии должен быть больше 0.");
                return false;
            }
            return true;
        }

        private bool ValidateAddModeFields()
        {
            if (string.IsNullOrWhiteSpace(_clientLastName.Text) || string.IsNullOrWhiteSpace(_clientFirstName.Text))
            {
                MessageBox.Show("Укажите фамилию и имя клиента.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_vehicleVin.Text))
            {
                MessageBox.Show("Введите VIN автомобиля.");
                return false;
            }
            if (_vehicleBrand.SelectedValue == null || _vehicleModel.SelectedValue == null || _vehicleCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите марку, модель и категорию автомобиля.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_clientPhone.Text) || string.IsNullOrWhiteSpace(_clientEmail.Text))
            {
                MessageBox.Show("Заполните телефон и e-mail клиента.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_vehiclePlate.Text) || string.IsNullOrWhiteSpace(_vehiclePtsSeries.Text) || string.IsNullOrWhiteSpace(_vehiclePtsNumber.Text))
            {
                MessageBox.Show("Заполните госномер, серию и номер ПТС.");
                return false;
            }
            if (!Regex.IsMatch(_clientPassportSeries.Text.Trim(), @"^\d{4}$"))
            {
                MessageBox.Show("Серия паспорта должна содержать ровно 4 цифры.");
                return false;
            }
            if (!Regex.IsMatch(_clientPassportNumber.Text.Trim(), @"^\d{6}$"))
            {
                MessageBox.Show("Номер паспорта должен содержать ровно 6 цифр.");
                return false;
            }
            if (!Regex.IsMatch(_clientInn.Text.Trim(), @"^\d{10}(\d{2})?$"))
            {
                MessageBox.Show("ИНН должен содержать 10 или 12 цифр.");
                return false;
            }
            if (!Regex.IsMatch(_clientDriverSeries.Text.Trim(), @"^\d{4}$"))
            {
                MessageBox.Show("Серия ВУ должна содержать ровно 4 цифры.");
                return false;
            }

            var digitsPhone = Regex.Replace(_clientPhone.Text, @"\D", string.Empty);
            if (digitsPhone.Length != 11)
            {
                MessageBox.Show("Телефон должен содержать 11 цифр.");
                return false;
            }

            var email = _clientEmail.Text.Trim();
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный E-mail.");
                return false;
            }

            var vin = _vehicleVin.Text.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$"))
            {
                MessageBox.Show("VIN должен содержать ровно 17 символов (латинские буквы и цифры, без I, O, Q).");
                return false;
            }

            var plate = _vehiclePlate.Text.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(plate, @"^[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}$"))
            {
                MessageBox.Show("Госномер должен быть в формате A123BC77 или A123BC777.");
                return false;
            }
            return true;
        }

        private static void DigitsOnlyKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private static void VinKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            var c = char.ToUpperInvariant(e.KeyChar);
            if (!char.IsLetterOrDigit(c) || c == 'I' || c == 'O' || c == 'Q')
                e.Handled = true;
        }

        private static int NextId(SqlConnection connection, SqlTransaction tx, string tableName, string idField)
        {
            using (var cmd = new SqlCommand(string.Format("SELECT ISNULL(MAX({0}), 0) + 1 FROM {1}", idField, tableName), connection, tx))
                return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void ExecuteInTransaction(SqlConnection connection, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, connection, tx))
            {
                if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool ExistsInTransaction(SqlConnection connection, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, connection, tx))
            {
                if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
