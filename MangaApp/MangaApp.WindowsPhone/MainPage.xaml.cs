using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaApp
{

    class Transition : NavigationTransitionInfo
    {

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (a, b) =>
            {
                if (Frame.CanGoBack) Frame.GoBack();
                if (canvas.Children.Count >= 1)
                {
                    var item = (canvas.Children[0] as Grid).Tag as Grid;
                    var inner = canvas.Children[0] as Grid;
                    Storyboard st = new Storyboard();

                    ExponentialEase ease = new ExponentialEase();


                    DoubleAnimation xanim = new DoubleAnimation();

                    Storyboard.SetTargetProperty(xanim, "(Canvas.Left)");
                    xanim.EasingFunction = ease;
                    xanim.From = 0;
                    xanim.To = screenCoords.X - innerPos.X;
                    xanim.Duration = TimeSpan.FromSeconds(0.3);
                    DoubleAnimation yanim = new DoubleAnimation();
                    Storyboard.SetTargetProperty(yanim, "(Canvas.Top)");
                    yanim.EasingFunction = ease;
                    yanim.To = screenCoords.Y - innerPos.Y;
                    yanim.From = 0;
                    yanim.Duration = TimeSpan.FromSeconds(0.3);

                    DoubleAnimation w = new DoubleAnimation();
                    w.EnableDependentAnimation = true;
                    w.EasingFunction = ease;
                    Storyboard.SetTargetProperty(w, "Width");
                    w.From = inner.ActualWidth;
                    w.To = item.ActualWidth;
                    w.Duration = TimeSpan.FromSeconds(0.5);

                    DoubleAnimation h = new DoubleAnimation();
                    h.EasingFunction = ease;
                    h.EnableDependentAnimation = true;
                    Storyboard.SetTargetProperty(h, "Height");
                    h.From = inner.ActualHeight;
                    h.To = item.ActualHeight;
                    h.Duration = TimeSpan.FromSeconds(0.5);


                    Storyboard.SetTarget(st, inner);
                    st.Children.Add(xanim);
                    st.Children.Add(yanim);
                    st.Children.Add(w);
                    st.Children.Add(h);
                    chapters.Visibility = Visibility.Collapsed;
                    grid.Visibility = Visibility.Collapsed;
                    st.Completed += (x, k) =>
                    {
                        
                        canvas.Children.Remove(inner);
                        item.Children.Add(inner);
                    };

                    st.Begin();
                }

                b.Handled = true;
            };
            Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            //   .SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += MainPage_Loaded;

            



            
        }

        AppModel model = new AppModel();
        bool loaded = false;
        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {

                
                DataContext = model;
                
                loadingRing.IsActive = true;
                await model.Provider.getLatest();
                loadingRing.IsActive = false;
                
                loaded = true;

            }
        }

        

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void latest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((sender as ListView).ContainerFromIndex((sender as ListView).SelectedIndex) as ListViewItem).Content;
            //var pos = item.TransformToVisual(Window.Current.Content);
            //Point screenCoords = pos.TransformPoint(new Point(0, 0));
            Manga m = (sender as ListView).SelectedValue as Manga;
            model.CurrentManga = m;
            if (m == null) return;
            Frame.Navigate(typeof(MangaPage), model);
        }

        Point screenCoords;
        Point innerPos;
        async void item_tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (sender as Grid);

            Manga m = (sender as Grid).DataContext as Manga;
            model.CurrentManga = m;
            if (!m.Saved)
            {
                m.Chapters.Clear();
                HtmlDocument htmlDocument2 = new HtmlDocument();
                htmlDocument2.OptionFixNestedTags = true;
                htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync(m.Url));

                m.Image = htmlDocument2.DocumentNode.Descendants("div").Where(x => x.Id == "mangaimg").First().Descendants().First().Attributes["src"].Value;

                foreach (HtmlNode link in htmlDocument2.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href") && x.ParentNode.OriginalName == "td" && x.ParentNode.Descendants("div").Any(y => y.Attributes["class"].Value == "chico_manga")))
                {
                    m.Chapters.Insert(0, new Chapter { Name = link.ParentNode.InnerText.Replace("\n", ""), Url = link.Attributes["href"].Value });
                }

                m.Description = htmlDocument2.DocumentNode.Descendants("div").Where(x => x.Id == "readmangasum").First().Descendants("p").First().InnerText;
            }


            //var pos = item.TransformToVisual(Window.Current.Content);
            //Point screenCoords = pos.TransformPoint(new Point(0, 0));

             screenCoords = e.GetPosition(canvas);
             innerPos = e.GetPosition(item);

            var inner = item.Children[0];
            




            var context = (inner as Grid).DataContext;
            item.Children.Remove(inner);
            canvas.Children.Add(inner);
            (inner as Grid).DataContext = context;
            (inner as Grid).Children[1].Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            (inner as Grid).Children[2].Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Canvas.SetLeft(inner, screenCoords.X - innerPos.X);
            Canvas.SetTop(inner, screenCoords.Y - innerPos.Y);
            Storyboard st = new Storyboard();
            
            ExponentialEase ease = new ExponentialEase();
            
            
            DoubleAnimation xanim = new DoubleAnimation();
            
            Storyboard.SetTargetProperty(xanim, "(Canvas.Left)");
            xanim.EasingFunction = ease;
            xanim.From = screenCoords.X - innerPos.X;
            xanim.To = 0;
            xanim.Duration = TimeSpan.FromSeconds(0.5);
            DoubleAnimation yanim = new DoubleAnimation();
            Storyboard.SetTargetProperty(yanim, "(Canvas.Top)");
            yanim.EasingFunction = ease;
            yanim.From = screenCoords.Y - innerPos.Y;
            yanim.To = 0;
            yanim.Duration = TimeSpan.FromSeconds(0.5);

            DoubleAnimation w = new DoubleAnimation();
            w.EnableDependentAnimation = true;
            w.EasingFunction = ease;
            Storyboard.SetTargetProperty(w, "Width");
            w.From = item.ActualWidth;
            w.To = canvas.ActualWidth;
            w.Duration = TimeSpan.FromSeconds(0.3);

            DoubleAnimation h = new DoubleAnimation();
            h.EasingFunction = ease;
            h.EnableDependentAnimation = true;
            Storyboard.SetTargetProperty(h, "Height");
            h.From = item.ActualHeight;
            h.To = 400;
            h.Duration = TimeSpan.FromSeconds(0.3);


            Storyboard.SetTarget(st, inner);
            st.Children.Add(xanim);
            st.Children.Add(yanim);
            st.Children.Add(w);
            st.Children.Add(h);
            (inner as Grid).Tag = item;
            st.Completed += async (a, b) =>
            {
                chapters.ItemsSource = model.CurrentManga.Chapters;
                if (m == null) return;
                //svar v = new NavigationThemeTransition();
                //v.DefaultNavigationTransitionInfo = null;
                //Frame.Navigate(typeof(MangaPage), model, null);
                chapters.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Visible;
                introInfo.Begin();
            };
            st.Begin();
        }

        private void saved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manga m = (sender as ListView).SelectedValue as Manga;
            model.CurrentManga = m;
            if (m == null) return;
            Frame.Navigate(typeof(MangaPage), model);
        }

        

        private void showDownloads(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SavedManga));
        }

        private void search_tapped(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //if (e.Key == Windows.System.VirtualKey.Enter)
            //{
            //    (DataContext as AppModel).Provider.Search((sender as TextBox).Text);
            //}
            Frame.Navigate(typeof(SearchPage), model);
        }

        private void item_clicked(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            return;
            var item= ((sender as ListView).ContainerFromItem(e.ClickedItem) as ListViewItem).ContentTemplateRoot as Grid;

            var pos = item.TransformToVisual(Window.Current.Content);
            Point screenCoords = pos.TransformPoint(new Point(0, 0));
            Grid inner = item.Children[0] as Grid;
            

            int x = 0;
        }

        private void chapter_changed(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }
    }
}
