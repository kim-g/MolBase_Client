/****************************************************************/
/*                                                              */
/*                     Модуль ввода строки                      */
/*                          Версия 1.0                          */
/*                                                              */
/*                     Автор – Григорий Ким                     */
/*                      kim-g@ios.uran.ru                       */
/*    Федеральное государственное бюджетное учреждение науки    */
/*     Институт органического синтеза им. И.Я. Постовского      */
/* Уральского отделения Российской академии наук (ИОС УрО РАН)  */
/*                                                              */
/*                 Распространяется на условиях                 */
/*            Berkeley Software Distribution license            */
/*                                                              */
/****************************************************************/

using System;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class Input_String : Form    // Окно ввода текстовой строки
    {
        private string Res="@Null@";            // Результат (@Null@ по умолчанию)

        public Input_String()
        {
            InitializeComponent();
        }

        public static string GetString(string Title, string Label) // Запрос текстовой строки извне
        {
            Input_String IS = new Input_String();

            IS.Text = Title;               // Поставить заголовок окна
            IS.label1.Text = Label;        // Поставить надпись перед полем ввода

            IS.ShowDialog();               // Показать модально

            if (IS.Res == "@Null@")        // Если не получилось, вернуть пустую строку
            { IS.Res = ""; };

            return IS.Res;                 // Вернуть результат
        }

        private void button2_Click(object sender, EventArgs e)  // Если пользователь отменил
        {
            Res = "@Cancel@";           // Вернём "@Cancel@"
            Close();                    // И закроем окно
        }

        private void button1_Click(object sender, EventArgs e)  //Если пользователь нажал «OK»
        {
            Res = textBox1.Text;        // Вернёмто, что он ввёл
            Close();                    // И закроем окно
        }
    }
}
