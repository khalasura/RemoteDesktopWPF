using AxMSTSCLib;

namespace RemoteDesktopWPF.Common
{
    public class RdpControl : AxMsRdpClient9NotSafeForScripting
    {      
        // 생성자
        public RdpControl() : base() { }

        // WndProc
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // rdp 클라이언트 구성 요소에서 누락 된 포커스 문제 수정
            if (m.Msg == 0x0021) // WM_MOUSEACTIVATE
            {
                if (!this.ContainsFocus)
                {
                    this.Focus();
                }
            }

            base.WndProc(ref m);
        }
    }
}
