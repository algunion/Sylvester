﻿namespace Sylvester.tf

open System
open System.Collections.Generic;
open System.Runtime.CompilerServices

open TensorFlow

open Sylvester
open Sylvester.Arithmetic
open Sylvester.Arithmetic.N10
open Sylvester.Collections
open Sylvester.Graphs
open Sylvester.Tensors

/// A graph of tensor operations.
type ITensorGraph =
    inherit IGraph
    abstract member NameScope:string with get,set
    abstract member Scope:string->unit
    abstract member Ends:unit->unit
    abstract member MakeName:string->string
    abstract member GetName:string->string
    abstract member Ops:ITensorFlowOps
    abstract member Add: Edge -> unit
    abstract member Add: Node -> unit
    
/// A graph of tensor operations with a known number of inputs and outputs.
and TensorGraph<'input, 'output when 'input :> Number and 'output :> Number>(scope:string) = 
    inherit Graph<'input, 'output, Edge>(scope)
    
    let tfGraph = c_api.TF_NewGraph() |?? lazy failwith "Could not create new TF_Graph."
    
    do tfGraph.Dependencies <- Array.empty<TF_Operation>

    do tfGraph.SetNameScope(scope)

    do base.Initialized <- tfGraph <> null && tfGraph.NameScope = scope
        
    ///Flat list of graph nodes
    member val Nodes = new Dictionary<string, Node>() with get
        
    /// Flat list of graph edges
    member val Edges = new Dictionary<string, Edge>() with get

    /// Stack of sub-scopes
    member val Scopes = new Stack<string>()

    /// VArray of graph inputs
    member val Inputs = vanew<'input, Edge> with get,set 

    /// VArray of graph outputs
    member val Outputs = vanew<'output, Edge> with get,set

    member internal x._Graph = tfGraph

    member x.NameScope with get() = tfGraph.NameScope and set(value) = tfGraph.SetNameScope(value)

    member x.Scope(subName:string) = 
        do if empty subName then failwith "New sub-scope name cannot be empty." 
        do if x.IsEmpty then failwith "Cannot create sub-scope name from empty graph."
        x.Scopes.Push (x.NameScope + "/" + subName)
        x.NameScope <- x.NameScope + "/" + subName

    member x.Ends() = if x.Scopes.Count > 0 then x.NameScope <- x.Scopes.Pop() else failwith "No sub-scopes currently exist."

    member x.WithOpName(opName:string) = tfGraph.MakeUniqueName(opName) 
    
    /// Add an edge (tensor) to the graph
    member x.AddEdge(e:Edge) =
        if (e.TensorGraph :> ITensorGraph).NameScope <> x.NameScope then 
            failwith "This tensor does not belong to this graph's namescope."
        else if x.Edges.ContainsKey(e.Name) then
                failwithf "The edge with name %s already exists in this graph." e.Name
            else
                x.Edges.Add(e.Name, e)
                do if not <| x.Nodes.ContainsKey(e.Head.Name) then x.AddNode(e.Head)
                
    /// Add a node (operation) to the graph
    member x.AddNode(n:Node) =
        do if (n.TensorGraph :> ITensorGraph).NameScope <> x.NameScope then failwith "This node does not belong to this graph's namescope."
        if x.Nodes.ContainsKey(n.Name) then
            failwithf "The node with name %s already exists in this graph." n.Name
        else        
            x.Nodes.Add(n.Name, n)
            Seq.iter (fun (e:Edge) -> if not <| x.Edges.ContainsKey(e.Name) then x.AddEdge(e) |> ignore) n.Inputs
                   
    member x.IsEmpty = x.NameScope = "_"

    static member val EmptyGraph = TensorGraph<zero, zero>("_") :> ITensorGraph with get

    static member val DefaultGraph:ITensorGraph = TensorGraph<zero, zero>("_") :> ITensorGraph with get, set

    interface ITensorGraph with
        member x.Handle = tfGraph.__Instance
        member x.MakeName s = tfGraph.MakeName s
        member x.GetName s = tfGraph.GetName s
        member x.NameScope with get() = x.NameScope and set(value) = x.NameScope <- value
        member x.Scope s = x.Scope(s)
        member x.Ends() = x.Ends()
        member x.Ops = tfGraph :> ITensorFlowOps
        member x.Add n = x.AddNode(n)
        member x.Add e = x.AddEdge(e)
        
    new() = TensorGraph("")
        
