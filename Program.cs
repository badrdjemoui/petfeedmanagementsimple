using System;
using System.Windows.Forms;
using AnimalFeedApp.Helpers;

namespace AnimalFeedApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 🔹 تهيئة إعدادات النوافذ
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 🔹 تحديد التاريخ الذي يبدأ فيه التحذير
            DateTime warningDate = new DateTime(2026, 04, 16); // عدّل التاريخ كما تريد
            DateTime today = DateTime.Today;

            if (today >= warningDate)
            {
                // 🔹 عرض رسالة تحذير
                MessageBox.Show(" تحذير: البرنامج قد يتوقف لانه ملكية خاصة بالمهندس جموعي بدر .  اتصل ب الرقم  0668704231 قبل فوات الاوان!",
                                "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 🔹 إنشاء قاعدة البيانات والجداول إذا لم تكن موجودة
            DatabaseHelper.InitializeDatabase();

            // 🔹 تشغيل الواجهة الرئيسية
            Application.Run(new FormMain());
        }
    }
}
