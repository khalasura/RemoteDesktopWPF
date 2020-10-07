using Prism.Commands;
using Prism.Mvvm;
using RemoteDesktopWPF.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteDesktopWPF.ViewModels
{
    public class RdpHostDialogViewModel : BindableBase
    {
        // 원격연결 객체
        private RdpHost host;
        public RdpHost Host
        {
            get 
            {
                if (host == null)
                {
                    host = new RdpHost
                    {
                        ColorDepth = 32,
                        FullScreen = false,
                        SmartSizing = true
                    };
                }
                return host; 
            }
            set { SetProperty(ref host, value); }
        }

        // 색품질 콤보박스 컬렉션
        private List<int> colorDepth;
        public List<int> ColorDepth
        {
            get { return colorDepth; }
            set { SetProperty(ref colorDepth, value); }
        }

        // 생성자
        public RdpHostDialogViewModel()
        {
            ColorDepth = new List<int> { 8, 16, 24, 32 };


        }
    }
}
