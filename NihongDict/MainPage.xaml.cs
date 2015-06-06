using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NihongDict.Resources;
using NihongDict.util;
using System.IO;
using System.Windows.Media;
using Microsoft.Phone.Tasks;
using System.Threading;

namespace NihongDict
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string dictDirPath = "dictionarys/";

        private string[] dictList;
        private Dictionary dict;
        private HistoryUtil hstr;

        private string thisWord;
        private Int32 thisIndex;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            dictList = this.getDictList();
            displayDicList();

            hstr = new HistoryUtil(100);
            hstr.loadHistory();
            displayHistory();

            // 检查是否是第一次运行，如果是，打个广告.
            if (!File.Exists("CheckFirst"))
            {
                About_Tap(null, null);
                File.Create("CheckFirst");
            }
            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }

        // 为 ViewModel 项加载数据
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private bool shouldExit = false;
        // 重载后退按钮
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.candidatePanel.Visibility == System.Windows.Visibility.Visible)
            {
                this.candidatePanel.Visibility = System.Windows.Visibility.Collapsed;
                e.Cancel = true;
            }
            if (!shouldExit)
            {
                e.Cancel = true;
                shouldExit = true;
                Thread t = new Thread(countTime);
                t.Start();
            }
        }

        private void countTime()
        {
            Dispatcher.BeginInvoke(new Action(againToExitVisible));
            Thread.Sleep(1500);
            shouldExit = false;
            Dispatcher.BeginInvoke(new Action(againToExitInvisible));
        }

        private void againToExitInvisible()
        {
            againToExitPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void againToExitVisible()
        {
            againToExitPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void selectDict(int n)
        {
            selectDict(dictList[n]);
        }

        private void selectDict(string name)
        {
            dict = new Dictionary(dictDirPath + name + ".dict", dictDirPath + name + ".idx", dictDirPath + name + ".ifo");
            dict.onWordReferenced = this.onWordReferenced;
            if (candidatePanel.Visibility == System.Windows.Visibility.Visible)
            {
                TextBox_TextChanged(null, null);
            }
        }

        private string[] getDictList()
        {
            // 文件第一行是字典个数，后面每行一个词典名
            FileStream fs = new FileStream(dictDirPath + "dictList.dat", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            int listLen = Convert.ToInt32(sr.ReadLine());
            string[] dictList = new string[listLen];
            for (int i = 0; i < listLen; ++i)
                dictList[i] = sr.ReadLine();
            sr.Close();
            fs.Close();

            return dictList;
        }

        private void displayDicList()
        {
            dictListPanel.Children.Clear();
            for (int i = 0; i < dictList.Length; ++i)
            {
                RadioButton rb = new RadioButton();
                rb.GroupName = "dictGroup";
                TextBlock tb = new TextBlock();
                tb.Text = dictList[i];
                rb.Content = tb;
                rb.Checked += rb_Checked;
                dictListPanel.Children.Add(rb);
            }
            this.selectDict(0);
            ((RadioButton)dictListPanel.Children[0]).IsChecked = true;
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            this.selectDict(((TextBlock)(((RadioButton)sender).Content)).Text);
        }

        private void displayHistory()
        {
            historyPanel.Children.Clear();

            int i = 0;
            foreach (string s in hstr.bufferQueue.Reverse<string>())
            {
                TextBlock tb = new TextBlock();
                StackPanel sp = new StackPanel();

                tb.FontSize = 24;
                tb.Text = s;
                if (i++ % 2 == 0)
                    sp.Background = new SolidColorBrush(Color.FromArgb(255, 33, 128, 159));
                else
                    sp.Background = new SolidColorBrush(Color.FromArgb(255, 33, 159, 128));
                sp.Children.Add(tb);
                sp.Tap += wordBlock_Tap2;
                historyPanel.Children.Add(sp);
            }
        }

        private void wordBlock_Tap2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // 防止弹出候选框
            wordBox.TextChanged -= TextBox_TextChanged;
            thisIndex = dict.getWordIndex(((sender as StackPanel).Children[0] as TextBlock).Text);
            wordBox.Text = dict[thisIndex].Key;
            showDetail(thisIndex);
            wordBox.TextChanged += TextBox_TextChanged;
            candidatePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private string[] separator = new string[] { "\r\n" };
        private void showDetail(int selectedIndex)
        {
            string[] tmp = dict.getContent(selectedIndex).Split(separator, StringSplitOptions.RemoveEmptyEntries);

            TextBlock tb = null;
            detailPanel.Children.Clear();

            tb = new TextBlock();
            tb.Text = dict[selectedIndex].Key;
            tb.FontSize = 40;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 48, 48));
            tb.Margin = new Thickness(0, 5, 0, 5);
            detailPanel.Children.Add(tb);
            for (int i = 0; i < tmp.Length; ++i)
            {
                tb = new TextBlock();
                tb.Text = tmp[i];
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Margin = new Thickness(5, 5, 5, 5);
                detailPanel.Children.Add(tb);
            }
            // 切换到解释标签
            mainPivot.SelectedIndex = 0;
        }


        private void wordBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            thisIndex = (int)((StackPanel)sender).Tag;
            candidatePanel.Children.Clear();

            // 防止弹出候选框
            wordBox.TextChanged -= TextBox_TextChanged;
            wordBox.Text = ((TextBlock)((StackPanel)sender).Children[0]).Text;
            wordBox.TextChanged += TextBox_TextChanged;

            showDetail(thisIndex);
            candidatePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            detailPanel.Children.Clear();

            candidatePanel.Visibility = System.Windows.Visibility.Visible;
            candidatePanel.Children.Clear();
            candidateScroll.ScrollToVerticalOffset(0);


            this.thisIndex = dict.getWordIndex(wordBox.Text);
            this.thisWord = wordBox.Text;

            int index = thisIndex;
            for (int i = 0; i < 50 && index < dict.length; ++i, ++index)
            {
                TextBlock tb = new TextBlock();
                StackPanel sp = new StackPanel();

                tb.Text = dict[index].Key;
                tb.FontSize = 32;

                if (i % 2 == 0)
                    sp.Background = new SolidColorBrush(Color.FromArgb(255, 33, 128, 159));
                else
                    sp.Background = new SolidColorBrush(Color.FromArgb(255, 33, 159, 128));
                sp.Children.Add(tb);
                sp.Tag = index;       // tag保存该单词的下标
                sp.Tap += wordBlock_Tap;
                sp.Width = candidatePanel.Width;
                candidatePanel.Children.Add(sp);
            }
        }


        private void onWordReferenced(string word)
        {
            hstr.newHistory(word);
            hstr.storeHistory();
            displayHistory();
        }

        private void ClearHistory_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBoxResult.Cancel == MessageBox.Show("确认清除历史？", "警告", MessageBoxButton.OKCancel))
                return;
            hstr.clear();
            hstr.storeHistory();
            displayHistory();
        }

        private void About_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("学生一名，自己做了自己用。分享出来，希望大家喜欢^.^\r\n\r\n免费无广告\r\n\r\n想去日本溜达溜达但是没钱\r\n\r\n希望喜欢本软件的土豪愿意的可以打赏一点……支付渠道准备中……");
        }

        private void goodPraise_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void donate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.Show();
        }

        private void PrevWord_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.thisIndex <= 0)
                return;
            // 防止弹出候选框
            thisIndex--;
            wordBox.TextChanged -= TextBox_TextChanged;
            this.wordBox.Text = dict[thisIndex].Key;
            showDetail(thisIndex);
            wordBox.TextChanged += TextBox_TextChanged;
            candidatePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void NextWord_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.thisIndex >= dict.length)
                return;
            // 防止弹出候选框
            thisIndex++;
            wordBox.TextChanged -= TextBox_TextChanged;
            this.wordBox.Text = dict[thisIndex].Key;
            showDetail(thisIndex);
            wordBox.TextChanged += TextBox_TextChanged;
            candidatePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}