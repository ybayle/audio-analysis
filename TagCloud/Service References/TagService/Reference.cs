﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4200
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 3.0.40624.0
// 
namespace TagCloud.TagService {
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TagItem", Namespace="http://schemas.datacontract.org/2004/07/")]
    public partial class TagItem : object, System.ComponentModel.INotifyPropertyChanged {
        
        private bool IsSelectedField;
        
        private string NameField;
        
        private int WeightField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsSelected {
            get {
                return this.IsSelectedField;
            }
            set {
                if ((this.IsSelectedField.Equals(value) != true)) {
                    this.IsSelectedField = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Weight {
            get {
                return this.WeightField;
            }
            set {
                if ((this.WeightField.Equals(value) != true)) {
                    this.WeightField = value;
                    this.RaisePropertyChanged("Weight");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TagService.ITagCloudServicesvc")]
    public interface ITagCloudServicesvc {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ITagCloudServicesvc/GetTags", ReplyAction="http://tempuri.org/ITagCloudServicesvc/GetTagsResponse")]
        System.IAsyncResult BeginGetTags(System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<TagItem> EndGetTags(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ITagCloudServicesvc/GetTagsFiltered", ReplyAction="http://tempuri.org/ITagCloudServicesvc/GetTagsFilteredResponse")]
        System.IAsyncResult BeginGetTagsFiltered(System.Guid audioReadingID, System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<TagItem> EndGetTagsFiltered(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ITagCloudServicesvcChannel : ITagCloudServicesvc, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class GetTagsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetTagsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<TagItem> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<TagItem>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class GetTagsFilteredCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetTagsFilteredCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<TagItem> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<TagItem>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class TagCloudServicesvcClient : System.ServiceModel.ClientBase<ITagCloudServicesvc>, ITagCloudServicesvc {
        
        private BeginOperationDelegate onBeginGetTagsDelegate;
        
        private EndOperationDelegate onEndGetTagsDelegate;
        
        private System.Threading.SendOrPostCallback onGetTagsCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetTagsFilteredDelegate;
        
        private EndOperationDelegate onEndGetTagsFilteredDelegate;
        
        private System.Threading.SendOrPostCallback onGetTagsFilteredCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public TagCloudServicesvcClient() {
        }
        
        public TagCloudServicesvcClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TagCloudServicesvcClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TagCloudServicesvcClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TagCloudServicesvcClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                            "ookieContainerBindingElement.");
                }
            }
        }
        
        public event System.EventHandler<GetTagsCompletedEventArgs> GetTagsCompleted;
        
        public event System.EventHandler<GetTagsFilteredCompletedEventArgs> GetTagsFilteredCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult ITagCloudServicesvc.BeginGetTags(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetTags(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<TagItem> ITagCloudServicesvc.EndGetTags(System.IAsyncResult result) {
            return base.Channel.EndGetTags(result);
        }
        
        private System.IAsyncResult OnBeginGetTags(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((ITagCloudServicesvc)(this)).BeginGetTags(callback, asyncState);
        }
        
        private object[] OnEndGetTags(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<TagItem> retVal = ((ITagCloudServicesvc)(this)).EndGetTags(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetTagsCompleted(object state) {
            if ((this.GetTagsCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetTagsCompleted(this, new GetTagsCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetTagsAsync() {
            this.GetTagsAsync(null);
        }
        
        public void GetTagsAsync(object userState) {
            if ((this.onBeginGetTagsDelegate == null)) {
                this.onBeginGetTagsDelegate = new BeginOperationDelegate(this.OnBeginGetTags);
            }
            if ((this.onEndGetTagsDelegate == null)) {
                this.onEndGetTagsDelegate = new EndOperationDelegate(this.OnEndGetTags);
            }
            if ((this.onGetTagsCompletedDelegate == null)) {
                this.onGetTagsCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetTagsCompleted);
            }
            base.InvokeAsync(this.onBeginGetTagsDelegate, null, this.onEndGetTagsDelegate, this.onGetTagsCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult ITagCloudServicesvc.BeginGetTagsFiltered(System.Guid audioReadingID, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetTagsFiltered(audioReadingID, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<TagItem> ITagCloudServicesvc.EndGetTagsFiltered(System.IAsyncResult result) {
            return base.Channel.EndGetTagsFiltered(result);
        }
        
        private System.IAsyncResult OnBeginGetTagsFiltered(object[] inValues, System.AsyncCallback callback, object asyncState) {
            System.Guid audioReadingID = ((System.Guid)(inValues[0]));
            return ((ITagCloudServicesvc)(this)).BeginGetTagsFiltered(audioReadingID, callback, asyncState);
        }
        
        private object[] OnEndGetTagsFiltered(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<TagItem> retVal = ((ITagCloudServicesvc)(this)).EndGetTagsFiltered(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetTagsFilteredCompleted(object state) {
            if ((this.GetTagsFilteredCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetTagsFilteredCompleted(this, new GetTagsFilteredCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetTagsFilteredAsync(System.Guid audioReadingID) {
            this.GetTagsFilteredAsync(audioReadingID, null);
        }
        
        public void GetTagsFilteredAsync(System.Guid audioReadingID, object userState) {
            if ((this.onBeginGetTagsFilteredDelegate == null)) {
                this.onBeginGetTagsFilteredDelegate = new BeginOperationDelegate(this.OnBeginGetTagsFiltered);
            }
            if ((this.onEndGetTagsFilteredDelegate == null)) {
                this.onEndGetTagsFilteredDelegate = new EndOperationDelegate(this.OnEndGetTagsFiltered);
            }
            if ((this.onGetTagsFilteredCompletedDelegate == null)) {
                this.onGetTagsFilteredCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetTagsFilteredCompleted);
            }
            base.InvokeAsync(this.onBeginGetTagsFilteredDelegate, new object[] {
                        audioReadingID}, this.onEndGetTagsFilteredDelegate, this.onGetTagsFilteredCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override ITagCloudServicesvc CreateChannel() {
            return new TagCloudServicesvcClientChannel(this);
        }
        
        private class TagCloudServicesvcClientChannel : ChannelBase<ITagCloudServicesvc>, ITagCloudServicesvc {
            
            public TagCloudServicesvcClientChannel(System.ServiceModel.ClientBase<ITagCloudServicesvc> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginGetTags(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetTags", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<TagItem> EndGetTags(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<TagItem> _result = ((System.Collections.ObjectModel.ObservableCollection<TagItem>)(base.EndInvoke("GetTags", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetTagsFiltered(System.Guid audioReadingID, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = audioReadingID;
                System.IAsyncResult _result = base.BeginInvoke("GetTagsFiltered", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<TagItem> EndGetTagsFiltered(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<TagItem> _result = ((System.Collections.ObjectModel.ObservableCollection<TagItem>)(base.EndInvoke("GetTagsFiltered", _args, result)));
                return _result;
            }
        }
    }
}
