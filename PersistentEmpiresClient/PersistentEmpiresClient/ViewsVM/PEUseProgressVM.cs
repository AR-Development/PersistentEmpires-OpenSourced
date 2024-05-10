using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEUseProgressVM : ViewModel
    {
        private bool _isActive;
        private string _progressTitle;
        private int _pastDuration;
        private long _startedAt;
        private int _countDownTime;

        public PEUseProgressVM()
        {
            PEInformationManager.OnStartCounter += this.StartCounter;
            PEInformationManager.OnStopCounter += this.StopCounter;
        }
        public void StartCounter(string progressTitle, int seconds)
        {
            this.CountDownTime = seconds;
            this._startedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.IsActive = true;
            this.ProgressTitle = progressTitle;
        }

        public void StopCounter()
        {
            this.CountDownTime = 0;
            this.PastDuration = 0;
            this.IsActive = false;
        }

        [DataSourceProperty]
        public int CountDownTime
        {
            get => this._countDownTime;
            set
            {
                if (value != this._countDownTime)
                {
                    this._countDownTime = value;
                    base.OnPropertyChangedWithValue(value, "CountDownTime");
                }
            }
        }
        [DataSourceProperty]
        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    base.OnPropertyChangedWithValue(value, "IsActive");
                }
            }
        }
        [DataSourceProperty]
        public string ProgressTitle
        {
            get => this._progressTitle;
            set
            {
                if (value != this._progressTitle)
                {
                    this._progressTitle = value;
                    base.OnPropertyChangedWithValue(value, "ProgressTitle");
                }
            }
        }
        [DataSourceProperty]
        public int PastDuration
        {
            get => this._pastDuration;
            set
            {
                if (value != this._pastDuration)
                {
                    this._pastDuration = value;
                    base.OnPropertyChangedWithValue(value, "PastDuration");
                }
            }
        }

        public void Tick(float dt)
        {
            if (this.IsActive)
            {
                if (this.PastDuration > this.CountDownTime)
                {
                    this.StopCounter();
                }
                this.PastDuration = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds() - this._startedAt);
            }
        }
    }
}
