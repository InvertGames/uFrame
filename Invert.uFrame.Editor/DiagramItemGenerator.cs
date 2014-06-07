using System;
using System.Collections.Generic;

namespace Invert.uFrame.Editor
{
    public abstract class DiagramItemGenerator
    {
        public abstract Type DiagramItemType
        {
            get;
        }

        [Inject]
        public IUFrameContainer Container { get; set; }

        [Inject]
        public IElementsDataRepository Repository { get; set; }

        public object ObjectData { get; set; }

        public abstract IEnumerable<CodeGenerator> GetGenerators(ElementDesignerData diagramData, IDiagramItem item);
    }

    public abstract class DiagramItemGenerator<TData> : DiagramItemGenerator where TData : DiagramItem
    {
        public override Type DiagramItemType
        {
            get { return typeof(TData); }
        }

        public sealed override IEnumerable<CodeGenerator> GetGenerators(ElementDesignerData diagramData, IDiagramItem item)
        {
            return CreateGenerators(diagramData, item as TData);
        }
        public abstract IEnumerable<CodeGenerator> CreateGenerators(ElementDesignerData diagramData, TData item);

    }
}