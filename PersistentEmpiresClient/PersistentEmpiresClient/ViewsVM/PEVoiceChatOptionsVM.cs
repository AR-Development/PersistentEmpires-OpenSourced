using NAudio.Wave;
using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEVoiceChatOptionsVM : ViewModel
    {
        private SelectorVM<SelectorItemVM> _microphones;
        private int _inputGain;
        private SelectorVM<SelectorItemVM> _outputDevices;
        private int _outputGain;
        private bool _voiceChatEnabled;
        private Action onClose;
        private Action<int, int> onStartTest;
        private Action onStopTest;
        private Action<int, int, int, int, bool> onApply;
        private int selectedOutputDevice;
        private int selectedInputDevice;
        private bool _testing = false;

        public PEVoiceChatOptionsVM(List<WaveInCapabilities> devices, List<WaveOutCapabilities> outputDevices, bool voiceChatEnabled, Action onClose, Action<int, int, int, int, bool> onApply, Action<int, int> onStartTest, Action onStopTest)
        {
            this.Microphones = new SelectorVM<SelectorItemVM>(0, OnChangeMicrophone);
            this.InputGain = 100;
            for (int i = 0; i < devices.Count; i++)
            {
                WaveInCapabilities device = devices[i];
                this.Microphones.AddItem(new SelectorItemVM(device.ProductName));
            }

            this.OutputDevices = new SelectorVM<SelectorItemVM>(0, OnChangeOutput);

            for (int i = 0; i < outputDevices.Count; i++)
            {
                WaveOutCapabilities device = outputDevices[i];
                this.OutputDevices.AddItem(new SelectorItemVM(device.ProductName));
            }
            this.OutputDevices.SelectedIndex = 0;
            this.Microphones.SelectedIndex = 0;
            this.OutputGain = 100;
            this.onClose = onClose;
            this.onApply = onApply;
            this.VoiceChatEnabled = voiceChatEnabled;
            this.onStartTest = onStartTest;
            this.onStopTest = onStopTest;
        }

        private void OnChangeOutput(SelectorVM<SelectorItemVM> selected)
        {
            this.selectedOutputDevice = selected.SelectedIndex;
        }

        public void OnChangeMicrophone(SelectorVM<SelectorItemVM> selected)
        {
            this.selectedInputDevice = selected.SelectedIndex;
        }

        public void ExecuteTest()
        {
            if (this.IsTesting)
            {
                this.IsTesting = false;
                this.onStopTest();
            }
            else
            {
                this.IsTesting = true;
                this.onStartTest(this.selectedInputDevice, InputGain);
            }
        }

        public void ExecuteApply()
        {
            this.onApply(this.selectedInputDevice, this.InputGain, this.selectedOutputDevice, this.OutputGain, this.VoiceChatEnabled);
        }

        public void ExecuteClose()
        {
            this.onClose();
        }

        [DataSourceProperty]
        public bool IsTesting
        {
            get => this._testing;
            set
            {
                if (value != this._testing)
                {
                    this._testing = value;
                    base.OnPropertyChangedWithValue(value, "IsTesting");
                    base.OnPropertyChanged("TestText");
                    base.OnPropertyChanged("TestingEnabled");
                }
            }
        }

        [DataSourceProperty]
        public bool TestingEnabled
        {
            get => !this._testing;
        }

        [DataSourceProperty]
        public string TestText
        {
            get => this._testing ? "Finish Testing" : "Test Microphone";
        }

        [DataSourceProperty]
        public bool VoiceChatEnabled
        {
            get => this._voiceChatEnabled;
            set
            {
                if (value != this._voiceChatEnabled)
                {
                    this._voiceChatEnabled = value;
                    base.OnPropertyChangedWithValue(value, "VoiceChatEnabled");
                }
            }
        }

        [DataSourceProperty]
        public int OutputGain
        {
            get => this._outputGain;
            set
            {
                if (value != this._outputGain)
                {
                    this._outputGain = value;
                    base.OnPropertyChangedWithValue(value, "OutputGain");
                }
            }
        }

        [DataSourceProperty]
        public int InputGain
        {
            get => this._inputGain;
            set
            {
                if (value != this._inputGain)
                {
                    this._inputGain = value;
                    base.OnPropertyChangedWithValue(value, "InputGain");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> Microphones
        {
            get => this._microphones;
            set
            {
                if (value != this._microphones)
                {
                    this._microphones = value;
                    base.OnPropertyChangedWithValue(value, "Microphones");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> OutputDevices
        {
            get => this._outputDevices;
            set
            {
                if (value != this._outputDevices)
                {
                    this._outputDevices = value;
                    base.OnPropertyChangedWithValue(value, "OutputDevices");
                }
            }
        }

    }
}
