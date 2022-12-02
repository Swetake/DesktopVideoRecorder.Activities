using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using DesktopVideoRecorder.Activities.Design.Designers;
using DesktopVideoRecorder.Activities.Design.Properties;

namespace DesktopVideoRecorder.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(StartRecording), categoryAttribute);
            builder.AddCustomAttributes(typeof(StartRecording), new DesignerAttribute(typeof(StartRecordingDesigner)));
            builder.AddCustomAttributes(typeof(StartRecording), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(StopRecording), categoryAttribute);
            builder.AddCustomAttributes(typeof(StopRecording), new DesignerAttribute(typeof(StopRecordingDesigner)));
            builder.AddCustomAttributes(typeof(StopRecording), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(RecordDesktopVideoScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(RecordDesktopVideoScope), new DesignerAttribute(typeof(RecordDesktopVideoScopeDesigner)));
            builder.AddCustomAttributes(typeof(RecordDesktopVideoScope), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
