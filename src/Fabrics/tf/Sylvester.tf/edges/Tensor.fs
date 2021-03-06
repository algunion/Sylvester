﻿namespace Sylvester.tf

open System

open Sylvester.Arithmetic

type IDataType<'t> = interface end

/// Tensor of unknown rank and dimensions
type Tensor<'t when 't:> ValueType and 't : struct  and 't: (new: unit -> 't) and 't :> IEquatable<'t> and 't :> IFormattable and 't :> IComparable>
        (graph:ITensorGraph, head:Node, output:int, ?shape:int64[]) as this = 
    inherit Edge(graph, head, output, dataType<'t>, defaultArg shape null) 
    do graph.Add this
    interface IDataType<'t>

    new (name:string, ?shape:int64[], ?graph: ITensorGraph) = 
        let g = defaultArg graph defaultGraph
        let op = tf(g).Placeholder(dataType<'t>, defaultArg shape null, name)
        new Tensor<'t>(g, new Node(g, op, []), 0, defaultArg shape null)
            
/// Tensor of known rank but unknown dimensions
type Tensor<'r, 't when 'r :> Number and 't:> ValueType and 't : struct  and 't: (new: unit -> 't) and 't :> IEquatable<'t> and 't :> IFormattable and 't :> IComparable>
        (graph:ITensorGraph, head:Node, output:int, ?shape:int64[]) as this = 
    inherit Edge<'r>(graph, head, output, dataType<'t>, defaultArg shape (Array.create number<'r>.IntVal 0L))
    do if number<'r>.IntVal > 0 && shape.IsSome && shape.Value.Length <> number<'r>.IntVal then failwith "The shape array parameter length does not match the tensor's type rank."
    do graph.Add this

    member x.Rank = number<'r>
    interface IPartialShape<'r> with member x.Rank = x.Rank
        
    new(name:string, ?shape:int64[], ?graph:ITensorGraph) = 
        let g = defaultArg graph defaultGraph
        let op = tf(g).Placeholder(dataType<'t>, defaultArg shape (Array.create number<'r>.IntVal 0L), name)
        new Tensor<'r, 't>(g, new Node(g, op, []), 0, defaultArg shape (Array.create number<'r>.IntVal 0L))
        
    //Arithmetic operators for fully known shapes
    static member (+) (l:'x, r:'x when 'x :> Tensor<'r, _> and 'x :> IFullShape<'r>) =  
        let node = add l.Head r.Head
        node.Inputs <- [l; r]
        createEdge<'x>(l.Graph, node, 0)

    static member (-) (l:'x, r:'x when 'x :> Tensor<'r, _> and 'x :> IFullShape<'r>) = 
        let node = sub l.Head r.Head
        node.Inputs <- [l; r]
        createEdge<'x>(l.Graph, node, 0)

        