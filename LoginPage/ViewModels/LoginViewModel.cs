
using DataAccess;
using DataAccess.NetWork;
using LoginPage.Views;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using SettingPage.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace LoginPage.ViewModels
{
    public class LoginViewModel : PrismCommonViewModelBase, INetReceiver
    {
        private readonly string _localPath;
        private readonly string _filePath;
        public ReactiveProperty<string> IP { get; set; }
        public ReactiveProperty<int> Port { get; set; }
        public ReactiveProperty<string> ID { get; set; }
        public ReactiveProperty<string> Password { get; set; }
        public ReactiveCommand TryLogin { get; set; }
        public ReactiveCommand Exit { get; set; }
        public ReactiveProperty<bool> IsWorking { get; set; }
        public ReactiveProperty<bool> AutoLogin { get; set; }
        public ReactiveProperty<bool> SaveLoginData { get; set; }
        public IContainerProvider containerProvider { get; set; }
        public LoginViewModel(IContainerProvider containerProvider)
        {
            this.AutoLogin = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            this.SaveLoginData = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            this.IP = new ReactiveProperty<string>().AddTo(this.disposable);
            this.Port = new ReactiveProperty<int>().AddTo(this.disposable);
            this.ID = new ReactiveProperty<string>().AddTo(this.disposable);
            this.Password = new ReactiveProperty<string>().AddTo(this.disposable);
            this.TryLogin = new ReactiveCommand().WithSubscribe(() => ExecuteTryLogin());
            this.Exit = new ReactiveCommand().WithSubscribe(() => ExecuteExit());
            this.IsWorking = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            this.containerProvider = containerProvider;

            #region 로그인 정보 저장 
            this._localPath = AppDomain.CurrentDomain.BaseDirectory;
            this._filePath = Path.Combine(this._localPath, "LocalData.json");
            
            #endregion
            initLoacalData();
        }

        private void initLoacalData()
        {
            JObject jobj = null;
            if (File.Exists(this._filePath))
            {
                jobj = JObject.Parse(File.ReadAllText(this._filePath)); 
                if (jobj["SaveLoginData"] != null)
                    this.SaveLoginData.Value = jobj["SaveLoginData"].ToObject<bool>();
                if (jobj["AutoLogin"] != null)
                    this.AutoLogin.Value = jobj["AutoLogin"].ToObject<bool>();
                if (this.SaveLoginData.Value)
                {
                    if (jobj["Id"] != null)
                        this.ID.Value = jobj["Id"].ToObject<string>();
                    if (jobj["Pw"] != null)
                        this.Password.Value = jobj["Pw"].ToString();
                    if (jobj["IP"] != null)
                        this.IP.Value = jobj["IP"].ToString();
                    if (jobj["Port"] != null)
                        this.Port.Value = jobj["Port"].ToObject<int>();
                }
                if (this.AutoLogin.Value)
                    ExecuteTryLogin();
            }
        }

        public bool ExecuteTryLogin() {
            this.IsWorking.Value = true;
            using (var Agent = containerProvider.Resolve<DataAgent.LoginAgent>()) {
                Agent.SetReceiver(this);
                if (this.IP.Value == String.Empty || this.Port.Value == 0 || this.ID.Value == String.Empty || this.Password.Value == String.Empty)
                {

                }
                else { 
                    Agent.TryLogin(this.IP.Value, this.Port.Value, this.ID.Value, this.Password.Value);
                }
                //Agent.TryLogin("192.168.0.121", 2001, "admin", "1234");
                ErpLogWriter.LogWriter.Error(string.Format("Try Connect IP : {0} , {1}, {2},{3}",this.IP.Value, this.Port.Value,this.ID.Value,this.Password.Value));
            }
            //로그인 시도 중
            return true;
        }
        public void ExecuteExit() {
            Process[] p = Process.GetProcessesByName("LucelasV4");
            if (p.Length > 0) {
                p[0].Kill();
            }
            
        }

        public void OnRceivedData(ErpPacket packet)
        {
            this.IsWorking.Value = false;
            if (packet.Body.Length > 0)
            {
                string Body = Encoding.UTF8.GetString(packet.Body);
                JObject jobj = new JObject(JObject.Parse(Body));

                if (packet.Header.CMD == (ushort)COMMAND.LOGIN)
                {
                    
                    //들어오는 값에 따라 다른 메시지 박스 출력 switch 문


                    //정상로그인 일때 Result = true 넣음
                    var win = this.containerProvider.Resolve<Login>();
                    if (win != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            win.DialogResult = true;
                        });
                    }
                    SaveData();
                }
            }
        }

        private void SaveData()
        {
            JObject jobj = new JObject();
            jobj["SaveLoginData"] = this.SaveLoginData.Value;
            jobj["AutoLogin"] = this.AutoLogin.Value;
            if (this.SaveLoginData.Value) {
                jobj["IP"] = this.IP.Value;
                jobj["Port"] = this.Port.Value;
                jobj["Id"] = this.ID.Value;
                jobj["Pw"] = this.Password.Value;
            }
            File.WriteAllText(this._filePath, jobj.ToString());
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

        public void OnSent()
        {
            throw new NotImplementedException();
        }

    }
}
