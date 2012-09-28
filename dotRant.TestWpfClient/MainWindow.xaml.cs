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
            var conn = _conn = _fact.Create("irc.irchighway.net");
            ObservableCollection<string> log = new ObservableCollection<string>();
            OutView.ItemsSource = log;
            conn.RawMessageIn += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, ">> " + ev.Command)));
            conn.RawMessageOut += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, "<< " + ev.Command)));
            conn.Join += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, "!! Joined channel: " + ev.Channel.Name)));
            conn.Part += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, "!! Parted channel: " + ev.ChannelName)));
            conn.ChannelTopicChanged += (s, ev) => Dispatcher.BeginInvoke(new Action(() => log.Insert(0, String.Format("!! Topic set for {0} by {1} to \"{2}\"", ev.Channel.Name, ev.Channel.TopicCreator, ev.Channel.Topic))));
            await conn.Connect();
            //MessageBox.Show("Connected");
            var channel = await conn.Channels.Join("#watashiwaten");
            //MessageBox.Show("Joined");
            //await channel.Send("Found users: " + String.Join(", ", channel.Users));
            //MessageBox.Show("Sent");
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = Cmd.Text;
            Cmd.Text = "";
            await _conn.SendRawCommand(text);
        }

        private void Cmd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendBtn_Click(sender, new RoutedEventArgs());
        }
    }
}
