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

namespace dotRant.TestWpfClient
{
    /// <summary>
    /// Interaction logic for IrcWindow.xaml
    /// </summary>
    public partial class IrcWindow : Window
    {
        readonly IIrcConnection _conn;
        readonly IrcViewModel _vm;

        public IrcWindow(IIrcConnection conn)
        {
            _conn = conn;
            DataContext = _vm = new IrcViewModel(Dispatcher, conn);
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tb = ((TextBox)sender);
                var text = tb.Text;
                tb.Text = "";

                var index = Channels.SelectedIndex;
                var buffer = _vm.Buffers[index];
                buffer.SendTo(text);
            }
        }
    }
}
