﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AudioMerger {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    public sealed partial class main : global::System.Configuration.ApplicationSettingsBase {
        
        private static main defaultInstance = ((main)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new main())));
        
        public static main Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\Tape")]
        public string TapeRecorderPath {
            get {
                return ((string)(this["TapeRecorderPath"]));
            }
            set {
                this["TapeRecorderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\RECORD\\VOICE")]
        public string PhysicalRecorderPath {
            get {
                return ((string)(this["PhysicalRecorderPath"]));
            }
            set {
                this["PhysicalRecorderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\Record project")]
        public string MergeTo {
            get {
                return ((string)(this["MergeTo"]));
            }
            set {
                this["MergeTo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300000")]
        public double FileCheckFrequency {
            get {
                return ((double)(this["FileCheckFrequency"]));
            }
            set {
                this["FileCheckFrequency"] = value;
            }
        }
    }
}
