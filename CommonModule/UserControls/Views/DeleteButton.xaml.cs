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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonModule.UserControls.Views
{
    /// <summary>
    /// DeleteButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeleteButton : UserControl
    {
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(UserControl),
                new UIPropertyMetadata(null));
        public DeleteButton()
        {
            InitializeComponent();
        }
    }
}
