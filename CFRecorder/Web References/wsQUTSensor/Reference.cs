﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.CompactFramework.Design.Data, Version 2.0.50727.42.
// 
namespace QUT.wsQUTSensor {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    using System.Data;
    
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="WebServiceSoap", Namespace="http://tempuri.org/")]
    public partial class WebService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public WebService() {
            this.Url = "http://www.mquter.qut.edu.au/qutsensor/service.asmx";
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/HelloWorld", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string HelloWorld() {
            object[] results = this.Invoke("HelloWorld", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginHelloWorld(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("HelloWorld", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public string EndHelloWorld(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTimeTable", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet GetTimeTable(string sensorID) {
            object[] results = this.Invoke("GetTimeTable", new object[] {
                        sensorID});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetTimeTable(string sensorID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetTimeTable", new object[] {
                        sensorID}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndGetTimeTable(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/AddAudioReading", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddAudioReading(string sensorGuid, string readingGuid, System.DateTime time, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] buffer) {
            this.Invoke("AddAudioReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer});
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAddAudioReading(string sensorGuid, string readingGuid, System.DateTime time, byte[] buffer, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("AddAudioReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer}, callback, asyncState);
        }
        
        /// <remarks/>
        public void EndAddAudioReading(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
    }
}
