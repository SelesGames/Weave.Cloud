using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using weave;
using System.Windows.Data;
using System.ComponentModel;

namespace WeaveFeedTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel vm = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm;

            wbOriginal.SuppressScriptErrors(true);
            wbReadability.SuppressScriptErrors(true);

            this.Loaded += MainWindow_Loaded;
        }

        void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            var wb = sender as WebBrowser;
            wb.SuppressScriptErrors(true);
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await vm.LoadFeeds();
            //var cvs = new CollectionViewSource
            //{
            //    Source = vm.Feeds,
            //};
            var view = (CollectionView)CollectionViewSource.GetDefaultView(vm.Feeds);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            list.ItemsSource = view;      
        }

        async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsHitTestVisible = false;
            var selection = e.AddedItems.Cast<FeedSource>().First() as FeedSource;
            DebugEx.WriteLine(selection);
            vm.SelectedFeed = selection;
            try
            {
                await vm.LoadArticles();
                currentNode = vm.Articles.First;
                vm.SelectedArticle = currentNode.Value;
                wbOriginal.Navigate(vm.SelectedArticle.Link, null, null, "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0");
                await vm.SelectedArticle.LoadFormattedWebView();
                wbReadability.NavigateToString(vm.SelectedArticle.MobilizedHtml);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            this.IsHitTestVisible = true;
        }

        LinkedListNode<Article> currentNode;

        async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString().Equals("next", StringComparison.OrdinalIgnoreCase))
            {
                if (currentNode.Next != null)
                    currentNode = currentNode.Next;
            }
            else
            {
                if (currentNode.Previous != null)
                    currentNode = currentNode.Previous;
            }
            try
            {
                await GetCurrentArticle();
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        async Task GetCurrentArticle()
        {
            if (currentNode.Value == vm.SelectedArticle)
                return;

            vm.SelectedArticle = currentNode.Value;
            wbOriginal.Navigate(vm.SelectedArticle.Link, null, null, "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0");
            await vm.SelectedArticle.LoadFormattedWebView();
            wbReadability.NavigateToString(vm.SelectedArticle.MobilizedHtml);
        }
    }

    public static class WebBrowserExtensions
    {
        public static void SuppressScriptErrors(this WebBrowser webBrowser, bool hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null)
                return;
            object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
            if (objComWebBrowser == null)
                return;

            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
    }
}
