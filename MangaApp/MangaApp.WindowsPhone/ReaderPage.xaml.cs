using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MangaApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReaderPage : Page
    {
        public ReaderPage()
        {
            this.InitializeComponent();
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView()
   .SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            
        }


        Manga manga;
        Chapter chapter;
        int index;
        Point initialpoint;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            manga = ((object[]) e.Parameter)[1] as Manga
                ;
            index = (int)(e.Parameter as object[])[0];
            chapter = manga.Chapters[index];

            AppModel.Current.Provider.GetImages(manga.Chapters[index]);

           



            DataContext = chapter;
            FillChapter(chapter);
            images.SelectionChanged += (a, b) =>
            {
                bar.Maximum = images.Items.Count;
                bar.Value = images.SelectedIndex + 1;
                if (images.SelectedIndex == images.Items.Count - 1)
                {
                    //appbar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                }
                else
                {
                    //appbar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                }
            };

            bar.ValueChanged += (a, b) =>
            {
                images.SelectedIndex = (int)bar.Value - 1;
            };
            images.ManipulationStarted += (a, b) => initialpoint = b.Position;
            images.ManipulationDelta += images_ManipulationDelta;

            this.Loaded += ReaderPage_Loaded;
            
        }

        void ReaderPage_Loaded(object sender, RoutedEventArgs e)
        {
            //ListPickerFlyout menu = new ListPickerFlyout();
            //menu.ItemTemplate = Resources["listTemplate"] as DataTemplate;

            /*_switch.Click += async (a, b) =>
            {
                await menu.ShowAtAsync(a as FrameworkElement);
            */

            


            

            overlay.Tapped += (AccessibilitySettings, b) => b.Handled = true;
            overlay2.Tapped += (AccessibilitySettings, b) => b.Handled = true;

            bool overlayShown = true;
            this.Tapped += (AccessibilitySettings, b) =>
            {
                //overlay.Visibility = overlay.Visibility == Windows.UI.Xaml.Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                //overlay2.Visibility = overlay2.Visibility == Windows.UI.Xaml.Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                if(overlayShown){
                    hideOverlays.Begin();
                    overlayShown = false;
                }
                else
                {
                    showOverlays.Begin();
                    overlayShown = true;
                }
            };

            horizontalButton.Tapped += (a, b) =>
            {
                verticalButton.IsChecked = false;
                images.Visibility = Windows.UI.Xaml.Visibility.Visible;
                images2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                for (int k = 0; k < images2.Items.Count; k++)
                {
                    object i = images2.Items[k];

                    if (i == null) continue;
                    object x = i;
                    images2.Items.Remove(i);
                    (images.Items[k] as FlipViewItem).Content = i;
                }
            };

            verticalButton.Tapped += (a, b) =>
            {
                horizontalButton.IsChecked = false;
                images.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                images2.Visibility = Windows.UI.Xaml.Visibility.Visible;
                foreach (FlipViewItem i in images.Items)
                {
                    if (i.Content == null) continue;
                    object x = i.Content;
                    i.Content = null;
                    images2.Items.Add(x);
                }
            };

            
        }

        void images_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
            {
                Point currentpoint = e.Position;
                if (currentpoint.X - initialpoint.X >= 200)
                {
                    if (images.SelectedIndex == images.Items.Count - 1) Debug.WriteLine("hello");
                    e.Complete();
                }
            }
        }

        public async void FillChapter(Chapter c)
        {
            DataContext = c;
            //if (manga.Chapters.Count > index + 1) previousChapterTitle.Text = manga.Chapters[index + 1].Name;
            //if (index - 1 >= 0) nextChapterTitle.Text = manga.Chapters[index - 1].Name;

            if (!c.Saved)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
                StatusBar.GetForCurrentView().ProgressIndicator.Text = "Loading pages...";
                // Chapter c = DataContext as Chapter;
                if (c == null) return;
                
                

                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            else
            {
                for (int x = 0; x < c.Images.Count(); x++)
                {
                    FlipViewItem item = new FlipViewItem();

                    images.Items.Add(item);
                }
                int count = 0;
                foreach (string link in c.Images)
                {
                    (images.Items.ElementAt(count) as FlipViewItem).Content = new ScrollViewer
                    {
                        Content = new Image
                        {
                            Source = new BitmapImage(new Uri(link))
                        },
                        ZoomMode = ZoomMode.Enabled,
                        MaxZoomFactor = 3,
                        MinZoomFactor = 1
                    };
                    count++;
                }
            }
        }

       

        
    }
}
