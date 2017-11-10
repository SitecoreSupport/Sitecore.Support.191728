using Sitecore.ExperienceEditor.Pipelines.GetExperienceEditorRibbon;
using Sitecore.ExperienceEditor.Speak.Ribbon.PageExtender;
using Sitecore.Publishing;
using System;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Pipelines.GetExperienceEditorRibbon
{
  public class AddWebEditRibbon: WebEditStateProcessor
  {
    public override void AddControl(GetExperienceEditorRibbonArgs args)
    {
      if (!WebEditStateProcessor.IsWebEditContent())
      {
        string str = string.Empty;
        if (WebEditStateProcessor.IsWebEditState())
        {
          str = Sitecore.ExperienceEditor.Speak.Ribbon.Constants.Ribbons["edit"];
          PreviewManager.RestoreUser();
        }
        else if (WebEditStateProcessor.IsPreviewState())
        {
          str = Sitecore.ExperienceEditor.Speak.Ribbon.Constants.Ribbons["preview"];
        }
        else if (WebEditStateProcessor.IsDebugState())
        {
          str = Sitecore.ExperienceEditor.Speak.Ribbon.Constants.Ribbons["debug"];
        }
        if (str != string.Empty)
        {
          RibbonWebControl control = new RibbonWebControl
          {
            State = str
          };
          args.Control = control;
        }
      }
    }

  }
}