/// A tensor graph node consists of an operation with input and edges
and Node(graph: ITensorGraph, name:string, op:TF_Output[], inputs: Edge list) = 
    inherit Api()
    
    member val TensorGraph = graph  with get

    member val Name = graph.GetName(name) with get 

    member val Inputs = inputs with get, set

    member val Op = op with get

    interface INode<TF_Output[]> with
        member val Graph = graph :> IGraph with get,set
        member x.Name = x.Name
        member x.Output = op

    new(graph: ITensorGraph, name:string, op:TF_Output, inputs: Edge list) = Node(graph, name, [|op|], inputs)

/// A tensor graph edge represents tensor data of known or unknown shape flowing into or out of a graph and between graph nodes.
and Edge(graph: ITensorGraph, name:string, head:Node, output:int, dt:TF_DataType, ?shape:int64[]) = 
    inherit Api()
    
    member val TensorGraph = graph with get

    member val DataType = dt with get

    member val _Type = Convert.ToInt64(int dt) with get
    
    member val Name = graph.MakeName(name) with get

    member val Head:Node = head with get
    
    member x.Output = x.Head.Op.[output]
    
    member x.Shape = x :> IUnknownShape

    interface IUnknownShape with  
        member val Rank:Option<int> = if shape.IsSome then Some shape.Value.Length else None  with get, set
        member val Dims:Option<int64[]> = shape with get, set

    interface IEdge with
        member val Graph = graph :> IGraph with get,set
        member x.Name = x.TensorGraph.MakeName(name)
        member x._DataType = Convert.ToInt64(int dt)

/// A tensor graph edge with partially known shape
and Edge<'r when 'r :> Number>(graph:ITensorGraph, name:string, head: Node, output:int, dt:TF_DataType, shape:int64[]) =
    inherit Edge(graph, name, head, output, dt, shape)

    interface IEdge<'r> with
        member x.Rank = number<'r>

and Scope(g:ITensorGraph, name:string) =
    let parentScope = g.NameScope
    do g.NameScope <- name
    interface IDisposable with member x.Dispose() = g.NameScope <- parentScope

[<AutoOpen>]
module TensorGraph = 
    let dataType<'t> =
        match typeof<'t>.Name with
        | "Boolean" -> TF_DataType.TF_BOOL;
        | "SByte" -> TF_DataType.TF_INT8;
        | "Byte" -> TF_DataType.TF_UINT8;
        | "Int16" -> TF_DataType.TF_INT16;
        | "UInt16"-> TF_DataType.TF_UINT16;
        | "Int32" -> TF_DataType.TF_INT32;
        | "UInt32" -> TF_DataType.TF_UINT32;
        | "Int64" -> TF_DataType.TF_INT64;
        | "UInt64" -> TF_DataType.TF_UINT64;
        | "Single" -> TF_DataType.TF_FLOAT;
        | "Double" -> TF_DataType.TF_DOUBLE;
        | "Complex" -> TF_DataType.TF_COMPLEX128;
        | _ -> failwithf "The type %s cannot be converted to a TensorFlow tensor type" typeof<'t>.Name

    let ops (x:obj) =
        match x with
        | :? Node as node -> node.TensorGraph.Ops
        | :? Edge as edge -> edge.TensorGraph.Ops
        | :? ITensorGraph as graph -> graph.Ops
        | _ -> failwith "This type is not a tensor graph element."
      

    let tf (x:obj) =
        match x with
        | :? Node as node -> node.TensorGraph.Ops :?> TF_Graph
        | :? Edge as edge -> edge.TensorGraph.Ops :?> TF_Graph
        | :? ITensorGraph as graph -> graph.Ops :?> TF_Graph
        | _ -> failwith "This type is not a tensor graph element."
      
    let tg<'input, 'output when 'input :> Number and 'output :>Number>(g:ITensorGraph) = g :?> TensorGraph<'input, 'output>

    let defaultGraph = TensorGraph<zero, zero>.DefaultGraph

    let setDefaultGraph (graph:TensorGraph<_,_>) = 
        TensorGraph<zero, zero>.DefaultGraph <- graph
        graph

    let resetDefaultGraph() = TensorGraph<zero, zero>.DefaultGraph <- new TensorGraph<zero, zero>("_")

    let scope = defaultGraph.Scope

    let ends = defaultGraph.Ends