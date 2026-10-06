using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;

namespace pluginIcarusVerilog.Views
{
    public partial class SimPanel : UserControl
    {
        public SimPanel()
        {
            InitializeComponent();

            ItemsControl0.ItemsSource = listItems;
        }

        private ObservableCollection<ListBoxItem> listItems = new ObservableCollection<ListBoxItem>();

        public void LineReceived(string lineString, Color? color)
        {
            appendLog(lineString, color);
        }

        private void appendLog(string lineString, Color? color)
        {
            Dispatcher.UIThread.Post(
                    new Action(() =>
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = lineString;
                        textBlock.FontSize = 10;
                        textBlock.Height = 11;
                        textBlock.MinHeight = 11;
                        textBlock.Margin = new Avalonia.Thickness(2, 0, 0, 0);
                        if (color != null)
                        {
                            textBlock.Foreground = new SolidColorBrush((Color)color);
                        }

                        lock (listItems)
                        {
                            listItems.Add(new ListBoxItem()
                            {
                                Content = textBlock,
                                Padding = new Avalonia.Thickness(0),
                                Margin = new Avalonia.Thickness(0),
                                MinHeight = 11
                            });

                            if (listItems.Count > 1000)
                            {
                                listItems.RemoveAt(0);
                            }
                        }

                        // auto scroll to bottom
                        Dispatcher.UIThread.Post(() =>
                        {
                            ScrollViewer0.ScrollToEnd();
                        });
                    })
                );
        }

    }
}
