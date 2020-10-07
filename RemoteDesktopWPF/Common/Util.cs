using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopWPF.Common
{
    public class Util
    {

        private static Util singleton = null;
        // 원격서버 목록 파일 저장 경로
        private string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $@"\{System.Diagnostics.Process.GetCurrentProcess().ProcessName}_RdpHost.json";
        // 원격서버 목록 
        public List<RdpHost> HostList { get; set; }

        public Util()
        {

        }

        public static Util Instance 
        { 
            get 
            {
                if (singleton == null)
                    singleton = new Util();
                return singleton;
            } 
        }

        public void AddHost(RdpHost host)
        {
            var find = HostList.FirstOrDefault(g => g.Server.Equals(host.Server));
            if (find == null)
                HostList.Add(host);
            else
                find = host;
            SaveHosts();
        }

        public void RemoveHost(RdpHost host)
        {
            var find = HostList.FirstOrDefault(g => g.Server.Equals(host.Server));
            if (find != null)
                HostList.Remove(find);
            SaveHosts();
        }

        // 원격서버 목록 불러오기
        public void LoadHosts()
        {
            if (File.Exists(FilePath))
            {
                HostList = File.ReadAllText(FilePath).FromJson<List<RdpHost>>();
            }
            else
            {
                HostList = new List<RdpHost>();
            }
        }

        // 원격서버 목록 저장
        public void SaveHosts()
        {
            File.WriteAllText(FilePath, HostList.ToJson());
        }
    }
}
