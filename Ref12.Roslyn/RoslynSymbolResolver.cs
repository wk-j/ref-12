﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using SLaks.Ref12.Services;

using MySymbolInfo = SLaks.Ref12.Services.SymbolInfo;

namespace Ref12.Roslyn {
	public class RoslynSymbolResolver : ISymbolResolver {
		public MySymbolInfo GetSymbolAt(string sourceFileName, SnapshotPoint point) {
			// Yes; this is evil and synchronously waits for async tasks.
			// That is exactly what Roslyn's GoToDefinitionCommandHandler
			// does; apparently a VS command handler can't be truly async
			// (Roslyn does use IWaitIndicator, which I can't).


			var doc = point.Snapshot.GetRelatedDocumentsWithChanges().FirstOrDefault();
			var model = doc.GetSemanticModelAsync().Result;
			var symbol = SymbolFinder.FindSymbolAtPosition(model, point, doc.Project.Solution.Workspace);
			if (symbol == null || symbol.ContainingAssembly == null)
				return null;

			return new MySymbolInfo(
				IndexIdTranslator.GetId(symbol),
				isLocal: doc.Project.Solution.Workspace.Kind != WorkspaceKind.MetadataAsSource && doc.Project.Solution.GetProject(symbol.ContainingAssembly) != null,
				assemblyPath: null,	// Cannot determine?
				assemblyName: symbol.ContainingAssembly.Identity.Name
			);
		}
	}

}
