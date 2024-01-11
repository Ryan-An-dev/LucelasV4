using CommonModel;
using CommonServiceLocator;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HomePage.UserControls
{
    /// <summary>
    /// Interaction logic for MontlyCard
    /// </summary>
    public partial class MontlyCard : UserControl
    {
        public MontlyCard()
        {
            InitializeComponent();
           
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MontlyCard));

        public string Params
        {
            get { return (string)GetValue(ParamsProperty); }
            set { SetValue(ParamsProperty, value); }
        }
        public static readonly DependencyProperty ParamsProperty =
            DependencyProperty.Register("Params", typeof(string), typeof(MontlyCard));

        public string Number
        {
            get { return (string)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }
        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(string), typeof(MontlyCard));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(MontlyCard));
        public bool IsTechnical
        {
            get { return (bool)GetValue(IsTechnicalProperty); }
            set { SetValue(IsTechnicalProperty, value); }
        }
        public static readonly DependencyProperty IsTechnicalProperty =
            DependencyProperty.Register("IsTechnical", typeof(bool), typeof(MontlyCard));


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(ICommand), typeof(MontlyCard), new PropertyMetadata(null));
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Command?.Execute(Params);
        }
    }
}
