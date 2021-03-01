using GalaSoft.MvvmLight.Messaging;
using System;
using System.IO;

namespace AudioMerger {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class main {
        
        public main() {
            //Get tape folder
            if (TapeRecorderPath == "\\Tape")
			{
                TapeRecorderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoiceMeeter");
			}
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
            Messenger.Default.Send(new Messages.SettingChanged()
            {
                Name = e.SettingName,
                New = e.NewValue
            });
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
