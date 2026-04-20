using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class CommissionsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        public CommissionsForm()
        {
            Theme.StyleForm(this); Text = "Комиссии"; Width = 900; Height = 560; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            Controls.Add(_grid); Load += delegate { _grid.DataSource = Db.Query("SELECT commission_id AS [Код комиссии], payment_date AS [Дата выплаты] FROM commissions ORDER BY commission_id DESC"); };
        }
    }
}
