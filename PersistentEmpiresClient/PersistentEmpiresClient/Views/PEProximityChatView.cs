using Concentus.Structs;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PlayerVoiceData
    {
        public NetworkCommunicator Player;
        public BufferedWaveProvider InputWaveProvider;
        public VolumeWaveProvider16 VolumeSampleProvider;
        public OpusDecoder OpusDecoder;
        public int RecordedMs;
        public bool HasVoiceData;
        public PlayerVoiceData(NetworkCommunicator player)
        {
            this.Player = player;
            this.InputWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            this.VolumeSampleProvider = new VolumeWaveProvider16(this.InputWaveProvider);
            this.OpusDecoder = new OpusDecoder(48000, 1);
            this.RecordedMs = 0;
        }

        public void AddSample(byte[] outputByte, int offset, int length)
        {
            try
            {
                InputWaveProvider.AddSamples(outputByte, offset, length);
            }
            catch (Exception e)
            {
                InputWaveProvider.ClearBuffer();
            }
        }

        public byte[] ReadSample(int size)
        {
            byte[] readedByte = new byte[size * 2];
            this.VolumeSampleProvider.Read(readedByte, 0, size * 2);
            return readedByte;
        }
    }
    public class PEProximityChatView : MissionView
    {
        private ProximityChatComponent _proximityChatComponent;
        private bool IsActive = false;
        private bool IsRecording = false;
        private bool SelfRecording = false;
        private bool VoiceChatEnabled = true;
        private OpusEncoder opusEncoder;
        private float inputGain = 1f;
        // private OpusDecoder opusDecoder;
        private WaveInEvent sourceStream;
        private WaveOutEvent outputStream;
        private BufferedWaveProvider bufferedWaveProvider;
        private VolumeSampleProvider outputVolume;
        private Dictionary<NetworkCommunicator, PlayerVoiceData> _playerVoiceData = new Dictionary<NetworkCommunicator, PlayerVoiceData>();

        private List<NetworkCommunicator> _tempHaveData = new List<NetworkCommunicator>();
        private BufferedWaveProvider selfBufferedWaveProvider;
        private WaveOutEvent selfOutputStream;
        private GauntletLayer _gauntletLayer;
        private PEVoiceChatOptionsVM _dataSource;

        public delegate void HandlePeerVoiceStatusUpdated(PlayerVoiceData playerVoiceData, NetworkCommunicator player);
        public event HandlePeerVoiceStatusUpdated OnPeerVoiceStatusUpdated;

        private int bufferMs = 180;

        private void InitializeVoiceChatComponents()
        {
            opusEncoder = OpusEncoder.Create(48000, 1, Concentus.Enums.OpusApplication.OPUS_APPLICATION_VOIP);
            opusEncoder.Bitrate = 16 * 1024;

            // opusDecoder = OpusDecoder.Create(48000, 1);

            sourceStream = new WaveInEvent();
            sourceStream.BufferMilliseconds = bufferMs;
            sourceStream.NumberOfBuffers = 1;
            sourceStream.WaveFormat = new WaveFormat(48000, 16, 1);

            sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(OnVoiceRecord);
            // sourceStream.StartRecording();

            selfBufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            selfOutputStream = new WaveOutEvent();
            selfOutputStream.Init(selfBufferedWaveProvider);

            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                this._playerVoiceData[peer] = new PlayerVoiceData(peer);
            }

            outputStream = new WaveOutEvent();

            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            outputVolume = new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider());
            outputStream.Init(outputVolume);
            outputStream.Play();


            // selfOutputStream.Play();


        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            var networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            networkMessageHandlerRegisterer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(this.HandleEnableVoiceChat));
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            var networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
            networkMessageHandlerRegisterer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(this.HandleEnableVoiceChat));
        }        

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
        }

        private void HandleEnableVoiceChat(GameNetworkMessage networkMessage)
        {
            var message = networkMessage as EnableVoiceChat;
            if (message != null)
            {
                VoiceChatEnabled = message.Enabled;
                if (!message.Enabled)
                {
                    return;
                }

                this._proximityChatComponent = base.Mission.GetMissionBehavior<ProximityChatComponent>();
                if (this._proximityChatComponent != null)
                {
                    this.IsActive = true;
                    this._proximityChatComponent.OnVoicePlayMessage += HandleVoicePlayMessage;
                    this._proximityChatComponent.OnVoicePlayerJoined += HandleVoicePlayerJoined;
                    this._proximityChatComponent.OnVoicePlayerLeaved += HandleVoicePlayerLeaved;
                    this._proximityChatComponent.OnOptionsClicked += HandleOptionsClicked;
                }
                try
                {
                    InitializeVoiceChatComponents();
                }
                catch (Exception e)
                {
                    this.IsActive = false;
                    this.VoiceChatEnabled = false;
                }

                int waveInDevices = WaveIn.DeviceCount;
                List<WaveInCapabilities> devices = new List<WaveInCapabilities>();
                for (int waveInDevice = -1; waveInDevice < waveInDevices; waveInDevice++)
                {
                    try
                    {
                        WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                        devices.Add(deviceInfo);
                    }
                    catch (Exception e)
                    {
                        // 
                    }
                }

                List<WaveOutCapabilities> outDevices = new List<WaveOutCapabilities>();
                int waveOutDevices = WaveOut.DeviceCount;
                for (int waveOutDevice = -1; waveOutDevice < waveOutDevices; waveOutDevice++)
                {
                    try
                    {
                        outDevices.Add(WaveOut.GetCapabilities(waveOutDevice));
                    }
                    catch (Exception e)
                    {
                        // 
                    }
                }

                this._dataSource = new PEVoiceChatOptionsVM(devices, outDevices, true, this.CloseOptions, this.ApplyOptions, this.StartTest, this.StopTest);
            }
        }

        private void CloseOptions()
        {
            if (this._gauntletLayer == null || this.MissionScreen == null) return;
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }
        private void ApplyOptions(int inputDevice, int inputGain, int outputDevice, int outputGain, bool vcEnabled)
        {
            outputStream.DeviceNumber = outputDevice;
            sourceStream.DeviceNumber = inputDevice;

            outputVolume.Volume = (outputGain / 100f);
            this.inputGain = (inputGain / 100f);
            this.VoiceChatEnabled = vcEnabled;

            try
            {
                if (this.IsActive == false)
                {
                    outputStream.Play();
                    this.IsActive = true;
                    this.VoiceChatEnabled = true;
                }
            }
            catch (Exception e)
            {
                this.IsActive = false;
                this.VoiceChatEnabled = false;
            }

            // this.CloseOptions();
        }

        private void StartTest(int device, int gain)
        {
            if (this.IsActive == false || this.selfBufferedWaveProvider == null || this.sourceStream == null || this.selfOutputStream == null)
            {
                return;
            }
            this.SelfRecording = true;
            this.selfBufferedWaveProvider.ClearBuffer();
            this.sourceStream.DeviceNumber = device;
            this.inputGain = (gain / 100f);
            this.selfOutputStream.Play();
            this.sourceStream.StartRecording();
        }
        private void StopTest()
        {
            if (this.IsActive == false)
            {
                return;
            }
            this.SelfRecording = false;
            this.selfBufferedWaveProvider.ClearBuffer();
            this.selfOutputStream.Pause();
            this.sourceStream.StopRecording();
        }

        private void HandleOptionsClicked()
        {
            this.OpenOptionsMenu();
        }
        private void OpenOptionsMenu()
        {
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEVoiceChatOptions", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
        }

        private void HandleVoicePlayerLeaved(NetworkCommunicator player)
        {
            if (this._playerVoiceData.ContainsKey(player))
            {
                this._playerVoiceData.Remove(player);
            }
        }

        private void HandleVoicePlayerJoined(NetworkCommunicator player)
        {
            _playerVoiceData[player] = new PlayerVoiceData(player);
        }

        private void OnVoiceRecord(object sender, WaveInEventArgs e)
        {
            if (this.IsActive == false || this.VoiceChatEnabled == false) return;
            if (this.IsRecording)
            {
                short[] pcmBuffer = new short[48 * bufferMs];
                Buffer.BlockCopy(e.Buffer, 0, pcmBuffer, 0, e.BytesRecorded);
                for (int i = 0; i < 48 * bufferMs; i++)
                {
                    pcmBuffer[i] = (short)((float)pcmBuffer[i] * inputGain);
                }

                int x = 0;
                int size = 2880;
                short[][] encodeChunks = pcmBuffer.GroupBy(s => x++ / size).Select(s => s.ToArray()).ToArray();

                byte[][] byteBatch = new byte[encodeChunks.Length][];
                int[] lengths = new int[encodeChunks.Length];
                for (int i = 0; i < encodeChunks.Length; i++)
                {
                    byteBatch[i] = new byte[1440];
                    int length = opusEncoder.Encode(encodeChunks[i], 0, 2880, byteBatch[i], 0, byteBatch[i].Length);
                    lengths[i] = length;
                }
                this._proximityChatComponent.SendEncodedVoiceToServer(byteBatch, lengths);
            }
            else if (this.SelfRecording)
            {
                short[] pcmBuffer = new short[48 * bufferMs];
                Buffer.BlockCopy(e.Buffer, 0, pcmBuffer, 0, e.BytesRecorded);
                for (int i = 0; i < 48 * bufferMs; i++)
                {
                    pcmBuffer[i] = (short)((float)pcmBuffer[i] * inputGain);
                }
                byte[] manipulated = new byte[e.BytesRecorded];
                Buffer.BlockCopy(pcmBuffer, 0, manipulated, 0, e.BytesRecorded);
                selfBufferedWaveProvider.AddSamples(manipulated, 0, e.BytesRecorded);
            }
        }

        public void HandleVoicePlayMessage(SendBatchVoiceToPlay message)
        {
            if (this.VoiceChatEnabled == false || this.IsActive == false || message.Peer == null) return;
            if (this._playerVoiceData.ContainsKey(message.Peer))
            {
                int srcOffset = 0;
                for (int i = 0; i < message.BufferLens.Length; i++)
                {
                    short[] outPcm = new short[5000];
                    byte[] buffer = new byte[message.BufferLens[i]];
                    Buffer.BlockCopy(message.PackedBuffer, srcOffset, buffer, 0, message.BufferLens[i]);
                    srcOffset += message.BufferLens[i];
                    int length = this._playerVoiceData[message.Peer].OpusDecoder.Decode(buffer, 0, message.BufferLens[i], outPcm, 0, outPcm.Length);
                    byte[] outputByte = new byte[length * 2];
                    Buffer.BlockCopy(outPcm, 0, outputByte, 0, outputByte.Length);
                    this._playerVoiceData[message.Peer].AddSample(outputByte, 0, outputByte.Length);
                    if (this._tempHaveData.Contains(message.Peer) == false)
                    {
                        this._tempHaveData.Add(message.Peer);
                    }
                    if (this.OnPeerVoiceStatusUpdated != null) this.OnPeerVoiceStatusUpdated(this._playerVoiceData[message.Peer], message.Peer);
                }
            }
        }

        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);
            if (bufferedWaveProvider == null) return;
            if (GameNetwork.MyPeer == null || GameNetwork.MyPeer.ControlledAgent == null) return;
            if (this.IsActive == false || this.VoiceChatEnabled == false) return;

            Agent controlledAgent = GameNetwork.MyPeer.ControlledAgent;
            if (controlledAgent == null) return;
            int pcmSize = MathF.Ceiling(dt * 1000 * 48);

            bool hasDataToPlay = false;
            short[] mixedData = new short[pcmSize];
            int count = 0;
            foreach (NetworkCommunicator peer in this._tempHaveData.ToList())
            {
                if (this._playerVoiceData.ContainsKey(peer) == false)
                {
                    this._tempHaveData.Remove(peer);
                    continue;
                }
                PlayerVoiceData voiceData = this._playerVoiceData[peer];
                if (voiceData.InputWaveProvider.BufferedDuration.TotalMilliseconds > 0)
                {
                    hasDataToPlay = true;
                    voiceData.HasVoiceData = true;
                    Agent speakerAgent = peer.ControlledAgent;
                    float distance = speakerAgent == null ? 31f : controlledAgent.Position.Distance(speakerAgent.Position);
                    float distanceVolumeMultiplier = 1 - (distance / 31f);

                    voiceData.VolumeSampleProvider.Volume = distanceVolumeMultiplier < 0 ? 0 : distanceVolumeMultiplier;
                    if (peer.GetComponent<MissionPeer>().IsMutedFromGameOrPlatform)
                    {
                        voiceData.VolumeSampleProvider.Volume = 0;
                    }

                    byte[] output = new byte[pcmSize * 2];
                    output = voiceData.ReadSample(pcmSize);
                    short[] outputPcm = new short[pcmSize];
                    Buffer.BlockCopy(output, 0, outputPcm, 0, output.Length);


                    // Mix
                    for (int i = 0; i < pcmSize; i++)
                    {
                        int total = (int)mixedData[i] + (int)outputPcm[i];
                        if (total > Int16.MaxValue) total = Int16.MaxValue;
                        if (total < Int16.MinValue) total = Int16.MinValue;
                        mixedData[i] = (short)total;
                    }
                    count++;
                    if (this.OnPeerVoiceStatusUpdated != null) this.OnPeerVoiceStatusUpdated(voiceData, peer);

                }
                if (voiceData.InputWaveProvider.BufferedDuration.TotalMilliseconds == 0) this._tempHaveData.Remove(peer);
            }


            if (hasDataToPlay)
            {
                for (int i = 0; i < pcmSize; i++)
                {
                    mixedData[i] = (short)(mixedData[i] / count);
                }
                byte[] readedByte = new byte[pcmSize * 2];
                Buffer.BlockCopy(mixedData, 0, readedByte, 0, readedByte.Length);
                try
                {
                    bufferedWaveProvider.AddSamples(readedByte, 0, pcmSize * 2);
                }
                catch (InvalidOperationException e)
                {
                    bufferedWaveProvider.ClearBuffer();
                }
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this.IsActive == false || this.VoiceChatEnabled == false) return;
            try
            {
                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.V) && !this.IsRecording)
                {
                    this.IsRecording = true;
                    this.sourceStream.StartRecording();
                }
                if (Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.V) && this.IsRecording)
                {
                    this.IsRecording = false;
                    this.sourceStream.StopRecording();
                }
            }
            catch (MmException e)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PlayerVoiceDataError", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
            }
            catch (InvalidOperationException e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Invalid microphone please change it from your options menu.", Color.ConvertStringToColor("#FF0000FF")));
            }
        }


    }
}
