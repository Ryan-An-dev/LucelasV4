using Prism.Services.Dialogs;
using System.Windows;

namespace CommonModule.Views
{
    /// <summary>
    /// Interaction logic for CommonDialogWindow.xaml
    /// </summary>
    public partial class CommonDialogWindow : IDialogWindow
    {
        public CommonDialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }
    }
}
