using AxMSTSCLib;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using RemoteDesktopWPF.Common;
using RemoteDesktopWPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RemoteDesktopWPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private Dispatcher dispatcher;
        private MainWindow thisWindow;  // 메인 윈도우
        private Dictionary<string, Form> rdpConnects = null;  // 원격연결된 객체 컬렉션

        #region [Propp]
        // 제목
        private string _title = "PYJ Remote Desktop Protocol";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        // 원격연결 대상 컴퓨터 리스트
        private ObservableCollection<RdpHost> hostList;
        public ObservableCollection<RdpHost> HostList
        {
            get { return hostList; }
            set { SetProperty(ref hostList, value); }
        }

        // 선택된 원격연결 대상 컴퓨터
        private RdpHost selectedHost;
        public RdpHost SelectedHost
        {
            get { return selectedHost; }
            set { SetProperty(ref selectedHost, value); }
        }

        // 상태바
        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }
        #endregion

        // [Ctor] 생성자
        public MainWindowViewModel()
        {
            dispatcher = System.Windows.Application.Current.Dispatcher;
            rdpConnects = new Dictionary<string, Form>();
            Util.Instance.LoadHosts();
            HostList = new ObservableCollection<RdpHost>(Util.Instance.HostList);
            Status = "Ready";
        }

        // [Command] 폼로드
        private DelegateCommand<RoutedEventArgs> loadedCommand;
        public DelegateCommand<RoutedEventArgs> LoadedCommand =>
            loadedCommand ?? (loadedCommand = new DelegateCommand<RoutedEventArgs>(e =>
            {
                if (thisWindow == null)
                {
                    thisWindow = e.Source as MainWindow;
                    thisWindow.xGrid.MouseDoubleClick += XGrid_MouseDoubleClick;
                    //thisWindow.xCheckedAll.Checked += XCheckedAll_Checked;
                    thisWindow.xCheckedAll.Click += XCheckedAll_Click;
                    thisWindow.Focus();
                }
            }));

        // [Event] 체크박스 전체선택, 전체해제
        private void XCheckedAll_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            HostList.ToList().ForEach(g => g.IsChecked = checkBox.IsChecked.Value);
        }

        // [Event] 그리드 더블클릭 시 연결
        private void XGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExecuteConnect(SelectedHost);
        }

        // [Command] 메뉴 버튼
        private DelegateCommand<string> cmdMenu;
        public DelegateCommand<string> CmdMenu =>
            cmdMenu ?? (cmdMenu = new DelegateCommand<string>(async (s)=> 
            {
                switch (s.ToLower())
                {
                    case "add":
                        ExecuteAdd(null);
                        break;
                    case "edit":
                        if (SelectedHost != null)
                            ExecuteAdd(SelectedHost);
                        break;
                    case "delete":
                        HostList.Where(g => g.IsChecked).ToList().ForEach(g => { ExecuteDelete(g); });
                        break;
                    case "connect":
                        await HostList.Where(g => g.IsChecked).ToList().ForEachAsync(async g => 
                        {
                            ExecuteConnect(g);
                            await Task.Delay(500);
                        });
                        break;
                    case "disconnect":
                        HostList.Where(g => g.IsChecked).ToList().ForEach(g => { ExecuteDisconnect(g); });
                        break;
                    default:
                        break;
                }
            }));

        // [Method] 원격서버 추가, 수정
        private async void ExecuteAdd(RdpHost host)
        {
            var view = new RdpHostDialog();
            var viewmodel = view.DataContext as RdpHostDialogViewModel;
            viewmodel.Host = host;
            var result = await DialogHost.Show(view, "RootDialog", (object s, DialogClosingEventArgs e) => 
            {
                if (e.Parameter == null) return;
                var rdpHost = e.Parameter as RdpHost;
                Util.Instance.AddHost(rdpHost);
                HostList = new ObservableCollection<RdpHost>(Util.Instance.HostList);
            });
        }


        // [Method] 원격서버 삭제하기
        private void ExecuteDelete(RdpHost host)
        {
            if (host == null) return;
            Util.Instance.RemoveHost(host);
            HostList = new ObservableCollection<RdpHost>(Util.Instance.HostList);
        }
        // [Method] 원격서버 연결
        private void ExecuteConnect(RdpHost host)
        {
            if (host == null) return;
            CreateRdpClient(host);
        }
        // [Method] 원격서버 연결 끊기
        private void ExecuteDisconnect(RdpHost host)
        {
            if (host == null) return;
            var key = host.Server.Replace(".", "");
            if (rdpConnects.ContainsKey(key))
            {
                var find = rdpConnects[key];
                find.Close();
                find.Dispose();
            }
        }


        // [Method] 원격연결 객체 생성
        private async void CreateRdpClient(RdpHost host)
        {
            await dispatcher.Invoke(async () =>
            {
                string[] ServerIps = host.Server.Split(':');

                // 원격연결 표시 폼 생성
                Form rdpFrom = new Form();
                rdpFrom.ShowIcon = false;
                rdpFrom.Name = host.Server.Replace(".", "");
                rdpFrom.Text = $"({rdpFrom.Name}) {host.Description}";
                rdpFrom.Size = new System.Drawing.Size(1024, 768);
                rdpFrom.FormClosed += new FormClosedEventHandler(this.rdpForm_Closed);

                var rdp = new RdpControl();

                // 이미 연결된 경우
                if (rdpConnects.ContainsKey(rdpFrom.Name))
                {
                    Status = $"({host.Server}) 이미 연결 되었습니다.";
                    return;
                }
                // 연결 컬렉션에 추가
                rdpConnects.Add(rdpFrom.Name, rdpFrom);

                // 원격연결 초기화 설정
                ((System.ComponentModel.ISupportInitialize)(rdp)).BeginInit();
                rdp.Dock = DockStyle.Fill;
                rdp.Enabled = true;
                rdp.OnConnecting += new EventHandler(this.rdp_OnConnecting);
                rdp.OnDisconnected += new IMsTscAxEvents_OnDisconnectedEventHandler(this.rdp_OnDisconnected);
                rdp.OnConnected += Rdp_OnConnected;
                rdpFrom.Controls.Add(rdp);
                rdpFrom.WindowState = FormWindowState.Maximized;
                rdpFrom.Show();
                ((System.ComponentModel.ISupportInitialize)(rdp)).EndInit();

                // 원격연결 객체 속성 설정
                rdp.Name = rdpFrom.Name;
                rdp.Server = ServerIps.Length == 1 ? host.Server : ServerIps[0];
                rdp.UserName = host.UserName;
                rdp.AdvancedSettings7.RDPPort = ServerIps.Length == 1 ? 3389 : Convert.ToInt32(ServerIps[1]);
                //rdp.AdvancedSettings7.ContainerHandledFullScreen = 1;
                rdp.AdvancedSettings7.SmartSizing = host.SmartSizing;
                rdp.AdvancedSettings7.EnableCredSspSupport = true;
                rdp.AdvancedSettings7.ClearTextPassword = host.ClearTextPassword;
                //rdp.AdvancedSettings7.PublicMode = false;
                rdp.ColorDepth = host.ColorDepth;
                rdp.FullScreen = host.FullScreen;
                //rdp.DesktopWidth = rdpFrom.ClientRectangle.Width;
                //rdp.DesktopHeight = rdpFrom.ClientRectangle.Height;
                var screenRect = Screen.PrimaryScreen.Bounds;
                rdp.DesktopWidth = screenRect.Width;
                rdp.DesktopHeight = screenRect.Height;

                // 연결
                rdp.Connect();
                await Task.Delay(100);
            });



        }

        // [Event] 원격연결 OnConnected
        private void Rdp_OnConnected(object sender, EventArgs e)
        {
            var rdp = sender as AxMsRdpClient9NotSafeForScripting;
            
            Status = $"({rdp.Server}) 에 연결되었습니다.";
        }


        // [Event] 원격연결 OnDisconnected
        private void rdp_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
        {
            var rdp = sender as AxMsRdpClient9NotSafeForScripting;
            var test = rdp.GetErrorDescription(0, 0);
            var test2 = rdp.ExtendedDisconnectReason;
            string msg = $"원격 데스크탑 {rdp.Server} 연결이 끊겼습니다!";
            rdp.DisconnectedText = msg;
            var form = rdp.FindForm();
            //form.Close();
            //form.Dispose();
            //rdp.Dispose();
            Status = msg;            
        }

        // [Event] 원격연결 OnConnecting
        private void rdp_OnConnecting(object sender, EventArgs e)
        {
            var rdp = sender as AxMsRdpClient9NotSafeForScripting;
            rdp.ConnectingText = rdp.GetStatusText(Convert.ToUInt32(rdp.Connected));
            rdp.FindForm().WindowState = FormWindowState.Normal;
        }

        // [Event] 원격연결 창 Closed
        private void rdpForm_Closed(object sender, FormClosedEventArgs e)
        {
            Form frm = (Form)sender;
            foreach (Control ctrl in frm.Controls)
            {
                if (ctrl.GetType().ToString() == "RemoteDesktopWPF.Common.RdpControl")
                {
                    if (rdpConnects.ContainsKey(ctrl.Name)) 
                        rdpConnects.Remove(ctrl.Name);

                    var rdp = ctrl as RdpControl;
                    if (rdp.Connected != 0)
                    {
                        rdp.Disconnect();
                        rdp.Dispose();
                    }
                }
            }
        }
    }
}
