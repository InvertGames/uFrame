namespace Invert.uFrame.Editor.Refactoring
{
    public class RenamePropertyRefactorer : RenameRefactorer
    {
        public RenameIdentifierRefactorer Name { get; set; }
        public RenameIdentifierRefactorer NameAsChangedMethod { get; set; }
        public RenameIdentifierRefactorer NameAsTwoWayMethod { get; set; }
        public RenameIdentifierRefactorer FieldName { get; set; }
        public RenameIdentifierRefactorer ViewFieldName { get; set; }
        public RenameIdentifierRefactorer NameAsPrefabBindingOption { get; set; }
        public RenameIdentifierRefactorer NameAsTwoWayBindingOption { get; set; }
        
        public RenamePropertyRefactorer(ViewModelPropertyData data)
        {
            Name = new RenameIdentifierRefactorer() { From = data.Name };
            NameAsChangedMethod = new RenameIdentifierRefactorer() { From = data.NameAsChangedMethod };
            NameAsTwoWayMethod = new RenameIdentifierRefactorer() { From = data.NameAsTwoWayMethod };
            FieldName = new RenameIdentifierRefactorer() { From = data.FieldName };
            ViewFieldName = new RenameIdentifierRefactorer() { From = data.ViewFieldName };
            NameAsPrefabBindingOption = new RenameIdentifierRefactorer() { From = data.NameAsPrefabBindingOption };
            NameAsTwoWayBindingOption = new RenameIdentifierRefactorer() { From = data.NameAsTwoWayBindingOption };
            

        }
        public override void Set(ISelectable data)
        {
            Set((ViewModelPropertyData)data);
        }
        public void Set(ViewModelPropertyData data)
        {
            Name.To = data.Name;
            NameAsChangedMethod.To = data.NameAsChangedMethod;
            NameAsTwoWayMethod.To = data.NameAsTwoWayMethod;
            FieldName.To = data.FieldName;
            ViewFieldName.To = data.ViewFieldName;
            NameAsPrefabBindingOption.To = data.NameAsPrefabBindingOption;
            NameAsTwoWayBindingOption.To = data.NameAsTwoWayBindingOption;
            
        }
        public override void Process(RefactorContext context)
        {
            //Name.Process(context);
            //NameAsChangedMethod.Process(context);
            //NameAsTwoWayMethod.Process(context);
            //FieldName.Process(context);
            //ViewFieldName.Process(context);
            //NameAsPrefabBindingOption.Process(context);
            //NameAsTwoWayBindingOption.Process(context);
        }
        public override void PostProcess(RefactorContext context)
        {
            //Name.PostProcess(context);
            //NameAsChangedMethod.PostProcess(context);
            //NameAsTwoWayMethod.PostProcess(context);
            //FieldName.PostProcess(context);
            //ViewFieldName.PostProcess(context);
            //NameAsPrefabBindingOption.PostProcess(context);
            //NameAsTwoWayBindingOption.PostProcess(context);
        }
    }
}