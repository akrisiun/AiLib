// Copy left (c) Microsoft. no rights reserved.

using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            list.Items.Add(
                    new ListBoxItem { Content = "Line 3" });

            IDictionary env = System.Environment.GetEnvironmentVariables();

            IDictionaryEnumerator num = env.GetEnumerator();
            while (num.MoveNext())
                list.Items.Add(
                    new ListBoxItem { Content = num.Key + " = " + num.Value }
                    );
            // + Environment.NewLine;

            this.Showdown.Click += (s, e)
                =>
            {
                Application.Current.Exit();
            };

        }

        private void ClickMe_Click(object sender, RoutedEventArgs e)
        {
            HelloMessage.Text = "Hello, Windows 10 IoT Core!";
        }
    }
}
