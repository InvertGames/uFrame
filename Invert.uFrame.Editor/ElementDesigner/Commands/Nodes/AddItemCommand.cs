using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public abstract class AddItemCommand<TType> : AddNewCommand, IDiagramContextCommand
    {
        public override string CanPerform(ElementsDiagram item)
        {
            if (item == null) return "Diagram must be loaded first.";

            if (!item.Data.CurrentFilter.IsAllowed(null, typeof(TType)))
                return "Item is not allowed in this part of the diagram.";

            return null;
        }

        public override string Path
        {
            get { return  "Add New/" + Title; }
        }
    }

    public interface IChildCommand
    {
        Type ChildCommandFor { get; }
    }

    public class RenameCommand : EditorCommand<IDiagramItem>, IDiagramItemCommand
    {
        public override void Perform(IDiagramItem item)
        {
            item.BeginEditing();
        }

        public override string CanPerform(IDiagramItem item)
        {
            if (item == null) return "Invalid argument";
            return null;
        }
    }

    public interface IDiagramContextCommand
    {
        
    }
    public interface IDiagramItemCommand
    {
        
    }

    public interface IDiagramSubItemCommand
    {
        
    }
    
    public class DeleteCommand : EditorCommand<IDiagramItem>, IDiagramItemCommand
    {
        public override void Perform(IDiagramItem item)
        {
            item.RemoveFromDiagram();
        }

        public override string CanPerform(IDiagramItem item)
        {
            return null;
        }
    }
    public class HideCommand : EditorCommand<IDiagramItem>, IDiagramItemCommand
    {
        public override void Perform(IDiagramItem item)
        {
            item.Data.CurrentFilter.Locations.Remove(item.Identifier);
        }

        public override string CanPerform(IDiagramItem item)
        {
            if (item == null) return "Diagram Item must not be null.";
            return null;
        }
    }

    public class OpenCommand : EditorCommand<IDiagramItem>, IDynamicOptionsCommand, IDiagramItemCommand
    {
        public override void Perform(IDiagramItem item)
        {
            //var filename = repository.GetControllerCustomFilename(this.Name);

            //var scriptAsset = AssetDatabase.LoadAssetAtPath(filename, typeof(TextAsset));

            //AssetDatabase.OpenAsset(scriptAsset);
        }

        public override string CanPerform(IDiagramItem item)
        {
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var diagramItem = item as IDiagramItem;
            if (diagramItem == null) yield break;
            var generators = uFrameEditor.GetAllCodeGenerators(diagramItem.Data)
                .Where(p =>!p.IsDesignerFile && p.ObjectData == item);

            foreach (var codeGenerator in generators)
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Open/" + codeGenerator.Filename,
                    Value = codeGenerator
                };
            }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }

    public class RemoveLinkCommand : EditorCommand<IDiagramItem>, IDynamicOptionsCommand, IDiagramItemCommand
    {
        public override void Perform(IDiagramItem item)
        {
            var link = SelectedOption.Value as IDiagramLink;

            if (link != null) 
                link.Source.RemoveLink(item);
        }

        public override string CanPerform(IDiagramItem item)
        {
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var diagramItem = item as IDiagramItem;
            if (diagramItem == null) yield break;
            var links = diagramItem.Data.Links.Where(p => p.Target == item);
            foreach (var link in links)
            {
                yield return new UFContextMenuItem()
                {
                    Checked = true,
                    Name = "Remove Link From/" + link.Source.Label,
                    Value = link
                };
            }

        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }

    public class SelectViewBaseElement : EditorCommand<ViewData>, IDynamicOptionsCommand, IDiagramItemCommand
    {
        public override void Perform(ViewData item)
        {
            if (item == null) return;
            var view = SelectedOption.Value as ViewData;
            if (view == null)
            {
                item.BaseViewIdentifier = null;
            }
            else
            {
                item.BaseViewIdentifier = view.Identifier;    
            }
            
        }

        public override string CanPerform(ViewData item)
        {
            if (item == null) return "This operation can only be performed on a view.";
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var view = item as ViewData;
            if (view == null) yield break;
            var element = view.ViewForElement;
            if (element == null) yield break;

            var baseViews = element.AllBaseTypes.SelectMany(
                p => view.Data.AllDiagramItems.OfType<ViewData>().Where(x => x.ViewForElement == p));
            yield return new UFContextMenuItem()
            {
                Name = "Base View/" + element.NameAsViewBase,
                Value = null
            };
            foreach (var baseView in baseViews)
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Base View/" + baseView.NameAsView,
                    Value = baseView
                };
            }

        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }

    public class ScaleCommand : EditorCommand<float>
    {
        public float Scale { get; set; }

        public override void Perform(float item)
        {

            UFStyles.Scale = Scale;

            
        }

        public override string CanPerform(float item)
        {
            if (Scale < 0.5f)
            {
                return "Can't scale that small";
            }
            return null;
        }
    }
}
