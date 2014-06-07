using System.Collections.Generic;

public interface IViewModelItem : IDiagramSubItem, IRefactorable
{
    string RelatedType { get; set; }
    string RelatedTypeName { get; }
    bool AllowEmptyRelatedType { get;  }
}