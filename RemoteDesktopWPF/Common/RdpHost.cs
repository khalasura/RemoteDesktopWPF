using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopWPF.Common
{
    public class RdpHost : BindableBase
    {
        public string Server { get; set; }                  // IP
        public string UserName { get; set; }                // 계정
        public string ClearTextPassword { get; set; }       // 비밀번호
        public int ColorDepth { get; set; }                 // 색 품질
        public bool FullScreen { get; set; }                // 전체화면 여부
        public string Description { get; set; }             // 서버설명  
        public bool SmartSizing { get; set; }               // 자동해상도

        // 선택여부
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }
    }
}
