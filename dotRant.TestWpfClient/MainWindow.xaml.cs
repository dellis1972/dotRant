using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace dotRant.TestWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IIrcConnection _conn;
        IrcConnectionFactory _fact;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _fact = new dotRant.Net45.IrcConnectionFactory();
            var conn = _fact.Create("irc.hitos.no");
            ObservableCollection<string> log = new ObservableCollection<string>();
            OutView.ItemsSource = log;
            conn.RawMessageIn += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, ">> " + ev.Command)));
            conn.RawMessageOut += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, "<< " + ev.Command)));
            await conn.Connect();
            MessageBox.Show("Connected");
        }
    }
}
