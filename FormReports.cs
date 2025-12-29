using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AnimalFeedApp.Helpers;

namespace AnimalFeedApp.Forms
{
    public partial class FormReports : Form
    {
        public FormReports()
        {
            InitializeComponent();
            this.Resize += FormReports_Resize;
        }

        private void FormReports_Load(object sender, EventArgs e)
        {
            comboReportType.Items.AddRange(new string[] { "المخزون", "المبيعات", "المشتريات" });
            comboDuration.Items.AddRange(new string[] { "يومي", "شهري", "سنوي" });

            comboReportType.SelectedIndex = 0;
            comboDuration.SelectedIndex = 0;

            LoadReport();
            CenterAndResizeUI();
        }

        private void FormReports_Resize(object sender, EventArgs e)
        {
            CenterAndResizeUI();
        }

        private void CenterAndResizeUI()
        {
            float screenW = this.ClientSize.Width;
            float screenH = this.ClientSize.Height;

            float fontScale = Math.Max(screenW, screenH) / 1200f;
            Font baseFont = new Font("Segoe UI", 10f * fontScale, FontStyle.Bold);

            comboReportType.Font = baseFont;
            comboDuration.Font = baseFont;
            dataGridView1.Font = new Font("Segoe UI", 10f * fontScale);
            dataGridTotals.Font = new Font("Segoe UI", 10f * fontScale);
            btnClose.Font = baseFont;

            int controlWidth = (int)(screenW * 0.25);
            int controlHeight = (int)(40 * fontScale);

            comboReportType.Width = comboDuration.Width = controlWidth;
            comboReportType.Height = comboDuration.Height = controlHeight;

            comboReportType.Left = (int)(screenW / 2 - controlWidth - 10);
            comboDuration.Left = (int)(screenW / 2 + 10);
            comboReportType.Top = comboDuration.Top = (int)(screenH * 0.05);

            dataGridView1.Width = (int)(screenW * 0.9);
            dataGridView1.Height = (int)(screenH * 0.5);
            dataGridView1.Left = (int)(screenW * 0.05);
            dataGridView1.Top = (int)(screenH * 0.15);

            dataGridTotals.Width = (int)(screenW * 0.9);
            dataGridTotals.Left = (int)(screenW * 0.05);
            dataGridTotals.Top = dataGridView1.Top + dataGridView1.Height + 10;

            btnClose.Width = (int)(controlWidth * 0.8);
            btnClose.Height = controlHeight;
            btnClose.Left = (int)(screenW / 2 - btnClose.Width / 2);
            btnClose.Top = dataGridTotals.Bottom + 20;
        }

        private void ComboReportType_SelectedIndexChanged(object sender, EventArgs e) => LoadReport();
        private void ComboDuration_SelectedIndexChanged(object sender, EventArgs e) => LoadReport();

        private void LoadReport()
        {
            if (comboReportType.SelectedItem == null || comboDuration.SelectedItem == null)
                return;

            string type = comboReportType.SelectedItem.ToString();
            string duration = comboDuration.SelectedItem.ToString();

            string query = "";

            if (type == "المخزون")
                query = "SELECT ItemName AS 'الصنف', Quantity AS 'الكمية', UnitPrice AS 'سعر الوحدة', (Quantity * UnitPrice) AS 'السعر الإجمالي', DateAdded AS 'تاريخ الإضافة' FROM Inventory ORDER BY DateAdded DESC";
            else if (type == "المبيعات")
                // ✅ عرض كل البيانات بدون فلترة مثل المخزون
                query = "SELECT CustomerName AS 'الزبون', ItemName AS 'الصنف', Quantity AS 'الكمية', UnitPrice AS 'سعر الوحدة', (Quantity * UnitPrice) AS 'السعر الإجمالي', SaleDate AS 'تاريخ البيع' FROM Sales ORDER BY SaleDate DESC";
            else if (type == "المشتريات")
                // ✅ عرض كل البيانات بدون فلترة مثل المخزون
                query = "SELECT SupplierName AS 'المورد', ItemName AS 'الصنف', Quantity AS 'الكمية', UnitPrice AS 'سعر الوحدة', (Quantity * UnitPrice) AS 'التكلفة الإجمالية', PurchaseDate AS 'تاريخ الشراء' FROM Purchases ORDER BY PurchaseDate DESC";

            DataTable dt = DatabaseHelper.GetDataTable(query);
            dataGridView1.DataSource = dt;
            CalculateTotals(dt, type);
        }

        private void CalculateTotals(DataTable dt, string type)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                dataGridTotals.DataSource = null;
                return;
            }

            if (type == "المخزون" && dt.Columns.Contains("السعر الإجمالي"))
            {
                double total = dt.AsEnumerable()
                    .Where(r => r["السعر الإجمالي"] != DBNull.Value)
                    .Sum(r => Convert.ToDouble(r["السعر الإجمالي"]));
                dataGridTotals.DataSource = new[] { new { الوصف = "📦 إجمالي قيمة المخزون", القيمة = $"{total:F2} دج" } };
            }
            else if (type == "المبيعات" && dt.Columns.Contains("السعر الإجمالي"))
            {
                double total = dt.AsEnumerable()
                    .Where(r => r["السعر الإجمالي"] != DBNull.Value)
                    .Sum(r => Convert.ToDouble(r["السعر الإجمالي"]));
                dataGridTotals.DataSource = new[] { new { الوصف = "💰 إجمالي المبيعات", القيمة = $"{total:F2} دج" } };
            }
            else if (type == "المشتريات" && dt.Columns.Contains("التكلفة الإجمالية"))
            {
                double totalQuantity = 0;
                double totalCost = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (double.TryParse(row["الكمية"]?.ToString(), out double q))
                        totalQuantity += q;
                    if (double.TryParse(row["التكلفة الإجمالية"]?.ToString(), out double c))
                        totalCost += c;
                }

                double percent = totalCost * 0.05;
                double totalWithPercent = totalCost + percent;

                dataGridTotals.DataSource = new[]
                {
                    new { الوصف = "🧾 إجمالي المشتريات", القيمة = $"{totalCost:F2} دج" },
                    new { الوصف = "📦 مجموع الكمية", القيمة = $"{totalQuantity:F2} قنطار" },
                    new { الوصف = "💸 نسبة 5%", القيمة = $"{percent:F2} دج" },
                    new { الوصف = "💰 المجموع الكلي مع الزيادة", القيمة = $"{totalWithPercent:F2} دج" }
                };
            }
        }

        private void BtnClose_Click(object sender, EventArgs e) => this.Close();
    }
}
