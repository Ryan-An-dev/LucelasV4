using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CommonModule.Views
{
    /// <summary>
    /// AlertWindow1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlertWindowBasic : IDialogWindow
    {
        public string TiTle
        {
            get { return (string)GetValue(TiTleProperty); }
            set { SetValue(TiTleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TiTleProperty =
            DependencyProperty.Register("TiTle", typeof(string), typeof(AlertWindowBasic), new PropertyMetadata(null));


        public AlertWindowBasic()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
