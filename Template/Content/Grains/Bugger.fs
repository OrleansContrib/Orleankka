module BuggerGrains

/// Need to explicitly reference smth from Orleans
/// as F# linker is being too agressive and removes reference 
/// to Orleans.Abstractions which is required for codegen to work
type IFSharpGotcha =
   inherit Orleans.IGrainWithGuidKey