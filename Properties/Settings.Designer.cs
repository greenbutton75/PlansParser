﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PlansParser.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Program Files (x86)\\ABBYY FineReader 12\\FineCmd.exe")]
        public string PathToFineCmd {
            get {
                return ((string)(this["PathToFineCmd"]));
            }
            set {
                this["PathToFineCmd"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://askebsa.dol.gov/FOIA%20Files/{0}/All/")]
        public string GovAskaUrl {
            get {
                return ((string)(this["GovAskaUrl"]));
            }
            set {
                this["GovAskaUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("vvk@inbox.com")]
        public string AJAXFCTLogin {
            get {
                return ((string)(this["AJAXFCTLogin"]));
            }
            set {
                this["AJAXFCTLogin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("000b59c67bf196a4758191e42f76670ceba000")]
        public string AJAXFCTPassword {
            get {
                return ((string)(this["AJAXFCTPassword"]));
            }
            set {
                this["AJAXFCTPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int DownloadGovPlansPdfThreadCount {
            get {
                return ((int)(this["DownloadGovPlansPdfThreadCount"]));
            }
            set {
                this["DownloadGovPlansPdfThreadCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int FineCmdThreadCount {
            get {
                return ((int)(this["FineCmdThreadCount"]));
            }
            set {
                this["FineCmdThreadCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F:\\Tor Browser\\firefox.exe")]
        public string TorBrowserPath {
            get {
                return ((string)(this["TorBrowserPath"]));
            }
            set {
                this["TorBrowserPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ScraperFilePath {
            get {
                return ((string)(this["ScraperFilePath"]));
            }
            set {
                this["ScraperFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://12.158.148.210/portal/app/disseminate?execution=e13s2")]
        public string DownloadPdfUrl {
            get {
                return ((string)(this["DownloadPdfUrl"]));
            }
            set {
                this["DownloadPdfUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int XlsToCsvConverThreadCount {
            get {
                return ((int)(this["XlsToCsvConverThreadCount"]));
            }
            set {
                this["XlsToCsvConverThreadCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int DownloadPdfMaxRetryCount {
            get {
                return ((int)(this["DownloadPdfMaxRetryCount"]));
            }
            set {
                this["DownloadPdfMaxRetryCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool NeedTorBrowser {
            get {
                return ((bool)(this["NeedTorBrowser"]));
            }
            set {
                this["NeedTorBrowser"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F:\\Rextema\\Plans 03.01.2017\\")]
        public string BaseDirFolder {
            get {
                return ((string)(this["BaseDirFolder"]));
            }
            set {
                this["BaseDirFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{\"\" : [\"ii\", \"portfolio\", \"administrative\", \"Annuity\", \"fund\", \"class\", \"the\", \"i" +
            "nc\"] }")]
        public string WordToReplace {
            get {
                return ((string)(this["WordToReplace"]));
            }
            set {
                this["WordToReplace"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://rixtrema.net/RixtremaWS401k/")]
        public string AJAXFCTUrl {
            get {
                return ((string)(this["AJAXFCTUrl"]));
            }
            set {
                this["AJAXFCTUrl"] = value;
            }
        }
    }
}
