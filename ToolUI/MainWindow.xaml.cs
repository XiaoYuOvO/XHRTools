using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using CsvHelper.Configuration;
using XHRTools;
using XHRTools.requests;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace ToolUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<User> _userList = new List<User>();
        private int _success;
        private int _fail;
        private int _totalFinish;
        private bool _requestStop;
        private readonly BigLearnClient _bigLearnClient = new BigLearnClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomSessionId == null) return;
            CustomSessionId.Opacity = 100;
            CustomSessionId.IsEnabled = true;
        }

        private void LatestSessionCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if (CustomSessionId == null) return;
            CustomSessionId.Opacity = 0;
            CustomSessionId.IsEnabled = false;
        }

        private void ImportUserListButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            openFileDialog.DefaultExt = ".csv";
            switch (openFileDialog.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.None:
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                    break;
                case System.Windows.Forms.DialogResult.Abort:
                    break;
                case System.Windows.Forms.DialogResult.Retry:
                    break;
                case System.Windows.Forms.DialogResult.Ignore:
                    break;
                case System.Windows.Forms.DialogResult.OK:
                case System.Windows.Forms.DialogResult.Yes:
                    LoadCsvUserList(openFileDialog.OpenFile());
                    break;
                case System.Windows.Forms.DialogResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void LoadCsvUserList(Stream stream)
        {
            var readerConfiguration = new CsvConfiguration(CultureInfo.CurrentUICulture)
            {
                HasHeaderRecord = false
            };
            var csvReader = new CsvHelper.CsvReader(new StreamReader(stream, Encoding.Default),readerConfiguration);
            _userList.AddRange(csvReader.GetRecords<User>());
            LoadedUserLabel.SetValue(ContentProperty,"已加载: "+(_userList.Count) + "个账号");
        }

        private void StartToLearn(int sessionId)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            ClearStatus();
            StartButton.IsEnabled = false;
            ImportUserListButton.IsEnabled = false;
            logBox.AppendText($"[信息] ===========开始学习,期数: {sessionId}================\n");
            backgroundWorker.DoWork += (sender, args) =>
            {
                foreach (var user in _userList)
                {
                    _bigLearnClient.ClearCookie();
                    try
                    {
                        _bigLearnClient.PostRequest(new LoginRequest(user));
                        _bigLearnClient.PostRequest(new LearnHitRequest(sessionId));
                        backgroundWorker.ReportProgress(_totalFinish / _userList.Count, WorkerStatus.UpdateLog("[信息] 账号成功学习: " + user.Username + "\n"));
                        _success += 1;
                    }
                    catch (Exception e)
                    {
                        backgroundWorker.ReportProgress(_totalFinish / _userList.Count, WorkerStatus.UpdateLog($"[错误] 目标账号({user.Username}:{user.Id})无法学习: " + e.Message + "\n"));
                        _fail += 1;
                    }
                    backgroundWorker.ReportProgress(_totalFinish / _userList.Count, WorkerStatus.UpdateProgress());
                    if (_requestStop)
                    {
                        _requestStop = false;
                        break;
                    }
                }
                backgroundWorker.ReportProgress(100, WorkerStatus.UpdateLog("[信息] ===========大学习完成!================" + "\n"));
            };
            backgroundWorker.ProgressChanged += (sender, args) =>
            {
                ((WorkerStatus)args.UserState).UpdateUi(this);
                if (args.ProgressPercentage != 100) return;
                ImportUserListButton.IsEnabled = true;
                StartButton.IsEnabled = true;
                EndButton.IsEnabled = false;
            };
            backgroundWorker.RunWorkerAsync();
        }

        private void ClearStatus()
        {
            _success = 0;
            _fail = 0;
            _totalFinish = 0;
            EndButton.IsEnabled = true;
            ProgressBar.SetValue(RangeBase.ValueProperty, 0d);
            logBox.Text = "";
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            _totalFinish = _success + _fail;
            SuccessCountLabel.SetValue(ContentProperty, $"成功:{_success}");
            FailCountLabel.SetValue(ContentProperty, $"失败:{_fail}");
            ProgressBar.SetValue(RangeBase.ValueProperty, (double)_totalFinish);
            ProgressBar.SetValue(RangeBase.MaximumProperty, (double)_userList.Count);
        }

        private void EndButton_OnClick(object sender, RoutedEventArgs e)
        {
            _requestStop = true;
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_userList.Count == 0)
            {
                ShowErrorMsg(Properties.Resources.error_no_userinfo);
                return;
            }
            var targetSession = 0;
            if (LatestSessionCheck.IsChecked != null && LatestSessionCheck.IsChecked.Value)
            {
                try
                {
                    targetSession = _bigLearnClient.PostRequest(new GetLatestSessionRequest()).SessionId;
                }
                catch (Exception exception)
                {
                    ShowErrorMsg(Properties.Resources.error_cannot_get_latest_session + exception.Message);
                }
                
            }
            else
            {
                targetSession = int.Parse((string)CustomSessionId.GetValue(TextBox.TextProperty));
            }

            if (targetSession == 0)
            {
                ShowErrorMsg(Properties.Resources.error_invalid_session_id + targetSession);
            }
            StartToLearn(targetSession);
        }

        private static void ShowErrorMsg([Localizable(true)]string msg)
        {
            MessageBox.Show(msg, Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private delegate void UiUpdate(MainWindow mainWindow);

        private class WorkerStatus
        {
            private readonly UiUpdate _updateFunc;

            private WorkerStatus(UiUpdate updateFunc)
            {
                _updateFunc = updateFunc;
            }
            
            public void UpdateUi(MainWindow window)
            {
                _updateFunc.Invoke(window);
            }

            public static WorkerStatus UpdateLog(string content)
            {
                return new WorkerStatus((window => window.logBox.AppendText(content)));
            }

            public static WorkerStatus UpdateProgress()
            {
                return new WorkerStatus(window => window.UpdateProgress());
            }
        }
    }
}