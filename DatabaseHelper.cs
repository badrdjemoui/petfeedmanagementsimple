using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace AnimalFeedApp.Helpers
{
    internal static class DatabaseHelper
    {
        private static string dbFile = "AnimalFeed.db"; // القيمة الافتراضية
        private static string connectionString => $"Data Source={dbFile};Version=3;";
        private static bool databaseInitialized = false; // ✅ علامة تتبع التهيئة

        static DatabaseHelper()
        {
            LoadSettingsPath(); // تحميل المسار عند تشغيل التطبيق
            InitializeDatabase(); // ✅ تهيئة قاعدة البيانات مرة واحدة
        }

        // ✅ تحميل مسار قاعدة البيانات من ملف الإعدادات
        private static void LoadSettingsPath()
        {
            try
            {
                string settingsFile = "appsettings.txt";
                if (File.Exists(settingsFile))
                {
                    foreach (var line in File.ReadAllLines(settingsFile))
                    {
                        if (line.StartsWith("DB_PATH="))
                        {
                            string path = line.Substring(8).Trim();
                            if (File.Exists(path))
                                dbFile = path; // استخدم المسار من الإعدادات
                            else
                                MessageBox.Show($"⚠️ لم يتم العثور على قاعدة البيانات في المسار:\n{path}\nسيتم إنشاء واحدة جديدة.", "تنبيه");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ خطأ أثناء قراءة إعدادات قاعدة البيانات:\n" + ex.Message);
            }
        }

        /*************************************************/

        // ✅ تهيئة قاعدة البيانات مرة واحدة فقط
        public static void InitializeDatabase()
        {
            if (databaseInitialized) return; // ✅ لا تعيد التنفيذ

            try
            {
                // إنشاء الملف إذا لم يوجد
                if (!File.Exists(dbFile))
                {
                    SQLiteConnection.CreateFile(dbFile);
                }

                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    
                    // 1️⃣ إنشاء الجداول
                    string createTables = @"
                        CREATE TABLE IF NOT EXISTS Inventory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ItemName TEXT NOT NULL,
                            Quantity REAL NOT NULL,
                            UnitPrice REAL,
                            DateAdded TEXT
                        );

                        CREATE TABLE IF NOT EXISTS Sales (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CustomerName TEXT,
                            ItemName TEXT,
                            UnitPrice REAL,
                            Quantity REAL,
                            TotalPrice REAL,
                            SaleDate TEXT
                        );

                        CREATE TABLE IF NOT EXISTS Purchases (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SupplierName TEXT,
                            ItemName TEXT,
                            Quantity REAL,
                            UnitPrice REAL,
                            TotalCost REAL,
                            PurchaseDate TEXT
                        );";
                    
                    using (var cmd = new SQLiteCommand(createTables, conn))
                        cmd.ExecuteNonQuery();

                    // 2️⃣ إنشاء الفهارس (Indexes) لتحسين الأداء
                    string createIndexes = @"
                        CREATE INDEX IF NOT EXISTS idx_inventory_item ON Inventory(ItemName);
                        CREATE INDEX IF NOT EXISTS idx_inventory_date ON Inventory(DateAdded);
                        CREATE INDEX IF NOT EXISTS idx_sales_date ON Sales(SaleDate);
                        CREATE INDEX IF NOT EXISTS idx_sales_item ON Sales(ItemName);
                        CREATE INDEX IF NOT EXISTS idx_purchases_date ON Purchases(PurchaseDate);
                        CREATE INDEX IF NOT EXISTS idx_purchases_item ON Purchases(ItemName);";
                    
                    using (var cmd = new SQLiteCommand(createIndexes, conn))
                        cmd.ExecuteNonQuery();

                   
                }

                databaseInitialized = true; // ✅ تم التهيئة بنجاح
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تهيئة قاعدة البيانات:\n{ex.Message}", "خطأ التهيئة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*************************************************/

        public static DataTable GetDataTable(string query)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /*************************************************/

        public static void ExecuteNonQuery(string query)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
            }
        }

        /*************************************************/

        public static object ExecuteScalar(string query)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                return cmd.ExecuteScalar();
            }
        }

        /*******************************************************/

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }
    }

    /*******************************************************
     * تقارير المبيعات والمشتريات والمخزون
     *******************************************************/
    public static class Reports
    {
        public static DataTable GetSalesReport()
        {
            string query = @"
                SELECT CustomerName AS 'الزبون', 
                       ItemName AS 'الصنف', 
                       Quantity AS 'الكمية', 
                       UnitPrice AS 'سعر الوحدة', 
                       (Quantity * UnitPrice) AS 'السعر الإجمالي', 
                       SaleDate AS 'تاريخ البيع' 
                FROM Sales 
                ORDER BY SaleDate DESC";
            return DatabaseHelper.GetDataTable(query);
        }

        public static DataTable GetPurchasesReport()
        {
            string query = @"
                SELECT SupplierName AS 'المورد', 
                       ItemName AS 'الصنف', 
                       Quantity AS 'الكمية', 
                       UnitPrice AS 'سعر الوحدة', 
                       (Quantity * UnitPrice) AS 'التكلفة الإجمالية', 
                       PurchaseDate AS 'تاريخ الشراء' 
                FROM Purchases 
                ORDER BY PurchaseDate DESC";
            return DatabaseHelper.GetDataTable(query);
        }

        public static DataTable GetInventoryReport()
        {
            string query = @"
                SELECT ItemName AS 'الصنف', 
                       Quantity AS 'الكمية', 
                       UnitPrice AS 'سعر الوحدة', 
                       (Quantity * UnitPrice) AS 'السعر الإجمالي', 
                       DateAdded AS 'تاريخ الإضافة' 
                FROM Inventory 
                WHERE Quantity > 0
                ORDER BY DateAdded DESC";
            return DatabaseHelper.GetDataTable(query);
        }
    }
}
