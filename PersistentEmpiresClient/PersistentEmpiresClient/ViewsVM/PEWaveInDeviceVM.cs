using NAudio.Wave;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEWaveInDeviceVM : ViewModel
    {
        private string _name;
        public int DeviceIndex;

        [DataSourceProperty]
        public string Name
        {
            get => this._name;
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }
        public PEWaveInDeviceVM(WaveInCapabilities device, int index)
        {
            this.Name = device.ProductName;
            this.DeviceIndex = index;
        }
    }
}
