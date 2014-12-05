using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Invert.StateMachine;
using Invert.uFrame;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UniRx;
using UnityEngine;

public class UFrameEditorPlugin : DiagramPlugin
{
    public override decimal LoadPriority
    {
        get { return -1; }
    }

    public override bool Enabled
    {
        get { return true; }
        set
        {

        }
    }
    public override void Initialize(uFrameContainer container)
    {

        container.RegisterInstance<IEditorCommand>(new FindInSceneCommand(), "ViewDoubleClick");

        var typeContainer = uFrameEditor.TypesContainer;
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(int), Group = "", Label = "int", IsPrimitive = true }, "int");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(string), Group = "", Label = "string", IsPrimitive = true }, "string");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(decimal), Group = "", Label = "decimal", IsPrimitive = true }, "decimal");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(float), Group = "", Label = "float", IsPrimitive = true }, "float");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(bool), Group = "", Label = "bool", IsPrimitive = true }, "bool");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(char), Group = "", Label = "char", IsPrimitive = true }, "char");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(DateTime), Group = "", Label = "date", IsPrimitive = true }, "date");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(Vector2), Group = "", Label = "Vector2", IsPrimitive = true }, "Vector2");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(Vector3), Group = "", Label = "Vector3", IsPrimitive = true }, "Vector3");

        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ViewModelPropertyTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ClassPropertyTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ClassCollectionTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = true, PrimitiveOnly = false }, "ViewModelCommandTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ViewModelCollectionTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ComputedPropertyTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false, IncludeUnityEngine = true }, "StateMachineVariableTypeSelection");

        container.RegisterInstance<IDiagramNodeCommand>(new CreateSceneCommand(), "CreateScene");
        container.RegisterInstance<IDiagramNodeCommand>(new AddManagerToSceneCommand(), "AddToScene");
        container.RegisterInstance<IDiagramNodeCommand>(new AddManagerToSceneSelectionCommand(), "AddToSceneSelection");
        container.RegisterInstance<IDiagramNodeCommand>(new AddViewToSceneCommand(), "AddViewToScene");
        container.RegisterInstance<IDiagramNodeCommand>(new AddViewToSceneSelectionCommand(), "AddViewToSceneSelection");

        container.RegisterInstance<IUFrameTypeProvider>(new uFrameTypeProvider());
    }

    public class uFrameTypeProvider : IUFrameTypeProvider
    {

        public Type ViewModel
        {
            get { return typeof(ViewModel); }
        }

        public Type Controller
        {
            get { return typeof(Controller); }
        }

        public Type SceneManager
        {
            get { return typeof(SceneManager); }
        }

        public Type GameManager
        {
            get { return typeof(GameManager); }
        }

        public Type ViewComponent
        {
            get { return typeof(ViewComponent); }
        }

        public Type ViewBase
        {
            get { return typeof(ViewBase); }
        }

        public Type UFToggleGroup
        {
            get { return typeof(UFToggleGroup); }
        }

        public Type UFGroup
        {
            get { return typeof(UFGroup); }
        }

        public Type UFRequireInstanceMethod
        {
            get { return typeof(UFRequireInstanceMethod); }
        }

        public Type DiagramInfoAttribute
        {
            get { return typeof(DiagramInfoAttribute); }
        }

        public Type GetModelCollectionType<T>()
        {
            return typeof(ModelCollection<T>);
        }

        public Type UpdateProgressDelegate
        {
            get { return typeof(UpdateProgressDelegate); }
        }

        public Type CommandWithSenderT
        {
            get { return typeof(CommandWithSender<>); }
        }

        public Type CommandWith
        {
            get { return null; }
        }

        public Type CommandWithSenderAndArgument
        {
            get { return typeof(CommandWithSenderAndArgument<,>); }
        }

        //public Type YieldCommandWithSenderT
        //{
        //    get { return typeof (YieldCommandWithSender<>); }
        //}

        //public Type YieldCommandWith
        //{
        //    get { return typeof (YieldCommandWith<>); }
        //}

        //public Type YieldCommandWithSenderAndArgument
        //{
        //    get { return typeof (YieldCommandWithSenderAndArgument<,>); }
        //}

        //public Type YieldCommand
        //{
        //    get { return typeof (YieldCommand); }
        //}

        public Type Command
        {
            get { return typeof(Command); }
        }

        public Type ICommand
        {
            get { return typeof(IuFrameCommand); }
        }

        public Type ListOfViewModel
        {
            get { return typeof(List<ViewModel>); }
        }

        public Type ISerializerStream
        {
            get { return typeof(ISerializerStream); }
        }

        public Type P
        {
            get { return typeof(P<>); }
        }

        public Type ModelCollection
        {
            get { return typeof(ModelCollection<>); }
        }

        public Type Computed
        {
            get { return typeof(Computed<>); }
        }

        public Type State
        {
            get { return typeof(State); }
        }

        public Type StateMachine
        {
            get { return typeof(StateMachine); }
        }

        public Type IObservable
        {
            get { return typeof(IObservable<>); }
        }


        public override string ToString()
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();
            sb.AppendFormat("public class uFrameStringTypeProvider : IUFrameTypeProvider {{");
            sb.AppendLine();
            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(Type))
                    continue;
                var type = property.GetValue(this, null) as Type;
                if (type == null) continue;
                sb.AppendFormat("\tprivate Type _{0};", property.Name);
                sb.AppendLine();
                sb.AppendFormat("\tpublic Type {0} {{ get {{ return _{0} ?? (_{0} = uFrameEditor.FindType(\"{1}\")); }} }}", property.Name,
                    type.FullName);
                sb.AppendLine();
            }
            sb.Append("}");
            sb.AppendLine();
            return sb.ToString();
        }
    }

}