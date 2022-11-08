namespace GPM_AGV_LAT_CORE.Emulators.GangHao
{

    /// <summary>
    /// 罡豪AGVC通訊Port響應模擬
    /// </summary>
    public class GangHaoAGVCEmulate
    {
        /// <summary>
        /// 模擬車IP
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 罡豪AGVC通訊Port響應模擬
        /// </summary>
        /// <param name="ip">車子IP</param>
        public GangHaoAGVCEmulate(string ip)
        {
            IP = ip;
            stateEmulator = new GangHaoAgvc_StateEmulator(ip, 19204);
            controlEmulator = new GangHaoAgvc_ControlEmulator(ip, 19205);
            mapEmulator = new GangHaoAgvc_MapEmulator(ip, 19206, stateEmulator);
        }
        /// <summary>
        /// 狀態模擬
        /// </summary>
        public GangHaoAgvc_StateEmulator stateEmulator { get; set; }
        /// <summary>
        /// 控制模擬
        /// </summary>
        public GangHaoAgvc_ControlEmulator controlEmulator { get; set; }
        /// <summary>
        /// 導航模擬
        /// </summary>
        public GangHaoAgvc_MapEmulator mapEmulator { get; set; }

        /// <summary>
        /// 把各個Server打開 開始監聽連線與訊息
        /// </summary>
        public void Start()
        {
            stateEmulator.Start();
            controlEmulator.Start();
            mapEmulator.Start();
        }

    }
}
